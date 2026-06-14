using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Domain.Constants;
using Microsoft.AspNetCore.SignalR;
using CourseMarketplaceBE.Hubs;

namespace CourseMarketplaceBE.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepo;
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly ICourseRepository _courseRepo;
    private readonly IReviewRepository _reviewRepo;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepo;
    private readonly IInstructorRepository _instructorRepo;
    private readonly IMapper _mapper;
    private readonly ILockoutRepository _lockoutRepo;
    private readonly IRedisService _redisService;
    private readonly IHubContext<NotificationHub> _hubContext;

    public ReportService(
        IReportRepository reportRepo,
        IEnrollmentRepository enrollmentRepo,
        ICourseRepository courseRepo,
        IReviewRepository reviewRepo,
        INotificationService notificationService,
        IUserRepository userRepo,
        IInstructorRepository instructorRepo,
        IMapper mapper,
        ILockoutRepository lockoutRepo,
        IRedisService redisService,
        IHubContext<NotificationHub> hubContext)
    {
        _reportRepo = reportRepo;
        _enrollmentRepo = enrollmentRepo;
        _courseRepo = courseRepo;
        _reviewRepo = reviewRepo;
        _notificationService = notificationService;
        _userRepo = userRepo;
        _instructorRepo = instructorRepo;
        _mapper = mapper;
        _lockoutRepo = lockoutRepo;
        _redisService = redisService;
        _hubContext = hubContext;
    }

    // ── User / Instructor: Tạo report khóa học ─────────────────────────────

    public async Task<bool> CreateCourseReportAsync(int reporterId, CreateCourseReportRequest request, bool isInstructor)
    {
        var course = await _courseRepo.GetByIdAsync(request.CourseId)
            ?? throw new InvalidOperationException("Course not found.");

        if (course.CourseStatus.Equals(CourseStatus.Pending.ToValue(), StringComparison.OrdinalIgnoreCase) || 
            course.CourseStatus.Equals("under_review", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("This course is currently under review and cannot be reported.");

        if (course.CourseStatus.Equals(CourseStatus.Archived.ToValue(), StringComparison.OrdinalIgnoreCase) && (course.CourseFlagCount ?? 0) >= 3)
            throw new InvalidOperationException("This course is permanently locked and cannot be reported.");

        // Cannot report your own course
        if (course.InstructorId == reporterId)
            throw new InvalidOperationException("You cannot report your own course.");

        if (!isInstructor)
        {
            // Regular user: must be enrolled
            var enrollment = await _enrollmentRepo.GetEnrollmentWithProgressAsync(reporterId, request.CourseId);
            if (enrollment == null)
                throw new InvalidOperationException("You must be enrolled in the course before reporting it.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new InvalidOperationException("Report reason cannot be empty.");

        // Kiểm tra duplicate pending report cùng lý do
        var existing = await _reportRepo.GetPendingCourseReportAsync(reporterId, request.CourseId, request.Reason);
        if (existing != null)
            throw new InvalidOperationException("You already have a pending report with this reason for this course.");

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
        int numRowsAffected = await _reportRepo.SaveChangesAsync();
        if (numRowsAffected == 0)
            throw new InvalidOperationException("Failed to save changes");
            
        return true;
    }

    // ── User / Instructor: Tạo report review khóa học ──────────────────────

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
        int numRowsAffected = await _reportRepo.SaveChangesAsync();
        if (numRowsAffected == 0)
            throw new InvalidOperationException("Failed to save changes");
            
        return true;
    }

    // ── User / Instructor: Tạo report review bài học ───────────────────────

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
        int numRowsAffected = await _reportRepo.SaveChangesAsync();
        if (numRowsAffected == 0)
            throw new InvalidOperationException("Failed to save changes");
            
        return true;
    }

    // ── User / Instructor: Xem lịch sử report của mình ─────────────────────

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

    // ── Instructor: Xem reports nhận được trên khóa học của mình ───────────

    public async Task<IEnumerable<CourseReportDetailResponse>> GetReportsOnMyCourseAsync(int instructorId, int courseId)
    {
        // Xác minh instructor sở hữu course này
        var isOwner = await _courseRepo.IsOwnerAsync(instructorId, courseId);
        if (!isOwner)
            throw new UnauthorizedAccessException("You do not have permission to view reports for this course.");

        var reports = await _reportRepo.GetCourseReportsByCourseAsync(courseId);
        return _mapper.Map<IEnumerable<CourseReportDetailResponse>>(reports);
    }

    // ── Staff / Admin: Xem tất cả reports ──────────────────────────────────

    public async Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseReportDetailResponse>> GetAllCourseReportsAsync(string? status = null, int page = 1, int pageSize = 10)
    {
        var (reports, totalCount) = await _reportRepo.GetAllCourseReportsAsync(status, page, pageSize);
        var items = _mapper.Map<IEnumerable<CourseReportDetailResponse>>(reports).ToList();
        return new CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseReportDetailResponse>(items, totalCount, page, pageSize);
    }

    public async Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<ReviewReportDetailResponse>> GetAllCourseReviewReportsAsync(string? status = null, int page = 1, int pageSize = 10)
    {
        var (reports, totalCount) = await _reportRepo.GetAllCourseReviewReportsAsync(status, page, pageSize);
        var items = _mapper.Map<IEnumerable<ReviewReportDetailResponse>>(reports).ToList();
        return new CourseMarketplaceBE.Application.DTOs.Common.PagedResult<ReviewReportDetailResponse>(items, totalCount, page, pageSize);
    }

    public async Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<ReviewReportDetailResponse>> GetAllLessonReviewReportsAsync(string? status = null, int page = 1, int pageSize = 10)
    {
        var (reports, totalCount) = await _reportRepo.GetAllLessonReviewReportsAsync(status, page, pageSize);
        var items = _mapper.Map<IEnumerable<ReviewReportDetailResponse>>(reports).ToList();
        return new CourseMarketplaceBE.Application.DTOs.Common.PagedResult<ReviewReportDetailResponse>(items, totalCount, page, pageSize);
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

    // ── Staff / Admin: Xử lý course report ─────────────────────────────────

    public async Task<bool> ResolveCourseReportAsync(int reportId, int resolverId, ResolveReportRequest request)
    {
        var report = await _reportRepo.GetCourseReportByIdAsync(reportId);
        if (report == null) return false;

        // Chặn Staff giải quyết báo cáo đã bị escalated
        if (report.CourseReportsStatus == ReportStatus.Escalated.ToValue())
        {
            var resolverRole = await _userRepo.GetRoleByAccountIdAsync(resolverId);
            if (resolverRole != "admin")
            {
                throw new UnauthorizedAccessException("Only admins can resolve escalated reports.");
            }
        }

        report.CourseReportsStatus = request.Status;
        report.ResolutionNote = request.ResolutionNote;
        report.ResolverId = resolverId;
        report.ResolvedAt = DateTime.Now;

        Course? course = null;
        if (report.CourseId.HasValue)
        {
            course = await _courseRepo.GetByIdAsync(report.CourseId.Value);
        }

        if (course != null)
        {
            if (request.Status == ReportStatus.Resolved.ToValue())
            {
                if (request.RemoveContent)
                {
                    var currentFlags = (course.CourseFlagCount ?? 0) + 1;
                    course.CourseFlagCount = currentFlags;

                    if (currentFlags < 3)
                    {
                        if (course.InstructorId.HasValue)
                        {
                            await _notificationService.SendNotificationAsync(
                                course.InstructorId.Value,
                                "Course Violation Warning",
                                $"Your course '{course.Title}' has received a policy violation warning. This is Strike {currentFlags}. Reason: {request.ResolutionNote}. Please review and correct the content.",
                                $"/InstructorCourse/Editor/{course.CourseId}"
                            );
                        }
                    }
                    else if (currentFlags >= 3)
                    {
                        course.CourseStatus = CourseStatus.Archived.ToValue();
                        if (course.InstructorId.HasValue)
                        {
                            var instructor = await _instructorRepo.GetByIdAsync(course.InstructorId.Value);
                            if (instructor != null)
                            {
                                await _lockoutRepo.AddAsync(new Lockout
                                {
                                    AccountId = instructor.InstructorId,
                                    LockoutType = "instructor",
                                    LockoutLevel = "severe",
                                    LockoutEnd = DateTime.Now.AddDays(30)
                                });
                                await NotifyStudentsAboutInstructorSuspensionAsync(course.InstructorId.Value);
                            }
                            
                            await _notificationService.SendNotificationAsync(
                                course.InstructorId.Value,
                                "Permanent Course Discontinuation Notice",
                                $"Your course '{course.Title}' has violated our policies. It has been permanently discontinued and archived (Strike {currentFlags}). New enrollments are disabled. Furthermore, your instructor rights are locked for 30 days (you cannot create, update, or delete courses, lessons, and materials).",
                                $"/Course/Details/{course.CourseId}"
                            );
                        }
                    }
                }
                else
                {
                    // Accept report without removing content
                    if (course.InstructorId.HasValue)
                    {
                        await _notificationService.SendNotificationAsync(
                            course.InstructorId.Value,
                            "Course Policy Warning",
                            $"Your course '{course.Title}' received a report regarding a policy violation. Please review and ensure your content complies with our guidelines.",
                            $"/InstructorCourse/Editor/{course.CourseId}"
                        );
                    }
                }
            }
        }

        _reportRepo.UpdateCourseReport(report);
        await _reportRepo.SaveChangesAsync();

        if (course != null && request.Status == ReportStatus.Resolved.ToValue() && request.RemoveContent)
        {
            await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(course.CourseId));
        }

        // Notify reporter of outcome
        if (report.ReporterId.HasValue)
        {
            string message = "Your report has been updated.";
            if (request.Status == ReportStatus.Resolved.ToValue()) message = "Your report has been accepted and appropriate action has been taken.";
            else if (request.Status == ReportStatus.Rejected.ToValue()) message = "No issues found on course, your report has been dismissed.";
            else if (request.Status == ReportStatus.UnderReview.ToValue()) message = "Your report is currently under review.";
            else if (request.Status == ReportStatus.Escalated.ToValue()) message = "Your report has been escalated to senior administrators.";

            await _notificationService.SendNotificationAsync(
                report.ReporterId.Value,
                "Report Resolution Update",
                message,
                $"/Course/Details/{report.CourseId}"
            );
        }

        if (request.Status == ReportStatus.Escalated.ToValue())
        {
            var adminId = await _userRepo.GetAdminIdAsync();
            if (adminId.HasValue) await _notificationService.SendNotificationAsync(adminId.Value, "Report Escalated", $"A course report has been escalated to you. Report ID: {reportId}", $"/AdminModeration/Reports");
        }

        return true;
    }

    // ── Staff / Admin: Xử lý course review report ──────────────────────────

    public async Task<bool> ResolveCourseReviewReportAsync(int reportId, int resolverId, ResolveReportRequest request)
    {
        var report = await _reportRepo.GetCourseReviewReportByIdAsync(reportId);
        if (report == null) return false;

        bool hasSaved = false;

        report.UserReportsStatus = request.Status;
        report.ResolutionNote = request.ResolutionNote;
        report.ResolverId = resolverId;
        report.ResolvedAt = DateTime.Now;

        // Nếu approve: soft-remove review và gán trạng thái vi phạm (giữ nguyên nội dung comment gốc)
        if (request.Status == ReportStatus.Resolved.ToValue() && report.CourseReviewId.HasValue)
        {
            var review = await _reviewRepo.GetCourseReviewByIdAsync(report.CourseReviewId.Value);
            if (review != null)
            {
                if (request.RemoveContent)
                {
                    review.IsRemoved = true;
                    review.CourseReviewStatus = ReviewStatus.Violating.ToValue();
                    review.UpdatedAt = DateTime.Now;
                    _reviewRepo.UpdateCourseReview(review);
                    
                    hasSaved = await HandleReviewRemovalPenaltyAsync(review.Enrollment, request.ResolutionNote, $"/Course/Details/{review.Enrollment?.CourseId}#review-card-{review.CourseReviewId}");
                }
                else
                {
                    await _notificationService.SendNotificationAsync(
                        review.Enrollment?.UserId ?? 0,
                        "Review Policy Warning",
                        $"Your course review has received a policy violation warning. Reason: {request.ResolutionNote}. Please ensure your content complies with our guidelines.",
                        $"/Course/Details/{review.Enrollment?.CourseId}#review-card-{review.CourseReviewId}"
                    );
                }
            }
        }

        _reportRepo.UpdateCourseReviewReport(report);
        await _reportRepo.SaveChangesAsync();

        // Notify reporter of outcome
        if (report.ReporterId.HasValue)
        {
            string message = "Your report about the course review has been updated.";
            if (request.Status == ReportStatus.Resolved.ToValue()) message = "Your report about the course review has been accepted.";
            else if (request.Status == ReportStatus.Rejected.ToValue()) message = "No issues found on course review, your report has been dismissed.";
            else if (request.Status == ReportStatus.UnderReview.ToValue()) message = "Your report about the course review is currently under review.";
            else if (request.Status == ReportStatus.Escalated.ToValue()) message = "Your report about the course review has been escalated to senior administrators.";

            string? linkAction = null;
            if (report.CourseReviewId.HasValue)
            {
                var r = await _reviewRepo.GetCourseReviewByIdAsync(report.CourseReviewId.Value);
                if (r != null)
                {
                    linkAction = $"/Course/Details/{r.Enrollment?.CourseId}#review-card-{r.CourseReviewId}";
                }
            }

            await _notificationService.SendNotificationAsync(
                report.ReporterId.Value,
                "Report Resolution Update",
                message,
                linkAction!
            );
        }

        if (request.Status == ReportStatus.Escalated.ToValue())
        {
            var adminId = await _userRepo.GetAdminIdAsync();
            if (adminId.HasValue) await _notificationService.SendNotificationAsync(adminId.Value, "Report Escalated", $"A course review report has been escalated to you. Report ID: {reportId}", $"/AdminModeration/Reports");
        }

        return true;
    }

    // ── Staff / Admin: Xử lý lesson review report ──────────────────────────

    public async Task<bool> ResolveLessonReviewReportAsync(int reportId, int resolverId, ResolveReportRequest request)
    {
        var report = await _reportRepo.GetLessonReviewReportByIdAsync(reportId);
        if (report == null) return false;

        bool hasSaved = false;

        report.UserReportsStatus = request.Status;
        report.ResolutionNote = request.ResolutionNote;
        report.ResolverId = resolverId;
        report.ResolvedAt = DateTime.Now;

        // Nếu approve: soft-remove review và gán trạng thái vi phạm (giữ nguyên nội dung comment gốc)
        if (request.Status == ReportStatus.Resolved.ToValue() && report.LessonReviewId.HasValue)
        {
            var review = await _reviewRepo.GetLessonReviewByIdAsync(report.LessonReviewId.Value);
            if (review != null)
            {
                if (request.RemoveContent)
                {
                    review.IsRemoved = true;
                    review.LessonReviewStatus = ReviewStatus.Violating.ToValue();
                    review.UpdatedAt = DateTime.Now;
                    _reviewRepo.UpdateLessonReview(review);

                    hasSaved = await HandleReviewRemovalPenaltyAsync(review.Enrollment, request.ResolutionNote, $"/Course/Learn?id={review.Enrollment?.CourseId}&lessonId={review.LessonId}#review-card-{review.LessonReviewId}");
                }
                else
                {
                    await _notificationService.SendNotificationAsync(
                        review.Enrollment?.UserId ?? 0,
                        "Review Policy Warning",
                        $"Your lesson review has received a policy violation warning. Reason: {request.ResolutionNote}. Please ensure your content complies with our guidelines.",
                        $"/Course/Learn?id={review.Enrollment?.CourseId}&lessonId={review.LessonId}#review-card-{review.LessonReviewId}"
                    );
                }
            }
        }

        _reportRepo.UpdateLessonReviewReport(report);
        await _reportRepo.SaveChangesAsync();

        // Notify reporter of outcome
        if (report.ReporterId.HasValue)
        {
            string message = "Your report about the lesson review has been updated.";
            if (request.Status == ReportStatus.Resolved.ToValue()) message = "Your report about the lesson review has been accepted.";
            else if (request.Status == ReportStatus.Rejected.ToValue()) message = "No issues found on lesson review, your report has been dismissed.";
            else if (request.Status == ReportStatus.UnderReview.ToValue()) message = "Your report about the lesson review is currently under review.";
            else if (request.Status == ReportStatus.Escalated.ToValue()) message = "Your report about the lesson review has been escalated to senior administrators.";

            string? linkAction = null;
            if (report.LessonReviewId.HasValue)
            {
                var r = await _reviewRepo.GetLessonReviewByIdAsync(report.LessonReviewId.Value);
                if (r != null)
                {
                    linkAction = $"/Course/Learn?id={r.Enrollment?.CourseId}&lessonId={r.LessonId}#review-card-{r.LessonReviewId}";
                }
            }

            await _notificationService.SendNotificationAsync(
                report.ReporterId.Value,
                "Report Resolution Update",
                message,
                linkAction!
            );
        }

        if (request.Status == ReportStatus.Escalated.ToValue())
        {
            var adminId = await _userRepo.GetAdminIdAsync();
            if (adminId.HasValue) await _notificationService.SendNotificationAsync(adminId.Value, "Report Escalated", $"A lesson review report has been escalated to you. Report ID: {reportId}", $"/AdminModeration/Reports");
        }

        return true;
    }

    // ── Admin: Gỡ khóa học khỏi nền tảng (xóa mềm/khóa) ─────────────────────
    public async Task<bool> RemoveCourseAsync(int courseId, int adminId)
    {
        var course = await _courseRepo.GetByIdAsync(courseId);
        if (course == null) return false;

        // Gỡ khóa học khỏi nền tảng (xóa mềm/khóa)
        course.IsRemoved = true;
        course.CourseStatus = CourseStatus.Archived.ToValue();
        course.UpdatedAt = DateTime.Now;

        await _reportRepo.SaveChangesAsync();
        
        await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
        return true;
    }


    // ── Private helpers ─────────────────────────────────────────────────────

    private async Task NotifyStudentsAboutInstructorSuspensionAsync(int instructorId)
    {
        var courses = await _courseRepo.GetInstructorCoursesAsync(instructorId);
        var notifiedUserIds = new HashSet<int>();
        foreach (var c in courses)
        {
            var studentIds = await _enrollmentRepo.GetEnrolledUserIdsAsync(c.CourseId);
            foreach (var sId in studentIds)
            {
                if (notifiedUserIds.Add(sId))
                {
                    await _notificationService.SendNotificationAsync(
                        sId,
                        "Instructor Temporarily Suspended",
                        "This instructor has been temporarily suspended for 30 days. During this period, their courses will not receive new updates and you will not be able to contact them. We apologize for any inconvenience.",
                        null!
                    );
                }
            }
        }
    }

    private async Task<bool> ProcessReviewStrikeAsync(int userId, string resolutionNote, string? linkAction)
    {
        var account = await _userRepo.GetAccountByIdAsync(userId);
        if (account == null) return false;

        account.AccountFlagCount = (account.AccountFlagCount ?? 0) + 1;

        if (account.AccountFlagCount == 1)
        {
            await _notificationService.SendNotificationAsync(userId, "Community Standards Violation (1st Warning)", "Your comment has been removed for violating community standards. This is your first warning.", linkAction!);
        }
        else if (account.AccountFlagCount == 2)
        {
            await _lockoutRepo.AddAsync(new Lockout
            {
                AccountId = userId,
                LockoutType = "review",
                LockoutLevel = "moderate",
                LockoutEnd = DateTime.Now.AddDays(7)
            });
            await _notificationService.SendNotificationAsync(userId, "Commenting Restricted (2nd Violation)", "Due to repeated violations, you are restricted from posting comments or reviews for 7 days.", linkAction!);
        }
        else if (account.AccountFlagCount >= 3)
        {
            await _lockoutRepo.AddAsync(new Lockout
            {
                AccountId = userId,
                LockoutType = "account",
                LockoutLevel = "severe",
                LockoutEnd = DateTime.Now.AddDays(30)
            });
            account.AccountStatus = AccountStatus.Banned.ToValue();
            await _notificationService.SendNotificationAsync(userId, "Account Suspended (3rd Violation)", "Your account has been suspended for 30 days due to repeated and severe community standards violations.", linkAction!);
            await _hubContext.Clients.User(userId.ToString()).SendAsync("AccountLockedOut");
            var inst = await _instructorRepo.GetByIdAsync(userId);
            if (inst != null && string.Equals(inst.ApprovalStatus, InstructorApprovalStatus.Approved.ToValue(), StringComparison.OrdinalIgnoreCase))
            {
                await NotifyStudentsAboutInstructorSuspensionAsync(userId);
            }
        }
        return await _userRepo.UpdateAccountAsync(account);
    }

    private async Task<bool> HandleReviewRemovalPenaltyAsync(Enrollment? enrollment, string? resolutionNote, string? linkAction)
    {
        if (enrollment != null && enrollment.UserId != 0)
        {
            return await ProcessReviewStrikeAsync((int)enrollment.UserId, resolutionNote ?? "Review violation", linkAction);
        }
        return false;
    }

}
