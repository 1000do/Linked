using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CourseMarketplaceBE.Domain.Constants;

namespace CourseMarketplaceBE.Application.Services;

public class GiftCheckoutService : IGiftCheckoutService
{
    private readonly ICheckoutRepository _repo;
    private readonly IPaymentGatewayService _paymentGateway;
    private readonly ILogger<GiftCheckoutService> _logger;
    private readonly IHubContext<FinanceHub> _hubContext;
    private readonly INotificationService _notificationService;
    private readonly ICourseRepository _courseRepo;
    private readonly IUserRepository _userRepo;
    private readonly IAdminFinanceService _adminFinanceService;
    private readonly IGiftRepository _giftRepo;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public GiftCheckoutService(
        ICheckoutRepository repo,
        IPaymentGatewayService paymentGateway,
        ILogger<GiftCheckoutService> logger,
        IHubContext<FinanceHub> hubContext,
        IAdminFinanceService adminFinanceService,
        INotificationService notificationService,
        ICourseRepository courseRepo,
        IUserRepository userRepo,
        IGiftRepository giftRepo,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _repo = repo;
        _paymentGateway = paymentGateway;
        _logger = logger;
        _hubContext = hubContext;
        _notificationService = notificationService;
        _courseRepo = courseRepo;
        _userRepo = userRepo;
        _adminFinanceService = adminFinanceService;
        _giftRepo = giftRepo;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<CheckoutResponse> InitiateGiftCheckoutAsync(int userId, GiftCheckoutRequest request)
    {
        var course = await _courseRepo.GetCourseWithInstructorAsync(request.CourseId);
        await ValidateCourseForGiftAsync(course, request.RecipientEmail, request.CourseId);

        var stripeAccountId = await _userRepo.GetInstructorStripeAccountIdAsync(course!.InstructorId ?? 0);
        if (string.IsNullOrEmpty(stripeAccountId))
            throw new InvalidOperationException("Instructor has not connected a Stripe payment account.");

        decimal purchasePrice = Math.Round(course.Price, 2);
        var userEmail = await _userRepo.GetUserEmailAsync(userId);

        var paymentLineItems = new List<PaymentLineItem>
        {
            new PaymentLineItem
            {
                CourseName = $"Gift: {course.Title}",
                ThumbnailUrl = course.CourseThumbnailUrl,
                UnitPrice = purchasePrice
            }
        };

        var instructorCountry = await _userRepo.GetInstructorStripeCountryAsync(course.InstructorId ?? 0);
        var sessionCurrency = GetCurrencyFromCountry(instructorCountry);

        var orderReference = $"gift_{Guid.NewGuid().ToString("N")}";
        var metadata = BuildGiftMetadata(userId, request, course.CourseId);

        var paymentResult = await _paymentGateway.CreateCheckoutSessionAsync(
            paymentLineItems,
            request.SuccessUrl,
            request.CancelUrl,
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

    public async Task<CheckoutResponse> InitiateGiftPaymentIntentAsync(int userId, GiftCheckoutRequest request)
    {
        var course = await _courseRepo.GetCourseWithInstructorAsync(request.CourseId);
        await ValidateCourseForGiftAsync(course, request.RecipientEmail, request.CourseId);

        var stripeAccountId = await _userRepo.GetInstructorStripeAccountIdAsync(course!.InstructorId ?? 0);
        if (string.IsNullOrEmpty(stripeAccountId))
            throw new InvalidOperationException("Instructor has not connected a Stripe payment account.");

        decimal purchasePrice = Math.Round(course.Price, 2);
        var metadata = BuildGiftMetadata(userId, request, course.CourseId);

        var paymentResult = await _paymentGateway.CreatePaymentIntentAsync(
            purchasePrice,
            "usd",
            metadata);

        return new CheckoutResponse
        {
            SessionUrl = paymentResult.ClientSecret,
            SessionId = paymentResult.PaymentIntentId
        };
    }

    public async Task ProcessPaymentSuccessAsync(string sessionId, bool failOnTransferFailure = false)
    {
        Console.WriteLine($"[GIFT-CHECKOUT-DEBUG] ═══ ProcessPaymentSuccess START ═══ SessionId={sessionId}");
        try 
        {
            if (await IsTransactionAlreadyProcessedAsync(sessionId)) return;

            var metadata = await _paymentGateway.GetSessionMetadataAsync(sessionId);
            if (metadata == null || !metadata.TryGetValue("userId", out var userIdStr))
                throw new InvalidOperationException("Valid metadata or UserId was not found in the Stripe Session.");

            int userId = int.Parse(userIdStr);
            var paymentIntentId = await _paymentGateway.GetPaymentReferenceAsync(sessionId);
            await ProcessSuccessfulGiftPaymentCoreAsync(sessionId, paymentIntentId, metadata, userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GIFT-CHECKOUT-DEBUG] ❌ TOP-LEVEL EXCEPTION: {ex.Message}");
            throw;
        }
    }

    public async Task ProcessPaymentIntentSuccessAsync(string paymentIntentId)
    {
        Console.WriteLine($"[GIFT-CHECKOUT-DEBUG] ═══ ProcessPaymentIntentSuccess START ═══ PaymentIntentId={paymentIntentId}");
        try 
        {
            if (await IsTransactionAlreadyProcessedAsync(paymentIntentId)) return;

            var metadata = await _paymentGateway.GetPaymentIntentMetadataAsync(paymentIntentId);
            if (metadata == null || !metadata.TryGetValue("userId", out var userIdStr))
                throw new InvalidOperationException("Valid metadata or UserId was not found in the Stripe PaymentIntent.");

            int userId = int.Parse(userIdStr);
            await ProcessSuccessfulGiftPaymentCoreAsync(paymentIntentId, paymentIntentId, metadata, userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GIFT-CHECKOUT-DEBUG] ❌ TOP-LEVEL EXCEPTION: {ex.Message}");
            throw;
        }
    }

    private async Task ProcessSuccessfulGiftPaymentCoreAsync(string sessionId, string? paymentIntentId, Dictionary<string, string> metadata, int userId)
    {
        string courseIdsStr = metadata.TryGetValue("courseIds", out var cids) ? cids : "";

        if (string.IsNullOrEmpty(courseIdsStr))
            throw new InvalidOperationException($"Valid Course ID list was not found in the Stripe session/intent.");

        int courseId = int.Parse(courseIdsStr);
        var currentTransferRate = await _adminFinanceService.GetCurrentTransferRateAsync();

        Console.WriteLine("[GIFT-CHECKOUT-DEBUG] 🔄 Starting DB Transaction...");
        await using var dbTransaction = await _repo.BeginTransactionAsync();
        try
        {
            var order = await CreateGiftOrderAsync(userId);
            await ProcessGiftCourseOrderItemAsync(order.OrderId, courseId, userId, sessionId, paymentIntentId, currentTransferRate, metadata);

            await _repo.SaveChangesAsync();
            Console.WriteLine("[GIFT-CHECKOUT-DEBUG] 🏁 Committing Transaction...");
            await dbTransaction.CommitAsync();
            Console.WriteLine("[GIFT-CHECKOUT-DEBUG] ✅ SUCCESS!");

            await _hubContext.Clients.Group("AdminFinance").SendAsync("UpdatePayoutStatus", new { refresh = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GIFT-CHECKOUT-DEBUG] ❌ INNER EXCEPTION: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    // ─── Local Extracted Methods for DB Operations ───────────────────────────────────

    private async Task<OrderInfo> CreateGiftOrderAsync(int userId)
    {
        var order = new OrderInfo
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            OrderStatus = "paid",
            PaymentMethod = "stripe"
        };
        await _repo.AddOrderAsync(order);
        await _repo.SaveChangesAsync();
        return order;
    }

    private async Task ProcessGiftCourseOrderItemAsync(int orderId, int courseId, int userId, string sessionId, string? paymentIntentId, decimal currentTransferRate, Dictionary<string, string> metadata)
    {
        var course = await _courseRepo.GetCourseWithInstructorAsync(courseId);
        if (course == null) return;

        if (course.CourseStatus != CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue())
            throw new InvalidOperationException($"The course \"{course.Title}\" is not published and cannot be purchased.");

        decimal purchasePrice = Math.Round(course.Price, 2);

        var orderItem = new OrderItem
        {
            OrderId = orderId,
            CourseId = courseId,
            PurchasePrice = purchasePrice,
            CouponUsed = false,
            OriginalPrice = course.Price,
            DiscountAmount = 0
        };
        await _repo.AddOrderItemAsync(orderItem);
        await _repo.SaveChangesAsync();

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

        await ProcessGiftFulfillmentAsync(userId, orderItem.Id, course, metadata);
        await ProcessPayoutAndNotificationAsync(transaction, course.InstructorId, course.Title, purchasePrice, currentTransferRate);
    }

    // ─── Shared Duplicated Helpers ───────────────────────────────────────────────────

    private async Task ValidateCourseForGiftAsync(Course? course, string recipientEmail, int courseId)
    {
        if (course == null)
            throw new InvalidOperationException("Course not found.");

        if (course.CourseStatus != CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue())
            throw new InvalidOperationException($"The course \"{course.Title}\" is not published and cannot be purchased.");

        var recipientAccount = await _userRepo.GetAccountByEmailAsync(recipientEmail);
        if (recipientAccount != null && await _courseRepo.IsEnrolledAsync(recipientAccount.AccountId, courseId))
        {
            throw new InvalidOperationException("The recipient has already owned/joined this course.");
        }
    }

    private Dictionary<string, string> BuildGiftMetadata(int userId, GiftCheckoutRequest request, int courseId)
    {
        var feBaseUrl = request.SuccessUrl;
        int idx = feBaseUrl.IndexOf("/Gift/CheckoutSuccess", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            feBaseUrl = feBaseUrl.Substring(0, idx);
        }
        else
        {
            feBaseUrl = _configuration.GetValue<string>("FrontendBaseUrl") ?? "http://localhost:5208";
        }

        return new Dictionary<string, string>
        {
            { "userId", userId.ToString() },
            { "checkoutType", "gift" },
            { "courseIds", courseId.ToString() },
            { "recipientEmail", request.RecipientEmail },
            { "recipientName", request.RecipientName ?? "" },
            { "giftMessage", request.GiftMessage ?? "" },
            { "cardTheme", request.CardTheme },
            { "feBaseUrl", feBaseUrl }
        };
    }

    private async Task<bool> IsTransactionAlreadyProcessedAsync(string stripeId)
    {
        var existingTransactions = await _repo.GetTransactionsBySessionIdAsync(stripeId);
        if (existingTransactions.Any())
        {
            var allSucceeded = existingTransactions.All(t => t.TransactionsStatus == "succeeded");
            var allPayoutsCreated = existingTransactions.All(t => t.InstructorPayouts.Any());

            if (allSucceeded && allPayoutsCreated)
            {
                return true;
            }
        }
        return false;
    }

    private async Task ProcessGiftFulfillmentAsync(int userId, int orderItemId, Course course, Dictionary<string, string> metadata)
    {
        string recipientEmail = metadata.TryGetValue("recipientEmail", out var re) ? re : "";
        string recipientName = metadata.TryGetValue("recipientName", out var rn) ? rn : null;
        string giftMessage = metadata.TryGetValue("giftMessage", out var gm) ? gm : null;
        string cardTheme = metadata.TryGetValue("cardTheme", out var theme) ? theme : "classic";
        string feBaseUrl = metadata.TryGetValue("feBaseUrl", out var fbUrl) ? fbUrl : (_configuration.GetValue<string>("FrontendBaseUrl") ?? "http://localhost:5208");

        var token = Guid.NewGuid().ToString("N");

        var gift = new Gift
        {
            OrderItemId = orderItemId,
            SenderId = userId,
            RecipientEmail = recipientEmail,
            RecipientName = recipientName,
            GiftMessage = giftMessage,
            CardTheme = cardTheme,
            RedemptionToken = token,
            IsClaimed = false,
            DeliveryStatus = "sent",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _giftRepo.AddAsync(gift);
        await _giftRepo.SaveChangesAsync();

        var claimLink = $"{feBaseUrl}/Gift/Claim?token={token}";
        var senderName = "A Friend";
        var senderAccount = await _userRepo.GetAccountByIdAsync(userId);
        if (senderAccount != null)
        {
            senderName = senderAccount.User?.FullName ?? senderAccount.Username ?? senderAccount.Email ?? "A Friend";
        }

        var subject = $"🎁 You received a gift course from {senderName}!";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden;'>
                <div style='background: linear-gradient(135deg, #059669 0%, #0d9488 100%); padding: 30px; text-align: center; color: white;'>
                    <h1 style='margin: 0; font-size: 24px;'>A Special Gift For You!</h1>
                </div>
                <div style='padding: 30px;'>
                    <p>Hi {(string.IsNullOrEmpty(recipientName) ? "" : recipientName)},</p>
                    <p>Great news! <strong>{senderName}</strong> has sent you a course gift card on LinkedLearn:</p>
                    <div style='background-color: #f8fafc; border-left: 4px solid #059669; padding: 15px; margin: 20px 0; border-radius: 4px;'>
                        <p style='margin: 0; font-weight: bold;'>Course:</p>
                        <p style='margin: 5px 0 15px 0; font-size: 18px; color: #0f172a;'>{course.Title}</p>
                        {(string.IsNullOrWhiteSpace(giftMessage) ? "" : $@"
                        <p style='margin: 0; font-weight: bold;'>Personal Message:</p>
                        <p style='margin: 5px 0 0 0; font-style: italic; color: #475569;'>""{giftMessage}""</p>")}
                    </div>
                    <p>Click the button below to claim your gift and start learning now:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{claimLink}' style='background-color: #059669; color: white; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1);'>Claim Your Gift Course</a>
                    </div>
                    <p style='font-size: 12px; color: #64748b;'>If you cannot click the button, copy and paste this link into your browser:<br/><a href='{claimLink}'>{claimLink}</a></p>
                </div>
                <div style='background-color: #f1f5f9; padding: 20px; text-align: center; font-size: 12px; color: #64748b; border-top: 1px solid #e2e8f0;'>
                    <p style='margin: 0;'>LinkedLearn Course Marketplace</p>
                </div>
            </div>";

        try
        {
            await _emailService.SendEmailAsync(recipientEmail, subject, body);
        }
        catch (Exception emailEx)
        {
            _logger.LogError(emailEx, $"Failed to send gift email to {recipientEmail}");
        }

        try
        {
            var recipientAccount = await _userRepo.GetAccountByEmailAsync(recipientEmail);
            if (recipientAccount != null)
            {
                var notiTitle = $"🎁 You received a gift course from {senderName}!";
                var notiContent = $"{senderName} has sent you the course '{course.Title}'. Click here to claim your gift card.";
                await _notificationService.SendNotificationAsync(
                    recipientAccount.AccountId,
                    notiTitle,
                    notiContent,
                    claimLink
                );
            }
        }
        catch (Exception notiEx)
        {
            _logger.LogError(notiEx, $"Failed to send in-app notification to {recipientEmail}");
        }
    }

    private async Task ProcessPayoutAndNotificationAsync(Transaction transaction, int? instructorId, string? courseTitle, decimal purchasePrice, decimal currentTransferRate)
    {
        var stripeFee = Math.Round(purchasePrice * 0.029m + 0.30m, 2);
        stripeFee = Math.Min(stripeFee, purchasePrice); 
        var netPrice = purchasePrice - stripeFee;

        var payoutAmount = Math.Round(netPrice * (currentTransferRate / 100m), 2);
        var payout = new InstructorPayout
        {
            TransactionId = transaction.TransactionId,
            InstructorId = instructorId ?? 0,
            PayoutAmount = payoutAmount,
            PayoutDate = await CalculatePayoutDateAsync(DateTime.Now),
            IsPaid = false,
            PayoutStatus = "pending",
            StripeTransferId = null
        };
        await _repo.AddInstructorPayoutAsync(payout);

        if (instructorId.HasValue)
        {
            var title = courseTitle ?? "your course";
            await _notificationService.SendNotificationAsync(
                instructorId.Value,
                "You have a new order",
                $"The course '{title}' has been successfully sold. Expected revenue: ${payoutAmount:N2} USD.",
                $"/Transaction/Instructor#tx-{transaction.TransactionId}"
            );
        }
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
            "HR" => "EUR",
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
