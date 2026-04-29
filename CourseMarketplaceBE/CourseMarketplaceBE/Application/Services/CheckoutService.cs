using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Application.Services;

/// <summary>
/// SOLID — SRP: Class này CHỈ điều phối luồng nghiệp vụ Checkout.
///   - Không biết Stripe SDK (dùng IPaymentGatewayService — DIP).
///   - Không biết EF Core (dùng ICheckoutRepository — DIP).
///   - Không xử lý HTTP/JWT (Controller lo — SRP).
///
/// ★ Stripe Connect Marketplace Flow:
///   Bước 1 (Checkout): Thu tiền vào platform account (transfer_group = orderId)
///   Bước 2 (Success):  Tạo Transfer từ platform → instructor connected account
///                      (dựa trên transfer_rate: 70% instructor, 30% platform)
///
/// Luồng giao dịch được bảo vệ bởi IDbContextTransaction (Unit of Work):
///   Nếu BẤT KỲ bước nào lỗi → Rollback toàn bộ, không để data inconsistent.
/// </summary>
public class CheckoutService : ICheckoutService
{
    private readonly ICheckoutRepository _repo;
    private readonly IPaymentGatewayService _paymentGateway;
    private readonly ILogger<CheckoutService> _logger;

    // ── Tỷ lệ instructor nhận được (có thể chuyển sang system_configs sau) ──
    private const decimal DefaultTransferRate = 70.00m; // 70% cho instructor, 30% cho sàn

    public CheckoutService(
        ICheckoutRepository repo,
        IPaymentGatewayService paymentGateway,
        ILogger<CheckoutService> logger)
    {
        _repo = repo;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BƯỚC 1: INITIATE CHECKOUT
    // Cart → Order → Stripe Session (với transfer_group) → Transaction
    // ═══════════════════════════════════════════════════════════════════════════
    public async Task<CheckoutResponse> InitiateCheckoutAsync(
        int userId,
        string? couponCode,
        string successUrl,
        string cancelUrl)
    {
        // ── 1.1 Lấy cart items kèm Course + Instructor ──────────────────────
        var cartItems = await _repo.GetCartItemsWithCourseAndInstructorAsync(userId);
        if (!cartItems.Any())
            throw new InvalidOperationException("Giỏ hàng trống. Không thể thanh toán.");

        // ── 1.2 Kiểm tra không mua trùng (đã enrolled) ──────────────────────
        foreach (var item in cartItems)
        {
            if (item.CourseId.HasValue && await _repo.IsEnrolledAsync(userId, item.CourseId.Value))
                throw new InvalidOperationException(
                    $"Bạn đã mua khóa học \"{item.Course?.Title}\" rồi. Vui lòng xóa khỏi giỏ hàng.");
        }

        // ── 1.3 Tính toán coupon (nếu có) ────────────────────────────────────
        Coupon? coupon = null;
        decimal discountAmount = 0m;
        decimal subTotal = cartItems.Sum(c => c.Price ?? c.Course?.Price ?? 0m);

        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            coupon = await _repo.GetValidCouponAsync(couponCode, DateTime.Now);
            if (coupon != null && subTotal >= coupon.MinOrderValue)
            {
                discountAmount = coupon.CouponType == "percentage"
                    ? subTotal * (coupon.DiscountValue / 100m)
                    : coupon.DiscountValue;
                discountAmount = Math.Round(Math.Min(discountAmount, subTotal), 2);
            }
        }

        // ── 1.4 Tính giá thực tế cho mỗi item (phân bổ discount đều) ────────
        var totalAfterDiscount = Math.Max(subTotal - discountAmount, 0m);
        var discountRatio = subTotal > 0 ? totalAfterDiscount / subTotal : 1m;

        // Lấy email user cho Stripe
        var userEmail = await _repo.GetUserEmailAsync(userId);

        // ════ BẮT ĐẦU DB TRANSACTION (Unit of Work) ════════════════════════
        await using var dbTransaction = await _repo.BeginTransactionAsync();
        try
        {
            // ── 1.5 INSERT order_info (status: 'pending') ────────────────────
            var order = new OrderInfo
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                OrderStatus = "pending",
                PaymentMethod = "stripe"
            };
            await _repo.AddOrderAsync(order);
            await _repo.SaveChangesAsync(); // Để lấy OrderId (auto-generated)

            // ★ transfer_group = "order_{orderId}" — nhóm transfers theo đơn hàng
            var transferGroup = $"order_{order.OrderId}";

            // ── 1.6 INSERT order_items ───────────────────────────────────────
            var orderItems = new List<OrderItem>();
            var paymentLineItems = new List<PaymentLineItem>();

            foreach (var cartItem in cartItems)
            {
                var originalPrice = cartItem.Price ?? cartItem.Course?.Price ?? 0m;
                var purchasePrice = Math.Round(originalPrice * discountRatio, 2);
                var hasCoupon = coupon != null && discountAmount > 0;

                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    CourseId = cartItem.CourseId,
                    PurchasePrice = purchasePrice,
                    CouponUsed = hasCoupon
                };
                await _repo.AddOrderItemAsync(orderItem);
                orderItems.Add(orderItem);

                // Chuẩn bị line items cho Stripe (DIP — dùng DTO, không expose Entity)
                paymentLineItems.Add(new PaymentLineItem
                {
                    CourseName = cartItem.Course?.Title ?? "Khóa học",
                    ThumbnailUrl = cartItem.Course?.CourseThumbnailUrl,
                    UnitPrice = purchasePrice
                });
            }

