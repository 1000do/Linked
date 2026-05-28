using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices
{
    /// <summary>
    /// Service for calling AI moderation pipeline.
    /// Communicates with FastAPI service via HTTP and manages AI logs.
    /// </summary>
    public interface IAiModerationService
    {
        Task<CourseModerationResult> ModerateCourseFullPipelineAsync(SemanticDuplicationRequest semanticReq, CourseHarmfulRequest harmfulReq);
        Task<Dictionary<string, float>> GetScoreThresholdConfigAsync(string key);
        Task<List<int>> GetModelIdsByType(string type);
        Task<List<AiModelDto>> GetModelsByTypeAsync(string modelType);
        Task SaveCourseAiUsageLog(SaveCourseAiUsageLogCommand command);
        Task<bool> HealthCheckAsync();
    }
}
