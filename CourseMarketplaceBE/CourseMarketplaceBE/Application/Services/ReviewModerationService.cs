using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Application.Services;

public class ReviewModerationService : IReviewModerationService
{
    private readonly IReviewModerationRecordRepository _moderationRecordRepo;
    private readonly IReviewService _reviewService;
    private readonly INotificationService _notificationService;
    private readonly ICourseRepository _courseRepo;
    private readonly ILessonRepository _lessonRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewModerationService> _logger;

    public ReviewModerationService(
        IReviewModerationRecordRepository moderationRecordRepo,
        IReviewService reviewService,
        INotificationService notificationService,
        ICourseRepository courseRepo,
        ILessonRepository lessonRepo,
        IMapper mapper,
        ILogger<ReviewModerationService> logger)
    {
        _moderationRecordRepo = moderationRecordRepo;
        _reviewService = reviewService;
        _notificationService = notificationService;
        _courseRepo = courseRepo;
        _lessonRepo = lessonRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<ReviewModerationRecordDto>> GetCourseReviewModerationRecordsAsync(PagedReviewModerationRequest request)
    {
        var result = await _moderationRecordRepo.GetCourseReviewModerationRecordsAsync(request);
        if (result.TotalCount == 0)
            throw new KeyNotFoundException("No course review moderation records found matching the specified filters.");

        var mappedItems = _mapper.Map<List<ReviewModerationRecordDto>>(result.Items);
        return new PagedResult<ReviewModerationRecordDto>(mappedItems, result.TotalCount, request.Page, request.PageSize);
    }

    public async Task<PagedResult<ReviewModerationRecordDto>> GetLessonReviewModerationRecordsAsync(PagedReviewModerationRequest request)
    {
        var result = await _moderationRecordRepo.GetLessonReviewModerationRecordsAsync(request);
        if (result.TotalCount == 0)
            throw new KeyNotFoundException("No lesson review moderation records found matching the specified filters.");

        var mappedItems = _mapper.Map<List<ReviewModerationRecordDto>>(result.Items);
        return new PagedResult<ReviewModerationRecordDto>(mappedItems, result.TotalCount, request.Page, request.PageSize);
    }

    private static int GetPriority(string status) => status switch { "pending" => 3, "rejected" => 2, "approved" => 1, _ => 0 };
    private static int GetThreatLevel(string status) => status switch { "flagged" => 4, "manual_audit" => 3, "approved" => 2, "pending" => 1, _ => 0 };

    public async Task<object> GetModerationStatsAsync()
    {
        var pending = await _moderationRecordRepo.CountByModerationStatusAsync(CourseMarketplaceBE.Domain.Constants.ModerationStatus.Pending.ToValue());
        var approved = await _moderationRecordRepo.CountByModerationStatusAsync(CourseMarketplaceBE.Domain.Constants.ModerationStatus.Approved.ToValue());
        var rejected = await _moderationRecordRepo.CountByModerationStatusAsync(CourseMarketplaceBE.Domain.Constants.ModerationStatus.Rejected.ToValue());

        return new
        {
            TotalPending = pending,
            TotalApproved = approved,
            TotalRejected = rejected,
            TotalRecords = pending + approved + rejected
        };
    }

    public async Task ApproveReviewAsync(ApproveRejectReviewRequest request)
    {
        if (request.Type.ToLower() == "lesson")
        {
            var record = await _moderationRecordRepo.GetLessonReviewModerationRecordByIdAsync(request.RecordId);
            if (record == null) throw new KeyNotFoundException("Lesson moderation record not found.");
            
            record.ModerationStatus = ModerationStatus.Approved.ToValue().ToLower();
            record.ModerationNote = request.ModerationNote;
            record.UpdatedAt = DateTime.Now;
            
            var tempDto = new TempReviewDto
            {
                ReviewId = record.LessonReviewId,
                LessonId = record.LessonReviewId,
                Rating = (float)record.TempRating,
                ReviewComment = record.TempComment
            };

            await _reviewService.UpdateReviewInDatabaseAsync(tempDto, ReviewStatus.Ok.ToValue());
            await _moderationRecordRepo.UpdateLessonReviewModerationRecordAsync(record);
            
            await NotifyAuthorAsync(record.LessonReview.Enrollment.UserId, record.LessonReview.Enrollment.CourseId ?? 0, record.LessonReview.LessonId, record.TempComment, true, record.LessonReviewId);
        }
        else
        {
            var record = await _moderationRecordRepo.GetCourseReviewModerationRecordByIdAsync(request.RecordId);
            if (record == null) throw new KeyNotFoundException("Course moderation record not found.");
            
            record.ModerationStatus = ModerationStatus.Approved.ToValue().ToLower();
            record.ModerationNote = request.ModerationNote;
            record.UpdatedAt = DateTime.Now;
            
            var tempDto = new TempReviewDto
            {
                ReviewId = record.CourseReviewId,
                Rating = (float)record.TempRating,
                ReviewComment = record.TempComment
            };

            await _reviewService.UpdateReviewInDatabaseAsync(tempDto, ReviewStatus.Ok.ToValue());
            await _moderationRecordRepo.UpdateCourseReviewModerationRecordAsync(record);
            
            await NotifyAuthorAsync(record.CourseReview.Enrollment.UserId, record.CourseReview.Enrollment.CourseId ?? 0, null, record.TempComment, true, record.CourseReviewId);
        }

        await SaveModerationRecordChangesAsync();
    }

    public async Task RejectReviewAsync(ApproveRejectReviewRequest request)
    {

        if (request.Type.ToLower() == "lesson")
        {
            var record = await _moderationRecordRepo.GetLessonReviewModerationRecordByIdAsync(request.RecordId);
            if (record == null) throw new KeyNotFoundException("Lesson moderation record not found.");
            
            record.ModerationStatus = ModerationStatus.Rejected.ToValue().ToLower();
            record.ModerationNote = request.ModerationNote;
            record.UpdatedAt = DateTime.Now;

            string targetStatus = record.IsUpdate ? ReviewStatus.Ok.ToValue() : ReviewStatus.Removed.ToValue();
            bool isRemoved = !record.IsUpdate;
            await _reviewService.UpdateReviewStatusInDatabaseAsync(record.LessonReviewId, true, targetStatus, isRemoved);
            await _moderationRecordRepo.UpdateLessonReviewModerationRecordAsync(record);
            
            await NotifyAuthorAsync(record.LessonReview.Enrollment.UserId, record.LessonReview.Enrollment.CourseId ?? 0, record.LessonReview.LessonId, record.TempComment, false, null);
        }
        else
        {
            var record = await _moderationRecordRepo.GetCourseReviewModerationRecordByIdAsync(request.RecordId);
            if (record == null) throw new KeyNotFoundException("Course moderation record not found.");
            
            record.ModerationStatus = ModerationStatus.Rejected.ToValue().ToLower();
            record.ModerationNote = request.ModerationNote;
            record.UpdatedAt = DateTime.Now;

            string targetStatus = record.IsUpdate ? ReviewStatus.Ok.ToValue() : ReviewStatus.Removed.ToValue();
            bool isRemoved = !record.IsUpdate;
            await _reviewService.UpdateReviewStatusInDatabaseAsync(record.CourseReviewId, false, targetStatus, isRemoved);
            await _moderationRecordRepo.UpdateCourseReviewModerationRecordAsync(record);
            
            await NotifyAuthorAsync(record.CourseReview.Enrollment.UserId, record.CourseReview.Enrollment.CourseId ?? 0, null, record.TempComment, false, null);
        }

        await SaveModerationRecordChangesAsync();
    }

    private async Task SaveModerationRecordChangesAsync()
    {
        try
        {
            await _moderationRecordRepo.SaveChangesAsync();
        }
        catch (ReviewModerationException ex)
        {
            throw new BadRequestException(ex.Message);
        }
    }

    private async Task NotifyAuthorAsync(int? authorId, int courseId, int? lessonId, string comment, bool isApproved, int? reviewId)
    {
        if (authorId == null) return;

        string message;
        string? linkAction = null;

        if (isApproved)
        {
            message = "Your review has been approved and is now visible.";
            if (reviewId.HasValue)
            {
                linkAction = lessonId.HasValue 
                    ? $"/Course/Learn/{courseId}#review-card-{reviewId}" 
                    : $"/Course/Details/{courseId}#review-card-{reviewId}";
            }
        }
        else
        {
            var course = await _courseRepo.GetByIdAsync(courseId);
            string courseTitle = course?.Title ?? "Unknown Course";
            
            string contextInfo = $"Course: {courseTitle}";
            if (lessonId.HasValue && lessonId.Value > 0)
            {
                var lesson = await _lessonRepo.GetByIdAsync(lessonId.Value);
                if (lesson != null)
                {
                    contextInfo += $"\nLesson: {lesson.Title}";
                }
            }

            message = $"Your review was flagged for inappropriate content and has been rejected by Admin.\n\n{contextInfo}\nReview Comment: \"{comment}\"";
            linkAction = (lessonId.HasValue && lessonId.Value > 0)
                ? $"/Course/Learn/{courseId}"
                : $"/Course/Details/{courseId}";
        }

        await _notificationService.SendNotificationAsync(
            authorId.Value,
            isApproved ? "Review Approved" : "Review Rejected",
            message,
            linkAction
        );
    }
}
