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
        var gift = await GetGiftByTokenOrThrowAsync(token);
        ValidateGiftForViewing(gift);
        
        var senderName = await GetSenderNameAsync(gift.SenderId);
        var course = gift.OrderItem!.Course!;

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
        var gift = await GetGiftByTokenOrThrowAsync(token);
        ValidateGiftForClaiming(gift);

        var course = await GetCourseForGiftOrThrowAsync(gift.OrderItem?.CourseId);
        await EnsureRecipientNotAlreadyEnrolledAsync(userId, course.CourseId);

        using var transaction = await _enrollmentRepo.BeginTransactionAsync();
        try
        {
            await CreateEnrollmentWithinTransactionAsync(userId, course);
            await MarkGiftAsClaimedWithinTransactionAsync(gift, userId);
            
            var rejected = await _transactionRepo.RejectPendingRefundForGiftClaimedAsync(gift.OrderItemId);
            
            await transaction.CommitAsync();

            if (rejected)
            {
                await NotifyAdminFinanceOfRefundRejectionAsync();
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
        ValidateGiftCreationInputs(recipientEmail, token);
        await EnsureNoExistingGiftAsync(orderItemId);

        var gift = CreateNewGiftEntity(orderItemId, senderId, recipientEmail, recipientName, message, theme, token);

        await _giftRepo.AddAsync(gift);
        await _giftRepo.SaveChangesAsync();
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

    // --- Private Helper Methods ---

    private async Task<Gift> GetGiftByTokenOrThrowAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.", nameof(token));

        var gift = await _giftRepo.GetByTokenAsync(token);
        if (gift == null)
            throw new InvalidOperationException("Gift token is invalid or does not exist.");

        return gift;
    }

    private void ValidateGiftForViewing(Gift gift)
    {
        if (gift.DeliveryStatus == "refunded")
            throw new InvalidOperationException("This gift has been refunded and is no longer active.");

        if (gift.OrderItem?.Course == null)
            throw new InvalidOperationException("Course associated with this gift was not found.");
    }

    private async Task<string> GetSenderNameAsync(int? senderId)
    {
        if (!senderId.HasValue)
            return "A Friend";

        var senderAccount = await _userRepo.GetAccountByIdAsync(senderId.Value);
        return senderAccount?.User?.FullName 
            ?? senderAccount?.Username 
            ?? senderAccount?.Email 
            ?? "A Friend";
    }

    private void ValidateGiftForClaiming(Gift gift)
    {
        if (gift.IsClaimed)
            throw new InvalidOperationException("This gift has already been claimed.");

        if (gift.DeliveryStatus == "refunded")
            throw new InvalidOperationException("This gift has been refunded and cannot be claimed.");
    }

    private async Task<Course> GetCourseForGiftOrThrowAsync(int? courseId)
    {
        if (!courseId.HasValue)
            throw new InvalidOperationException("Course associated with this gift is invalid.");

        var course = await _courseRepo.GetByIdAsync(courseId.Value);
        if (course == null)
            throw new InvalidOperationException("Course associated with this gift was not found.");
            
        return course;
    }

    private async Task EnsureRecipientNotAlreadyEnrolledAsync(int userId, int courseId)
    {
        var existingEnrollment = await _enrollmentRepo.GetEnrollmentAsync(userId, courseId);
        if (existingEnrollment != null && existingEnrollment.EnrollmentStatus == "active")
            throw new InvalidOperationException("You already own this course in your library.");
    }

    private async Task CreateEnrollmentWithinTransactionAsync(int userId, Course course)
    {
        var enrollment = new Enrollment
        {
            UserId = userId,
            CourseId = course.CourseId,
            Title = course.Title,
            Description = course.Description,
            EnrollDate = DateOnly.FromDateTime(DateTime.Today),
            IsCompleted = false,
            EnrollmentStatus = "active",
            LastAccessedAt = DateTime.Now
        };
        await _enrollmentRepo.AddEnrollmentAsync(enrollment);
        await _enrollmentRepo.SaveChangesAsync();
    }

    private async Task MarkGiftAsClaimedWithinTransactionAsync(Gift gift, int userId)
    {
        gift.IsClaimed = true;
        gift.ClaimedByUserId = userId;
        gift.ClaimedAt = DateTime.Now;
        gift.DeliveryStatus = "sent";
        gift.UpdatedAt = DateTime.Now;

        _giftRepo.Update(gift);
        await _giftRepo.SaveChangesAsync();
    }

    private async Task NotifyAdminFinanceOfRefundRejectionAsync()
    {
        try
        {
            await _hubContext.Clients.Group("AdminFinance").SendAsync("UpdatePayoutStatus", new { refresh = true });
            await _hubContext.Clients.All.SendAsync("UpdatePayoutStatus", new { refresh = true });
        }
        catch { }
    }

    private void ValidateGiftCreationInputs(string recipientEmail, string token)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
            throw new ArgumentException("Recipient email is required.", nameof(recipientEmail));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Redemption token is required.", nameof(token));
    }

    private async Task EnsureNoExistingGiftAsync(int orderItemId)
    {
        var existingGift = await _giftRepo.GetByOrderItemIdAsync(orderItemId);
        if (existingGift != null)
            throw new InvalidOperationException("A gift record already exists for this order item.");
    }

    private Gift CreateNewGiftEntity(int orderItemId, int senderId, string recipientEmail, string? recipientName, string? message, string theme, string token)
    {
        return new Gift
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
    }
}

