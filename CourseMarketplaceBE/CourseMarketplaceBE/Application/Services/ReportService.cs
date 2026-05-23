using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepo;
    private readonly ICheckoutRepository _checkoutRepo;
    private readonly ICourseRepository _courseRepo;
    private readonly IReviewRepository _reviewRepo;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepo;
    private readonly IInstructorRepository _instructorRepo;

    public ReportService(
        IReportRepository reportRepo,
        ICheckoutRepository checkoutRepo,
        ICourseRepository courseRepo,
        IReviewRepository reviewRepo,
        INotificationService notificationService,
        IUserRepository userRepo,
        IInstructorRepository instructorRepo)
    {
        _reportRepo = reportRepo;
        _checkoutRepo = checkoutRepo;
        _courseRepo = courseRepo;
        _reviewRepo = reviewRepo;
        _notificationService = notificationService;
        _userRepo = userRepo;
        _instructorRepo = instructorRepo;
    }

    // ── User / Instructor: Tạo report khóa học ─────────────────────────────

    public async Task CreateCourseReportAsync(int reporterId, CreateCourseReportRequest request, bool isInstructor)
    {
        var course = await _courseRepo.GetByIdAsync(request.CourseId)
            ?? throw new InvalidOperationException("Course not found.");

        // Cannot report your own course
        if (course.InstructorId == reporterId)
            throw new InvalidOperationException("You cannot report your own course.");

        if (!isInstructor)
        {
            // Regular user: must be enrolled
            var enrollment = await _checkoutRepo.GetEnrollmentWithProgressAsync(reporterId, request.CourseId);
            if (enrollment == null)
                throw new InvalidOperationException("You must be enrolled in the course before reporting it.");
        }

        // Kiểm tra duplicate pending report
        var existing = await _reportRepo.GetPendingCourseReportAsync(reporterId, request.CourseId);
        if (existing != null)
            throw new InvalidOperationException("You already have a pending report for this course.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new InvalidOperationException("Report reason cannot be empty.");

        var report = new CourseReport
        {
            ReporterId = reporterId,
            CourseId = request.CourseId,
            Reason = request.Reason,
            Description = request.Description,
            CourseReportsStatus = "pending",
            CreatedAt = DateTime.Now
        };

        await _reportRepo.AddCourseReportAsync(report);
        await _reportRepo.SaveChangesAsync();
    }

    // ── User / Instructor: Tạo report review khóa học ──────────────────────

    public async Task CreateCourseReviewReportAsync(int reporterId, CreateCourseReviewReportRequest request)
    {
        var review = await _reviewRepo.GetCourseReviewByIdAsync(request.CourseReviewId)
            ?? throw new InvalidOperationException("Review not found.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new InvalidOperationException("Report reason cannot be empty.");

        var existing = await _reportRepo.GetPendingCourseReviewReportAsync(reporterId, request.CourseReviewId);
        if (existing != null)
            throw new InvalidOperationException("You already have a pending report for this review.");

        var report = new CourseReviewReport
        {
            ReporterId = reporterId,
            CourseReviewId = request.CourseReviewId,
            Reason = request.Reason,
            Description = request.Description,
            UserReportsStatus = "pending",
            CreatedAt = DateTime.Now
        };

        await _reportRepo.AddCourseReviewReportAsync(report);
        await _reportRepo.SaveChangesAsync();
    }

    // ── User / Instructor: Tạo report review bài học ───────────────────────

    public async Task CreateLessonReviewReportAsync(int reporterId, CreateLessonReviewReportRequest request)
    {
        var review = await _reviewRepo.GetLessonReviewByIdAsync(request.LessonReviewId)
            ?? throw new InvalidOperationException("Review not found.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new InvalidOperationException("Report reason cannot be empty.");

        var existing = await _reportRepo.GetPendingLessonReviewReportAsync(reporterId, request.LessonReviewId);
        if (existing != null)
            throw new InvalidOperationException("You already have a pending report for this review.");

        var report = new LessonReviewReport
        {
            ReporterId = reporterId,
            LessonReviewId = request.LessonReviewId,
            Reason = request.Reason,
            Description = request.Description,
            UserReportsStatus = "pending",
            CreatedAt = DateTime.Now
        };

        await _reportRepo.AddLessonReviewReportAsync(report);
        await _reportRepo.SaveChangesAsync();
    }

    // ── User / Instructor: Xem lịch sử report của mình ─────────────────────

    public async Task<IEnumerable<MyCourseReportResponse>> GetMyCourseReportsAsync(int reporterId)
    {
        var reports = await _reportRepo.GetCourseReportsByReporterAsync(reporterId);
        return reports.Select(r => new MyCourseReportResponse
        {
            ReportId = r.CourseReportId,
            CourseId = r.CourseId,
            CourseTitle = r.Course?.Title,
            Reason = r.Reason,
            Description = r.Description,
            Status = r.CourseReportsStatus,
            ResolutionNote = r.ResolutionNote,
            CreatedAt = r.CreatedAt,
            ResolvedAt = r.ResolvedAt
        });
    }

    public async Task<IEnumerable<MyReviewReportResponse>> GetMyReviewReportsAsync(int reporterId)
    {
        var courseReviewReports = await _reportRepo.GetCourseReviewReportsByReporterAsync(reporterId);
        var lessonReviewReports = await _reportRepo.GetLessonReviewReportsByReporterAsync(reporterId);

        var courseResults = courseReviewReports.Select(r => new MyReviewReportResponse
        {
            ReportId = r.CourseReviewReportId,
            ReviewType = "course_review",
            ReviewId = r.CourseReviewId,
            ReviewComment = r.CourseReview?.Comment,
            Reason = r.Reason,
            Description = r.Description,
            Status = r.UserReportsStatus,
            ResolutionNote = r.ResolutionNote,
            CreatedAt = r.CreatedAt,
            ResolvedAt = r.ResolvedAt
        });

        var lessonResults = lessonReviewReports.Select(r => new MyReviewReportResponse
        {
            ReportId = r.LessonReviewReportId,
            ReviewType = "lesson_review",
            ReviewId = r.LessonReviewId,
            ReviewComment = r.LessonReview?.Comment,
            Reason = r.Reason,
            Description = r.Description,
            Status = r.UserReportsStatus,
            ResolutionNote = r.ResolutionNote,
            CreatedAt = r.CreatedAt,
            ResolvedAt = r.ResolvedAt
        });

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
        return reports.Select(r => MapToCourseReportDetail(r));
    }

    // ── Staff / Admin: Xem tất cả reports ──────────────────────────────────

    public async Task<IEnumerable<CourseReportDetailResponse>> GetAllCourseReportsAsync(string? status = null)
    {
        var reports = await _reportRepo.GetAllCourseReportsAsync(status);
        return reports.Select(r => MapToCourseReportDetail(r));
    }

    public async Task<IEnumerable<ReviewReportDetailResponse>> GetAllCourseReviewReportsAsync(string? status = null)
    {
        var reports = await _reportRepo.GetAllCourseReviewReportsAsync(status);
        return reports.Select(r => new ReviewReportDetailResponse
        {
            ReportId = r.CourseReviewReportId,
            ReviewType = "course_review",
            ReporterId = r.ReporterId,
            ReporterEmail = r.Reporter?.Email,
            ReporterName = null,
            ReviewId = r.CourseReviewId,
            ReviewComment = r.CourseReview?.Comment,
            ReviewRating = r.CourseReview?.Rating,
            Reason = r.Reason,
            Description = r.Description,
            Status = r.UserReportsStatus,
            ResolutionNote = r.ResolutionNote,
            ResolverEmail = r.Resolver?.Email,
            CreatedAt = r.CreatedAt,
            ResolvedAt = r.ResolvedAt
        });
    }

    public async Task<IEnumerable<ReviewReportDetailResponse>> GetAllLessonReviewReportsAsync(string? status = null)
    {
        var reports = await _reportRepo.GetAllLessonReviewReportsAsync(status);
        return reports.Select(r => new ReviewReportDetailResponse
        {
            ReportId = r.LessonReviewReportId,
            ReviewType = "lesson_review",
            ReporterId = r.ReporterId,
            ReporterEmail = r.Reporter?.Email,
            ReporterName = null,
            ReviewId = r.LessonReviewId,
            ReviewComment = r.LessonReview?.Comment,
            ReviewRating = r.LessonReview?.Rating,
            Reason = r.Reason,
            Description = r.Description,
            Status = r.UserReportsStatus,
            ResolutionNote = r.ResolutionNote,
            ResolverEmail = r.Resolver?.Email,
            CreatedAt = r.CreatedAt,
            ResolvedAt = r.ResolvedAt
        });
    }

    public async Task<ReportStatsResponse> GetReportStatsAsync()
    {
        var pendingCourse = await _reportRepo.GetAllCourseReportsAsync("pending");
        var pendingCourseReview = await _reportRepo.GetAllCourseReviewReportsAsync("pending");
        var pendingLessonReview = await _reportRepo.GetAllLessonReviewReportsAsync("pending");

        var today = DateTime.Today;

        var allCourse = await _reportRepo.GetAllCourseReportsAsync();
        var allCourseReview = await _reportRepo.GetAllCourseReviewReportsAsync();
        var allLessonReview = await _reportRepo.GetAllLessonReviewReportsAsync();

        int resolvedToday = allCourse.Count(r => r.ResolvedAt?.Date == today && r.CourseReportsStatus == "resolved")
                          + allCourseReview.Count(r => r.ResolvedAt?.Date == today && r.UserReportsStatus == "resolved")
                          + allLessonReview.Count(r => r.ResolvedAt?.Date == today && r.UserReportsStatus == "resolved");

        int rejectedToday = allCourse.Count(r => r.ResolvedAt?.Date == today && r.CourseReportsStatus == "rejected")
                          + allCourseReview.Count(r => r.ResolvedAt?.Date == today && r.UserReportsStatus == "rejected")
                          + allLessonReview.Count(r => r.ResolvedAt?.Date == today && r.UserReportsStatus == "rejected");

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
        if (report.CourseReportsStatus == "escalated")
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
            if (request.Status == "resolved")
            {
                // Chỉ tăng cờ vi phạm khi báo cáo được xác nhận là vi phạm (resolved)
                var currentFlags = (course.CourseFlagCount ?? 0) + 1;
                course.CourseFlagCount = currentFlags;

                // Áp dụng hình phạt theo cờ vi phạm
                if (currentFlags == 1)
                {
                    course.CourseStatus = "rejected";
                    if (course.InstructorId.HasValue)
                    {
                        await _notificationService.SendNotificationAsync(
                            course.InstructorId.Value,
                            "Course Violation Reminder (1st Time)",
                            $"Your course '{course.Title}' has received a policy violation warning and has been rejected. Reason: {request.ResolutionNote}. Please review and correct the content.",
                            "/instructor/courses"
                        );
                    }
                }
                else if (currentFlags == 2)
                {
                    // Cờ 2: Chuyển về rejected, vẫn giữ flag count
                    course.CourseStatus = "rejected";
                    if (course.InstructorId.HasValue)
                    {
                        await _notificationService.SendNotificationAsync(
                            course.InstructorId.Value,
                            "Severe Violation Warning (2nd Time)",
                            $"Your course '{course.Title}' has been flagged for violation (2nd time) and rejected from Marketplace. Reason: {request.ResolutionNote}. Please comply with platform policies.",
                            "/instructor/courses"
                        );
                    }
                }
                else if (currentFlags >= 3)
                {
                    // Cờ 3: Khóa vĩnh viễn khóa học, khóa instructor 30 ngày
                    course.CourseStatus = "archived";
                    if (course.InstructorId.HasValue)
                    {
                        var instructor = await _instructorRepo.GetByIdAsync(course.InstructorId.Value);
                        if (instructor != null)
                        {
                            instructor.LockoutInstructorUntil = DateTime.Now.AddDays(30);
                            _instructorRepo.Update(instructor);
                            await NotifyStudentsAboutInstructorSuspensionAsync(course.InstructorId.Value);
                        }
                        
                        // Reset course flags sau khi đã khóa instructor (thi hành xong án)
                        course.CourseFlagCount = 0;

                        await _notificationService.SendNotificationAsync(
                            course.InstructorId.Value,
                            "Permanent Course Discontinuation Notice (3rd Time)",
                            $"Your course '{course.Title}' has violated our policies for the 3rd time. It has been permanently discontinued and archived. New enrollments are disabled.",
                            "/instructor/courses"
                        );
                    }
                }

                // Nếu yêu cầu gỡ nội dung
                if (request.RemoveContent)
                {
                    course.IsRemoved = true;
                    course.CourseStatus = "archived";
                }
            }
        }

        _reportRepo.UpdateCourseReport(report);
        await _reportRepo.SaveChangesAsync();

        // Notify reporter of outcome
        if (report.ReporterId.HasValue)
        {
            var message = request.Status == "resolved"
                ? "Your report has been accepted and the violating content has been actioned."
                : "Your report has been reviewed and dismissed.";
            await _notificationService.SendNotificationAsync(
                report.ReporterId.Value,
                "Report Resolution Update",
                message,
                $"/courses/{report.CourseId}"
            );
        }

        // Notify instructor if content was removed
        if (request.RemoveContent && request.Status == "resolved" && course != null)
        {
            if (course.InstructorId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "Course Content Removed",
                    $"Your course '{course.Title}' has been removed from the platform due to a policy violation. Reason: {request.ResolutionNote}",
                    "/instructor/courses"
                );
            }
        }

        return true;
    }

    // ── Staff / Admin: Xử lý course review report ──────────────────────────

    public async Task<bool> ResolveCourseReviewReportAsync(int reportId, int resolverId, ResolveReportRequest request)
    {
        var report = await _reportRepo.GetCourseReviewReportByIdAsync(reportId);
        if (report == null) return false;

        report.UserReportsStatus = request.Status;
        report.ResolutionNote = request.ResolutionNote;
        report.ResolverId = resolverId;
        report.ResolvedAt = DateTime.Now;

        // Nếu approve: soft-remove review và ghi đè nội dung bình luận vi phạm
        if (request.RemoveContent && request.Status == "resolved" && report.CourseReviewId.HasValue)
        {
            var review = await _reviewRepo.GetCourseReviewByIdAsync(report.CourseReviewId.Value);
            if (review != null)
            {
                review.IsRemoved = true;
                review.Comment = "This comment has been removed for violating community standards";
                review.CourseReviewStatus = "removed";
                review.UpdatedAt = DateTime.Now;
                _reviewRepo.UpdateCourseReview(review);
                
                // Strike mechanism cho review vi phạm
                if (review.Enrollment != null && review.Enrollment.UserId != 0)
                {
                    await ProcessReviewStrikeAsync((int)review.Enrollment.UserId, request.ResolutionNote ?? "Review violation");
                }
            }
        }

        _reportRepo.UpdateCourseReviewReport(report);
        await _reportRepo.SaveChangesAsync();

        // Notify reporter of outcome
        if (report.ReporterId.HasValue)
        {
            var message = request.Status == "resolved"
                ? "Your report about the course review has been accepted."
                : "Your report about the course review has been reviewed and dismissed.";
            await _notificationService.SendNotificationAsync(
                report.ReporterId.Value,
                "Report Resolution Update",
                message,
                null!
            );
        }

        return true;
    }

    // ── Staff / Admin: Xử lý lesson review report ──────────────────────────

    public async Task<bool> ResolveLessonReviewReportAsync(int reportId, int resolverId, ResolveReportRequest request)
    {
        var report = await _reportRepo.GetLessonReviewReportByIdAsync(reportId);
        if (report == null) return false;

        report.UserReportsStatus = request.Status;
        report.ResolutionNote = request.ResolutionNote;
        report.ResolverId = resolverId;
        report.ResolvedAt = DateTime.Now;

        // Nếu approve: soft-remove review và ghi đè nội dung bình luận vi phạm
        if (request.RemoveContent && request.Status == "resolved" && report.LessonReviewId.HasValue)
        {
            var review = await _reviewRepo.GetLessonReviewByIdAsync(report.LessonReviewId.Value);
            if (review != null)
            {
                review.IsRemoved = true;
                review.Comment = "This comment has been removed for violating community standards";
                review.LessonReviewStatus = "removed";
                review.UpdatedAt = DateTime.Now;
                _reviewRepo.UpdateLessonReview(review);

                // Strike mechanism cho review vi phạm
                if (review.Enrollment != null && review.Enrollment.UserId != 0)
                {
                    await ProcessReviewStrikeAsync((int)review.Enrollment.UserId, request.ResolutionNote ?? "Review violation");
                }
            }
        }

        _reportRepo.UpdateLessonReviewReport(report);
        await _reportRepo.SaveChangesAsync();

        // Notify reporter of outcome
        if (report.ReporterId.HasValue)
        {
            var message = request.Status == "resolved"
                ? "Your report about the lesson review has been accepted."
                : "Your report about the lesson review has been reviewed and dismissed.";
            await _notificationService.SendNotificationAsync(
                report.ReporterId.Value,
                "Report Resolution Update",
                message,
                null!
            );
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
        course.CourseStatus = "archived";
        course.UpdatedAt = DateTime.Now;

        await _reportRepo.SaveChangesAsync();
        return true;
    }


    // ── Private helpers ─────────────────────────────────────────────────────

    private async Task NotifyStudentsAboutInstructorSuspensionAsync(int instructorId)
    {
        var courses = await _courseRepo.GetInstructorCoursesAsync(instructorId);
        var notifiedUserIds = new HashSet<int>();
        foreach (var c in courses)
        {
            var studentIds = await _checkoutRepo.GetEnrolledUserIdsAsync(c.CourseId);
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

    private async Task ProcessReviewStrikeAsync(int userId, string resolutionNote)
    {
        var account = await _userRepo.GetAccountByIdAsync(userId);
        if (account == null) return;

        account.AccountFlagCount = (account.AccountFlagCount ?? 0) + 1;

        if (account.AccountFlagCount == 1)
        {
            await _notificationService.SendNotificationAsync(userId, "Community Standards Violation (1st Warning)", "Your comment has been removed for violating community standards. This is your first warning.", null!);
        }
        else if (account.AccountFlagCount == 2)
        {
            account.CommentLockoutEnd = DateTime.Now.AddDays(7);
            await _notificationService.SendNotificationAsync(userId, "Commenting Restricted (2nd Violation)", "Due to repeated violations, you are restricted from posting comments or reviews for 7 days.", null!);
        }
        else if (account.AccountFlagCount >= 3)
        {
            account.AccountLockoutEnd = DateTime.Now.AddDays(30);
            account.AccountStatus = "Banned";
            await _notificationService.SendNotificationAsync(userId, "Account Suspended (3rd Violation)", "Your account has been suspended for 30 days due to repeated and severe community standards violations.", null!);

            var inst = await _instructorRepo.GetByIdAsync(userId);
            if (inst != null && string.Equals(inst.ApprovalStatus, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                await NotifyStudentsAboutInstructorSuspensionAsync(userId);
            }
        }
        await _userRepo.UpdateAccountAsync(account);
    }

    private CourseReportDetailResponse MapToCourseReportDetail(CourseReport r)
        => new CourseReportDetailResponse
        {
            ReportId = r.CourseReportId,
            ReporterId = r.ReporterId,
            ReporterEmail = r.Reporter?.Email,
            CourseId = r.CourseId,
            CourseTitle = r.Course?.Title,
            CourseFlagCount = r.Course?.CourseFlagCount ?? 0,
            Reason = r.Reason,
            Description = r.Description,
            Status = r.CourseReportsStatus,
            ResolutionNote = r.ResolutionNote,
            ResolverEmail = r.Resolver?.Email,
            CreatedAt = r.CreatedAt,
            ResolvedAt = r.ResolvedAt,
            AccessGrantedUntil = r.AccessGrantedUntil
        };
}
