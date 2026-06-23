using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;

namespace CourseMarketplaceBE.Application.IServices;

public interface IReportModerationService
{
    Task<PagedResult<CourseReportDetailResponse>> GetAllCourseReportsAsync(PagedReportRequestDto request);
    Task<PagedResult<ReviewReportDetailResponse>> GetAllCourseReviewReportsAsync(PagedReportRequestDto request);
    Task<PagedResult<ReviewReportDetailResponse>> GetAllLessonReviewReportsAsync(PagedReportRequestDto request);
    Task<ReportStatsResponse> GetReportStatsAsync();

    Task<bool> ResolveCourseReportAsync(int reportId, int resolverId, ResolveReportRequest request);
    Task<bool> ResolveCourseReviewReportAsync(int reportId, int resolverId, ResolveReportRequest request);
    Task<bool> ResolveLessonReviewReportAsync(int reportId, int resolverId, ResolveReportRequest request);

    Task<bool> RemoveCourseAsync(int courseId, int adminId);
}
