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

    public ReportSubmissionService(
        IReportRepository reportRepo,
        IEnrollmentRepository enrollmentRepo,
        ICourseRepository courseRepo,
        IReviewRepository reviewRepo,
        IMapper mapper)
    {
        _reportRepo = reportRepo;
        _enrollmentRepo = enrollmentRepo;
        _courseRepo = courseRepo;
        _reviewRepo = reviewRepo;
        _mapper = mapper;
    }

    public async Task<bool> CreateCourseReportAsync(int reporterId, CreateCourseReportRequest request)
    {
        await ValidateCourseEligibilityAsync(reporterId, request.CourseId);

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new InvalidOperationException("Report reason cannot be empty.");

        await CheckDuplicateCourseReportAsync(reporterId, request.CourseId, request.Reason);

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
        int numRowsAffected;
        try
        {
            numRowsAffected = await _reportRepo.SaveChangesAsync();
        }
        catch (ReportException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        if (numRowsAffected == 0)
            throw new InvalidOperationException("Failed to save changes.");
            
        return true;
    }

    public async Task<bool> CreateCourseReviewReportAsync(int reporterId, CreateCourseReviewReportRequest request)
    {
        var review = await _reviewRepo.GetCourseReviewByIdAsync(request.CourseReviewId)
            ?? throw new InvalidOperationException("Review not found.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new InvalidOperationException("Report reason cannot be empty.");

        var existing = await _reportRepo.GetPendingCourseReviewReportAsync(reporterId, request.CourseReviewId, request.Reason);
        if (existing != null)
            throw new InvalidOperationException("You already have a pending report with this reason for this review.");

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
        int numRowsAffected;
        try
        {
            numRowsAffected = await _reportRepo.SaveChangesAsync();
        }
        catch (ReportException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        if (numRowsAffected == 0)
            throw new InvalidOperationException("Failed to save changes.");
            
        return true;
    }

    public async Task<bool> CreateLessonReviewReportAsync(int reporterId, CreateLessonReviewReportRequest request)
    {
        var review = await _reviewRepo.GetLessonReviewByIdAsync(request.LessonReviewId)
            ?? throw new InvalidOperationException("Review not found.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new InvalidOperationException("Report reason cannot be empty.");

        var existing = await _reportRepo.GetPendingLessonReviewReportAsync(reporterId, request.LessonReviewId, request.Reason);
        if (existing != null)
            throw new InvalidOperationException("You already have a pending report with this reason for this review.");

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
        int numRowsAffected;
        try
        {
            numRowsAffected = await _reportRepo.SaveChangesAsync();
        }
        catch (ReportException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        if (numRowsAffected == 0)
            throw new InvalidOperationException("Failed to save changes.");
            
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

    private async Task ValidateCourseEligibilityAsync(int reporterId, int courseId)
    {
        var course = await _courseRepo.GetByIdAsync(courseId)
            ?? throw new InvalidOperationException("Course not found.");

        if (course.CourseStatus.Equals(CourseStatus.Pending.ToValue(), StringComparison.OrdinalIgnoreCase) || 
            course.CourseStatus.Equals("under_review", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("This course is currently under review and cannot be reported.");

        if (course.CourseStatus.Equals(CourseStatus.Archived.ToValue(), StringComparison.OrdinalIgnoreCase) && (course.CourseFlagCount ?? 0) >= 3)
            throw new InvalidOperationException("This course is permanently locked and cannot be reported.");

        if (course.InstructorId == reporterId)
            throw new InvalidOperationException("You cannot report your own course.");

        var enrollment = await _enrollmentRepo.GetEnrollmentWithProgressAsync(reporterId, courseId);
        if (enrollment == null)
            throw new InvalidOperationException("You must be enrolled in the course before reporting it.");
    }

    private async Task CheckDuplicateCourseReportAsync(int reporterId, int courseId, string reason)
    {
        var existing = await _reportRepo.GetPendingCourseReportAsync(reporterId, courseId, reason);
        if (existing != null)
            throw new InvalidOperationException("You already have a pending report with this reason for this course.");
    }
}
