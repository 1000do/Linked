using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;

namespace CourseMarketplaceBE.Application.IServices;

public interface IReportModerationService
{
    Task<PagedResult<CourseReportDetailResponse>> GetAllCourseReportsAsync(string? status = null, int page = 1, int pageSize = 10);
    Task<PagedResult<ReviewReportDetailResponse>> GetAllCourseReviewReportsAsync(string? status = null, int page = 1, int pageSize = 10);
    Task<PagedResult<ReviewReportDetailResponse>> GetAllLessonReviewReportsAsync(string? status = null, int page = 1, int pageSize = 10);
    Task<ReportStatsResponse> GetReportStatsAsync();

    Task<bool> ResolveCourseReportAsync(int reportId, int resolverId, ResolveReportRequest request);
    Task<bool> ResolveCourseReviewReportAsync(int reportId, int resolverId, ResolveReportRequest request);
    Task<bool> ResolveLessonReviewReportAsync(int reportId, int resolverId, ResolveReportRequest request);

    Task<bool> RemoveCourseAsync(int courseId, int adminId);
}
