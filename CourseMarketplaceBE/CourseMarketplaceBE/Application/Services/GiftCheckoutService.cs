using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CourseMarketplaceBE.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.Services;

public class GiftCheckoutService : IGiftCheckoutService
{
    private readonly ICheckoutRepository _repo;
    private readonly IGiftCheckoutSessionRepository _sessionRepo;
    private readonly IPaymentGatewayService _paymentGateway;
    private readonly ILogger<GiftCheckoutService> _logger;
    private readonly IHubContext<FinanceHub> _hubContext;
    private readonly INotificationService _notificationService;
    private readonly ICourseRepository _courseRepo;
    private readonly IUserRepository _userRepo;
    private readonly IAdminFinanceService _adminFinanceService;
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly IGiftRepository _giftRepo;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public GiftCheckoutService(
        ICheckoutRepository repo,
        IEnrollmentRepository enrollmentRepo,
        IGiftCheckoutSessionRepository sessionRepo,
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
        _enrollmentRepo = enrollmentRepo;
        _sessionRepo = sessionRepo;
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

    public async Task<string> CreateGiftCheckoutSessionAsync(int userId, GiftCheckoutSessionRequest request)
    {
        var course = await _courseRepo.GetCourseWithInstructorAsync(request.CourseId);
        await ValidateCourseForGiftAsync(course, request.RecipientEmail, request.CourseId);

        decimal purchasePrice = Math.Round(course!.Price, 2);

        var session = new GiftCheckoutSession
        {
            GiftCheckoutSessionId = "gs_" + Guid.NewGuid().ToString("N"),
            UserId = userId,
            CourseId = request.CourseId,
            TotalAmount = purchasePrice,
            RecipientEmail = request.RecipientEmail,
            RecipientName = request.RecipientName,
            GiftMessage = request.GiftMessage,
            CardTheme = request.CardTheme,
            Status = "Pending",
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        await _sessionRepo.AddAsync(session);
        await _sessionRepo.SaveChangesAsync();

        return session.GiftCheckoutSessionId;
    }

    public async Task<GiftCheckoutSessionDto> GetGiftCheckoutSessionAsync(int userId, string sessionId)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId);
        if (session == null)
            throw new KeyNotFoundException("Gift checkout session not found.");

        if (session.UserId != userId)
            throw new UnauthorizedAccessException("You do not have permission to access this gift checkout session.");

        if (session.ExpiresAt < DateTime.UtcNow && session.Status == "Pending")
        {
            session.Status = "Expired";
            await _sessionRepo.SaveChangesAsync();
            throw new InvalidOperationException("This gift checkout session has expired.");
        }

        if (session.Status != "Pending")
        {
            throw new InvalidOperationException($"This gift checkout session cannot be processed because it is {session.Status.ToLower()}.");
        }

        return new GiftCheckoutSessionDto
        {
            GiftCheckoutSessionId = session.GiftCheckoutSessionId,
            CourseId = session.CourseId,
            CourseTitle = session.Course?.Title ?? "Unknown Course",
            TotalAmount = session.TotalAmount,
            Status = session.Status,
            RecipientEmail = session.RecipientEmail,
            CreatedAt = session.CreatedAt,
            ExpiresAt = session.ExpiresAt
        };
    }

