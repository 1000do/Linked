using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepo;
    private readonly ICheckoutRepository _checkoutRepo;
    private readonly ICourseRepository _courseRepo;
    private readonly INotificationService _notificationService;
    private readonly IReportService _reportService;
    private readonly IUserRepository _userRepo;
    private readonly ILockoutRepository _lockoutRepo;

    public ReviewService(
        IReviewRepository reviewRepo, 
        ICheckoutRepository checkoutRepo, 
        ICourseRepository courseRepo,
        INotificationService notificationService,
        IReportService reportService,
        IUserRepository userRepo,
        ILockoutRepository lockoutRepo)
    {
        _reviewRepo = reviewRepo;
        _checkoutRepo = checkoutRepo;
        _courseRepo = courseRepo;
        _notificationService = notificationService;
        _reportService = reportService;
        _userRepo = userRepo;
        _lockoutRepo = lockoutRepo;
    }


    // ── Helper: Kiểm tra user có phải instructor sở hữu khóa học ───────

    private async Task<bool> IsOwnerAsync(int userId, int courseId)
    {
        return await _courseRepo.IsOwnerAsync(userId, courseId);
    }

    // ── Lấy hoặc tạo enrollment cho instructor (auto-enroll chủ khóa) ──

    private async Task<Enrollment> GetOrCreateOwnerEnrollmentAsync(int userId, int courseId)
    {
        var enrollment = await _checkoutRepo.GetEnrollmentWithProgressAsync(userId, courseId);

        if (enrollment == null)
        {
            // Auto-enroll instructor vào khóa của chính mình
            var course = await _courseRepo.GetByIdAsync(courseId);
            enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                Title = course?.Title ?? "Owner Access",
                EnrollDate = DateOnly.FromDateTime(DateTime.Now),
                IsCompleted = true, // Owner mặc định completed
                EnrollmentStatus = "active",
                LastAccessedAt = DateTime.Now
            };
            await _checkoutRepo.AddEnrollmentAsync(enrollment);
            int numberOfRowsAffected = await _checkoutRepo.SaveChangesAsync();
            if (numberOfRowsAffected <= 0)
                throw new InvalidOperationException("Failed to save changes");

            // Tạo progress record
            var progress = new EnrollmentProgress
            {
                EnrollmentId = enrollment.EnrollmentId,
                LearnedMaterialCount = 0,
                LastModifiedAt = DateTime.Now
            };
            await _checkoutRepo.AddEnrollmentProgressAsync(progress);
            int numberOfRowsAffected2 = await _checkoutRepo.SaveChangesAsync();
            if (numberOfRowsAffected2 <= 0)
                throw new InvalidOperationException("Failed to save changes");
            
            enrollment.Progress = progress;
        }

        return enrollment;
    }

    // ── Lấy danh sách reviews ──────────────────────────────────────────

    public async Task<PagedResult<ReviewResponse>> GetCourseReviewsAsync(int courseId, int page = 1, int pageSize = 10)
    {
        // Lấy instructor_id của khóa học
        var course = await _courseRepo.GetByIdAsync(courseId);
        var instructorId = course?.InstructorId;

        var (courseReviews, totalCount) = await _reviewRepo.GetCourseReviewsWithDetailsAsync(courseId, page, pageSize);

        var responses = new List<ReviewResponse>();
        foreach (var r in courseReviews)
        {
            var isInstructor = instructorId.HasValue && r.Enrollment?.UserId == instructorId.Value;
            responses.Add(new ReviewResponse
            {
                ReviewId = r.CourseReviewId,
                UserFullName = r.Enrollment?.User?.FullName ?? "Anonymous",
                UserAvatarUrl = r.Enrollment?.User?.UserNavigation?.AvatarUrl,
                Rating = r.Rating ?? 0,
                Comment = r.Comment ?? "",
                CreatedAt = r.CreatedAt ?? DateTime.Now,
                LessonTitle = null,
                LessonId = null,
                IsInstructor = isInstructor
            });
        }
        
        return new PagedResult<ReviewResponse>(responses, totalCount, page, pageSize);
    }

    public async Task<PagedResult<ReviewResponse>> GetLessonReviewsAsync(int lessonId, int page = 1, int pageSize = 10)
    {
        var (lessonReviews, totalCount) = await _reviewRepo.GetLessonReviewsWithDetailsAsync(lessonId, page, pageSize);
        var firstReview = lessonReviews.FirstOrDefault();
        var instructorId = firstReview?.Lesson?.Course?.InstructorId;

        var responses = lessonReviews.Select(r => new ReviewResponse
        {
            ReviewId = r.LessonReviewId,
            UserFullName = r.Enrollment!.User!.FullName ?? "Anonymous",
            UserAvatarUrl = r.Enrollment!.User!.UserNavigation.AvatarUrl,
            Rating = r.Rating ?? 0,
            Comment = r.Comment ?? "",
            CreatedAt = r.CreatedAt ?? DateTime.Now,
            LessonTitle = r.Lesson != null ? r.Lesson.Title : null,
            LessonId = r.LessonId,
            IsInstructor = instructorId.HasValue && r.Enrollment!.UserId == instructorId.Value
        }).ToList();

        return new PagedResult<ReviewResponse>(responses, totalCount, page, pageSize);
    }

    // ── Thống kê phân bổ sao ───────────────────────────────────────────

    public async Task<ReviewStatsResponse> GetReviewStatsAsync(int courseId)
    {
        var ratings = await _reviewRepo.GetCourseReviewRatingsAsync(courseId);

        var total = ratings.Count;
        return new ReviewStatsResponse
        {
            AverageRating = total > 0 ? Math.Round(ratings.Average(), 1) : 0,
            TotalReviews = total,
            Star5Count = ratings.Count(r => r >= 4.5f),
            Star4Count = ratings.Count(r => r >= 3.5f && r < 4.5f),
            Star3Count = ratings.Count(r => r >= 2.5f && r < 3.5f),
            Star2Count = ratings.Count(r => r >= 1.5f && r < 2.5f),
            Star1Count = ratings.Count(r => r < 1.5f)
        };
    }

    // ── Trạng thái enrollment + quyền review ───────────────────────────

    public async Task<EnrollmentStatusResponse> GetEnrollmentStatusAsync(int userId, int courseId)
    {
        bool isOwner = await IsOwnerAsync(userId, courseId);

        // Nếu là chủ nhân → bypass, không tạo record DB khi chỉ xem status
        if (isOwner)
        {
            var courseStats = await _courseRepo.GetCourseStatsAsync(courseId);
            var totalMats = courseStats?.TotalMaterials ?? 0;
            var ownerEnrollment = await _checkoutRepo.GetEnrollmentWithProgressAsync(userId, courseId);
            bool hasReviewed = ownerEnrollment != null && (await _reviewRepo.GetCourseReviewByEnrollmentAsync(ownerEnrollment.EnrollmentId)) != null;

            return new EnrollmentStatusResponse
            {
                IsEnrolled = true, // Bypass: Luôn coi như đã ghi danh
                IsCompleted = true,
                ProgressPercentage = 100,
                LearnedMaterialCount = totalMats,
                TotalMaterialCount = totalMats,
                CanReview = true, 
                ReviewBlockedReason = null,
                HasReviewed = hasReviewed,
                IsOwner = true
            };
        }

        var enrollment = await _checkoutRepo.GetEnrollmentWithProgressAsync(userId, courseId);

        if (enrollment == null)
        {
            return new EnrollmentStatusResponse
            {
                IsEnrolled = false,
                CanReview = false,
                ReviewBlockedReason = "You need to enroll in the course before writing a review.",
                IsOwner = false
            };
        }

        var stats = await _courseRepo.GetCourseStatsAsync(courseId);
        var totalMaterials = stats?.TotalMaterials ?? 0;
        var learnedCount = await _checkoutRepo.GetCompletedMaterialCountAsync(enrollment.EnrollmentId);
        var pct = totalMaterials > 0 ? (double)learnedCount / totalMaterials * 100 : 0;
        var isCompleted = enrollment.IsCompleted == true;

        var hasReviewedNormal = (await _reviewRepo.GetCourseReviewByEnrollmentAsync(enrollment.EnrollmentId)) != null;

        return new EnrollmentStatusResponse
        {
            IsEnrolled = true,
            IsCompleted = isCompleted,
            ProgressPercentage = Math.Round(pct, 1),
            LearnedMaterialCount = learnedCount,
            TotalMaterialCount = totalMaterials,
            CanReview = learnedCount > 0,
            ReviewBlockedReason = learnedCount == 0 ? "You need to complete at least 1 lesson before writing a review." : null,
            HasReviewed = hasReviewedNormal,
            IsOwner = false
        };
    }

    // ── Gửi review ─────────────────────────────────────────────────────

    public async Task SubmitReviewAsync(int userId, ReviewRequest request, bool requireCompletion)
    {
        var activeLockout = await _lockoutRepo.GetActiveLockoutAsync(userId, "review");
        if (activeLockout != null)
        {
            throw new BadRequestException($"Your account has been restricted from posting comments and reviews until {activeLockout.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss} due to repeated community standards violations.");
        }

        bool isOwner = await IsOwnerAsync(userId, request.CourseId);

        Enrollment enrollment;

        if (isOwner)
        {
            // Chủ nhân khóa học → auto-enroll + bypass mọi ràng buộc
            enrollment = await GetOrCreateOwnerEnrollmentAsync(userId, request.CourseId);
        }
        else
        {
            // Người dùng bình thường → kiểm tra enrollment + ràng buộc
            enrollment = await _checkoutRepo.GetEnrollmentWithProgressAsync(userId, request.CourseId)
                ?? throw new InvalidOperationException("You need to enroll in the course before writing a review.");

            if (requireCompletion)
            {
                if (enrollment.IsCompleted != true)
                    throw new InvalidOperationException("You need to complete the course before writing a review on the detail page.");
            }
            else
            {
                var learnedCount = await _checkoutRepo.GetCompletedMaterialCountAsync(enrollment.EnrollmentId);
                if (learnedCount <= 0)
                    throw new InvalidOperationException("You need to complete at least 1 lesson before writing a review.");
            }
        }

        // ── Validate input ──
        if (request.Rating < 1 || request.Rating > 5)
            throw new InvalidOperationException("Rating must be between 1 and 5 stars.");

        if (string.IsNullOrWhiteSpace(request.Comment))
            throw new InvalidOperationException("Review content cannot be empty.");

        // ── Upsert logic ──
        if (request.LessonId.HasValue)
        {
            // Review cho lesson cụ thể
            var existingReview = await _reviewRepo.GetLessonReviewByEnrollmentAsync(enrollment.EnrollmentId, request.LessonId.Value);

            if (existingReview != null)
            {
                existingReview.Rating = request.Rating;
                existingReview.Comment = request.Comment;
                existingReview.UpdatedAt = DateTime.Now;
                _reviewRepo.UpdateLessonReview(existingReview);
            }
            else
            {
                var review = new LessonReview
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    LessonId = request.LessonId.Value,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsRemoved = false
                };
                await _reviewRepo.AddLessonReviewAsync(review);
            }
        }
        else
        {
            // Review cho khóa học
            var existingReview = await _reviewRepo.GetCourseReviewByEnrollmentAsync(enrollment.EnrollmentId);

            if (existingReview != null)
            {
                existingReview.Rating = request.Rating;
                existingReview.Comment = request.Comment;
                existingReview.UpdatedAt = DateTime.Now;
                _reviewRepo.UpdateCourseReview(existingReview);
            }
            else
            {
                var review = new CourseReview
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsRemoved = false
                };
                await _reviewRepo.AddCourseReviewAsync(review);
            }
        }

        int numberOfRowsAffected = await _reviewRepo.SaveChangesAsync();
        if (numberOfRowsAffected <= 0)
            throw new InvalidOperationException("Failed to save changes");

        // ── Thông báo cho instructor (nếu không phải instructor tự đánh giá) ──
        if (!isOwner)
        {
            var course = await _courseRepo.GetByIdAsync(request.CourseId);
            if (course != null && course.InstructorId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "New Review",
                    $"Your course '{course.Title}' just received a {request.Rating}-star review from a student.",
                    $"/InstructorCourse/Editor?id={request.CourseId}"
                );
            }
        }
    }

    public async Task ReportReviewAsync(int userId, int reviewId, string type, string reason)
    {
        // Delegate sang IReportService để tạo report record đúng chuẩn
        // Giữ backward compatibility với ReviewController cũ
        if (type.ToLower() == "course")
        {
            var request = new CourseMarketplaceBE.Application.DTOs.CreateCourseReviewReportRequest
            {
                CourseReviewId = reviewId,
                Reason = string.IsNullOrWhiteSpace(reason) ? "Violates community standards" : reason
            };
            await _reportService.CreateCourseReviewReportAsync(userId, request);
        }
        else
        {
            var request = new CourseMarketplaceBE.Application.DTOs.CreateLessonReviewReportRequest
            {
                LessonReviewId = reviewId,
                Reason = string.IsNullOrWhiteSpace(reason) ? "Violates community standards" : reason
            };
            await _reportService.CreateLessonReviewReportAsync(userId, request);
        }
    }
}

