using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Application.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;
    private readonly ICheckoutRepository _checkoutRepo;
    private readonly ICourseRepository _courseRepo;

    public ReviewService(AppDbContext context, ICheckoutRepository checkoutRepo, ICourseRepository courseRepo)
    {
        _context = context;
        _checkoutRepo = checkoutRepo;
        _courseRepo = courseRepo;
    }

    // ── Helper: Kiểm tra user có phải instructor sở hữu khóa học ───────

    private async Task<bool> IsOwnerAsync(int userId, int courseId)
    {
        return await _context.Courses
            .AnyAsync(c => c.CourseId == courseId && c.InstructorId == userId);
    }

    // ── Lấy hoặc tạo enrollment cho instructor (auto-enroll chủ khóa) ──

    private async Task<Enrollment> GetOrCreateOwnerEnrollmentAsync(int userId, int courseId)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Progress)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

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
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Tạo progress record
            var progress = new EnrollmentProgress
            {
                EnrollmentId = enrollment.EnrollmentId,
                LearnedMaterialCount = 0,
                LastModifiedAt = DateTime.Now
            };
            await _context.EnrollmentProgresses.AddAsync(progress);
            await _context.SaveChangesAsync();
        }

        return enrollment;
    }

    // ── Lấy danh sách reviews ──────────────────────────────────────────

    public async Task<IEnumerable<ReviewResponse>> GetCourseReviewsAsync(int courseId)
    {
        // Lấy instructor_id của khóa học
        var course = await _context.Courses.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CourseId == courseId);
        var instructorId = course?.InstructorId;

        return await _context.Reviews
            .Include(r => r.Enrollment)
                .ThenInclude(e => e!.User)
                    .ThenInclude(u => u!.UserNavigation)
            .Include(r => r.Lesson)
            .Where(r => r.Enrollment != null && r.Enrollment.CourseId == courseId && r.IsRemoved != true)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewResponse
            {
                ReviewId = r.ReviewId,
                UserFullName = r.Enrollment!.User!.FullName ?? "Anonymous",
                UserAvatarUrl = r.Enrollment!.User!.UserNavigation.AvatarUrl,
                Rating = r.Rating ?? 0,
                Comment = r.Comment ?? "",
                CreatedAt = r.CreatedAt ?? DateTime.Now,
                ReviewSource = r.ReviewSource ?? "detail",
                LessonTitle = r.Lesson != null ? r.Lesson.Title : null,
                LessonId = r.LessonId,
                IsInstructor = r.Enrollment.UserId == instructorId
            })
            .ToListAsync();
    }

    // ── Thống kê phân bổ sao ───────────────────────────────────────────

    public async Task<ReviewStatsResponse> GetReviewStatsAsync(int courseId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Enrollment)
            .Where(r => r.Enrollment != null && r.Enrollment.CourseId == courseId && r.IsRemoved != true && r.Rating != null)
            .Select(r => r.Rating!.Value)
            .ToListAsync();

        var total = reviews.Count;
        return new ReviewStatsResponse
        {
            AverageRating = total > 0 ? Math.Round(reviews.Average(), 1) : 0,
            TotalReviews = total,
            Star5Count = reviews.Count(r => r >= 4.5f),
            Star4Count = reviews.Count(r => r >= 3.5f && r < 4.5f),
            Star3Count = reviews.Count(r => r >= 2.5f && r < 3.5f),
            Star2Count = reviews.Count(r => r >= 1.5f && r < 2.5f),
            Star1Count = reviews.Count(r => r < 1.5f)
        };
    }

    // ── Trạng thái enrollment + quyền review ───────────────────────────

    public async Task<EnrollmentStatusResponse> GetEnrollmentStatusAsync(int userId, int courseId)
    {
        bool isOwner = await IsOwnerAsync(userId, courseId);

        // Nếu là chủ nhân → auto-enroll nếu chưa có
        if (isOwner)
        {
            var ownerEnrollment = await GetOrCreateOwnerEnrollmentAsync(userId, courseId);
            var hasReviewed = await _context.Reviews
                .AnyAsync(r => r.EnrollmentId == ownerEnrollment.EnrollmentId && r.IsRemoved != true);

            return new EnrollmentStatusResponse
            {
                IsEnrolled = true,
                IsCompleted = true,
                ProgressPercentage = 100,
                LearnedMaterialCount = 0,
                TotalMaterialCount = 0,
                CanReview = true, // Chủ nhân luôn được review
                ReviewBlockedReason = null,
                HasReviewed = hasReviewed,
                IsOwner = true
            };
        }

        var enrollment = await _context.Enrollments
            .Include(e => e.Progress)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

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
        var learnedCount = enrollment.Progress?.LearnedMaterialCount ?? 0;
        var pct = totalMaterials > 0 ? (double)learnedCount / totalMaterials * 100 : 0;
        var isCompleted = enrollment.IsCompleted == true;

        var hasReviewedNormal = await _context.Reviews
            .AnyAsync(r => r.EnrollmentId == enrollment.EnrollmentId && r.IsRemoved != true);

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
            enrollment = await _context.Enrollments
                .Include(e => e.Progress)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == request.CourseId)
                ?? throw new InvalidOperationException("Bạn cần ghi danh khóa học trước khi đánh giá.");

            if (requireCompletion)
            {
                if (enrollment.IsCompleted != true)
                    throw new InvalidOperationException("Bạn cần hoàn thành khóa học trước khi đánh giá tại trang chi tiết.");
            }
            else
            {
                var learnedCount = enrollment.Progress?.LearnedMaterialCount ?? 0;
                if (learnedCount <= 0)
                    throw new InvalidOperationException("Bạn cần học ít nhất 1 bài học trước khi đánh giá.");
            }
        }

        // ── Validate input ──
        if (request.Rating < 1 || request.Rating > 5)
            throw new InvalidOperationException("Rating phải từ 1 đến 5 sao.");

        if (string.IsNullOrWhiteSpace(request.Comment))
            throw new InvalidOperationException("Nội dung đánh giá không được để trống.");

        // Xác định review source
        string source = requireCompletion ? "detail" : "learn";

        // ── Upsert logic ──
        // Cho phép mỗi enrollment có nhiều review (1 review tổng + N review per-lesson)
        // Nhưng mỗi lesson chỉ 1 review, và chỉ 1 review tổng
        Review? existingReview;

        if (request.LessonId.HasValue)
        {
            // Review cho lesson cụ thể → tìm theo enrollment + lessonId
            existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.EnrollmentId == enrollment.EnrollmentId 
                    && r.LessonId == request.LessonId.Value
                    && r.IsRemoved != true);
        }
        else
        {
            // Review tổng (detail page) → tìm theo enrollment + LessonId = null
            existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.EnrollmentId == enrollment.EnrollmentId 
                    && r.LessonId == null
                    && r.IsRemoved != true);
        }

        if (existingReview != null)
        {
            existingReview.Rating = request.Rating;
            existingReview.Comment = request.Comment;
            existingReview.ReviewSource = source;
            existingReview.UpdatedAt = DateTime.Now;
        }
        else
        {
            var review = new Review
            {
                EnrollmentId = enrollment.EnrollmentId,
                Rating = request.Rating,
                Comment = request.Comment,
                ReviewSource = source,
                LessonId = request.LessonId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsRemoved = false
            };
            await _context.Reviews.AddAsync(review);
        }

        await _context.SaveChangesAsync();
    }
}
