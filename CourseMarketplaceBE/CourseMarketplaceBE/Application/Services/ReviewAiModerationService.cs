using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Application.Services;

public class ReviewAiModerationService : IReviewAiModerationService
{
    private readonly IReviewService _reviewService;
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly IUserRepository _userRepo;
    private readonly INotificationService _notificationService;
    private readonly IAiModerationService _aiModerationService;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IAiModelRepository _aiModelRepository;
    private readonly IAiModerationLogService _aiModerationLogService;
    private readonly ILogger<ReviewAiModerationService> _logger;
    private readonly ICourseRepository _courseRepo;
    private readonly ILessonRepository _lessonRepo;

    public ReviewAiModerationService(
        IReviewService reviewService,
        IEnrollmentRepository enrollmentRepo,
        IUserRepository userRepo,
        INotificationService notificationService,
        IAiModerationService aiModerationService,
        ISystemConfigRepository systemConfigRepository,
        IAiModelRepository aiModelRepository,
        IAiModerationLogService aiModerationLogService,
        ILogger<ReviewAiModerationService> logger,
        ICourseRepository courseRepo,
        ILessonRepository lessonRepo)
    {
        _reviewService = reviewService;
        _enrollmentRepo = enrollmentRepo;
        _userRepo = userRepo;
        _notificationService = notificationService;
        _aiModerationService = aiModerationService;
        _systemConfigRepository = systemConfigRepository;
        _aiModelRepository = aiModelRepository;
        _aiModerationLogService = aiModerationLogService;
        _logger = logger;
        _courseRepo = courseRepo;
        _lessonRepo = lessonRepo;
    }

