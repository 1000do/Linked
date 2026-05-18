using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepo;
    private readonly ICheckoutRepository _checkoutRepo;
    private readonly ICourseRepository _courseRepo;
    private readonly INotificationService _notificationService;

    public ReviewService(
        IReviewRepository reviewRepo, 
        ICheckoutRepository checkoutRepo, 
        ICourseRepository courseRepo,
        INotificationService notificationService)
    {
        _reviewRepo = reviewRepo;
        _checkoutRepo = checkoutRepo;
        _courseRepo = courseRepo;
        _notificationService = notificationService;
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
            await _checkoutRepo.SaveChangesAsync();

            // Tạo progress record
            var progress = new EnrollmentProgress
            {
                EnrollmentId = enrollment.EnrollmentId,
                LearnedMaterialCount = 0,
                LastModifiedAt = DateTime.Now
            };
            await _checkoutRepo.AddEnrollmentProgressAsync(progress);
            await _checkoutRepo.SaveChangesAsync();
            
            enrollment.Progress = progress;
        }

        return enrollment;
    }

    // ── Lấy danh sách reviews ──────────────────────────────────────────

    public async Task<IEnumerable<ReviewResponse>> GetCourseReviewsAsync(int courseId)
    {
        // Lấy instructor_id của khóa học
        var course = await _courseRepo.GetByIdAsync(courseId);
        var instructorId = course?.InstructorId;

        var reviews = await _reviewRepo.GetCourseReviewsWithDetailsAsync(courseId);

        return reviews.Select(r => new ReviewResponse
        {
            ReviewId = r.CourseReviewId,
            UserFullName = r.Enrollment!.User!.FullName ?? "Anonymous",
            UserAvatarUrl = r.Enrollment!.User!.UserNavigation.AvatarUrl,
            Rating = r.Rating ?? 0,
            Comment = r.Comment ?? "",
            CreatedAt = r.CreatedAt ?? DateTime.Now,
            LessonTitle = null,
            LessonId = null,
            IsInstructor = r.Enrollment.UserId == instructorId
        });
    }

    public async Task<IEnumerable<ReviewResponse>> GetLessonReviewsAsync(int lessonId)
    {
        // Lấy course để lấy instructor_id (cần dùng Repository tương ứng)
        // Hiện tại giả định Lesson có Include Course hoặc dùng CourseRepo
        // Để nhanh, ta fetch Lesson trước.
        // Giả sử ILessonRepository tồn tại, nhưng ở đây ta dùng _context trong code cũ.
        // Tôi sẽ dùng _courseRepo nếu có cách lấy từ lessonId, hoặc dùng _reviewRepo trả về Lesson entity.
        
        var reviews = await _reviewRepo.GetLessonReviewsWithDetailsAsync(lessonId);
        var firstReview = reviews.FirstOrDefault();
        var instructorId = firstReview?.Lesson?.Course?.InstructorId;

        return reviews.Select(r => new ReviewResponse
        {
            ReviewId = r.LessonReviewId,
            UserFullName = r.Enrollment!.User!.FullName ?? "Anonymous",
            UserAvatarUrl = r.Enrollment!.User!.UserNavigation.AvatarUrl,
            Rating = r.Rating ?? 0,
            Comment = r.Comment ?? "",
            CreatedAt = r.CreatedAt ?? DateTime.Now,
            LessonTitle = r.Lesson != null ? r.Lesson.Title : null,
            LessonId = r.LessonId,
            IsInstructor = r.Enrollment!.UserId == instructorId
        });
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
                ReviewBlockedReason = "Bạn cần ghi danh khóa học trước khi đánh giá.",
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
            ReviewBlockedReason = learnedCount == 0 ? "Bạn cần học ít nhất 1 bài học trước khi đánh giá." : null,
            HasReviewed = hasReviewedNormal,
            IsOwner = false
        };
    }

    // ── Gửi review ─────────────────────────────────────────────────────

    public async Task SubmitReviewAsync(int userId, ReviewRequest request, bool requireCompletion)
    {
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
                ?? throw new InvalidOperationException("Bạn cần ghi danh khóa học trước khi đánh giá.");

            if (requireCompletion)
            {
                if (enrollment.IsCompleted != true)
                    throw new InvalidOperationException("Bạn cần hoàn thành khóa học trước khi đánh giá tại trang chi tiết.");
            }
            else
            {
                var learnedCount = await _checkoutRepo.GetCompletedMaterialCountAsync(enrollment.EnrollmentId);
                if (learnedCount <= 0)
                    throw new InvalidOperationException("Bạn cần học ít nhất 1 bài học trước khi đánh giá.");
            }
        }

        // ── Validate input ──
        if (request.Rating < 1 || request.Rating > 5)
            throw new InvalidOperationException("Rating phải từ 1 đến 5 sao.");

        if (string.IsNullOrWhiteSpace(request.Comment))
            throw new InvalidOperationException("Nội dung đánh giá không được để trống.");

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

        await _reviewRepo.SaveChangesAsync();

        // ── Thông báo cho instructor (nếu không phải instructor tự đánh giá) ──
        if (!isOwner)
        {
            var course = await _courseRepo.GetByIdAsync(request.CourseId);
            if (course != null && course.InstructorId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "Đánh giá mới",
                    $"Khóa học '{course.Title}' vừa nhận được đánh giá {request.Rating} sao từ học viên.",
                    $"/InstructorCourse/Editor?id={request.CourseId}"
                );
            }
        }
    }

    public async Task ReportReviewAsync(int userId, int reviewId, string type, string reason)
    {
        if (type.ToLower() == "course")
        {
            var review = await _reviewRepo.GetCourseReviewByIdAsync(reviewId);
            if (review != null)
            {
                review.CourseReviewStatus = "flagged";
                review.UpdatedAt = DateTime.Now;
                _reviewRepo.UpdateCourseReview(review);
            }
        }
        else
        {
            var review = await _reviewRepo.GetLessonReviewByIdAsync(reviewId);
            if (review != null)
            {
                review.LessonReviewStatus = "flagged";
                review.UpdatedAt = DateTime.Now;
                _reviewRepo.UpdateLessonReview(review);
            }
        }
        await _reviewRepo.SaveChangesAsync();
    }
}
