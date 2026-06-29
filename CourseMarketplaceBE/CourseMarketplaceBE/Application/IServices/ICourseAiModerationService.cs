using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface ICourseAiModerationService
    {
        // Task<ExactDuplicationResult> GetExactDuplicationResult(ExactDuplicationCommand command);
        // Task<ExactDuplicationResult> CheckExactDuplication(int courseId);
        // Task PrepareMaterialEmbeddingsAsync();
        // Task<AssignAIModeratorsToCourseResult> AssignAIModeratorsToCourseAsync(int courseId, List<AiModelDto> models);
        // Task<PrepareForCourseAIModerationResult> PrepareForCourseAIModeration(int courseId);
        // Task ResolveCourseAIModerationResult(CourseModerationResult result);
        // Task LogCourseAiModeration(LogCourseAiModerationCommand command);
        // Task<CourseModerationDetailResponse?> GetCourseForModerationAsync(int courseId);
        // Task UpdateCourseStatusAndClearCacheAsync(int courseId, string status, int instructorId);
        Task<CourseModerationResult> HandleCourseModerationWithAIAsync(CourseModerationRequest request);
        Task StartCourseModerationAsync(CourseModerationRequest request, int instructorId);
    }
}
