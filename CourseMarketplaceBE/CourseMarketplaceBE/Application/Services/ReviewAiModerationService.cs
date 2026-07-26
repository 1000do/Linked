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
    private readonly IReviewModerationRecordRepository _moderationRecordRepo;

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
        ILessonRepository lessonRepo,
        IReviewModerationRecordRepository moderationRecordRepo)
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
        _moderationRecordRepo = moderationRecordRepo;
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
            await ResolveAiModerationResultAsync(tempDto, response.ModerationStatus, response.Reason);

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
        string aiNote = notificationContent ?? $"AI Moderation result: {moderationStatus}\nReview Comment: {tempDto.ReviewComment}";

        if (tempDto.LessonId.HasValue && tempDto.LessonId.Value > 0)
        {
            var record = await _moderationRecordRepo.GetLessonReviewModerationRecordByIdAsync(tempDto.RecordId);
            if (record != null)
            {
                record.AiModerationStatus = moderationStatus.ToLower();
                record.AiModerationNote = aiNote;
                record.UpdatedAt = DateTime.Now;
                await _moderationRecordRepo.UpdateLessonReviewModerationRecordAsync(record);
            }
        }
        else
        {
            var record = await _moderationRecordRepo.GetCourseReviewModerationRecordByIdAsync(tempDto.RecordId);
            if (record != null)
            {
                record.AiModerationStatus = moderationStatus.ToLower();
                record.AiModerationNote = aiNote;
                record.UpdatedAt = DateTime.Now;
                await _moderationRecordRepo.UpdateCourseReviewModerationRecordAsync(record);
            }
        }

        await _moderationRecordRepo.SaveChangesAsync();

        await NotifyManagersAsync("Review AI Moderation Result", aiNote, $"/AdminModeration/Reviews");
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
