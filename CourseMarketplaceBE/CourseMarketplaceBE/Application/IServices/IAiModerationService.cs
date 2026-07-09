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
        Task<bool> HealthCheckAsync();
    }
}
