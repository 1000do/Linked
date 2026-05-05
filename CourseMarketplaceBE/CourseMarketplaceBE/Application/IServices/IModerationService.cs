using CourseMarketplaceBE.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IModerationService
    {
        // Khóa học
        Task<List<CourseModerationDto>> GetPendingCoursesAsync();
        Task<bool> ApproveCourseAsync(int courseId, string? feedback);
        Task<bool> RejectCourseAsync(int courseId, string reason);
        Task<bool> FlagCourseAsync(int courseId, string reason);

        // Khiếu nại
        Task<List<UserReportModerationDto>> GetAllReportsAsync();
        Task<bool> ResolveReportAsync(ResolveReportDto dto);
    }
}
