using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services;

public class ReportModerationService : IReportModerationService
{
    private readonly IReportRepository _reportRepo;
    private readonly ICourseRepository _courseRepo;
    private readonly IReviewRepository _reviewRepo;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepo;
    private readonly IRedisService _redisService;
    private readonly IMapper _mapper;
    private readonly IModerationPenaltyService _penaltyService;

    public ReportModerationService(
        IReportRepository reportRepo,
        ICourseRepository courseRepo,
        IReviewRepository reviewRepo,
        INotificationService notificationService,
        IUserRepository userRepo,
        IRedisService redisService,
        IMapper mapper,
        IModerationPenaltyService penaltyService)
    {
        _reportRepo = reportRepo;
        _courseRepo = courseRepo;
        _reviewRepo = reviewRepo;
        _notificationService = notificationService;
        _userRepo = userRepo;
        _redisService = redisService;
        _mapper = mapper;
        _penaltyService = penaltyService;
    }

    public async Task<PagedResult<CourseReportDetailResponse>> GetAllCourseReportsAsync(PagedReportRequestDto request)
    {
        var (reports, totalCount) = await _reportRepo.GetAllCourseReportsAsync(request.Status, request.Page, request.PageSize);
        if (totalCount == 0) throw new KeyNotFoundException("No reports found.");
        var items = _mapper.Map<IEnumerable<CourseReportDetailResponse>>(reports).ToList();
        return new PagedResult<CourseReportDetailResponse>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task<PagedResult<ReviewReportDetailResponse>> GetAllCourseReviewReportsAsync(PagedReportRequestDto request)
    {
        var (reports, totalCount) = await _reportRepo.GetAllCourseReviewReportsAsync(request.Status, request.Page, request.PageSize);
        if (totalCount == 0) throw new KeyNotFoundException("No course review reports found.");
        var items = _mapper.Map<IEnumerable<ReviewReportDetailResponse>>(reports).ToList();
        return new PagedResult<ReviewReportDetailResponse>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task<PagedResult<ReviewReportDetailResponse>> GetAllLessonReviewReportsAsync(PagedReportRequestDto request)
    {
        var (reports, totalCount) = await _reportRepo.GetAllLessonReviewReportsAsync(request.Status, request.Page, request.PageSize);
        if (totalCount == 0) throw new KeyNotFoundException("No lesson review reports found.");
        var items = _mapper.Map<IEnumerable<ReviewReportDetailResponse>>(reports).ToList();
        return new PagedResult<ReviewReportDetailResponse>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task<ReportStatsResponse> GetReportStatsAsync()
    {
        var today = DateTime.Today;

        int pendingCourseCount = await _reportRepo.CountCourseReportsByStatusAsync(ReportStatus.Pending.ToValue(), null);
        int pendingCourseReviewCount = await _reportRepo.CountCourseReviewReportsByStatusAsync(ReportStatus.Pending.ToValue(), null);
        int pendingLessonReviewCount = await _reportRepo.CountLessonReviewReportsByStatusAsync(ReportStatus.Pending.ToValue(), null);

        int resolvedCourseToday = await _reportRepo.CountCourseReportsByStatusAsync(ReportStatus.Resolved.ToValue(), today);
        int resolvedCourseReviewToday = await _reportRepo.CountCourseReviewReportsByStatusAsync(ReportStatus.Resolved.ToValue(), today);
        int resolvedLessonReviewToday = await _reportRepo.CountLessonReviewReportsByStatusAsync(ReportStatus.Resolved.ToValue(), today);

        int rejectedCourseToday = await _reportRepo.CountCourseReportsByStatusAsync(ReportStatus.Rejected.ToValue(), today);
        int rejectedCourseReviewToday = await _reportRepo.CountCourseReviewReportsByStatusAsync(ReportStatus.Rejected.ToValue(), today);
        int rejectedLessonReviewToday = await _reportRepo.CountLessonReviewReportsByStatusAsync(ReportStatus.Rejected.ToValue(), today);

        int resolvedToday = resolvedCourseToday + resolvedCourseReviewToday + resolvedLessonReviewToday;
        int rejectedToday = rejectedCourseToday + rejectedCourseReviewToday + rejectedLessonReviewToday;

        return new ReportStatsResponse
        {
            TotalPendingCourseReports = pendingCourseCount,
            TotalPendingCourseReviewReports = pendingCourseReviewCount,
            TotalPendingLessonReviewReports = pendingLessonReviewCount,
            TotalResolvedToday = resolvedToday,
            TotalRejectedToday = rejectedToday
        };
    }

    public async Task<bool> ResolveCourseReportAsync(int reportId, int resolverId, ResolveReportRequest request)
    {
        var (report, course) = await GetCourseReportAndEntityAsync(reportId);

        await ValidateReportResolutionAccessAsync(report.CourseReportsStatus!, resolverId);

        report.CourseReportsStatus = request.Status;
        report.ResolutionNote = request.ResolutionNote;
        report.ResolverId = resolverId;
        report.ResolvedAt = DateTime.Now;

        if (request.Status == ReportStatus.Resolved.ToValue())
        {
            await ApplyCoursePenaltyAsync(course, request.RemoveContent, request.ResolutionNote);
        }

        _reportRepo.UpdateCourseReport(report);
        await SaveReportChangesAsync();

        if (request.Status == ReportStatus.Resolved.ToValue() && request.RemoveContent)
        {
            await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(course.CourseId));
        }

        await SendResolutionNotificationsAsync(request.Status, reportId, report.ReporterId, "course", report.CourseId.Value);

        return true;
    }

    public async Task<bool> ResolveCourseReviewReportAsync(int reportId, int resolverId, ResolveReportRequest request)
    {
        var (report, review) = await GetCourseReviewReportAndEntityAsync(reportId);

        await ValidateReportResolutionAccessAsync(report.UserReportsStatus!, resolverId);

        report.UserReportsStatus = request.Status;
        report.ResolutionNote = request.ResolutionNote;
        report.ResolverId = resolverId;
        report.ResolvedAt = DateTime.Now;

        if (request.Status == ReportStatus.Resolved.ToValue())
        {
            await ApplyCourseReviewPenaltyAsync(review, request.RemoveContent, request.ResolutionNote);
        }

        _reportRepo.UpdateCourseReviewReport(report);
        await SaveReportChangesAsync();

        await SendResolutionNotificationsAsync(request.Status, reportId, report.ReporterId, "course review", report.CourseReviewId.Value);

        return true;
    }

    public async Task<bool> ResolveLessonReviewReportAsync(int reportId, int resolverId, ResolveReportRequest request)
    {
        var (report, review) = await GetLessonReviewReportAndEntityAsync(reportId);

        await ValidateReportResolutionAccessAsync(report.UserReportsStatus!, resolverId);

        report.UserReportsStatus = request.Status;
        report.ResolutionNote = request.ResolutionNote;
        report.ResolverId = resolverId;
        report.ResolvedAt = DateTime.Now;

        if (request.Status == ReportStatus.Resolved.ToValue())
        {
            await ApplyLessonReviewPenaltyAsync(review, request.RemoveContent, request.ResolutionNote);
        }

        _reportRepo.UpdateLessonReviewReport(report);
        await SaveReportChangesAsync();

        await SendResolutionNotificationsAsync(request.Status, reportId, report.ReporterId, "lesson review", report.LessonReviewId.Value);

        return true;
    }

    // ── Extracted Helper Methods ────────────────────────────────────────────

    private async Task<(CourseReport, Course)> GetCourseReportAndEntityAsync(int reportId)
    {
        var report = await _reportRepo.GetCourseReportByIdAsync(reportId);
        if (report == null) throw new KeyNotFoundException("Report not found.");

        if (!report.CourseId.HasValue) throw new BadRequestException("Course ID is missing from the report.");
        var course = await _courseRepo.GetByIdAsync(report.CourseId.Value);
        if (course == null) throw new KeyNotFoundException("Course not found.");

        return (report, course);
    }

    private async Task<(CourseReviewReport, CourseReview)> GetCourseReviewReportAndEntityAsync(int reportId)
    {
        var report = await _reportRepo.GetCourseReviewReportByIdAsync(reportId);
        if (report == null) throw new KeyNotFoundException("Report not found.");

        if (!report.CourseReviewId.HasValue) throw new BadRequestException("Course Review ID is missing from the report.");
        var review = await _reviewRepo.GetCourseReviewByIdAsync(report.CourseReviewId.Value);
        if (review == null) throw new KeyNotFoundException("Course review not found.");

        return (report, review);
    }

    private async Task<(LessonReviewReport, LessonReview)> GetLessonReviewReportAndEntityAsync(int reportId)
    {
        var report = await _reportRepo.GetLessonReviewReportByIdAsync(reportId);
        if (report == null) throw new KeyNotFoundException("Report not found.");

        if (!report.LessonReviewId.HasValue) throw new BadRequestException("Lesson Review ID is missing from the report.");
        var review = await _reviewRepo.GetLessonReviewByIdAsync(report.LessonReviewId.Value);
        if (review == null) throw new KeyNotFoundException("Lesson review not found.");

        return (report, review);
    }

    private async Task ValidateReportResolutionAccessAsync(string currentStatus, int resolverId)
    {
        var account = await _userRepo.GetAccountByIdAsync(resolverId);
        // if (account == null || account.Manager == null || (account.Manager.Role != "admin" && account.Manager.Role != "staff"))
        if (account == null || account.Manager == null )
        {
            throw new UnauthorizedAccessException("Only staff or admin can resolve reports.");
        }

        if (currentStatus == ReportStatus.Escalated.ToValue() && account.Manager.Role != "admin")
        {
            throw new UnauthorizedAccessException("Only admins can resolve escalated reports.");
        }
    }

    private async Task ApplyCoursePenaltyAsync(Course course, bool removeContent, string resolutionNote)
    {
        if (removeContent)
        {
            await _penaltyService.ProcessCourseStrikeAsync(course.CourseId, resolutionNote);
        }
        else if (course.InstructorId.HasValue)
        {
            await _notificationService.SendNotificationAsync(
                course.InstructorId.Value,
                "Course Policy Warning",
                $"Your course '{course.Title}' received a report regarding a policy violation. Please review and ensure your content complies with our guidelines.",
                $"/InstructorCourse/Editor/{course.CourseId}"
            );
        }
    }

    private async Task ApplyCourseReviewPenaltyAsync(CourseReview review, bool removeContent, string resolutionNote)
    {
        string link = review.Enrollment?.CourseId is null or < 1 
            ? "/" 
            : $"/Course/Details/{review.Enrollment.CourseId}#review-card-{review.CourseReviewId}";

        if (removeContent)
        {
            MarkCourseReviewAsViolating(review);
            await ProcessReviewStrikeIfApplicableAsync(review.Enrollment?.UserId, resolutionNote, link);
        }
        else
        {
            await SendReviewWarningIfApplicableAsync(review.Enrollment?.UserId, "course review", resolutionNote, link);
        }
    }

    private void MarkCourseReviewAsViolating(CourseReview review)
    {
        review.IsRemoved = true;
        review.CourseReviewStatus = ReviewStatus.Violating.ToValue();
        review.UpdatedAt = DateTime.Now;
        _reviewRepo.UpdateCourseReview(review);
    }

    private async Task ApplyLessonReviewPenaltyAsync(LessonReview review, bool removeContent, string resolutionNote)
    {
        string link = review.Enrollment?.CourseId is null or < 1 
            ? "/" 
            : $"/Course/Learn?id={review.Enrollment.CourseId}&lessonId={review.LessonId}#review-card-{review.LessonReviewId}";

        if (removeContent)
        {
            MarkLessonReviewAsViolating(review);
            await ProcessReviewStrikeIfApplicableAsync(review.Enrollment?.UserId, resolutionNote, link);
        }
        else
        {
            await SendReviewWarningIfApplicableAsync(review.Enrollment?.UserId, "lesson review", resolutionNote, link);
        }
    }

    private void MarkLessonReviewAsViolating(LessonReview review)
    {
        review.IsRemoved = true;
        review.LessonReviewStatus = ReviewStatus.Violating.ToValue();
        review.UpdatedAt = DateTime.Now;
        _reviewRepo.UpdateLessonReview(review);
    }

    private async Task ProcessReviewStrikeIfApplicableAsync(int? userId, string resolutionNote, string link)
    {
        if (userId is not null and > 0)
        {
            await _penaltyService.ProcessReviewStrikeAsync(userId.Value, resolutionNote, link);
        }
    }

    private async Task SendReviewWarningIfApplicableAsync(int? userId, string reviewType, string resolutionNote, string link)
    {
        if (userId is not null and > 0)
        {
            await _notificationService.SendNotificationAsync(
                userId.Value,
                "Review Policy Warning",
                $"Your {reviewType} has received a policy violation warning. Reason: {resolutionNote}. Please ensure your content complies with our guidelines.",
                link
            );
        }
    }

    private async Task SaveReportChangesAsync()
    {
        int rowsAffected;
        try
        {
            rowsAffected = await _reportRepo.SaveChangesAsync();
        }
        catch (ReportException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        /* zero rows exception removed */
    }

    private async Task NotifyAdminOnEscalationAsync(string status, int reportId, string reportType)
    {
        if (status == ReportStatus.Escalated.ToValue())
        {
            var adminId = await _userRepo.GetAdminIdAsync();
            if (adminId.HasValue)
            {
                await _notificationService.SendNotificationAsync(adminId.Value, "Report Escalated", $"A {reportType} report has been escalated to you. Report ID: {reportId}", $"/AdminModeration/Reports");
            }
        }
    }

    // NOTE: This is a legacy method. Do not test it.
    // public async Task<bool> RemoveCourseAsync(int courseId, int adminId)
    // {
    //     var course = await _courseRepo.GetByIdAsync(courseId);
    //     if (course == null) throw new KeyNotFoundException("Course not found.");

    //     course.IsRemoved = true;
    //     course.CourseStatus = CourseStatus.Archived.ToValue();
    //     course.UpdatedAt = DateTime.Now;

    //     await SaveReportChangesAsync();

    //     await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
    //     return true;
    // }

    // ── Private Helpers ─────────────────────────────────────────────────────

    private async Task<string> ResolveLinkActionAsync(string reportType, int targetId)
    {
        return reportType switch
        {
            "course" => $"/Course/Details/{targetId}",
            "course review" => await BuildCourseReviewLinkAsync(targetId),
            "lesson review" => await BuildLessonReviewLinkAsync(targetId),
            _ => "/"
        };
    }

    private async Task<string> BuildCourseReviewLinkAsync(int targetId)
    {
        var r = await _reviewRepo.GetCourseReviewByIdAsync(targetId);
        if (r!.Enrollment?.CourseId is null or < 1)
        {
            return "/";
        }

        return $"/Course/Details/{r.Enrollment.CourseId}#review-card-{targetId}";
    }

    private async Task<string> BuildLessonReviewLinkAsync(int targetId)
    {
        var r = await _reviewRepo.GetLessonReviewByIdAsync(targetId);
        if (r!.Enrollment?.CourseId is null or < 1)
        {
            return "/";
        }

        return $"/Course/Learn?id={r.Enrollment.CourseId}&lessonId={r.LessonId}#review-card-{targetId}";
    }

    private async Task NotifyReporterOfOutcomeAsync(int? reporterId, string linkAction, string status, string reportType)
    {
        if (!reporterId.HasValue) return;

        string message = $"Your report about the {reportType} has been updated.";
        if (status == ReportStatus.Resolved.ToValue()) message = $"Your report about the {reportType} has been accepted.";
        else if (status == ReportStatus.Rejected.ToValue()) message = $"No issues found on {reportType}, your report has been dismissed.";
        else if (status == ReportStatus.UnderReview.ToValue()) message = $"Your report about the {reportType} is currently under review.";
        else if (status == ReportStatus.Escalated.ToValue()) message = $"Your report about the {reportType} has been escalated to senior administrators.";

        await _notificationService.SendNotificationAsync(
            reporterId.Value,
            "Report Resolution Update",
            message,
            linkAction
        );
    }


    private async Task SendResolutionNotificationsAsync(string status, int reportId, int? reporterId, string reportType, int targetId)
    {
        string linkAction = await ResolveLinkActionAsync(reportType, targetId);
        await NotifyReporterOfOutcomeAsync(reporterId, linkAction, status, reportType);
        await NotifyAdminOnEscalationAsync(status, reportId, reportType);
    }
}
