using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;

namespace CourseMarketplaceBE.Application.IServices;

public interface IReviewService
{
    // ── Lấy danh sách ────────────────────────────────────────────────────
    /// <param name="starFilter">Nếu không null, chỉ lấy reviews có rating làm tròn bằng starFilter.</param>
    Task<PagedResult<ReviewResponse>> GetCourseReviewsAsync(int courseId, int page = 1, int pageSize = 10, int? starFilter = null);
    Task<PagedResult<ReviewResponse>> GetLessonReviewsAsync(int lessonId, int page = 1, int pageSize = 10);

    // ── Thống kê sao ─────────────────────────────────────────────────────
    Task<ReviewStatsResponse> GetReviewStatsAsync(int courseId);
    Task<ReviewStatsResponse> GetLessonReviewStatsAsync(int lessonId);
    Task<List<LessonRatingStatsResponse>> GetLessonRatingsForCourseAsync(int courseId);

    // ── Gửi / Sửa / Xóa review ───────────────────────────────────────────
    Task SubmitReviewAsync(int userId, ReviewRequest request, bool requireCompletion);
    Task UpdateReviewAsync(int userId, UpdateReviewRequest request);
    Task DeleteReviewAsync(int userId, DeleteReviewRequest request);

    // ── Tiện ích ─────────────────────────────────────────────────────────
    Task<EnrollmentStatusResponse> GetEnrollmentStatusAsync(int userId, int courseId);
    Task ReportReviewAsync(int userId, int reviewId, string type, string reason);
    Task<int> CreateReviewInDatabaseAsync(TempReviewDto tempDto, int enrollmentId, string reviewStatus);
    Task<bool> UpdateReviewInDatabaseAsync(TempReviewDto tempDto, string reviewStatus);
}

