using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IAdminAiService
    {
        // Models
        Task<List<AiModelAdminDto>> GetAllModelsAsync();
        // Task<(List<AiModelAdminDto> Items, int TotalCount)> GetPagedModelsAsync(int page, int pageSize);
        Task<PagedResult<AiModelAdminDto>> GetPagedModelsAsync(PagedRequestDto req);
        Task<AiModelAdminDto> GetModelByIdAsync(int id);
        Task<AiModelAdminDto> AddModelAsync(CreateAiModelRequest req);
        Task<AiModelAdminDto> UpdateModelAsync(int id, UpdateAiModelRequest req);
        Task<bool> ToggleModelStatusAsync(int id);

        // Configurations
        Task<AiConfigurationDto> GetConfigurationsAsync();
        Task<bool> UpdateThresholdsAsync(UpdateThresholdsRequest req);
        Task<bool> UpdateIntegrationAsync(UpdateIntegrationRequest req);

        // Logs
        // Task<(List<CourseModerationLogAdminDto> Items, int TotalCount)> GetCourseModerationLogsAsync(int page, int pageSize);
        Task<PagedResult<CourseModerationLogAdminDto>> GetCourseModerationLogsAsync(PagedRequestDto req);
        Task<CourseModerationLogAdminDto?> GetCourseModerationLogDetailAsync(int logId);

        // Task<(List<ReviewModerationLogAdminDto> Items, int TotalCount)> GetCourseReviewModerationLogsAsync(int page, int pageSize);
        Task<PagedResult<ReviewModerationLogAdminDto>> GetCourseReviewModerationLogsAsync(PagedRequestDto req);
        Task<ReviewModerationLogAdminDto?> GetCourseReviewModerationLogDetailAsync(int logId);

        // Task<(List<ReviewModerationLogAdminDto> Items, int TotalCount)> GetLessonReviewModerationLogsAsync(int page, int pageSize);
        Task<PagedResult<ReviewModerationLogAdminDto>> GetLessonReviewModerationLogsAsync(PagedRequestDto req);
        Task<ReviewModerationLogAdminDto?> GetLessonReviewModerationLogDetailAsync(int logId);
    }
}