            await _repo.SaveChangesAsync(); // Lấy OrderItem.Id

            // ── 1.7 Gọi Payment Gateway (Stripe) ────────────────────────────
            // ★ Truyền transferGroup để Stripe nhóm các Transfer sau khi thanh toán
            var paymentResult = await _paymentGateway.CreateCheckoutSessionAsync(
                paymentLineItems,
                successUrl,
                cancelUrl,
                userEmail,
                transferGroup);  // ← NEW: transfer_group liên kết với order

            // ── 1.8 INSERT transactions (mỗi order_item 1 transaction) ──────
            foreach (var orderItem in orderItems)
            {
                var instructorId = cartItems
                    .FirstOrDefault(c => c.CourseId == orderItem.CourseId)?
                    .Course?.InstructorId;

                var transaction = new Transaction
                {
                    OrderItemId = orderItem.Id,
                    AccountFrom = userId,
                    AccountTo = instructorId, // Instructor nhận tiền
                    Amount = orderItem.PurchasePrice,
                    TransferRate = DefaultTransferRate,
                    StripeSessionId = paymentResult.SessionId,
                    Currency = "USD",
                    TransactionsStatus = "pending",
                    TransactionType = "payment",
                    TransactionCreatedAt = DateTime.Now
                };
                await _repo.AddTransactionAsync(transaction);
            }

            await _repo.SaveChangesAsync();

            // ════ COMMIT — Tất cả thành công ════════════════════════════════
            await dbTransaction.CommitAsync();

