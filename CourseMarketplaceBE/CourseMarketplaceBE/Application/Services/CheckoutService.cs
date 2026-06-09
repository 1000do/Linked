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
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly IPaymentGatewayService _paymentGateway;
    private readonly ILogger<CheckoutService> _logger;
    private readonly IHubContext<FinanceHub> _hubContext;
    private readonly INotificationService _notificationService;
    private readonly ICourseRepository _courseRepo;
    private readonly ICouponRepository _couponRepo;
    private readonly IUserRepository _userRepo;
    private readonly IAdminFinanceService _adminFinanceService;

    public CheckoutService(
        ICheckoutRepository repo,
        IEnrollmentRepository enrollmentRepo,
        IPaymentGatewayService paymentGateway,
        ILogger<CheckoutService> logger,
        IHubContext<FinanceHub> hubContext,
        IAdminFinanceService adminFinanceService,
        INotificationService notificationService,
        ICourseRepository courseRepo,
        ICouponRepository couponRepo,
        IUserRepository userRepo)
    {
        _repo = repo;
        _enrollmentRepo = enrollmentRepo;
        _paymentGateway = paymentGateway;
        _logger = logger;
        _hubContext = hubContext;
        _notificationService = notificationService;
        _courseRepo = courseRepo;
        _couponRepo = couponRepo;
        _userRepo = userRepo;
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
            if (item.CourseId.HasValue && await _courseRepo.IsEnrolledAsync(userId, item.CourseId.Value))
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
                var cp = await _couponRepo.GetByCodeAsync(code);
                if (cp == null)
                {
                    throw new InvalidOperationException("This coupon does not exist.");
                }

                var now = DateTime.Now;
                if (cp.IsActive != true ||
                    (cp.StartDate.HasValue && now < cp.StartDate.Value) ||
                    (cp.EndDate.HasValue && now > cp.EndDate.Value) ||
                    (cp.UsageLimit.HasValue && (cp.UsedCount ?? 0) >= cp.UsageLimit.Value))
                {
                    throw new InvalidOperationException("This coupon has expired or run out of usage.");
                }

                decimal eligibleSubTotal = cartItems
                    .Where(c => c.Course != null && c.Course.CouponId == cp.CouponId)
                    .Sum(c => c.Price ?? c.Course?.Price ?? 0m);

                if (eligibleSubTotal >= cp.MinOrderValue && eligibleSubTotal > 0)
                {
                    appliedCoupons.Add(cp);
                }
            }
        }

        // Lấy email user cho Stripe
        var userEmail = await _userRepo.GetUserEmailAsync(userId);

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
        var course = await _courseRepo.GetCourseWithInstructorAsync(courseId);
        if (course == null)
            throw new InvalidOperationException("Course not found.");

        if (await _courseRepo.IsEnrolledAsync(userId, courseId))
            throw new InvalidOperationException($"You have already purchased the course \"{course.Title}\".");

        var stripeAccountId = await _userRepo.GetInstructorStripeAccountIdAsync(course.InstructorId ?? 0);
        if (string.IsNullOrEmpty(stripeAccountId))
            throw new InvalidOperationException("Instructor has not connected a Stripe payment account.");

        Coupon? coupon = null;
        decimal discountAmount = 0m;
        decimal originalPrice = course.Price;
        
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            coupon = await _couponRepo.GetByCodeAsync(couponCode);
            if (coupon == null)
            {
                throw new InvalidOperationException("This coupon does not exist.");
            }

            var now = DateTime.Now;
            if (coupon.IsActive != true ||
                (coupon.StartDate.HasValue && now < coupon.StartDate.Value) ||
                (coupon.EndDate.HasValue && now > coupon.EndDate.Value) ||
                (coupon.UsageLimit.HasValue && (coupon.UsedCount ?? 0) >= coupon.UsageLimit.Value))
            {
                throw new InvalidOperationException("This coupon has expired or run out of usage.");
            }

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

        var purchasePrice = Math.Max(originalPrice - discountAmount, 0m);
        var userEmail = await _userRepo.GetUserEmailAsync(userId);

        var paymentLineItems = new List<PaymentLineItem>
        {
            new PaymentLineItem
            {
                CourseName = course.Title ?? "Course",
                ThumbnailUrl = course.CourseThumbnailUrl,
                UnitPrice = purchasePrice
            }
        };

        var instructorCountry = await _userRepo.GetInstructorStripeCountryAsync(course.InstructorId ?? 0);
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
                throw new InvalidOperationException("Valid metadata or UserId was not found in the Stripe Session.");
            }

            int userId = int.Parse(userIdStr);
            string couponCode = metadata.TryGetValue("couponCode", out var cc) ? cc : "";
            string checkoutType = metadata.TryGetValue("checkoutType", out var ct) ? ct : "cart";
            string courseIdsStr = metadata.TryGetValue("courseIds", out var cids) ? cids : "";

            if (string.IsNullOrEmpty(courseIdsStr))
            {
                throw new InvalidOperationException("Valid Course ID list was not found in the Stripe Session.");
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
                ? await _couponRepo.GetValidCouponAsync(couponCode, DateTime.Now)
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
                int rows1 = await _repo.SaveChangesAsync();
                if (rows1 <= 0) throw new InvalidOperationException("Failed to save changes");

                foreach (var courseId in courseIds)
                {
                    var course = await _courseRepo.GetCourseWithInstructorAsync(courseId);
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
                    int rows2 = await _repo.SaveChangesAsync();
                    if (rows2 <= 0) throw new InvalidOperationException("Failed to save changes");

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
                    int rows3 = await _repo.SaveChangesAsync();
                    if (rows3 <= 0) throw new InvalidOperationException("Failed to save changes");

                    // Cấp Enrollment (Hoặc kích hoạt lại nếu đã từng refund)
                    var existingEnrollment = await _enrollmentRepo.GetEnrollmentIncludingRefundedAsync(userId, courseId);
                    if (existingEnrollment != null)
                    {
                        existingEnrollment.EnrollmentStatus = "active";
                        existingEnrollment.EnrollDate = DateOnly.FromDateTime(DateTime.Now);
                        existingEnrollment.IsCompleted = false;
                        existingEnrollment.CompletedDate = null;
                        existingEnrollment.LastAccessedAt = DateTime.Now;

                        await _enrollmentRepo.ClearMaterialCompletionsAsync(existingEnrollment.EnrollmentId);
                    }
                    else
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
                        await _enrollmentRepo.AddEnrollmentAsync(enrollment);
                        int rows4 = await _enrollmentRepo.SaveChangesAsync();
                        if (rows4 <= 0) throw new InvalidOperationException("Failed to save changes");
                    }

                    // Tính toán phí Stripe (2.9% + $0.30) và lấy doanh thu thuần của khoá học
                    var stripeFee = Math.Round(purchasePrice * 0.029m + 0.30m, 2);
                    stripeFee = Math.Min(stripeFee, purchasePrice); // Tránh âm tiền nếu khoá học quá rẻ
                    var netPrice = purchasePrice - stripeFee;

                    // Tạo Payout record (chia tiền sau khi trừ phí Stripe)
                    var payoutAmount = Math.Round(netPrice * (currentTransferRate / 100m), 2);
                    var payout = new InstructorPayout
                    {
                        TransactionId = transaction.TransactionId,
                        InstructorId = course.InstructorId ?? 0,
                        PayoutAmount = payoutAmount,
                        PayoutDate = await CalculatePayoutDateAsync(DateTime.Now),
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
                            $"The course '{courseTitle}' has been successfully sold. Expected revenue: ${payoutAmount:N2} USD.",
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

                int rows5 = await _repo.SaveChangesAsync();
                if (rows5 <= 0) throw new InvalidOperationException("Failed to save changes");

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

    // ═══════════════════════════════════════════════════════════════════════════
    // BƯỚC 4: STRIPE PAYMENTINTENT ONBOARDING & PROCESSING
    // ═══════════════════════════════════════════════════════════════════════════
    public async Task<CheckoutResponse> InitiatePaymentIntentAsync(
        int userId,
        string? couponCode)
    {
        // ── 4.1 Lấy cart items kèm Course + Instructor ──────────────────────
        var cartItems = await _repo.GetCartItemsWithCourseAndInstructorAsync(userId);
        if (!cartItems.Any())
            throw new InvalidOperationException("Cart is empty. Cannot checkout.");

        // ── 4.2 Kiểm tra không mua trùng (đã enrolled) ──────────────────────
        foreach (var item in cartItems)
        {
            if (item.CourseId.HasValue && await _courseRepo.IsEnrolledAsync(userId, item.CourseId.Value))
                throw new InvalidOperationException(
                    $"You have already purchased the course \"{item.Course?.Title}\". Please remove it from the cart.");
        }

        // ── 4.3 Tính toán coupon (Hỗ trợ nhiều mã phân tách bằng dấu phẩy) ────
        var appliedCoupons = new List<Coupon>();

        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var codes = couponCode.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim().ToUpper())
                .Distinct()
                .ToList();

            foreach (var code in codes)
            {
                var cp = await _couponRepo.GetByCodeAsync(code);
                if (cp == null)
                {
                    throw new InvalidOperationException("This coupon does not exist.");
                }

                var now = DateTime.Now;
                if (cp.IsActive != true ||
                    (cp.StartDate.HasValue && now < cp.StartDate.Value) ||
                    (cp.EndDate.HasValue && now > cp.EndDate.Value) ||
                    (cp.UsageLimit.HasValue && (cp.UsedCount ?? 0) >= cp.UsageLimit.Value))
                {
                    throw new InvalidOperationException("This coupon has expired or run out of usage.");
                }

                decimal eligibleSubTotal = cartItems
                    .Where(c => c.Course != null && c.Course.CouponId == cp.CouponId)
                    .Sum(c => c.Price ?? c.Course?.Price ?? 0m);

                if (eligibleSubTotal >= cp.MinOrderValue && eligibleSubTotal > 0)
                {
                    appliedCoupons.Add(cp);
                }
            }
        }

        // ── 4.4 Tính tổng tiền cần thanh toán ──────────────────────────────
        decimal totalAmount = 0m;

        foreach (var cartItem in cartItems)
        {
            var originalPrice = cartItem.Price ?? cartItem.Course?.Price ?? 0m;
            decimal purchasePrice = originalPrice;
            Coupon? matchedCoupon = null;

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
            totalAmount += purchasePrice;
        }

        var sessionCurrency = "usd"; 

        // metadata để lưu thông tin đơn hàng lên Stripe PaymentIntent
        var metadata = new Dictionary<string, string>
        {
            { "userId", userId.ToString() },
            { "couponCode", couponCode ?? "" },
            { "checkoutType", "cart" },
            { "courseIds", string.Join(",", cartItems.Select(c => c.CourseId)) }
        };

        // Gọi Payment Gateway để tạo PaymentIntent
        var (clientSecret, paymentIntentId) = await _paymentGateway.CreatePaymentIntentAsync(
            totalAmount,
            sessionCurrency,
            metadata);

        return new CheckoutResponse
        {
            SessionUrl = clientSecret, // Tận dụng trường SessionUrl để trả về clientSecret
            SessionId = paymentIntentId // Tận dụng trường SessionId để trả về PaymentIntentId
        };
    }

    public async Task ProcessPaymentIntentSuccessAsync(string paymentIntentId)
    {
        Console.WriteLine($"[CHECKOUT-DEBUG] ═══ ProcessPaymentIntentSuccess START ═══ PaymentIntentId={paymentIntentId}");

        try 
        {
            // ── 4.5 Kiểm tra trùng lặp (Idempotency) ───────────────────────────
            var existingTransactions = await _repo.GetTransactionsBySessionIdAsync(paymentIntentId);
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

            // ── 4.6 Lấy Metadata từ Stripe PaymentIntent ────────────────────────
            Console.WriteLine("[CHECKOUT-DEBUG] Fetching Stripe PaymentIntent Metadata...");
            var metadata = await _paymentGateway.GetPaymentIntentMetadataAsync(paymentIntentId);
            if (metadata == null || !metadata.TryGetValue("userId", out var userIdStr))
            {
                throw new InvalidOperationException("Valid metadata or UserId was not found in the Stripe PaymentIntent.");
            }

            int userId = int.Parse(userIdStr);
            string couponCode = metadata.TryGetValue("couponCode", out var cc) ? cc : "";
            string checkoutType = metadata.TryGetValue("checkoutType", out var ct) ? ct : "cart";
            string courseIdsStr = metadata.TryGetValue("courseIds", out var cids) ? cids : "";

            if (string.IsNullOrEmpty(courseIdsStr))
            {
                throw new InvalidOperationException("Valid Course ID list was not found in the Stripe PaymentIntent.");
            }

            var courseIds = courseIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            // ── 4.7 Lấy thông tin cần thiết ─────────────────────────────────────
            var currentTransferRate = await _adminFinanceService.GetCurrentTransferRateAsync();
            Console.WriteLine($"[CHECKOUT-DEBUG] PaymentIntentId={paymentIntentId}");

            // ── 4.8 Load các khóa học & áp coupon để tính giá chính xác ─────────
            var coupon = !string.IsNullOrWhiteSpace(couponCode)
                ? await _couponRepo.GetValidCouponAsync(couponCode, DateTime.Now)
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
                int rows1 = await _repo.SaveChangesAsync();
                if (rows1 <= 0) throw new InvalidOperationException("Failed to save changes");

                foreach (var courseId in courseIds)
                {
                    var course = await _courseRepo.GetCourseWithInstructorAsync(courseId);
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
                    int rows2 = await _repo.SaveChangesAsync();
                    if (rows2 <= 0) throw new InvalidOperationException("Failed to save changes");

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
                        StripeSessionId = paymentIntentId, // Sử dụng paymentIntentId cho StripeSessionId
                        StripePaymentintentId = paymentIntentId,
                        Currency = "usd",
                        TransactionsStatus = "succeeded",
                        TransactionType = "payment",
                        TransactionCreatedAt = DateTime.Now
                    };
                    await _repo.AddTransactionAsync(transaction);
                    int rows3 = await _repo.SaveChangesAsync();
                    if (rows3 <= 0) throw new InvalidOperationException("Failed to save changes");

                    // Cấp Enrollment (Hoặc kích hoạt lại nếu đã từng refund)
                    var existingEnrollment = await _enrollmentRepo.GetEnrollmentIncludingRefundedAsync(userId, courseId);
                    if (existingEnrollment != null)
                    {
                        existingEnrollment.EnrollmentStatus = "active";
                        existingEnrollment.EnrollDate = DateOnly.FromDateTime(DateTime.Now);
                        existingEnrollment.IsCompleted = false;
                        existingEnrollment.CompletedDate = null;
                        existingEnrollment.LastAccessedAt = DateTime.Now;

                        await _enrollmentRepo.ClearMaterialCompletionsAsync(existingEnrollment.EnrollmentId);
                    }
                    else
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
                        await _enrollmentRepo.AddEnrollmentAsync(enrollment);
                        int rows4 = await _enrollmentRepo.SaveChangesAsync();
                        if (rows4 <= 0) throw new InvalidOperationException("Failed to save changes");
                    }

                    // Tính toán phí Stripe (2.9% + $0.30) và lấy doanh thu thuần của khoá học
                    var stripeFee = Math.Round(purchasePrice * 0.029m + 0.30m, 2);
                    stripeFee = Math.Min(stripeFee, purchasePrice); // Tránh âm tiền nếu khoá học quá rẻ
                    var netPrice = purchasePrice - stripeFee;

                    // Tạo Payout record (chia tiền sau khi trừ phí Stripe)
                    var payoutAmount = Math.Round(netPrice * (currentTransferRate / 100m), 2);
                    var payout = new InstructorPayout
                    {
                        TransactionId = transaction.TransactionId,
                        InstructorId = course.InstructorId ?? 0,
                        PayoutAmount = payoutAmount,
                        PayoutDate = await CalculatePayoutDateAsync(DateTime.Now),
                        IsPaid = false,
                        PayoutStatus = "pending",
                        StripeTransferId = null
                    };
                    await _repo.AddInstructorPayoutAsync(payout);

                    var instructorId = course.InstructorId;

                    // ── NEW: Thông báo cho giảng viên ──
                    if (instructorId.HasValue)
                    {
                        var courseTitle = course.Title ?? "your course";
                        await _notificationService.SendNotificationAsync(
                            instructorId.Value,
                            "You have a new order",
                            $"The course '{courseTitle}' has been successfully sold. Expected revenue: ${payoutAmount:N2} USD.",
                            $"/Instructor/Payouts"
                        );
                    }
                }

                // ── 4.9 CLEAR CART (nếu mua từ Cart) ───────────────────────────
                if (checkoutType == "cart")
                {
                    Console.WriteLine("[CHECKOUT-DEBUG] 🔄 Clearing cart...");
                    await _repo.ClearCartAsync(userId);
                }

                int rows5 = await _repo.SaveChangesAsync();
                if (rows5 <= 0) throw new InvalidOperationException("Failed to save changes");

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
                int rows6 = await _repo.SaveChangesAsync();
                if (rows6 <= 0) throw new InvalidOperationException("Failed to save changes");
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

    private async Task<DateTime> CalculatePayoutDateAsync(DateTime transactionDate)
    {
        var payoutDaysConfig = await _adminFinanceService.GetPayoutDaysConfigAsync();
        int payoutDay = 15;
        if (!string.IsNullOrWhiteSpace(payoutDaysConfig))
        {
            var firstConfigDay = payoutDaysConfig.Split(',')
                .Select(s => int.TryParse(s.Trim(), out var d) ? d : 0)
                .Where(d => d > 0)
                .FirstOrDefault();
            if (firstConfigDay > 0)
            {
                payoutDay = firstConfigDay;
            }
        }

        var nextMonth = transactionDate.AddMonths(1);
        int daysInNextMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
        int targetDay = Math.Min(payoutDay, daysInNextMonth);

        return new DateTime(nextMonth.Year, nextMonth.Month, targetDay, 0, 0, 0, transactionDate.Kind);
    }
}