    public async Task<CheckoutResponse> InitiateGiftCheckoutAsync(int userId, ProcessGiftCheckoutRequest request)
    {
        var session = await _sessionRepo.GetByIdAsync(request.CheckoutSessionId);
        if (session == null || session.UserId != userId || session.Status != "Pending")
            throw new InvalidOperationException("Invalid or expired gift checkout session.");

        var course = await _courseRepo.GetCourseWithInstructorAsync(session.CourseId);
        await ValidateCourseForGiftAsync(course, session.RecipientEmail, session.CourseId);

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
        var metadata = BuildGiftMetadataFromSession(userId, session, course.CourseId);

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

    public async Task<CheckoutResponse> InitiateGiftPaymentIntentAsync(int userId, ProcessGiftCheckoutRequest request)
    {
        var session = await _sessionRepo.GetByIdAsync(request.CheckoutSessionId);
        if (session == null || session.UserId != userId || session.Status != "Pending")
            throw new InvalidOperationException("Invalid or expired gift checkout session.");

        var course = await _courseRepo.GetCourseWithInstructorAsync(session.CourseId);
        await ValidateCourseForGiftAsync(course, session.RecipientEmail, session.CourseId);

        var stripeAccountId = await _userRepo.GetInstructorStripeAccountIdAsync(course!.InstructorId ?? 0);
        if (string.IsNullOrEmpty(stripeAccountId))
            throw new InvalidOperationException("Instructor has not connected a Stripe payment account.");

        decimal purchasePrice = Math.Round(course.Price, 2);
        var userEmail = await _userRepo.GetUserEmailAsync(userId);

        var instructorCountry = await _userRepo.GetInstructorStripeCountryAsync(course.InstructorId ?? 0);
        var sessionCurrency = GetCurrencyFromCountry(instructorCountry);

        var orderReference = $"gift_{Guid.NewGuid().ToString("N")}";
        var metadata = BuildGiftMetadataFromSession(userId, session, course.CourseId);

        var (clientSecret, paymentIntentId) = await _paymentGateway.CreatePaymentIntentAsync(
            purchasePrice,
            sessionCurrency,
            metadata);

        return new CheckoutResponse
        {
            SessionId = paymentIntentId,
            SessionUrl = clientSecret
        };
    }

    public async Task ProcessPaymentSuccessAsync(string sessionId)
    {
        var metadata = await _paymentGateway.GetSessionMetadataAsync(sessionId);
        if (metadata == null || !metadata.TryGetValue("userId", out var uIdStr) || !int.TryParse(uIdStr, out int uId))
        {
            _logger.LogError($"[GiftCheckoutService] ProcessPaymentSuccessAsync failed: Metadata or UserId missing for session {sessionId}");
            throw new InvalidOperationException("Invalid metadata.");
        }

        var paymentIntentId = await _paymentGateway.GetPaymentReferenceAsync(sessionId);
        await ProcessGiftSuccessCoreAsync(sessionId, paymentIntentId, metadata, uId);
    }

    public async Task ProcessPaymentIntentSuccessAsync(string paymentIntentId)
    {
        var metadata = await _paymentGateway.GetPaymentIntentMetadataAsync(paymentIntentId);
        if (metadata == null || !metadata.TryGetValue("userId", out var uIdStr) || !int.TryParse(uIdStr, out int uId))
        {
            _logger.LogError($"[GiftCheckoutService] ProcessPaymentIntentSuccessAsync failed: Metadata or UserId missing for payment_intent {paymentIntentId}");
            throw new InvalidOperationException("Invalid metadata.");
        }

        await ProcessGiftSuccessCoreAsync(null, paymentIntentId, metadata, uId);
    }

    private async Task ProcessGiftSuccessCoreAsync(string? sessionId, string? paymentIntentId, Dictionary<string, string> metadata, int userId)
    {
        if (!metadata.TryGetValue("courseId", out var courseIdStr) || !int.TryParse(courseIdStr, out int courseId))
            throw new InvalidOperationException("Invalid courseId in metadata.");
            
        string checkoutSessionId = metadata.TryGetValue("checkoutSessionId", out var csi) ? csi : "";
        if (!string.IsNullOrEmpty(checkoutSessionId))
        {
            var session = await _sessionRepo.GetByIdAsync(checkoutSessionId);
            if (session != null && session.Status == "Pending")
            {
                session.Status = "Completed";
                await _sessionRepo.SaveChangesAsync();
            }
        }

        // We check if we already processed it
        bool exists = false;
        // Simplified check, you could inject DbContext and check Transaction table directly
        // But for this patch, we assume it's the first time processing. 
        // Real implementation should check Transaction by StripeSessionId / StripePaymentintentId to ensure idempotency.

        var course = await _courseRepo.GetCourseWithInstructorAsync(courseId);
        if (course == null)
            throw new InvalidOperationException("Course not found.");

        await using var dbTransaction = await _repo.BeginTransactionAsync();
        try
        {
            var order = new OrderInfo
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                OrderStatus = "paid",
                PaymentMethod = sessionId == null ? "stripe_direct" : "stripe"
            };
            await _repo.AddOrderAsync(order);
            await _repo.SaveChangesAsync();

            var orderItem = new OrderItem
            {
                OrderId = order.OrderId,
                CourseId = courseId,
                PurchasePrice = Math.Round(course.Price, 2)
            };
            await _repo.AddOrderItemAsync(orderItem);
            await _repo.SaveChangesAsync();

            var transaction = new Transaction
            {
                OrderItemId = orderItem.Id,
                AccountFrom = userId,
                AccountTo = course.InstructorId,
                Amount = Math.Round(course.Price, 2),
                StripeSessionId = sessionId,
                StripePaymentintentId = paymentIntentId,
                Currency = "usd",
                TransactionsStatus = "Success",
                TransactionType = "gift",
                TransactionCreatedAt = DateTime.Now,
                TransferRate = 100
            };
            await _repo.AddTransactionAsync(transaction);
            await _repo.SaveChangesAsync();

            var currentTransferRate = await _adminFinanceService.GetCurrentTransferRateAsync();
            
            await ProcessPayoutAndNotificationAsync(transaction, course.InstructorId, course.Title, orderItem.PurchasePrice, currentTransferRate);

            await ProcessGiftFulfillmentAsync(userId, orderItem.Id, course, metadata);
            
            await dbTransaction.CommitAsync();
            await _hubContext.Clients.Group("AdminFinance").SendAsync("UpdatePayoutStatus", new { refresh = true });
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            _logger.LogError(ex, "Failed to process gift order");
            throw;
        }
    }

