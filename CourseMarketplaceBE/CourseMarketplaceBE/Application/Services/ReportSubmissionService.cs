using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services;

public class ReportSubmissionService : IReportSubmissionService
{
    private readonly IReportRepository _reportRepo;
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly ICourseRepository _courseRepo;
    private readonly IReviewRepository _reviewRepo;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepo;

    public ReportSubmissionService(
        IReportRepository reportRepo,
        IEnrollmentRepository enrollmentRepo,
        ICourseRepository courseRepo,
        IReviewRepository reviewRepo,
        IMapper mapper,
        INotificationService notificationService,
        IUserRepository userRepo)
    {
        _reportRepo = reportRepo;
        _enrollmentRepo = enrollmentRepo;
        _courseRepo = courseRepo;
        _reviewRepo = reviewRepo;
        _mapper = mapper;
        _notificationService = notificationService;
        _userRepo = userRepo;
    }

    public async Task<bool> CreateCourseReportAsync(int reporterId, CreateCourseReportRequest request)
    {
        await ValidateCourseReportAsync(reporterId, request.CourseId, request.Reason);

        var report = new CourseReport
        {
            ReporterId = reporterId,
            CourseId = request.CourseId,
            Reason = request.Reason,
            Description = request.Description,
            CourseReportsStatus = ReportStatus.Pending.ToValue(),
            CreatedAt = DateTime.Now
        };

        await _reportRepo.AddCourseReportAsync(report);
        await SaveChangesAndHandleExceptionsAsync();
        
        await NotifyManagersAsync(
            "New Course Report",
            $"A new report has been submitted for Course #{request.CourseId}.",
            "/AdminModeration/Reports?tab=course-reports");
            
        return true;
    }

    public async Task<bool> CreateCourseReviewReportAsync(int reporterId, CreateCourseReviewReportRequest request)
    {
        await ValidateCourseReviewReportAsync(reporterId, request.CourseReviewId, request.Reason);

        var report = new CourseReviewReport
        {
            ReporterId = reporterId,
            CourseReviewId = request.CourseReviewId,
            Reason = request.Reason,
            Description = request.Description,
            UserReportsStatus = ReportStatus.Pending.ToValue(),
            CreatedAt = DateTime.Now
        };

        await _reportRepo.AddCourseReviewReportAsync(report);
        await SaveChangesAndHandleExceptionsAsync();

        await NotifyManagersAsync(
            "New Course Review Report",
            $"A course review has been reported.",
            "/AdminModeration/Reports?tab=review-reports&subtab=course");
            
        return true;
    }

    public async Task<bool> CreateLessonReviewReportAsync(int reporterId, CreateLessonReviewReportRequest request)
    {
        await ValidateLessonReviewReportAsync(reporterId, request.LessonReviewId, request.Reason);

        var report = new LessonReviewReport
        {
            ReporterId = reporterId,
            LessonReviewId = request.LessonReviewId,
            Reason = request.Reason,
            Description = request.Description,
            UserReportsStatus = ReportStatus.Pending.ToValue(),
            CreatedAt = DateTime.Now
        };

        await _reportRepo.AddLessonReviewReportAsync(report);
        await SaveChangesAndHandleExceptionsAsync();

        await NotifyManagersAsync(
            "New Lesson Review Report",
            $"A lesson review has been reported.",
            "/AdminModeration/Reports?tab=review-reports&subtab=lesson");
            
        return true;
    }

    public async Task<IEnumerable<MyCourseReportResponse>> GetMyCourseReportsAsync(int reporterId)
    {
        var reports = await _reportRepo.GetCourseReportsByReporterAsync(reporterId);
        return _mapper.Map<IEnumerable<MyCourseReportResponse>>(reports);
    }

    public async Task<IEnumerable<MyReviewReportResponse>> GetMyReviewReportsAsync(int reporterId)
    {
        var courseReviewReports = await _reportRepo.GetCourseReviewReportsByReporterAsync(reporterId);
        var lessonReviewReports = await _reportRepo.GetLessonReviewReportsByReporterAsync(reporterId);

        var courseResults = _mapper.Map<IEnumerable<MyReviewReportResponse>>(courseReviewReports);
        var lessonResults = _mapper.Map<IEnumerable<MyReviewReportResponse>>(lessonReviewReports);

        return courseResults.Concat(lessonResults).OrderByDescending(r => r.CreatedAt);
    }

