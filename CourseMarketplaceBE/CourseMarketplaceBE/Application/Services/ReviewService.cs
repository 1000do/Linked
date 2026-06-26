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
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly ICourseRepository _courseRepo;
    private readonly INotificationService _notificationService;
    private readonly IReportSubmissionService _reportService;
    private readonly IUserRepository _userRepo;
    private readonly ILockoutRepository _lockoutRepo;

    public ReviewService(
        IReviewRepository reviewRepo,
        IEnrollmentRepository enrollmentRepo,
        ICourseRepository courseRepo,
        INotificationService notificationService,
        IReportSubmissionService reportService,
        IUserRepository userRepo,
        ILockoutRepository lockoutRepo)
    {
        _reviewRepo = reviewRepo;
        _enrollmentRepo = enrollmentRepo;
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
        var enrollment = await _enrollmentRepo.GetEnrollmentWithProgressAsync(userId, courseId);

        if (enrollment == null)
        {
            var course = await _courseRepo.GetByIdAsync(courseId);
            enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                Title = course?.Title ?? "Owner Access",
                EnrollDate = DateOnly.FromDateTime(DateTime.Now),
                IsCompleted = true,
                EnrollmentStatus = "active",
                LastAccessedAt = DateTime.Now
            };
            await _enrollmentRepo.AddEnrollmentAsync(enrollment);
            int rows = await _enrollmentRepo.SaveChangesAsync();
            /* zero rows exception removed */
        }

        return enrollment;
    }

    // ── Lấy danh sách reviews ──────────────────────────────────────────

    public async Task<PagedResult<ReviewResponse>> GetCourseReviewsAsync(
        int courseId, int page = 1, int pageSize = 10, int? starFilter = null)
    {
        var course = await _courseRepo.GetByIdAsync(courseId);
        var instructorId = course?.InstructorId;

        var (courseReviews, totalCount) = await _reviewRepo.GetCourseReviewsWithDetailsAsync(
            courseId, page, pageSize, starFilter);

        var responses = new List<ReviewResponse>();
        foreach (var r in courseReviews)
        {
            var isInstructor = instructorId.HasValue && r.Enrollment?.UserId == instructorId.Value;
            var isRemoved = r.IsRemoved ?? false;
            var status = r.CourseReviewStatus ?? "ok";
            var comment = r.Comment ?? "";
            var rating = r.Rating ?? 0;

            if (isRemoved)
            {
                rating = 0;
                // Only admin-removed (status="violating") reviews should reach here.
                // Self-deleted (status="removed") reviews are excluded at the repo query level.
                comment = status == "violating"
                    ? "This review was removed by a moderator for violating community standards."
                    : "[deleted]"; // fallback
            }

            responses.Add(new ReviewResponse
            {
                ReviewId = r.CourseReviewId,
                UserId = r.Enrollment?.UserId ?? 0,
                UserFullName = r.Enrollment?.User?.FullName ?? "Anonymous",
                UserAvatarUrl = r.Enrollment?.User?.UserNavigation?.AvatarUrl,
                Rating = rating,
                Comment = comment,
                CreatedAt = r.CreatedAt ?? DateTime.Now,
                UpdatedAt = r.UpdatedAt,
                LessonTitle = null,
                LessonId = null,
                IsInstructor = isInstructor,
                IsRemoved = isRemoved,
                ReviewStatus = status
            });
        }

        return new PagedResult<ReviewResponse>(responses, totalCount, page, pageSize);
    }

    public async Task<PagedResult<ReviewResponse>> GetLessonReviewsAsync(
        int lessonId, int page = 1, int pageSize = 10)
    {
        var (lessonReviews, totalCount) = await _reviewRepo.GetLessonReviewsWithDetailsAsync(
            lessonId, page, pageSize);
        var firstReview = lessonReviews.FirstOrDefault();
        var instructorId = firstReview?.Lesson?.Course?.InstructorId;

        var responses = lessonReviews.Select(r =>
        {
            var isInstructor = instructorId.HasValue && r.Enrollment?.UserId == instructorId.Value;
            var isRemoved = r.IsRemoved ?? false;
            var status = r.LessonReviewStatus ?? "ok";
            var comment = r.Comment ?? "";
            var rating = r.Rating ?? 0;

            if (isRemoved)
            {
                rating = 0;
                // Only admin-removed (status="violating") reviews should reach here.
                // Self-deleted (status="removed") reviews are excluded at the repo query level.
                comment = status == "violating"
                    ? "This review was removed by a moderator for violating community standards."
                    : "[deleted]"; // fallback
            }

            return new ReviewResponse
            {
                ReviewId = r.LessonReviewId,
                UserId = r.Enrollment?.UserId ?? 0,
                UserFullName = r.Enrollment?.User?.FullName ?? "Anonymous",
                UserAvatarUrl = r.Enrollment?.User?.UserNavigation?.AvatarUrl,
                Rating = rating,
                Comment = comment,
                CreatedAt = r.CreatedAt ?? DateTime.Now,
                UpdatedAt = r.UpdatedAt,
                LessonTitle = r.Lesson != null ? r.Lesson.Title : null,
                LessonId = r.LessonId,
                IsInstructor = isInstructor,
                IsRemoved = isRemoved,
                ReviewStatus = status
            };
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

    public async Task<ReviewStatsResponse> GetLessonReviewStatsAsync(int lessonId)
    {
        var ratings = await _reviewRepo.GetLessonReviewRatingsAsync(lessonId);
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

    public async Task<List<LessonRatingStatsResponse>> GetLessonRatingsForCourseAsync(int courseId)
    {
        var data = await _reviewRepo.GetLessonRatingsForCourseAsync(courseId);
        return data.Select(x => new LessonRatingStatsResponse
        {
            LessonId = x.LessonId,
            AverageRating = Math.Round(x.AvgRating, 1),
            TotalReviews = x.Count
        }).ToList();
    }

    // ── Trạng thái enrollment + quyền review ───────────────────────────

    public async Task<EnrollmentStatusResponse> GetEnrollmentStatusAsync(int userId, int courseId)
    {
        bool isOwner = await IsOwnerAsync(userId, courseId);

        if (isOwner)
        {
            var courseStats = await _courseRepo.GetCourseStatsAsync(courseId);
            var totalMats = courseStats?.TotalMaterials ?? 0;
            var ownerEnrollment = await _enrollmentRepo.GetEnrollmentWithProgressAsync(userId, courseId);
            bool hasReviewed = ownerEnrollment != null
                && (await _reviewRepo.GetCourseReviewByEnrollmentAsync(ownerEnrollment.EnrollmentId)) != null;

            return new EnrollmentStatusResponse
            {
                IsEnrolled = true,
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

        var enrollment = await _enrollmentRepo.GetEnrollmentWithProgressAsync(userId, courseId);

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
        var learnedCount = await _enrollmentRepo.GetCompletedMaterialCountAsync(enrollment.EnrollmentId);
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
            ReviewBlockedReason = learnedCount == 0
                ? "You need to complete at least 1 lesson before writing a review."
                : null,
            HasReviewed = hasReviewedNormal,
            IsOwner = false
        };
    }

    // ── Gửi review (luôn tạo record mới — không upsert) ───────────────

    public async Task SubmitReviewAsync(int userId, ReviewRequest request, bool requireCompletion)
    {
        var activeLockout = await _lockoutRepo.GetActiveLockoutAsync(userId, "review");
        if (activeLockout != null)
            throw new BadRequestException(
                $"Your account has been restricted from posting comments and reviews until " +
                $"{activeLockout.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss} due to repeated community standards violations.");

        bool isOwner = await IsOwnerAsync(userId, request.CourseId);
        Enrollment enrollment;

        if (isOwner)
        {
            enrollment = await GetOrCreateOwnerEnrollmentAsync(userId, request.CourseId);
        }
        else
        {
            enrollment = await _enrollmentRepo.GetEnrollmentWithProgressAsync(userId, request.CourseId)
                ?? throw new InvalidOperationException("You need to enroll in the course before writing a review.");

            if (requireCompletion)
            {
                if (enrollment.IsCompleted != true)
                    throw new InvalidOperationException(
                        "You need to complete the course before writing a review on the detail page.");
            }
            else
            {
                var learnedCount = await _enrollmentRepo.GetCompletedMaterialCountAsync(enrollment.EnrollmentId);
                /* zero rows exception removed */
            }
        }

        if (request.Rating < 1 || request.Rating > 5)
            throw new InvalidOperationException("Rating must be between 1 and 5 stars.");
        if (string.IsNullOrWhiteSpace(request.Comment))
            throw new InvalidOperationException("Review content cannot be empty.");

        // Luôn tạo record mới — không kiểm tra existing, không upsert
        LessonReview newLessonReview = null;
        CourseReview newCourseReview = null;

        if (request.LessonId.HasValue)
        {
            newLessonReview = new LessonReview
            {
                EnrollmentId = enrollment.EnrollmentId,
                LessonId = request.LessonId.Value,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsRemoved = false
            };
            await _reviewRepo.AddLessonReviewAsync(newLessonReview);
        }
        else
        {
            newCourseReview = new CourseReview
            {
                EnrollmentId = enrollment.EnrollmentId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsRemoved = false
            };
            await _reviewRepo.AddCourseReviewAsync(newCourseReview);
        }

        int saved = await _reviewRepo.SaveChangesAsync();
        /* zero rows exception removed */

        if (!isOwner)
        {
            var course = await _courseRepo.GetByIdAsync(request.CourseId);
            if (course != null && course.InstructorId.HasValue)
            {
                int reviewId = request.LessonId.HasValue ? newLessonReview.LessonReviewId : newCourseReview.CourseReviewId;
                string linkAction = request.LessonId.HasValue 
                    ? $"/Course/Learn/{request.CourseId}#review-card-{reviewId}" 
                    : $"/Course/Details/{request.CourseId}#review-card-{reviewId}";

                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "New Review",
                    $"Your course '{course.Title}' just received a {request.Rating}-star review from a student.",
                    linkAction
                );
            }
        }
    }

    // ── Chỉnh sửa review (chỉ chủ review) ────────────────────────────

    public async Task UpdateReviewAsync(int userId, UpdateReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
            throw new InvalidOperationException("Rating must be between 1 and 5 stars.");
        if (string.IsNullOrWhiteSpace(request.Comment))
            throw new InvalidOperationException("Review content cannot be empty.");

        var type = request.Type?.ToLower() ?? "course";

        if (type == "lesson")
        {
            var review = await _reviewRepo.GetLessonReviewByIdAsync(request.ReviewId)
                ?? throw new InvalidOperationException("Review not found.");
            if (review.Enrollment?.UserId != userId)
                throw new UnauthorizedAccessException("You can only edit your own reviews.");
            if (review.IsRemoved == true)
                throw new InvalidOperationException("This review has been removed and cannot be edited.");

            review.Rating = request.Rating;
            review.Comment = request.Comment;
            review.UpdatedAt = DateTime.Now;
            _reviewRepo.UpdateLessonReview(review);
        }
        else
        {
            var review = await _reviewRepo.GetCourseReviewByIdAsync(request.ReviewId)
                ?? throw new InvalidOperationException("Review not found.");
            if (review.Enrollment?.UserId != userId)
                throw new UnauthorizedAccessException("You can only edit your own reviews.");
            if (review.IsRemoved == true)
                throw new InvalidOperationException("This review has been removed and cannot be edited.");

            review.Rating = request.Rating;
            review.Comment = request.Comment;
            review.UpdatedAt = DateTime.Now;
            _reviewRepo.UpdateCourseReview(review);
        }

        int rows = await _reviewRepo.SaveChangesAsync();
        /* zero rows exception removed */
    }

    // ── Xóa mềm review (chỉ chủ review) ──────────────────────────────

    public async Task DeleteReviewAsync(int userId, DeleteReviewRequest request)
    {
        var type = request.Type?.ToLower() ?? "course";

        if (type == "lesson")
        {
            var review = await _reviewRepo.GetLessonReviewByIdAsync(request.ReviewId)
                ?? throw new InvalidOperationException("Review not found.");
            if (review.Enrollment?.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own reviews.");

            // Block deletion if there is an active moderation report
            var hasPending = await _reviewRepo.HasPendingLessonReviewReportsAsync(request.ReviewId);
            if (hasPending)
                throw new InvalidOperationException(
                    "This review is currently under moderation review and cannot be deleted.");

            review.IsRemoved = true;
            review.LessonReviewStatus = "removed"; // distinguishes user self-delete from admin removal ("violating")
            review.UpdatedAt = DateTime.Now;
            _reviewRepo.UpdateLessonReview(review);
        }
        else
        {
            var review = await _reviewRepo.GetCourseReviewByIdAsync(request.ReviewId)
                ?? throw new InvalidOperationException("Review not found.");
            if (review.Enrollment?.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own reviews.");

            // Block deletion if there is an active moderation report
            var hasPending = await _reviewRepo.HasPendingCourseReviewReportsAsync(request.ReviewId);
            if (hasPending)
                throw new InvalidOperationException(
                    "This review is currently under moderation review and cannot be deleted.");

            review.IsRemoved = true;
            review.CourseReviewStatus = "removed"; // distinguishes user self-delete from admin removal ("violating")
            review.UpdatedAt = DateTime.Now;
            _reviewRepo.UpdateCourseReview(review);
        }

        int rows = await _reviewRepo.SaveChangesAsync();
        /* zero rows exception removed */
    }

    // ── Report review ──────────────────────────────────────────────────

    public async Task ReportReviewAsync(int userId, int reviewId, string type, string reason)
    {
        if (type.ToLower() == "course")
        {
            var request = new CreateCourseReviewReportRequest
            {
                CourseReviewId = reviewId,
                Reason = string.IsNullOrWhiteSpace(reason) ? "Violates community standards" : reason
            };
            await _reportService.CreateCourseReviewReportAsync(userId, request);
        }
        else
        {
            var request = new CreateLessonReviewReportRequest
            {
                LessonReviewId = reviewId,
                Reason = string.IsNullOrWhiteSpace(reason) ? "Violates community standards" : reason
            };
            await _reportService.CreateLessonReviewReportAsync(userId, request);
        }
    }
}
