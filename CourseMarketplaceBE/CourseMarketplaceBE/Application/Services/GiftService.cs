using System;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CourseMarketplaceBE.Application.Services;

public class GiftService : IGiftService
{
    private readonly IGiftRepository _giftRepo;
    private readonly ICourseRepository _courseRepo;
    private readonly IUserRepository _userRepo;
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IHubContext<FinanceHub> _hubContext;

    public GiftService(
        IGiftRepository giftRepo,
        ICourseRepository courseRepo,
        IUserRepository userRepo,
        IEnrollmentRepository enrollmentRepo,
        ITransactionRepository transactionRepo,
        IHubContext<FinanceHub> hubContext)
    {
        _giftRepo = giftRepo;
        _courseRepo = courseRepo;
        _userRepo = userRepo;
        _enrollmentRepo = enrollmentRepo;
        _transactionRepo = transactionRepo;
        _hubContext = hubContext;
    }

    public async Task<GiftValidationResponse> ValidateGiftTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.", nameof(token));

        var gift = await _giftRepo.GetByTokenAsync(token);
        if (gift == null)
            throw new InvalidOperationException("Gift token is invalid or does not exist.");

        if (gift.DeliveryStatus == "refunded")
            throw new InvalidOperationException("This gift has been refunded and is no longer active.");

        var course = gift.OrderItem?.Course;
        if (course == null)
            throw new InvalidOperationException("Course associated with this gift was not found.");

        var senderName = "A Friend";
        if (gift.SenderId.HasValue)
        {
            var senderAccount = await _userRepo.GetAccountByIdAsync(gift.SenderId.Value);
            senderName = senderAccount?.User?.FullName ?? senderAccount?.Username ?? senderAccount?.Email ?? "A Friend";
        }

        return new GiftValidationResponse
        {
            GiftId = gift.GiftId,
            SenderName = senderName,
            RecipientName = gift.RecipientName,
            GiftMessage = gift.GiftMessage,
            CardTheme = gift.CardTheme ?? "classic",
            CourseTitle = course.Title,
            CourseThumbnailUrl = course.CourseThumbnailUrl,
            IsClaimed = gift.IsClaimed
        };
    }

    public async Task ClaimGiftAsync(int userId, string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.", nameof(token));

        var gift = await _giftRepo.GetByTokenAsync(token);
        if (gift == null)
            throw new InvalidOperationException("Gift token is invalid or does not exist.");

        if (gift.IsClaimed)
            throw new InvalidOperationException("This gift has already been claimed.");

        if (gift.DeliveryStatus == "refunded")
            throw new InvalidOperationException("This gift has been refunded and cannot be claimed.");

        var courseId = gift.OrderItem?.CourseId;
        if (!courseId.HasValue)
            throw new InvalidOperationException("Course associated with this gift is invalid.");

        var course = await _courseRepo.GetByIdAsync(courseId.Value);
        if (course == null)
            throw new InvalidOperationException("Course associated with this gift was not found.");

        // Check if recipient already has an active enrollment
        var existingEnrollment = await _enrollmentRepo.GetEnrollmentAsync(userId, courseId.Value);
        if (existingEnrollment != null && existingEnrollment.EnrollmentStatus == "active")
            throw new InvalidOperationException("You already own this course in your library.");

        // Use transaction to ensure data integrity
        using var transaction = await _enrollmentRepo.BeginTransactionAsync();
        try
        {
            // 1. Create enrollment record
            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId.Value,
                Title = course.Title,
                Description = course.Description,
                EnrollDate = DateOnly.FromDateTime(DateTime.Today),
                IsCompleted = false,
                EnrollmentStatus = "active",
                LastAccessedAt = DateTime.Now
            };
            await _enrollmentRepo.AddEnrollmentAsync(enrollment);
            await _enrollmentRepo.SaveChangesAsync();

            // 2. Update Gift record status
            gift.IsClaimed = true;
            gift.ClaimedByUserId = userId;
            gift.ClaimedAt = DateTime.Now;
            gift.DeliveryStatus = "sent";
            gift.UpdatedAt = DateTime.Now;

            _giftRepo.Update(gift);
            await _giftRepo.SaveChangesAsync();

            // 3. Reject pending refund if any
            var rejected = await _transactionRepo.RejectPendingRefundForGiftClaimedAsync(gift.OrderItemId);

            // Commit transaction
            await transaction.CommitAsync();

            // 4. Notify admin if a refund request was rejected
            if (rejected)
            {
                try
                {
                    await _hubContext.Clients.Group("AdminFinance").SendAsync("UpdatePayoutStatus", new { refresh = true });
                    await _hubContext.Clients.All.SendAsync("UpdatePayoutStatus", new { refresh = true });
                }
                catch { }
            }
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task CreateGiftRecordAsync(int orderItemId, int senderId, string recipientEmail, string? recipientName, string? message, string theme, string token)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
            throw new ArgumentException("Recipient email is required.", nameof(recipientEmail));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Redemption token is required.", nameof(token));

        var existingGift = await _giftRepo.GetByOrderItemIdAsync(orderItemId);
        if (existingGift != null)
            throw new InvalidOperationException("A gift record already exists for this order item.");

        var gift = new Gift
        {
            OrderItemId = orderItemId,
            SenderId = senderId > 0 ? senderId : null,
            RecipientEmail = recipientEmail,
            RecipientName = recipientName,
            GiftMessage = message,
            CardTheme = theme,
            RedemptionToken = token,
            IsClaimed = false,
            DeliveryStatus = "pending",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _giftRepo.AddAsync(gift);
        var rows = await _giftRepo.SaveChangesAsync();
        if (rows <= 0)
            throw new InvalidOperationException("Failed to save gift record database changes.");
    }

    public async Task<bool> IsRecipientEnrolledAsync(string email, int courseId)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var recipientAccount = await _userRepo.GetAccountByEmailAsync(email);
        if (recipientAccount != null)
        {
            return await _courseRepo.IsEnrolledAsync(recipientAccount.AccountId, courseId);
        }

        return false;
    }
}
