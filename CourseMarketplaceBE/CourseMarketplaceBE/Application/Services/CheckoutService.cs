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
/// ★ Stripe Connect Marketplace Flow (Updated to completely eliminate pending state in checkout DB):
///   Bước 1 (Checkout): Chỉ tạo Stripe Session với thông tin đơn hàng lưu trong metadata, KHÔNG ghi DB.
///   Bước 2 (Success):  Khi Stripe báo thành công, lúc này mới INSERT Order, OrderItems, Transactions
///                      và các thực thể liên quan trực tiếp ở trạng thái "paid" / "succeeded".
///                      Không sinh ra bất kỳ dữ liệu pending ảo hay rác DB nào khi user hủy/exit Stripe.
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
    // Chỉ tạo Stripe Session, không ghi DB pending.
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
                    }
                }
            }
        }

        // Lấy email user cho Stripe
        var userEmail = await _repo.GetUserEmailAsync(userId);

        // ── 1.4 Chuẩn bị line items cho Stripe ──────────────────────────────
        var paymentLineItems = new List<PaymentLineItem>();

        foreach (var cartItem in cartItems)
        {
            var originalPrice = cartItem.Price ?? cartItem.Course?.Price ?? 0m;
            decimal purchasePrice = originalPrice;
            Coupon? matchedCoupon = null;

            // Tìm coupon khớp với khóa học này trong danh sách appliedCoupons
            if (cartItem.Course != null)
            {
                matchedCoupon = appliedCoupons.FirstOrDefault(cp => cartItem.Course.CouponId == cp.CouponId);
            }

            if (matchedCoupon != null)
            {
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

                paymentLineItems.Add(new PaymentLineItem
                {
                    CourseName = cartItem.Course?.Title ?? "Course",
                    ThumbnailUrl = cartItem.Course?.CourseThumbnailUrl,
                    UnitPrice = purchasePrice
                });
            }

        var sessionCurrency = "usd"; 
        var orderReference = $"tx_{Guid.NewGuid().ToString("N")}";

        // metadata để lưu thông tin đơn hàng lên Stripe Session
        var metadata = new Dictionary<string, string>
        {
            { "userId", userId.ToString() },
            { "couponCode", couponCode ?? "" },
            { "checkoutType", "cart" },
            { "courseIds", string.Join(",", cartItems.Select(c => c.CourseId)) }
        };

        var paymentResult = await _paymentGateway.CreateCheckoutSessionAsync(
            paymentLineItems,
            successUrl,
            cancelUrl,
            userEmail,
            orderReference,
            sessionCurrency,
            null,
            null,
            metadata);

        return new CheckoutResponse
        {
            SessionUrl = paymentResult.SessionUrl,
            SessionId = paymentResult.SessionId
        };
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
            throw new InvalidOperationException("Giảng viên chưa kết nối tài khoản thanh toán Stripe.");

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

        var paymentLineItems = new List<PaymentLineItem>
        {
            new PaymentLineItem
            {
                CourseName = course.Title ?? "Khóa học",
                ThumbnailUrl = course.CourseThumbnailUrl,
                UnitPrice = purchasePrice
            }
        };

        var instructorCountry = await _repo.GetInstructorStripeCountryAsync(course.InstructorId ?? 0);
        var sessionCurrency = GetCurrencyFromCountry(instructorCountry);

        var orderReference = $"tx_{Guid.NewGuid().ToString("N")}";

        var metadata = new Dictionary<string, string>
        {
            { "userId", userId.ToString() },
            { "couponCode", couponCode ?? "" },
            { "checkoutType", "direct" },
            { "courseIds", courseId.ToString() }
        };

        var paymentResult = await _paymentGateway.CreateCheckoutSessionAsync(
            paymentLineItems,
            successUrl,
            cancelUrl,
            userEmail,
            orderReference,
            sessionCurrency,
            null,
            null,
            metadata);

        return new CheckoutResponse
        {
            SessionUrl = paymentResult.SessionUrl,
            SessionId = paymentResult.SessionId
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BƯỚC 2: PROCESS PAYMENT SUCCESS
    // Stripe báo thành công → Tạo Order & Transactions trực tiếp ở trạng thái success →
    // Cấp enrollment → Ghi payout record
    // ═══════════════════════════════════════════════════════════════════════════
    public async Task ProcessPaymentSuccessAsync(string sessionId, bool failOnTransferFailure = false)
    {
        Console.WriteLine($"[CHECKOUT-DEBUG] ═══ ProcessPaymentSuccess START ═══ SessionId={sessionId}");

        try 
        {
            // ── 2.1 Kiểm tra trùng lặp (Idempotency) ───────────────────────────
            var existingTransactions = await _repo.GetTransactionsBySessionIdAsync(sessionId);
            if (existingTransactions.Any())
            {
                var allSucceeded = existingTransactions.All(t => t.TransactionsStatus == "succeeded");
                var allPayoutsCreated = existingTransactions.All(t => t.InstructorPayouts.Any());

                if (allSucceeded && allPayoutsCreated)
                {
                    Console.WriteLine("[CHECKOUT-DEBUG] Already processed and successful. Skipping.");
                    return;
                }
            }

            // ── 2.2 Lấy Metadata từ Stripe Session ─────────────────────────────
            Console.WriteLine("[CHECKOUT-DEBUG] Fetching Stripe Session Metadata...");
            var metadata = await _paymentGateway.GetSessionMetadataAsync(sessionId);
            if (metadata == null || !metadata.TryGetValue("userId", out var userIdStr))
            {
                throw new InvalidOperationException("Không tìm thấy metadata hợp lệ hoặc thông tin UserId trên Stripe Session.");
            }

            int userId = int.Parse(userIdStr);
            string couponCode = metadata.TryGetValue("couponCode", out var cc) ? cc : "";
            string checkoutType = metadata.TryGetValue("checkoutType", out var ct) ? ct : "cart";
            string courseIdsStr = metadata.TryGetValue("courseIds", out var cids) ? cids : "";

            if (string.IsNullOrEmpty(courseIdsStr))
            {
                throw new InvalidOperationException("Không tìm thấy danh sách Course ID hợp lệ trong Stripe Session.");
            }

            var courseIds = courseIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            // ── 2.3 Lấy thông tin cần thiết ─────────────────────────────────────
            var currentTransferRate = await _adminFinanceService.GetCurrentTransferRateAsync();
            var paymentIntentId = await _paymentGateway.GetPaymentReferenceAsync(sessionId);
            Console.WriteLine($"[CHECKOUT-DEBUG] PaymentIntentId={paymentIntentId ?? "NULL"}");

            // ── 2.4 Load các khóa học & áp coupon để tính giá chính xác ─────────
            var coupon = !string.IsNullOrWhiteSpace(couponCode)
                ? await _repo.GetValidCouponAsync(couponCode, DateTime.Now)
                : null;

            // ════ BẮT ĐẦU DB TRANSACTION ════════════════════════════════════════
            Console.WriteLine("[CHECKOUT-DEBUG] 🔄 Starting DB Transaction...");
            await using var dbTransaction = await _repo.BeginTransactionAsync();
            try
            {
                // Tạo đơn hàng OrderInfo directly as paid!
                var order = new OrderInfo
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    OrderStatus = "paid",
                    PaymentMethod = checkoutType == "direct" ? "stripe_direct" : "stripe"
                };
                await _repo.AddOrderAsync(order);
                await _repo.SaveChangesAsync();

                foreach (var courseId in courseIds)
                {
                    var course = await _repo.GetCourseWithInstructorAsync(courseId);
                    if (course == null) continue;

                    decimal originalPrice = course.Price;
                    decimal purchasePrice = originalPrice;
                    bool isDiscounted = false;

                    if (coupon != null && course.CouponId == coupon.CouponId)
                    {
                        if (originalPrice >= coupon.MinOrderValue)
                        {
                            isDiscounted = true;
                            decimal discountAmount = coupon.CouponType == "percentage"
                                ? originalPrice * (coupon.DiscountValue / 100m)
                                : coupon.DiscountValue;
                            discountAmount = Math.Round(Math.Min(discountAmount, originalPrice), 2);
                            purchasePrice = Math.Max(originalPrice - discountAmount, 0m);
                        }
                    }

                    purchasePrice = Math.Round(purchasePrice, 2);

                    // Insert OrderItem
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        CourseId = courseId,
                        PurchasePrice = purchasePrice,
                        CouponUsed = isDiscounted,
                        OriginalPrice = originalPrice,
                        CouponCode = isDiscounted ? coupon?.CouponCode : null,
                        CouponType = isDiscounted ? coupon?.CouponType : null,
                        DiscountAmount = Math.Round(originalPrice - purchasePrice, 2)
                    };
                    await _repo.AddOrderItemAsync(orderItem);
                    await _repo.SaveChangesAsync();

                    // Increment coupon usage
                    if (isDiscounted && coupon != null)
                    {
                        await _repo.IncrementCouponUsageAsync(coupon.CouponId);
                    }

                    // Insert Transaction
                    var transaction = new Transaction
                    {
                        OrderItemId = orderItem.Id,
                        AccountFrom = userId,
                        AccountTo = course.InstructorId,
                        Amount = purchasePrice,
                        TransferRate = currentTransferRate,
                        StripeSessionId = sessionId,
                        StripePaymentintentId = paymentIntentId,
                        Currency = "usd",
                        TransactionsStatus = "succeeded",
                        TransactionType = "payment",
                        TransactionCreatedAt = DateTime.Now
                    };
                    await _repo.AddTransactionAsync(transaction);
                    await _repo.SaveChangesAsync();

                    // Cấp Enrollment & Progress
                    if (!await _repo.IsEnrolledAsync(userId, courseId))
                    {
                        var enrollment = new Enrollment
                        {
                            UserId = userId,
                            CourseId = courseId,
                            Title = course.Title,
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

                    // Tạo Payout record
                    var payoutAmount = Math.Round(purchasePrice * (currentTransferRate / 100m), 2);
                    var payout = new InstructorPayout
                    {
                        TransactionId = transaction.TransactionId,
                        InstructorId = course.InstructorId ?? 0,
                        PayoutAmount = payoutAmount,
                        PayoutDate = DateTime.Now.AddDays(14),
                        IsPaid = false,
                        PayoutStatus = "pending",
                        StripeTransferId = null
                    };
                    await _repo.AddInstructorPayoutAsync(payout);

                    var instructorId = course.InstructorId;

                    // ── NEW: Thông báo cho giảng viên ──
                    if (instructorId.HasValue)
                    {
                        var courseTitle =course.Title ?? "your course";
                        await _notificationService.SendNotificationAsync(
                            instructorId.Value,
                            "You have a new order",
                            $"The course '{courseTitle}' has been successfully sold. Expected revenue: {payoutAmount:N0} VND.",
                            $"/Instructor/Payouts"
                        );
                    }
                }

                // ── 2.6 CLEAR CART (nếu mua từ Cart) ───────────────────────────
                if (checkoutType == "cart")
                {
                    Console.WriteLine("[CHECKOUT-DEBUG] 🔄 Clearing cart...");
                    await _repo.ClearCartAsync(userId);
                }

                await _repo.SaveChangesAsync();

                Console.WriteLine("[CHECKOUT-DEBUG] 🏁 Committing Transaction...");
                await dbTransaction.CommitAsync();
                Console.WriteLine("[CHECKOUT-DEBUG] ✅ SUCCESS!");

                // 🔥 Gửi SignalR báo cho Admin
                await _hubContext.Clients.Group("AdminFinance").SendAsync("UpdatePayoutStatus", new {
                    refresh = true
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
    // ═══════════════════════════════════════════════════════════════════════════
    public async Task ProcessPaymentCancelAsync(int orderId)
    {
        Console.WriteLine($"[CHECKOUT-DEBUG] ═══ ProcessPaymentCancel START ═══ OrderId={orderId}");
        // Hệ thống không tạo rác pending trong DB nữa nên không cần làm gì ở đây.
    }

    private async Task CleanupAbandonedPendingOrdersAsync(int userId)
    {
        try
        {
            var pendingOrders = await _repo.GetPendingOrdersByUserAsync(userId);
            if (pendingOrders.Any())
            {
                _logger.LogInformation("[CHECKOUT] Cleaning up {Count} abandoned pending orders for user {UserId}...", pendingOrders.Count, userId);
                foreach (var order in pendingOrders)
                {
                    var transactions = await _repo.GetTransactionsByOrderIdAsync(order.OrderId);
                    if (transactions.Any())
                    {
                        await _repo.DeleteTransactionsAsync(transactions);
                    }
                    await _repo.DeleteOrderAsync(order);
                }
                await _repo.SaveChangesAsync();
                _logger.LogInformation("[CHECKOUT] Abandoned pending orders cleaned up successfully for user {UserId}.", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CHECKOUT] Error cleaning up abandoned pending orders for user {UserId}", userId);
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