            return new CheckoutResponse
            {
                SessionUrl = paymentResult.SessionUrl,
                SessionId = paymentResult.SessionId
            };
        }
        catch
        {
            // ════ ROLLBACK — Bất kỳ lỗi nào xảy ra ════════════════════════
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BƯỚC 2: PROCESS PAYMENT SUCCESS
    // Stripe báo thành công → Update trạng thái → Cấp enrollment →
    // ★ TẠO STRIPE TRANSFERS (chia tiền cho instructor) → Ghi payout record
    // ═══════════════════════════════════════════════════════════════════════════
    public async Task ProcessPaymentSuccessAsync(string sessionId)
    {
        Console.WriteLine($"[CHECKOUT] ═══ ProcessPaymentSuccess START ═══ SessionId={sessionId}");

        // ── 2.1 Tìm transactions theo Stripe session ID ─────────────────────
        var transactions = await _repo.GetTransactionsBySessionIdAsync(sessionId);
        if (!transactions.Any())
            throw new InvalidOperationException("Không tìm thấy giao dịch cho session này.");

        Console.WriteLine($"[CHECKOUT] Tìm thấy {transactions.Count} transactions cho session {sessionId}");

        var allSucceeded = transactions.All(t => t.TransactionsStatus == "succeeded");
        var allHavePayouts = transactions.All(t => t.InstructorPayouts.Any());

        Console.WriteLine($"[CHECKOUT] allSucceeded={allSucceeded}, allHavePayouts={allHavePayouts}");

        if (allSucceeded && allHavePayouts)
        {
            Console.WriteLine("[CHECKOUT] Đã xử lý hoàn tất, skip.");
            return;
        }

        // Lấy order từ transaction đầu tiên
        var firstTransaction = transactions.First();
        var orderId = firstTransaction.OrderItem?.OrderId;
        if (!orderId.HasValue)
            throw new InvalidOperationException("Không tìm thấy đơn hàng liên kết.");

        var order = await _repo.GetOrderWithDetailsAsync(orderId.Value);
        if (order == null)
            throw new InvalidOperationException("Đơn hàng không tồn tại.");

        var userId = order.UserId;
        if (!userId.HasValue)
            throw new InvalidOperationException("Không xác định được người mua.");

        // ★ Lấy PaymentIntent ID từ Stripe Session
        var paymentIntentId = await _paymentGateway.GetPaymentReferenceAsync(sessionId);
        var transferGroup = $"order_{orderId.Value}";

        // ★ Lấy Charge ID để dùng làm source_transaction
        // Khi có source_transaction, Stripe cho phép transfer từ PENDING balance
        // (không cần đợi available balance — fix lỗi "insufficient funds" trong test mode)
        string? chargeId = null;
        if (!string.IsNullOrEmpty(paymentIntentId))
        {
            chargeId = await _paymentGateway.GetChargeIdFromPaymentIntentAsync(paymentIntentId);
            Console.WriteLine($"[CHECKOUT] PaymentIntentId={paymentIntentId}, ChargeId={chargeId ?? "NULL"}");
        }

        // ════ BẮT ĐẦU DB TRANSACTION ════════════════════════════════════════
        await using var dbTransaction = await _repo.BeginTransactionAsync();
        try
        {
            // ── 2.2 UPDATE transactions → 'succeeded' (idempotent) ──────────
            if (!allSucceeded)
            {
                foreach (var txn in transactions)
                {
                    txn.TransactionsStatus = "succeeded";
                    if (!string.IsNullOrEmpty(paymentIntentId))
                        txn.StripePaymentintentId = paymentIntentId;
                }

                // ── 2.3 UPDATE order_info → 'paid' ─────────────────────────
                order.OrderStatus = "paid";

                await _repo.SaveChangesAsync();
            }

            // ── 2.4 INSERT enrollments + enrollment_progress (idempotent) ───
            foreach (var txn in transactions)
            {
                var courseId = txn.OrderItem?.CourseId;
                if (!courseId.HasValue) continue;

                // Kiểm tra chưa enrolled (idempotent)
                if (await _repo.IsEnrolledAsync(userId.Value, courseId.Value))
                    continue;

                var enrollment = new Enrollment
                {
                    UserId = userId.Value,
                    CourseId = courseId.Value,
                    Title = txn.OrderItem?.Course?.Title,
                    EnrollDate = DateOnly.FromDateTime(DateTime.Now),
                    IsCompleted = false,
                    EnrollmentStatus = "active",
                    LastAccessedAt = DateTime.Now
                };
                await _repo.AddEnrollmentAsync(enrollment);
                await _repo.SaveChangesAsync(); // Lấy EnrollmentId

                var progress = new EnrollmentProgress
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    LearnedMaterialCount = 0,
                    LastModifiedAt = DateTime.Now
                };
                await _repo.AddEnrollmentProgressAsync(progress);
            }

            // ═════════════════════════════════════════════════════════════════
            // ★ 2.5 TẠO STRIPE TRANSFERS — Chia tiền cho từng instructor
            //
            // IDEMPOTENT: Chỉ tạo transfer nếu chưa có InstructorPayout record.
            // Điều này xử lý cả trường hợp:
            //   - Lần đầu xử lý payment
            //   - Retry khi code cũ đã mark "succeeded" nhưng chưa tạo transfer
            // ═════════════════════════════════════════════════════════════════
            foreach (var txn in transactions)
            {
                // ★ Skip nếu đã có payout record (tránh duplicate transfer)
                if (txn.InstructorPayouts.Any())
                {
                    Console.WriteLine($"[CHECKOUT] Transaction {txn.TransactionId}: đã có payout, skip.");
                    continue;
                }

                var instructorId = txn.OrderItem?.Course?.InstructorId;
                if (!instructorId.HasValue)
                {
                    Console.WriteLine($"[CHECKOUT] Transaction {txn.TransactionId}: InstructorId is NULL! Course={txn.OrderItem?.CourseId}");
                    continue;
                }

                var stripeAccountId = await _repo.GetInstructorStripeAccountIdAsync(instructorId.Value);
                Console.WriteLine($"[CHECKOUT] Transaction {txn.TransactionId}: InstructorId={instructorId.Value}, StripeAccountId={stripeAccountId ?? "NULL"}");

                // Tính payout: amount * (transfer_rate / 100)
                var payoutAmount = Math.Round(
                    txn.Amount * (txn.TransferRate / 100m), 2);
                Console.WriteLine($"[CHECKOUT] PayoutAmount={payoutAmount} (Amount={txn.Amount} × TransferRate={txn.TransferRate}%)");

                // ★ Tạo Stripe Transfer nếu instructor có connected account
                var transferSucceeded = false;
                if (!string.IsNullOrEmpty(stripeAccountId))
                {
                    try
                    {
                        Console.WriteLine($"[CHECKOUT] ★ Creating Stripe Transfer: ${payoutAmount} AUD → {stripeAccountId} (group={transferGroup}, charge={chargeId ?? "NULL"})");

                        // ★ Currency PHẢI là AUD (settlement currency của platform AU)
                        // Khi dùng source_transaction, Stripe yêu cầu currency = platform's balance currency
                        // Stripe sẽ tự convert sang connected account's currency nếu khác
                        await _paymentGateway.CreateTransferAsync(
                            payoutAmount,
                            "aud",
                            stripeAccountId,
                            transferGroup,
                            $"Payout for course: {txn.OrderItem?.Course?.Title}",
                            chargeId);  // ★ source_transaction: cho phép transfer từ pending balance
                        transferSucceeded = true;

                        Console.WriteLine($"[CHECKOUT] ✅ Transfer SUCCEEDED for instructor {instructorId.Value}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CHECKOUT] ❌ Transfer FAILED for instructor {instructorId.Value}: {ex.Message}");
                        Console.WriteLine($"[CHECKOUT] ❌ Full exception: {ex}");
                    }
                }
                else
                {
                    Console.WriteLine($"[CHECKOUT] ⚠️ Instructor {instructorId.Value} has NO StripeAccountId — transfer skipped!");
                }

                // Ghi payout record — IsPaid phản ánh chính xác kết quả transfer
                var payout = new InstructorPayout
                {
                    TransactionId = txn.TransactionId,
                    InstructorId = instructorId.Value,
                    PayoutAmount = payoutAmount,
                    PayoutDate = DateTime.Now.AddDays(14),
                    IsPaid = transferSucceeded
                };
                await _repo.AddInstructorPayoutAsync(payout);
                Console.WriteLine($"[CHECKOUT] 📝 Payout record created: TxnId={txn.TransactionId}, IsPaid={transferSucceeded}");
            }

            // ── 2.6 UPDATE coupon.used_count (chỉ khi lần đầu xử lý) ───────
            if (!allSucceeded)
            {
                var firstItemWithCoupon = order.OrderItems.FirstOrDefault(oi => oi.CouponUsed == true);
                if (firstItemWithCoupon?.Course?.CouponId != null)
                {
                    await _repo.IncrementCouponUsageAsync(firstItemWithCoupon.Course.CouponId.Value);
                }

                // ── 2.7 DELETE cart_items ────────────────────────────────────
                await _repo.ClearCartAsync(userId.Value);
            }

            await _repo.SaveChangesAsync();

            // ════ COMMIT ════════════════════════════════════════════════════
            await dbTransaction.CommitAsync();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }
}
