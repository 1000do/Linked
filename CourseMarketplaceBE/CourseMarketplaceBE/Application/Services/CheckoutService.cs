using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.SignalR;
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
    private readonly IHubContext<FinanceHub> _hubContext;
    private readonly INotificationService _notificationService;
    private readonly ICourseRepository _courseRepo;
    private readonly IAdminFinanceService _adminFinanceService;

    public CheckoutService(
        ICheckoutRepository repo,
        IPaymentGatewayService paymentGateway,
        ILogger<CheckoutService> logger,
        IHubContext<FinanceHub> hubContext,
        IAdminFinanceService adminFinanceService,
        INotificationService notificationService,
        ICourseRepository courseRepo)
    {
        _repo = repo;
        _paymentGateway = paymentGateway;
        _logger = logger;
        _hubContext = hubContext;
        _notificationService = notificationService;
        _courseRepo = courseRepo;
        _adminFinanceService = adminFinanceService;
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
            throw new InvalidOperationException("Cart is empty. Cannot checkout.");

        // Lấy TransferRate từ system_configs
        var currentTransferRate = await _adminFinanceService.GetCurrentTransferRateAsync();

        // ── 1.2 Kiểm tra không mua trùng (đã enrolled) ──────────────────────
        foreach (var item in cartItems)
        {
            if (item.CourseId.HasValue && await _repo.IsEnrolledAsync(userId, item.CourseId.Value))
                throw new InvalidOperationException(
                    $"You have already purchased the course \"{item.Course?.Title}\". Please remove it from the cart.");
        }

        // ── 1.3 Tính toán coupon (Hỗ trợ nhiều mã phân tách bằng dấu phẩy) ────
        var appliedCoupons = new List<Coupon>();
        decimal totalDiscountAmount = 0m;

        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var codes = couponCode.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim().ToUpper())
                .Distinct()
                .ToList();

            foreach (var code in codes)
            {
                var cp = await _repo.GetValidCouponAsync(code, DateTime.Now);
                if (cp != null)
                {
                    decimal eligibleSubTotal = cartItems
                        .Where(c => c.Course != null && c.Course.CouponId == cp.CouponId)
                        .Sum(c => c.Price ?? c.Course?.Price ?? 0m);

                    if (eligibleSubTotal >= cp.MinOrderValue && eligibleSubTotal > 0)
                    {
                        appliedCoupons.Add(cp);
                        decimal cpDiscount = cp.CouponType == "percentage"
                            ? eligibleSubTotal * (cp.DiscountValue / 100m)
                            : cp.DiscountValue;

                        cpDiscount = Math.Round(Math.Min(cpDiscount, eligibleSubTotal), 2);
                        totalDiscountAmount += cpDiscount;
                    }
                }
            }
        }

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
            await _repo.SaveChangesAsync();

            var transferGroup = $"order_{order.OrderId}";

            // ── 1.6 INSERT order_items ───────────────────────────────────────
            var orderItems = new List<OrderItem>();
            var paymentLineItems = new List<PaymentLineItem>();

            foreach (var cartItem in cartItems)
            {
                var originalPrice = cartItem.Price ?? cartItem.Course?.Price ?? 0m;
                decimal purchasePrice = originalPrice;
                bool isItemDiscounted = false;
                Coupon? matchedCoupon = null;

                // Tìm coupon khớp với khóa học này trong danh sách appliedCoupons
                if (cartItem.Course != null)
                {
                    matchedCoupon = appliedCoupons.FirstOrDefault(cp => cartItem.Course.CouponId == cp.CouponId);
                }

                if (matchedCoupon != null)
                {
                    isItemDiscounted = true;
                    if (matchedCoupon.CouponType == "percentage")
                    {
                        purchasePrice = originalPrice * (1 - (matchedCoupon.DiscountValue / 100m));
                    }
                    else
                    {
                        purchasePrice = originalPrice - matchedCoupon.DiscountValue;
                    }
                }

                purchasePrice = Math.Round(Math.Max(purchasePrice, 0), 2);

                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    CourseId = cartItem.CourseId,
                    PurchasePrice = purchasePrice,
                    CouponUsed = isItemDiscounted,
                    // ★ Snapshot giá gốc & coupon tại thời điểm mua
                    OriginalPrice = originalPrice,
                    CouponCode = isItemDiscounted ? matchedCoupon?.CouponCode : null,
                    CouponType = isItemDiscounted ? matchedCoupon?.CouponType : null,
                    DiscountAmount = Math.Round(originalPrice - purchasePrice, 2)
                };
                await _repo.AddOrderItemAsync(orderItem);
                orderItems.Add(orderItem);

                paymentLineItems.Add(new PaymentLineItem
                {
                    CourseName = cartItem.Course?.Title ?? "Course",
                    ThumbnailUrl = cartItem.Course?.CourseThumbnailUrl,
                    UnitPrice = purchasePrice
                });
            }

            await _repo.SaveChangesAsync(); // Lấy OrderItem.Id

            // ── 1.7 Gọi Payment Gateway (Stripe) ────────────────────────────
            // ÉP SỬ DỤNG USD ĐỂ ĐỒNG NHẤT HỆ THỐNG
            var sessionCurrency = "usd"; 

            // ★ Truyền transferGroup để Stripe nhóm các Transfer sau khi thanh toán
            var cancelUrlWithOrder = cancelUrl.Contains("?") ? $"{cancelUrl}&order_id={order.OrderId}" : $"{cancelUrl}?order_id={order.OrderId}";

            var paymentResult = await _paymentGateway.CreateCheckoutSessionAsync(
                paymentLineItems,
                successUrl,
                cancelUrlWithOrder,
                userEmail,
                transferGroup,  // ← NEW: transfer_group liên kết với order
                sessionCurrency);

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
                    TransferRate = currentTransferRate,
                    StripeSessionId = paymentResult.SessionId,
                    Currency = sessionCurrency,
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
    // BƯỚC 1.5: INITIATE DIRECT CHECKOUT (Không qua giỏ hàng)
    // ═══════════════════════════════════════════════════════════════════════════
    public async Task<CheckoutResponse> InitiateDirectCheckoutAsync(
        int userId,
        int courseId,
        string? couponCode,
        string successUrl,
        string cancelUrl)
    {
        var course = await _repo.GetCourseWithInstructorAsync(courseId);
        if (course == null)
            throw new InvalidOperationException("Course not found.");

        if (await _repo.IsEnrolledAsync(userId, courseId))
            throw new InvalidOperationException($"You have already purchased the course \"{course.Title}\".");

        var stripeAccountId = await _repo.GetInstructorStripeAccountIdAsync(course.InstructorId ?? 0);
        if (string.IsNullOrEmpty(stripeAccountId))
            throw new InvalidOperationException("The instructor has not connected a Stripe payment account.");

        // Lấy TransferRate từ system_configs
        var currentTransferRate = await _adminFinanceService.GetCurrentTransferRateAsync();

        Coupon? coupon = null;
        decimal discountAmount = 0m;
        decimal originalPrice = course.Price;
        
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            coupon = await _repo.GetValidCouponAsync(couponCode, DateTime.Now);
            if (coupon != null)
            {
                if (course.CouponId != coupon.CouponId)
                {
                    throw new InvalidOperationException("This coupon is not applicable to this course.");
                }

                if (originalPrice < coupon.MinOrderValue)
                {
                    throw new InvalidOperationException($"This coupon requires a minimum course value of ${coupon.MinOrderValue:N2}.");
                }

                discountAmount = coupon.CouponType == "percentage"
                    ? originalPrice * (coupon.DiscountValue / 100m)
                    : coupon.DiscountValue;
                discountAmount = Math.Round(Math.Min(discountAmount, originalPrice), 2);
            }
            else
            {
                throw new InvalidOperationException("Coupon does not exist or has expired.");
            }
        }

        var purchasePrice = Math.Max(originalPrice - discountAmount, 0m);
        var userEmail = await _repo.GetUserEmailAsync(userId);

        // Tính toán platform fee
        var platformFee = Math.Round(purchasePrice * ((100m - currentTransferRate) / 100m), 2);

        await using var dbTransaction = await _repo.BeginTransactionAsync();
        try
        {
            var order = new OrderInfo
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                OrderStatus = "pending",
                PaymentMethod = "stripe_direct"
            };
            await _repo.AddOrderAsync(order);
            await _repo.SaveChangesAsync();

            var orderItem = new OrderItem
            {
                OrderId = order.OrderId,
                CourseId = courseId,
                PurchasePrice = purchasePrice,
                CouponUsed = coupon != null && discountAmount > 0,
                // ★ Snapshot giá gốc & coupon tại thời điểm mua
                OriginalPrice = originalPrice,
                CouponCode = coupon?.CouponCode,
                CouponType = coupon?.CouponType,
                DiscountAmount = discountAmount
            };
            await _repo.AddOrderItemAsync(orderItem);
            await _repo.SaveChangesAsync();

            var paymentLineItems = new List<PaymentLineItem>
            {
                new PaymentLineItem
                {
                    CourseName = course.Title ?? "Course",
                    ThumbnailUrl = course.CourseThumbnailUrl,
                    UnitPrice = purchasePrice
                }
            };

            var instructorCountry = await _repo.GetInstructorStripeCountryAsync(course.InstructorId ?? 0);
            var sessionCurrency = GetCurrencyFromCountry(instructorCountry);

            // Gọi Stripe (Standard Charges - Platform thu hết 100%)
            var cancelUrlWithOrder = cancelUrl.Contains("?") ? $"{cancelUrl}&order_id={order.OrderId}" : $"{cancelUrl}?order_id={order.OrderId}";
            
            var paymentResult = await _paymentGateway.CreateCheckoutSessionAsync(
                paymentLineItems,
                successUrl,
                cancelUrlWithOrder,
                userEmail,
                null, // transfer_group
                sessionCurrency);

            var transaction = new Transaction
            {
                OrderItemId = orderItem.Id,
                AccountFrom = userId,
                AccountTo = course.InstructorId,
                Amount = purchasePrice,
                TransferRate = currentTransferRate,
                StripeSessionId = paymentResult.SessionId,
                Currency = sessionCurrency,
                TransactionsStatus = "pending",
                TransactionType = "payment",
                TransactionCreatedAt = DateTime.Now
            };
            await _repo.AddTransactionAsync(transaction);
            await _repo.SaveChangesAsync();

            await dbTransaction.CommitAsync();

            return new CheckoutResponse
            {
                SessionUrl = paymentResult.SessionUrl,
                SessionId = paymentResult.SessionId
            };
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BƯỚC 2: PROCESS PAYMENT SUCCESS
    // Stripe báo thành công → Update trạng thái → Cấp enrollment →
    // ★ TẠO STRIPE TRANSFERS (chia tiền cho instructor) → Ghi payout record
    // ═══════════════════════════════════════════════════════════════════════════
    public async Task ProcessPaymentSuccessAsync(string sessionId, bool failOnTransferFailure = false)
    {
        Console.WriteLine($"[CHECKOUT-DEBUG] ═══ ProcessPaymentSuccess START ═══ SessionId={sessionId}");

        try 
        {
            // ── 2.1 Tìm transactions theo Stripe session ID ─────────────────────
            var transactions = await _repo.GetTransactionsBySessionIdAsync(sessionId);
            if (!transactions.Any())
            {
                Console.WriteLine($"[CHECKOUT-DEBUG] ❌ ERROR: Transactions not found for session {sessionId}");
                throw new InvalidOperationException("No transaction found for this session.");
            }

            Console.WriteLine($"[CHECKOUT-DEBUG] Found {transactions.Count} transactions.");

            var allSucceeded = transactions.All(t => t.TransactionsStatus == "succeeded");
            var allPayoutsPaid = transactions.All(t => t.InstructorPayouts.Any(p => p.IsPaid));

            if (allSucceeded && allPayoutsPaid)
            {
                Console.WriteLine("[CHECKOUT-DEBUG] Already processed. Skipping.");
                return;
            }

            // Lấy order từ transaction đầu tiên
            var firstTransaction = transactions.First();
            var orderId = firstTransaction.OrderItem?.OrderId;
            if (!orderId.HasValue) throw new InvalidOperationException("Order not found.");

            var order = await _repo.GetOrderWithDetailsAsync(orderId.Value);
            if (order == null) throw new InvalidOperationException("Order does not exist.");

            var userId = order.UserId;
            if (!userId.HasValue) throw new InvalidOperationException("Buyer is undefined.");

            // ★ Lấy PaymentIntent ID từ Stripe Session
            Console.WriteLine("[CHECKOUT-DEBUG] Fetching Stripe PaymentIntent...");
            var paymentIntentId = await _paymentGateway.GetPaymentReferenceAsync(sessionId);
            Console.WriteLine($"[CHECKOUT-DEBUG] PaymentIntentId={paymentIntentId ?? "NULL"}");

            // ════ BẮT ĐẦU DB TRANSACTION ════════════════════════════════════════
            Console.WriteLine("[CHECKOUT-DEBUG] 🔄 Starting DB Transaction...");
            await using var dbTransaction = await _repo.BeginTransactionAsync();
            try
            {
                // ── 2.2 UPDATE transactions ──────────────────────────────────
                if (!allSucceeded)
                {
                    Console.WriteLine("[CHECKOUT-DEBUG] 🔄 Updating Transactions & Order status...");
                    foreach (var txn in transactions)
                    {
                        txn.TransactionsStatus = "succeeded";
                        if (!string.IsNullOrEmpty(paymentIntentId))
                            txn.StripePaymentintentId = paymentIntentId;
                    }
                    order.OrderStatus = "paid";
                    await _repo.SaveChangesAsync();
                }

                // ── 2.4 INSERT enrollments ───────────────────────────────────
                Console.WriteLine("[CHECKOUT-DEBUG] 🔄 Processing Enrollments...");
                foreach (var txn in transactions)
                {
                    var courseId = txn.OrderItem?.CourseId;
                    if (!courseId.HasValue) continue;

                    // Increment coupon usage if used
                    if (txn.OrderItem?.CouponUsed == true && txn.OrderItem.Course?.CouponId.HasValue == true)
                    {
                        await _repo.IncrementCouponUsageAsync(txn.OrderItem.Course.CouponId.Value);
                    }

                    if (await _repo.IsEnrolledAsync(userId.Value, courseId.Value))
                    {
                        Console.WriteLine($"[CHECKOUT-DEBUG] User already enrolled in course {courseId}. Skipping.");
                        continue;
                    }

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
                    await _repo.SaveChangesAsync(); 

                    var progress = new EnrollmentProgress
                    {
                        EnrollmentId = enrollment.EnrollmentId,
                        LearnedMaterialCount = 0,
                        LastModifiedAt = DateTime.Now
                    };
                    await _repo.AddEnrollmentProgressAsync(progress);
                }

                // ── 2.5 GHI PAYOUT RECORDS ────────────────────────────────────
                Console.WriteLine("[CHECKOUT-DEBUG] 🔄 Creating Payout records (MoR pattern)...");
                foreach (var txn in transactions)
                {
                    if (txn.InstructorPayouts.Any()) 
                    {
                        Console.WriteLine($"[CHECKOUT-DEBUG] Payout record already exists for txn {txn.TransactionId}. Skipping.");
                        continue;
                    }

                    var instructorId = txn.OrderItem?.Course?.InstructorId;
                    if (!instructorId.HasValue) continue;

                    var payoutAmount = Math.Round(txn.Amount * (txn.TransferRate / 100m), 2);
                    
                    var payout = new InstructorPayout
                    {
                        TransactionId = txn.TransactionId,
                        InstructorId = instructorId.Value,
                        PayoutAmount = payoutAmount,
                        PayoutDate = DateTime.Now.AddDays(14),
                        IsPaid = false, // ★ PHƯƠNG ÁN B: CHƯA THANH TOÁN (Admin duyệt mới chuyển)
                        PayoutStatus = "pending", // Trạng thái: Chờ thanh toán
                        StripeTransferId = null // Sẽ có sau khi Admin bấm Pay via Stripe
                    };
                    await _repo.AddInstructorPayoutAsync(payout);
                    Console.WriteLine($"[CHECKOUT-DEBUG] 📝 Payout record created: TxnId={txn.TransactionId}, Status=pending");

                    // ── NEW: Thông báo cho giảng viên ──
                    if (instructorId.HasValue)
                    {
                        var courseTitle = txn.OrderItem?.Course?.Title ?? "your course";
                        await _notificationService.SendNotificationAsync(
                            instructorId.Value,
                            "You have a new order",
                            $"The course '{courseTitle}' has been successfully sold. Expected revenue: {payoutAmount:N0} VND.",
                            $"/Instructor/Payouts"
                        );
                    }
                }

                // ── 2.6 CLEAR CART ───────────────────────────────────────────
                Console.WriteLine("[CHECKOUT-DEBUG] 🔄 Clearing cart...");
                await _repo.ClearCartAsync(userId.Value);

                await _repo.SaveChangesAsync();

                Console.WriteLine("[CHECKOUT-DEBUG] 🏁 Committing Transaction...");
                await dbTransaction.CommitAsync();
                Console.WriteLine("[CHECKOUT-DEBUG] ✅ SUCCESS!");

                // 🔥 Gửi SignalR báo cho Admin (vì trạng thái mặc định là transferred)
                await _hubContext.Clients.Group("AdminFinance").SendAsync("UpdatePayoutStatus", new {
                    refresh = true // Báo Admin load lại cả bảng để thấy dòng mới
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CHECKOUT-DEBUG] ❌ INNER EXCEPTION: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                await dbTransaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CHECKOUT-DEBUG] ❌ TOP-LEVEL EXCEPTION: {ex.Message}");
            throw;
        }
    }
    // ═══════════════════════════════════════════════════════════════════════════
    // BƯỚC 3: PROCESS PAYMENT CANCEL
    // Xử lý khi user bấm hủy thanh toán hoặc back ra từ Stripe Checkout.
    // ═══════════════════════════════════════════════════════════════════════════
    public async Task ProcessPaymentCancelAsync(int orderId)
    {
        Console.WriteLine($"[CHECKOUT-DEBUG] ═══ ProcessPaymentCancel START ═══ OrderId={orderId}");
        try
        {
            var order = await _repo.GetOrderWithDetailsAsync(orderId);
            if (order == null || order.OrderStatus != "pending")
            {
                Console.WriteLine($"[CHECKOUT-DEBUG] Pending order not found for ID {orderId}");
                return;
            }

            var transactions = await _repo.GetTransactionsByOrderIdAsync(orderId);

            await using var dbTransaction = await _repo.BeginTransactionAsync();
            try
            {
                // XÓA CỨNG (HARD DELETE) các transactions và order
                // để không hiện "Thất bại" trong danh sách hóa đơn (như User yêu cầu).
                if (transactions.Any())
                {
                    await _repo.DeleteTransactionsAsync(transactions);
                }

                await _repo.DeleteOrderAsync(order);

                await _repo.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                
                Console.WriteLine("[CHECKOUT-DEBUG] Successfully DELETED pending order/transactions due to user canceling payment.");
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CHECKOUT-DEBUG] ❌ EXCEPTION during ProcessPaymentCancelAsync: {ex.Message}");
            throw;
        }
    }

    private string GetCurrencyFromCountry(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode)) return "USD";
        return countryCode.ToUpper() switch
        {
            "GB" => "GBP",
            "CA" => "CAD",
            "CH" => "CHF",
            "AT" or "BE" or "CY" or "EE" or "FI" or "FR" or "DE" or "GR" or 
            "IE" or "IT" or "LV" or "LT" or "LU" or "MT" or "NL" or "PT" or 
            "SK" or "SI" or "ES" => "EUR",
            "BG" => "BGN",
            "HR" => "EUR", // Croatia uses EUR since 2023
            "CZ" => "CZK",
            "DK" => "DKK",
            "HU" => "HUF",
            "IS" => "ISK",
            "NO" => "NOK",
            "PL" => "PLN",
            "RO" => "RON",
            "SE" => "SEK",
            "AU" => "AUD",
            "VN" => "VND",
            _ => "USD"
        };
    }
}

