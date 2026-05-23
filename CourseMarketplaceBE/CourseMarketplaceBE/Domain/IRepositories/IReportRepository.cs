using CourseMarketplaceBE.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface IReportRepository
{
    // ── Course Reports ──────────────────────────────────────────────────────

    Task<CourseReport?> GetCourseReportByIdAsync(int reportId);
    Task<List<CourseReport>> GetAllCourseReportsAsync(string? status = null);
    Task<List<CourseReport>> GetCourseReportsByReporterAsync(int reporterId);
    Task<List<CourseReport>> GetCourseReportsByCourseAsync(int courseId);
    Task<CourseReport?> GetPendingCourseReportAsync(int reporterId, int courseId);
    Task AddCourseReportAsync(CourseReport report);
    void UpdateCourseReport(CourseReport report);

    // ── Course Review Reports ───────────────────────────────────────────────

    Task<CourseReviewReport?> GetCourseReviewReportByIdAsync(int reportId);
    Task<List<CourseReviewReport>> GetAllCourseReviewReportsAsync(string? status = null);
    Task<List<CourseReviewReport>> GetCourseReviewReportsByReporterAsync(int reporterId);
    Task<CourseReviewReport?> GetPendingCourseReviewReportAsync(int reporterId, int reviewId);
    Task AddCourseReviewReportAsync(CourseReviewReport report);
    void UpdateCourseReviewReport(CourseReviewReport report);

    // ── Lesson Review Reports ───────────────────────────────────────────────

    Task<LessonReviewReport?> GetLessonReviewReportByIdAsync(int reportId);
    Task<List<LessonReviewReport>> GetAllLessonReviewReportsAsync(string? status = null);
    Task<List<LessonReviewReport>> GetLessonReviewReportsByReporterAsync(int reporterId);
    Task<LessonReviewReport?> GetPendingLessonReviewReportAsync(int reporterId, int reviewId);
    Task AddLessonReviewReportAsync(LessonReviewReport report);
    void UpdateLessonReviewReport(LessonReviewReport report);

    // ── Shared ──────────────────────────────────────────────────────────────

    Task SaveChangesAsync();
}
