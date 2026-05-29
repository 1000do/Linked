using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IModerationService
    {
        // Khóa học
        Task<List<CourseModerationDto>> GetPendingCoursesAsync(ModerationFilterDto filter);
        Task<bool> ApproveCourseAsync(int courseId, string? feedback);
        Task<bool> RejectCourseAsync(int courseId, string reason);
        Task<bool> RejectCourseDetailedAsync(RejectCourseDetailedRequest request);
        Task<bool> FlagCourseAsync(int courseId, string reason);
        Task<bool> FlagCourseDetailedAsync(RejectCourseDetailedRequest request);
        // Khiếu nại
        Task<List<UserReportModerationDto>> GetAllReportsAsync();
        Task<bool> ResolveReportAsync(ResolveReportDto dto);

        Task<ExactDuplicationResult> GetExactDuplicationResult(ExactDuplicationCommand command);
        Task<ExactDuplicationResult> CheckExactDuplication(int courseId);
        Task NotifyAdminAsync(string title, string content, string? linkAction);
        Task PrepareMaterialEmbeddingsAsync();
        Task<AssignAIModeratorsToCourseResult> AssignAIModeratorsToCourseAsync(int courseId, List<AiModelDto> models);
        Task<PrepareForCourseAIModerationResult> PrepareForCourseAIModeration(int courseId);
        Task ResolveCourseAIModerationResult(CourseModerationResult result);
        Task LogCourseAiModeration(LogCourseAiModerationCommand command);
        Task<CourseModerationResult> HandleCourseModerationWithAIAsync(CouresModerationRequest request);
        Task<CourseModerationResult> HandleCourseModerationAsync(CouresModerationRequest request);
    }
}
