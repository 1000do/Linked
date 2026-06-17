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

    public async Task<PagedResult<CourseReportDetailResponse>> GetAllCourseReportsAsync(string? status = null, int page = 1, int pageSize = 10)
    {
        var (reports, totalCount) = await _reportRepo.GetAllCourseReportsAsync(status, page, pageSize);
        if (totalCount == 0) throw new KeyNotFoundException("No reports found.");
        var items = _mapper.Map<IEnumerable<CourseReportDetailResponse>>(reports).ToList();
        return new PagedResult<CourseReportDetailResponse>(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<ReviewReportDetailResponse>> GetAllCourseReviewReportsAsync(string? status = null, int page = 1, int pageSize = 10)
    {
        var (reports, totalCount) = await _reportRepo.GetAllCourseReviewReportsAsync(status, page, pageSize);
        if (totalCount == 0) throw new KeyNotFoundException("No course review reports found.");
        var items = _mapper.Map<IEnumerable<ReviewReportDetailResponse>>(reports).ToList();
        return new PagedResult<ReviewReportDetailResponse>(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<ReviewReportDetailResponse>> GetAllLessonReviewReportsAsync(string? status = null, int page = 1, int pageSize = 10)
    {
        var (reports, totalCount) = await _reportRepo.GetAllLessonReviewReportsAsync(status, page, pageSize);
        if (totalCount == 0) throw new KeyNotFoundException("No lesson review reports found.");
        var items = _mapper.Map<IEnumerable<ReviewReportDetailResponse>>(reports).ToList();
        return new PagedResult<ReviewReportDetailResponse>(items, totalCount, page, pageSize);
    }

    public async Task<ReportStatsResponse> GetReportStatsAsync()
    {
        var (pendingCourse, _) = await _reportRepo.GetAllCourseReportsAsync("pending", 1, 100000);
        var (pendingCourseReview, _) = await _reportRepo.GetAllCourseReviewReportsAsync("pending", 1, 100000);
        var (pendingLessonReview, _) = await _reportRepo.GetAllLessonReviewReportsAsync("pending", 1, 100000);

        var today = DateTime.Today;

        var (allCourse, _) = await _reportRepo.GetAllCourseReportsAsync(null, 1, 100000);
        var (allCourseReview, _) = await _reportRepo.GetAllCourseReviewReportsAsync(null, 1, 100000);
        var (allLessonReview, _) = await _reportRepo.GetAllLessonReviewReportsAsync(null, 1, 100000);

        int resolvedToday = allCourse.Count(r => r.ResolvedAt?.Date == today && r.CourseReportsStatus == ReportStatus.Resolved.ToValue())
                          + allCourseReview.Count(r => r.ResolvedAt?.Date == today && r.UserReportsStatus == ReportStatus.Resolved.ToValue())
                          + allLessonReview.Count(r => r.ResolvedAt?.Date == today && r.UserReportsStatus == ReportStatus.Resolved.ToValue());

        int rejectedToday = allCourse.Count(r => r.ResolvedAt?.Date == today && r.CourseReportsStatus == ReportStatus.Rejected.ToValue())
                          + allCourseReview.Count(r => r.ResolvedAt?.Date == today && r.UserReportsStatus == ReportStatus.Rejected.ToValue())
                          + allLessonReview.Count(r => r.ResolvedAt?.Date == today && r.UserReportsStatus == ReportStatus.Rejected.ToValue());

        return new ReportStatsResponse
        {
            TotalPendingCourseReports = pendingCourse.Count,
            TotalPendingCourseReviewReports = pendingCourseReview.Count,
            TotalPendingLessonReviewReports = pendingLessonReview.Count,
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
            await ApplyCoursePenaltyAsync(course, request.RemoveContent, request.ResolutionNote!);
        }

        _reportRepo.UpdateCourseReport(report);
        await SaveReportChangesAsync();

        if (request.Status == ReportStatus.Resolved.ToValue() && request.RemoveContent)
        {
            await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(course.CourseId));
        }

        string linkAction = await ResolveLinkActionAsync("course", report.CourseId);
        await NotifyReporterOfOutcomeAsync(report.ReporterId, linkAction, request.Status, "course");
        await NotifyAdminOnEscalationAsync(request.Status, reportId, "course");

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
            await ApplyCourseReviewPenaltyAsync(review, request.RemoveContent, request.ResolutionNote!);
        }

        _reportRepo.UpdateCourseReviewReport(report);
        await SaveReportChangesAsync();

        string linkAction = await ResolveLinkActionAsync("course review", report.CourseReviewId);
        await NotifyReporterOfOutcomeAsync(report.ReporterId, linkAction, request.Status, "course review");
        await NotifyAdminOnEscalationAsync(request.Status, reportId, "course review");

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
            await ApplyLessonReviewPenaltyAsync(review, request.RemoveContent, request.ResolutionNote!);
        }

        _reportRepo.UpdateLessonReviewReport(report);
        await SaveReportChangesAsync();

        string linkAction = await ResolveLinkActionAsync("lesson review", report.LessonReviewId);
        await NotifyReporterOfOutcomeAsync(report.ReporterId, linkAction, request.Status, "lesson review");
        await NotifyAdminOnEscalationAsync(request.Status, reportId, "lesson review");

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
        var resolverRole = await _userRepo.GetRoleByAccountIdAsync(resolverId);
        if (resolverRole != "admin" && resolverRole != "staff")
        {
            throw new UnauthorizedAccessException("Only staff or admin can resolve reports.");
        }

        if (currentStatus == ReportStatus.Escalated.ToValue())
        {
            if (resolverRole != "admin")
            {
                throw new UnauthorizedAccessException("Only admins can resolve escalated reports.");
            }
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
        string link = $"/Course/Details/{review.Enrollment?.CourseId}#review-card-{review.CourseReviewId}";
        if (removeContent)
        {
            review.IsRemoved = true;
            review.CourseReviewStatus = ReviewStatus.Violating.ToValue();
            review.UpdatedAt = DateTime.Now;
            _reviewRepo.UpdateCourseReview(review);
            
            if (review.Enrollment != null && review.Enrollment.UserId != 0)
            {
                await _penaltyService.ProcessReviewStrikeAsync((int)review.Enrollment.UserId, resolutionNote, link);
            }
        }
        else
        {
            await _notificationService.SendNotificationAsync(
                review.Enrollment?.UserId ?? 0,
                "Review Policy Warning",
                $"Your course review has received a policy violation warning. Reason: {resolutionNote}. Please ensure your content complies with our guidelines.",
                link
            );
        }
    }

    private async Task ApplyLessonReviewPenaltyAsync(LessonReview review, bool removeContent, string resolutionNote)
    {
        string link = $"/Course/Learn?id={review.Enrollment?.CourseId}&lessonId={review.LessonId}#review-card-{review.LessonReviewId}";
        if (removeContent)
        {
            review.IsRemoved = true;
            review.LessonReviewStatus = ReviewStatus.Violating.ToValue();
            review.UpdatedAt = DateTime.Now;
            _reviewRepo.UpdateLessonReview(review);

            if (review.Enrollment != null && review.Enrollment.UserId != 0)
            {
                await _penaltyService.ProcessReviewStrikeAsync((int)review.Enrollment.UserId, resolutionNote, link);
            }
        }
        else
        {
            await _notificationService.SendNotificationAsync(
                review.Enrollment?.UserId ?? 0,
                "Review Policy Warning",
                $"Your lesson review has received a policy violation warning. Reason: {resolutionNote}. Please ensure your content complies with our guidelines.",
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

        if (rowsAffected == 0)
            throw new InvalidOperationException("Failed to save changes.");
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

    public async Task<bool> RemoveCourseAsync(int courseId, int adminId)
    {
        var course = await _courseRepo.GetByIdAsync(courseId);
        if (course == null) throw new KeyNotFoundException("Course not found.");

        course.IsRemoved = true;
        course.CourseStatus = CourseStatus.Archived.ToValue();
        course.UpdatedAt = DateTime.Now;

        int rowsAffected;
        try
        {
            rowsAffected = await _reportRepo.SaveChangesAsync();
        }
        catch (ReportException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        if (rowsAffected == 0)
            throw new InvalidOperationException("Failed to save changes.");
        
        await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
        return true;
    }

    // ── Private Helpers ─────────────────────────────────────────────────────

    private async Task<string> ResolveLinkActionAsync(string reportType, int? targetId)
    {
        if (!targetId.HasValue) return "/";

        if (reportType == "course")
        {
            return $"/Course/Details/{targetId.Value}";
        }
        else if (reportType == "course review")
        {
            var r = await _reviewRepo.GetCourseReviewByIdAsync(targetId.Value);
            return r != null ? $"/Course/Details/{r.Enrollment?.CourseId}#review-card-{targetId.Value}" : "/";
        }
        else if (reportType == "lesson review")
        {
            var r = await _reviewRepo.GetLessonReviewByIdAsync(targetId.Value);
            return r != null ? $"/Course/Learn?id={r.Enrollment?.CourseId}&lessonId={r.LessonId}#review-card-{targetId.Value}" : "/";
        }

        return "/";
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
}