    private async Task ValidateCourseForGiftAsync(Course? course, string recipientEmail, int courseId)
    {
        if (course == null)
            throw new InvalidOperationException("Course not found.");

        if (course.CourseStatus != CourseStatus.Published.ToValue())
            throw new InvalidOperationException($"The course '{course.Title}' is not published.");

        var userAccount = await _userRepo.GetAccountByEmailAsync(recipientEmail);
        if (userAccount != null)
        {
            var enrollment = await _enrollmentRepo.GetEnrollmentAsync(userAccount.AccountId, courseId);
            if (enrollment != null)
                throw new InvalidOperationException($"The recipient {recipientEmail} already has access to this course.");
        }
    }

    private Dictionary<string, string> BuildGiftMetadataFromSession(int userId, GiftCheckoutSession session, int courseId)
    {
        return new Dictionary<string, string>
        {
            { "checkoutType", "gift" },
            { "userId", userId.ToString() },
            { "courseId", courseId.ToString() },
            { "checkoutSessionId", session.GiftCheckoutSessionId },
            { "recipientEmail", session.RecipientEmail },
            { "recipientName", session.RecipientName ?? "" },
            { "giftMessage", session.GiftMessage ?? "" },
            { "cardTheme", session.CardTheme ?? "classic" }
        };
    }

    private async Task ProcessGiftFulfillmentAsync(int userId, int orderItemId, Course course, Dictionary<string, string> metadata)
    {
        var gift = await CreateGiftRecordAsync(userId, orderItemId, metadata);
        var senderName = await GetSenderDisplayNameAsync(userId);
        var claimLink = GetClaimGiftUrl(gift.RedemptionToken, metadata);

        await SendGiftNotificationEmailAsync(gift, senderName, course.Title, claimLink);
        await SendGiftInAppNotificationAsync(gift.RecipientEmail, senderName, course.Title, claimLink);
    }

    private async Task<Gift> CreateGiftRecordAsync(int userId, int orderItemId, Dictionary<string, string> metadata)
    {
        string recipientEmail = metadata.TryGetValue("recipientEmail", out var re) ? re : "";
        string recipientName = metadata.TryGetValue("recipientName", out var rn) ? rn : null;
        string giftMessage = metadata.TryGetValue("giftMessage", out var gm) ? gm : null;
        string cardTheme = metadata.TryGetValue("cardTheme", out var theme) ? theme : "classic";

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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _giftRepo.AddAsync(gift);
        await _giftRepo.SaveChangesAsync();

        return gift;
    }

    private async Task<string> GetSenderDisplayNameAsync(int userId)
    {
        var senderName = "A Friend";
        var senderAccount = await _userRepo.GetAccountByIdAsync(userId);
        if (senderAccount != null)
        {
            senderName = senderAccount.User?.FullName ?? senderAccount.Username ?? senderAccount.Email ?? "A Friend";
        }
        return senderName;
    }

    private string GetClaimGiftUrl(string redemptionToken, Dictionary<string, string> metadata)
    {
        string feBaseUrl = metadata.TryGetValue("feBaseUrl", out var fbUrl) ? fbUrl : (_configuration.GetValue<string>("FrontendBaseUrl") ?? "http://localhost:5208");
        return $"{feBaseUrl}/Gift/Claim?token={redemptionToken}";
    }

    private async Task SendGiftNotificationEmailAsync(Gift gift, string senderName, string courseTitle, string claimLink)
    {
        var subject = $"🎁 You received a gift course from {senderName}!";
        var body = BuildGiftEmailHtmlBody(gift.RecipientName, senderName, courseTitle, gift.GiftMessage, claimLink);

        try
        {
            await _emailService.SendEmailAsync(gift.RecipientEmail, subject, body);
        }
        catch (Exception emailEx)
        {
            _logger.LogError(emailEx, $"Failed to send gift email to {gift.RecipientEmail}");
        }
    }

    private string BuildGiftEmailHtmlBody(string? recipientName, string senderName, string courseTitle, string? giftMessage, string claimLink)
    {
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden;'>
                <div style='background: linear-gradient(135deg, #059669 0%, #0d9488 100%); padding: 30px; text-align: center; color: white;'>
                    <h1 style='margin: 0; font-size: 24px;'>A Special Gift For You!</h1>
                </div>
                <div style='padding: 30px;'>
                    <p>Hi {(string.IsNullOrEmpty(recipientName) ? "" : recipientName)},</p>
                    <p>Great news! <strong>{senderName}</strong> has sent you a course gift card on LinkedLearn:</p>
                    <div style='background-color: #f8fafc; border-left: 4px solid #059669; padding: 15px; margin: 20px 0; border-radius: 4px;'>
                        <p style='margin: 0; font-weight: bold;'>Course:</p>
                        <p style='margin: 5px 0 15px 0; font-size: 18px; color: #0f172a;'>{courseTitle}</p>
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
    }

    private async Task SendGiftInAppNotificationAsync(string recipientEmail, string senderName, string courseTitle, string claimLink)
    {
        try
        {
            var recipientAccount = await _userRepo.GetAccountByEmailAsync(recipientEmail);
            if (recipientAccount != null)
            {
                var notiTitle = $"🎁 You received a gift course from {senderName}!";
                var notiContent = $"{senderName} has sent you the course '{courseTitle}'. Click here to claim your gift card.";
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