    public async Task<ReviewAiModerationResponse> HandleReviewAiModerationAsync(TempReviewDto tempDto)
    {
        try
        {
            var isHealthy = await _aiModerationService.HealthCheckAsync();
            if (!isHealthy)
            {
                string notificationContent = $"AI Moderation service is currently unavailable. A manual audit is required for Course ID: {tempDto.CourseId}.";
                await ResolveAiModerationResultAsync(tempDto, ModerationStatus.ManualAudit.ToValue(), notificationContent);
                return new ReviewAiModerationResponse
                {
                    ModerationStatus = ModerationStatus.ManualAudit.ToValue(),
                    Reason = "AI service is currently unavailable."
                };
            }

            var config = await PrepareForReviewAiModerationAsync();
            if (config == null) 
            {
                // Fallback to manual audit if AI is not configured
                string notificationContent = $"AI Moderation service is not properly configured. A manual audit is required for Course ID: {tempDto.CourseId}.";
                await ResolveAiModerationResultAsync(tempDto, ModerationStatus.ManualAudit.ToValue(), notificationContent);
                return new ReviewAiModerationResponse
                {
                    ModerationStatus = ModerationStatus.ManualAudit.ToValue(),
                    Reason = "AI service is not configured."
                };
            }

            var request = new ReviewAiModerationRequest
            {
                ReviewComment = tempDto.ReviewComment,
                SpamScoreThreshold = config.Value.spamScoreThreshold,
                ToxicScoreThreshold = config.Value.toxicScoreThreshold,
                ClassificationModel = config.Value.model
            };

            var response = await _aiModerationService.ModerateReviewAsync(request);
            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            Console.WriteLine($"AI Moderation result for review comment:\n{System.Text.Json.JsonSerializer.Serialize(response, options)}");
            await ResolveAiModerationResultAsync(tempDto, response.ModerationStatus);

            await _aiModerationLogService.SaveReviewModerationLogAsync(new LogReviewAiModerationCommand 
            { 
                Review = tempDto, 
                Request = request, 
                Response = response 
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Review Moderation failed for TempReview {ReviewId}", tempDto.ReviewId);
            string notificationContent = $"AI Moderation service encountered an error: {ex.Message}. A manual audit is required for Course ID: {tempDto.CourseId}.";
            await ResolveAiModerationResultAsync(tempDto, ModerationStatus.ManualAudit.ToValue(), notificationContent);
            return new ReviewAiModerationResponse
            {
                ModerationStatus = ModerationStatus.ManualAudit.ToValue(),
                Reason = $"AI Moderation error: {ex.Message}"
            };
        }
    }

    private async Task<(AiModelDto model, float spamScoreThreshold, float toxicScoreThreshold)?> PrepareForReviewAiModerationAsync()
    {
        var modelPath = await _systemConfigRepository.GetValueAsync(SystemConfigKeys.ReviewHarmfulTextClassifier);
        if (string.IsNullOrEmpty(modelPath))
            return null;

        var model = await _aiModelRepository.GetByModelPathAsync(modelPath);
        if (model == null) return null;

        var thresholdStr = await _systemConfigRepository.GetValueAsync(SystemConfigKeys.ModerationThreshold);
        float spamThreshold = 0.7f;
        float toxicThreshold = 0.7f;
        if (!string.IsNullOrEmpty(thresholdStr))
        {
            try
            {
                using var doc = JsonDocument.Parse(thresholdStr);
                if (doc.RootElement.TryGetProperty("spam", out var spamProp) && spamProp.TryGetSingle(out var sVal))
                    spamThreshold = sVal;
                if (doc.RootElement.TryGetProperty("toxic", out var toxicProp) && toxicProp.TryGetSingle(out var tVal))
                    toxicThreshold = tVal;
            }
            catch { /* Ignore parsing errors */ }
        }

        var modelDto = new AiModelDto
        {
            ModelId = model.ModelId,
            ModelName = model.ModelName,
            ModelProvider = model.ModelProvider,
            ModelVersion = model.ModelVersion,
            ModelPath = model.ModelPath,
            ProcessType = model.ProcessType
        };

        return (modelDto, spamThreshold, toxicThreshold);
    }

    private async Task ResolveAiModerationResultAsync(TempReviewDto tempDto, string moderationStatus, string? notificationContent = null)
    {
        bool isApproved = moderationStatus.Equals(ModerationStatus.Approved.ToValue(), StringComparison.OrdinalIgnoreCase);

        if (tempDto.ReviewId == 0) // New review
        {
            await ProcessNewReviewModerationAsync(tempDto, moderationStatus, isApproved, notificationContent);
        }
        else // Updating existing review
        {
            await ProcessUpdateReviewModerationAsync(tempDto, moderationStatus, isApproved, notificationContent);
        }
    }

    private async Task ProcessNewReviewModerationAsync(TempReviewDto tempDto, string moderationStatus, bool isApproved, string? notificationContent)
    {
        if (isApproved || moderationStatus.Equals(ModerationStatus.ManualAudit.ToValue(), StringComparison.OrdinalIgnoreCase))
        {
            var enrollment = await _enrollmentRepo.GetEnrollmentWithProgressAsync(tempDto.AuthorId, tempDto.CourseId);
            if (enrollment == null) return;

            string reviewStatus = isApproved ? ReviewStatus.Ok.ToValue() : ReviewStatus.Pending.ToValue();
            int reviewId = await _reviewService.CreateReviewInDatabaseAsync(tempDto, enrollment.EnrollmentId, reviewStatus);

            if (isApproved)
            {
                await NotifyAuthorAsync(tempDto, true, reviewId);
                await NotifyManagersAsync("Review Moderation Result", $"A new review was approved (Course ID: {tempDto.CourseId}).", null);
            }
            else
            {
                string content = notificationContent ?? $"A new review requires manual audit (Course ID: {tempDto.CourseId}).";
                await NotifyManagersAsync("Review Moderation Required", content, $"/AdminModeration/Reviews");
            }
        }
        else // Flagged/Rejected
        {
            await NotifyAuthorAsync(tempDto, false, null);
            await NotifyManagersAsync("Review Moderation Result", $"A review was flagged/rejected (Course ID: {tempDto.CourseId}).", null);
        }
    }

    private async Task ProcessUpdateReviewModerationAsync(TempReviewDto tempDto, string moderationStatus, bool isApproved, string? notificationContent)
    {
        if (isApproved || moderationStatus.Equals(ModerationStatus.ManualAudit.ToValue(), StringComparison.OrdinalIgnoreCase))
        {
            string reviewStatus = isApproved ? ReviewStatus.Ok.ToValue() : ReviewStatus.Pending.ToValue();
            await _reviewService.UpdateReviewInDatabaseAsync(tempDto, reviewStatus);

            if (isApproved)
            {
                await NotifyAuthorAsync(tempDto, true, tempDto.ReviewId);
                await NotifyManagersAsync("Review Moderation Result", $"A review update was approved (Course ID: {tempDto.CourseId}).", null);
            }
            else
            {
                string content = notificationContent ?? $"An updated review requires manual audit (Review ID: {tempDto.ReviewId}).";
                await NotifyManagersAsync("Review Moderation Required", content, $"/AdminModeration/Reviews");
            }
        }
        else // Flagged/Rejected update
        {
            await NotifyAuthorAsync(tempDto, false, tempDto.ReviewId);
            await NotifyManagersAsync("Review Moderation Result", $"A review update was flagged/rejected (Course ID: {tempDto.CourseId}).", null);
        }
    }

    private async Task NotifyAuthorAsync(TempReviewDto tempDto, bool isApproved, int? reviewId)
    {
        string message;
        string? linkAction = null;

        if (isApproved)
        {
            message = "Your review has been approved and is now visible.";
            if (reviewId.HasValue)
            {
                linkAction = tempDto.LessonId.HasValue 
                    ? $"/Course/Learn/{tempDto.CourseId}#review-card-{reviewId}" 
                    : $"/Course/Details/{tempDto.CourseId}#review-card-{reviewId}";
            }
        }
        else
        {
            var course = await _courseRepo.GetByIdAsync(tempDto.CourseId);
            string courseTitle = course?.Title ?? "Unknown Course";
            
            string contextInfo = $"Course: {courseTitle}";
            if (tempDto.LessonId.HasValue && tempDto.LessonId.Value > 0)
            {
                var lesson = await _lessonRepo.GetByIdAsync(tempDto.LessonId.Value);
                if (lesson != null)
                {
                    contextInfo += $", Lesson: {lesson.Title}";
                }
            }

            message = $"Your review was flagged for inappropriate content and has been rejected.\n\n{contextInfo}\nReview Comment: \"{tempDto.ReviewComment}\"";
        }

        await _notificationService.SendNotificationAsync(
            tempDto.AuthorId,
            isApproved ? "Review Approved" : "Review Rejected",
            message,
            linkAction
        );
    }

    private async Task NotifyManagersAsync(string title, string content, string? linkAction)
    {
        var managerIds = await _userRepo.GetAllManagerIdsAsync();
        if (managerIds.Any())
        {
            var dtos = managerIds.Select(id => new NotificationBulkDto
            {
                ReceiverId = id,
                Title = title,
                Content = content,
                LinkAction = linkAction
            }).ToList();

            await _notificationService.SendBulkNotificationsAsync(dtos);
        }
    }
}
