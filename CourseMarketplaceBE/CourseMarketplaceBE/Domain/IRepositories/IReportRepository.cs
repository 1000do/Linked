using CourseMarketplaceBE.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface IReportRepository
{
    // ── Course Reports ──────────────────────────────────────────────────────

    Task<CourseReport?> GetCourseReportByIdAsync(int reportId);
    Task<(List<CourseReport> Items, int TotalCount)> GetAllCourseReportsAsync(string? status = null, int page = 1, int pageSize = 10);
    Task<List<CourseReport>> GetCourseReportsByReporterAsync(int reporterId);
    Task<List<CourseReport>> GetCourseReportsByCourseAsync(int courseId);
    Task<CourseReport?> GetPendingCourseReportAsync(int reporterId, int courseId, string reason);
    Task AddCourseReportAsync(CourseReport report);
    void UpdateCourseReport(CourseReport report);

    // ── Course Review Reports ───────────────────────────────────────────────

    Task<CourseReviewReport?> GetCourseReviewReportByIdAsync(int reportId);
    Task<(List<CourseReviewReport> Items, int TotalCount)> GetAllCourseReviewReportsAsync(string? status = null, int page = 1, int pageSize = 10);
    Task<List<CourseReviewReport>> GetCourseReviewReportsByReporterAsync(int reporterId);
    Task<CourseReviewReport?> GetPendingCourseReviewReportAsync(int reporterId, int reviewId, string reason);
    Task AddCourseReviewReportAsync(CourseReviewReport report);
    void UpdateCourseReviewReport(CourseReviewReport report);

    // ── Lesson Review Reports ───────────────────────────────────────────────

    Task<LessonReviewReport?> GetLessonReviewReportByIdAsync(int reportId);
    Task<(List<LessonReviewReport> Items, int TotalCount)> GetAllLessonReviewReportsAsync(string? status = null, int page = 1, int pageSize = 10);
    Task<List<LessonReviewReport>> GetLessonReviewReportsByReporterAsync(int reporterId);
    Task<LessonReviewReport?> GetPendingLessonReviewReportAsync(int reporterId, int reviewId, string reason);
    Task AddLessonReviewReportAsync(LessonReviewReport report);
    void UpdateLessonReviewReport(LessonReviewReport report);

    // ── Shared ──────────────────────────────────────────────────────────────

    Task<int> SaveChangesAsync();
    Task<int> GetResolvedReportsCountAsync(int resolverId);
}