    public async Task<IEnumerable<CourseReportDetailResponse>> GetReportsOnMyCourseAsync(int instructorId, int courseId)
    {
        var isOwner = await _courseRepo.IsOwnerAsync(instructorId, courseId);
        if (!isOwner)
            throw new UnauthorizedAccessException("You do not have permission to view reports for this course.");

        var reports = await _reportRepo.GetCourseReportsByCourseAsync(courseId);
        return _mapper.Map<IEnumerable<CourseReportDetailResponse>>(reports);
    }

    // ── Private helpers ─────────────────────────────────────────────────────

    private async Task ValidateCourseReportAsync(int reporterId, int courseId, string reason)
    {
        var role = await _userRepo.GetRoleByAccountIdAsync(reporterId);
        if (role == "manager")
            throw new BadRequestException("Managers cannot report courses.");

        var course = await _courseRepo.GetByIdAsync(courseId)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.CourseStatus.Equals(CourseStatus.Draft.ToValue(), StringComparison.OrdinalIgnoreCase) || 
            course.CourseStatus.Equals(CourseStatus.Pending.ToValue(), StringComparison.OrdinalIgnoreCase) || 
            course.CourseStatus.Equals("under_review", StringComparison.OrdinalIgnoreCase) ||
            course.CourseStatus.Equals(CourseStatus.Rejected.ToValue(), StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("This course is currently in draft, pending, under review, or rejected and cannot be reported.");

        if (course.CourseStatus.Equals(CourseStatus.Archived.ToValue(), StringComparison.OrdinalIgnoreCase) && (course.CourseFlagCount ?? 0) >= 3)
            throw new BadRequestException("This course is permanently locked and cannot be reported.");

        if (course.InstructorId == reporterId)
            throw new BadRequestException("You cannot report your own course.");

        var enrollment = await _enrollmentRepo.GetEnrollmentWithProgressAsync(reporterId, courseId);
        if (enrollment == null)
            throw new BadRequestException("You must be enrolled in the course before reporting it.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new BadRequestException("Report reason cannot be empty.");

        var existing = await _reportRepo.GetPendingCourseReportAsync(reporterId, courseId, reason);
        if (existing != null)
            throw new BadRequestException("You already have a pending report with this reason for this course.");
    }

    private async Task ValidateCourseReviewReportAsync(int reporterId, int courseReviewId, string reason)
    {
        var review = await _reviewRepo.GetCourseReviewByIdAsync(courseReviewId)
            ?? throw new KeyNotFoundException("Review not found.");

        if (review.IsRemoved == true || 
            review.CourseReviewStatus.Equals(ReviewStatus.Removed.ToValue(), StringComparison.OrdinalIgnoreCase) || 
            review.CourseReviewStatus.Equals(ReviewStatus.Violating.ToValue(), StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("This review has been removed and cannot be reported.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new BadRequestException("Report reason cannot be empty.");

        var existing = await _reportRepo.GetPendingCourseReviewReportAsync(reporterId, courseReviewId, reason);
        if (existing != null)
            throw new BadRequestException("You already have a pending report with this reason for this review.");
    }

    private async Task ValidateLessonReviewReportAsync(int reporterId, int lessonReviewId, string reason)
    {
        var review = await _reviewRepo.GetLessonReviewByIdAsync(lessonReviewId)
            ?? throw new KeyNotFoundException("Review not found.");

        if (review.IsRemoved == true || 
            review.LessonReviewStatus.Equals(ReviewStatus.Removed.ToValue(), StringComparison.OrdinalIgnoreCase) || 
            review.LessonReviewStatus.Equals(ReviewStatus.Violating.ToValue(), StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("This review has been removed and cannot be reported.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new BadRequestException("Report reason cannot be empty.");

        var existing = await _reportRepo.GetPendingLessonReviewReportAsync(reporterId, lessonReviewId, reason);
        if (existing != null)
            throw new BadRequestException("You already have a pending report with this reason for this review.");
    }

    private async Task SaveChangesAndHandleExceptionsAsync()
    {
        try
        {
            await _reportRepo.SaveChangesAsync();
        }
        catch (ReportException ex)
        {
            throw new BadRequestException(ex.Message);
        }
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
