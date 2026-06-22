using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;

namespace CourseMarketplaceBE.Application.IServices;

public interface IAiModerationLogService
{
    Task<PagedResult<CourseModerationLogAdminDto>> GetCourseModerationLogsAsync(PagedRequestDto req);
    Task<CourseModerationLogAdminDto?> GetCourseModerationLogDetailAsync(int logId);

    Task<PagedResult<ReviewModerationLogAdminDto>> GetCourseReviewModerationLogsAsync(PagedRequestDto req);
    Task<ReviewModerationLogAdminDto?> GetCourseReviewModerationLogDetailAsync(int logId);

    Task<PagedResult<ReviewModerationLogAdminDto>> GetLessonReviewModerationLogsAsync(PagedRequestDto req);
    Task<ReviewModerationLogAdminDto?> GetLessonReviewModerationLogDetailAsync(int logId);
}
