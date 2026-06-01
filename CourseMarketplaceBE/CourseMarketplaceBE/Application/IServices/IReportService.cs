using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IReportService
{
    // ── User / Instructor: Tạo report ──────────────────────────────────────

    /// <summary>
    /// Tạo report khóa học.
    /// - User: cần có enrollment hợp lệ.
    /// - Instructor: không cần enrollment, chỉ không thể tự report khóa của mình.
    /// </summary>
    Task CreateCourseReportAsync(int reporterId, CreateCourseReportRequest request, bool isInstructor);

    /// <summary>Tạo report cho một đánh giá khóa học.</summary>
    Task CreateCourseReviewReportAsync(int reporterId, CreateCourseReviewReportRequest request);

    /// <summary>Tạo report cho một đánh giá bài học.</summary>
    Task CreateLessonReviewReportAsync(int reporterId, CreateLessonReviewReportRequest request);

    // ── User / Instructor: Xem lịch sử report của mình ─────────────────────

    Task<IEnumerable<MyCourseReportResponse>> GetMyCourseReportsAsync(int reporterId);
    Task<IEnumerable<MyReviewReportResponse>> GetMyReviewReportsAsync(int reporterId);

    // ── Instructor: Xem reports nhận được trên khóa học của mình ───────────

    Task<IEnumerable<CourseReportDetailResponse>> GetReportsOnMyCourseAsync(int instructorId, int courseId);

    // ── Staff / Admin: Xem tất cả reports ──────────────────────────────────

    Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseReportDetailResponse>> GetAllCourseReportsAsync(string? status = null, int page = 1, int pageSize = 10);
    Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<ReviewReportDetailResponse>> GetAllCourseReviewReportsAsync(string? status = null, int page = 1, int pageSize = 10);
    Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<ReviewReportDetailResponse>> GetAllLessonReviewReportsAsync(string? status = null, int page = 1, int pageSize = 10);
    Task<ReportStatsResponse> GetReportStatsAsync();

    // ── Staff / Admin: Xử lý report ────────────────────────────────────────

    Task<bool> ResolveCourseReportAsync(int reportId, int resolverId, ResolveReportRequest request);
    Task<bool> ResolveCourseReviewReportAsync(int reportId, int resolverId, ResolveReportRequest request);
    Task<bool> ResolveLessonReviewReportAsync(int reportId, int resolverId, ResolveReportRequest request);

    // ── Admin chỉ: Gỡ khóa học khỏi nền tảng (xóa mềm/khóa) ────────────────
    Task<bool> RemoveCourseAsync(int courseId, int adminId);
}
