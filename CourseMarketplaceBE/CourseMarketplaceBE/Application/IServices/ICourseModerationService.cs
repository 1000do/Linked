using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface ICourseModerationService
    {
        Task<PagedResult<CourseModerationDto>> GetPendingCoursesAsync(ModerationFilterDto filter);
        Task<bool> ApproveCourseAsync(int courseId, string? feedback);
        Task<bool> RejectCourseAsync(int courseId, string reason);
        Task<bool> RejectCourseDetailedAsync(RejectCourseDetailedRequest request);
        Task<bool> FlagCourseAsync(int courseId, string reason);
        Task<bool> FlagCourseDetailedAsync(RejectCourseDetailedRequest request);
        Task NotifyAdminAsync(string title, string content, string? linkAction);
        Task<CourseModerationStatsDto> GetCourseModerationStatsAsync();
    }
}
