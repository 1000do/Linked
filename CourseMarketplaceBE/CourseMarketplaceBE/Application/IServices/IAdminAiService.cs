using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IAdminAiService
    {
        // Models
        Task<List<AiModelAdminDto>> GetAllModelsAsync();
        Task<(List<AiModelAdminDto> Items, int TotalCount)> GetPagedModelsAsync(int page, int pageSize);
        Task<AiModelAdminDto> GetModelByIdAsync(int id);
        Task<AiModelAdminDto> AddModelAsync(CreateAiModelRequest req);
        Task<AiModelAdminDto> UpdateModelAsync(int id, UpdateAiModelRequest req);
        Task ToggleModelStatusAsync(int id);

        // Configurations
        Task<AiConfigurationDto> GetConfigurationsAsync();
        Task UpdateThresholdsAsync(UpdateThresholdsRequest req);
        Task UpdateIntegrationAsync(UpdateIntegrationRequest req);

        // Logs
        Task<(List<CourseModerationLogAdminDto> Items, int TotalCount)> GetCourseModerationLogsAsync(int page, int pageSize);
        Task<CourseModerationLogAdminDto?> GetCourseModerationLogDetailAsync(int logId);

        Task<(List<ReviewModerationLogAdminDto> Items, int TotalCount)> GetCourseReviewModerationLogsAsync(int page, int pageSize);
        Task<ReviewModerationLogAdminDto?> GetCourseReviewModerationLogDetailAsync(int logId);

        Task<(List<ReviewModerationLogAdminDto> Items, int TotalCount)> GetLessonReviewModerationLogsAsync(int page, int pageSize);
        Task<ReviewModerationLogAdminDto?> GetLessonReviewModerationLogDetailAsync(int logId);
    }
}
