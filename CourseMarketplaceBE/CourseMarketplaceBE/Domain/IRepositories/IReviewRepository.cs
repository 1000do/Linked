using CourseMarketplaceBE.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface IReviewRepository
{
    // ── Lấy danh sách reviews ─────────────────────────────────────────────
    /// <param name="starFilter">Nếu không null, chỉ lấy reviews có rating làm tròn bằng starFilter.</param>
    Task<(List<CourseReview> Items, int TotalCount)> GetCourseReviewsWithDetailsAsync(int courseId, int page, int pageSize, int? starFilter = null);
    Task<(List<LessonReview> Items, int TotalCount)> GetLessonReviewsWithDetailsAsync(int lessonId, int page, int pageSize);

    // ── Thống kê sao ─────────────────────────────────────────────────────
    Task<List<float>> GetCourseReviewRatingsAsync(int courseId);
    Task<List<float>> GetLessonReviewRatingsAsync(int lessonId);
    /// <summary>Trả về avg rating + count cho từng lesson của 1 course (để render sidebar Learn).</summary>
    Task<List<(int LessonId, double AvgRating, int Count)>> GetLessonRatingsForCourseAsync(int courseId);

    // ── Lookup theo enrollment (cho upsert check cũ — giờ không dùng cho submit nhưng giữ để GetEnrollmentStatus) ──
    Task<CourseReview?> GetCourseReviewByEnrollmentAsync(int enrollmentId);
    Task<LessonReview?> GetLessonReviewByEnrollmentAsync(int enrollmentId, int lessonId);

    // ── Lookup theo reviewId (có include Enrollment.User để verify owner) ──
    Task<CourseReview?> GetCourseReviewByIdAsync(int reviewId);
    Task<LessonReview?> GetLessonReviewByIdAsync(int reviewId);

    // ── Thêm / Sửa / Xóa mềm ────────────────────────────────────────────
    Task AddCourseReviewAsync(CourseReview review);
    Task AddLessonReviewAsync(LessonReview review);
    void UpdateCourseReview(CourseReview review);
    void UpdateLessonReview(LessonReview review);
    void SoftDeleteCourseReview(CourseReview review);
    void SoftDeleteLessonReview(LessonReview review);

    // ── Pending moderation check ────────────────────────────────
    /// <summary>Returns true if the course review has a pending or under-review report.</summary>
    Task<bool> HasPendingCourseReviewReportsAsync(int reviewId);
    /// <summary>Returns true if the lesson review has a pending or under-review report.</summary>
    Task<bool> HasPendingLessonReviewReportsAsync(int reviewId);

    Task<int> SaveChangesAsync();
}

