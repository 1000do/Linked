using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;

namespace CourseMarketplaceBE.Application.IServices;

public interface IReviewModerationService
{
    Task<PagedResult<ReviewModerationRecordDto>> GetCourseReviewModerationRecordsAsync(PagedReviewModerationRequest request);
    Task<PagedResult<ReviewModerationRecordDto>> GetLessonReviewModerationRecordsAsync(PagedReviewModerationRequest request);
    Task ApproveReviewAsync(ApproveRejectReviewRequest request);
    Task RejectReviewAsync(ApproveRejectReviewRequest request);
    Task<object> GetModerationStatsAsync();
}
