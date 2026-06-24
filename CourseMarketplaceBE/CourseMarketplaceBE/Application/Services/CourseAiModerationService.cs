using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Enums;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Application.Services
{
    public class CourseAiModerationService : ICourseAiModerationService
    {
        private readonly IContentHashService _contentHashService;
        private readonly IAiModerationService _aiModerationService;
        private readonly ICourseQueryService _courseQueryService;
        private readonly ICourseCommandService _courseCommandService;
        private readonly ILessonService _lessonService;
        private readonly IRedisService _redisService;
        private readonly ICourseAiIntegrationRepository _aiIntegrationRepository;
        private readonly IAiModelRepository _aiModelRepository;
        private readonly ISystemConfigRepository _systemConfigRepository;
        private readonly ICourseModerationService _courseModerationService;
        private readonly ILogger<CourseAiModerationService> _logger;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly IHtmlTextManipulationService _htmlTextManipulationService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IBackgroundTaskQueue _taskQueue;

        public CourseAiModerationService(
            IContentHashService contentHashService,
            IAiModerationService aiModerationService,
            ICourseQueryService courseQueryService,
            ICourseCommandService courseCommandService,
            ILessonService lessonService,
            IRedisService redisService,
            ICourseAiIntegrationRepository aiIntegrationRepository,
            IAiModelRepository aiModelRepository,
            ISystemConfigRepository systemConfigRepository,
            ICourseModerationService courseModerationService,
            ILogger<CourseAiModerationService> logger,
            ICourseRepository courseRepository,
            IMapper mapper,
            IHtmlTextManipulationService htmlTextManipulationService,
            IEmbeddingService embeddingService,
            IBackgroundTaskQueue taskQueue)
        {
            _contentHashService = contentHashService;
            _aiModerationService = aiModerationService;
            _courseQueryService = courseQueryService;
            _courseCommandService = courseCommandService;
            _lessonService = lessonService;
            _redisService = redisService;
            _aiIntegrationRepository = aiIntegrationRepository;
            _aiModelRepository = aiModelRepository;
            _systemConfigRepository = systemConfigRepository;
            _courseModerationService = courseModerationService;
            _logger = logger;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _htmlTextManipulationService = htmlTextManipulationService;
            _embeddingService = embeddingService;
            _taskQueue = taskQueue;
        }

        public async Task StartCourseModerationAsync(CourseModerationRequest request, int instructorId)
        {
            // 1. Enforce all business validation checks, lockouts, and reset rejected statuses synchronously
            await UpdateCourseStatusAndClearCacheAsync(request.CourseId, CourseStatus.Pending.ToValue(), instructorId);

            // 2. Queue AI moderation for background processing
            await _taskQueue.QueueBackgroundWorkItemAsync<ICourseAiModerationService>(async (moderationService, token) =>
            {
                try
                {
                    await moderationService.HandleCourseModerationAsync(request);
                }
                catch (Exception)
                {
                    // Exceptions should be logged internally by the moderation service
                }
            });
        }

        public async Task<CourseModerationDetailResponse?> GetCourseForModerationAsync(int courseId)
        {
            string cacheKey = CacheKeys.CourseModerationDetail.GetKey(courseId);
            _logger.LogInformation("GetCourseForModerationAsync: {CacheKey}", cacheKey);
            var response = await _redisService.GetCacheAsync<CourseModerationDetailResponse>(cacheKey);

            if (response == null)
            {
                var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);
                if (course == null) return null;

                response = _mapper.Map<CourseModerationDetailResponse>(course);

                ExtractPlainTextForModerationResponse(response);

                await _redisService.SetCacheAsync(cacheKey, response, CacheTtl.Short.GetTtl());
                _logger.LogInformation("Cached moderation course {CourseId} with key {CacheKey}", courseId, cacheKey);
            }

            return response;
        }

        public async Task UpdateCourseStatusAndClearCacheAsync(int courseId, string status, int instructorId)
        {
            await _courseCommandService.UpdateCourseStatusAsync(courseId, status, instructorId);
            await _redisService.RemoveCacheAsync(CacheKeys.CourseModerationDetail.GetKey(courseId));
        }

        private void ExtractPlainTextForModerationResponse(CourseModerationDetailResponse response)
        {
            response.Description = _htmlTextManipulationService.ExtractPlainText(response.Description ?? "");
            response.WhatYouWillLearn = _htmlTextManipulationService.ExtractPlainText(response.WhatYouWillLearn ?? "");
            response.Requirements = _htmlTextManipulationService.ExtractPlainText(response.Requirements ?? "");

            if (response.Lessons != null)
            {
                foreach (var lesson in response.Lessons)
                {
                    if (lesson.LearningMaterials != null)
                    {
                        foreach (var material in lesson.LearningMaterials)
                        {
                            material.Description = _htmlTextManipulationService.ExtractPlainText(material.Description ?? "");
                        }
                    }
                }
            }
        }

        private async Task<ExactDuplicationResult> GetExactDuplicationResult(ExactDuplicationCommand command)
        {
            var res = new ExactDuplicationResult { CourseId = command.CourseExt.CourseId, IsDup = false };
            foreach (var ext in command.ExistingCourseExts)
            {
                if (ext.CourseId == command.CourseExt.CourseId) continue;
                if (ext.TitleHash == command.CourseExt.TitleHash) res.DupFields.Add("title");
                if (ext.DescriptionHash == command.CourseExt.DescriptionHash) res.DupFields.Add("description");
                if (ext.WhatYouWillLearnHash == command.CourseExt.WhatYouWillLearnHash) res.DupFields.Add("what_you_will_learn");
                if (ext.RequirementsHash == command.CourseExt.RequirementsHash) res.DupFields.Add("requirements");
                if (ext.ThumbnailHash == command.CourseExt.ThumbnailHash && !string.IsNullOrEmpty(ext.ThumbnailHash)) res.DupFields.Add("thumbnail");
            }
            if (res.DupFields.Any()) { res.IsDup = true; res.DupFields = res.DupFields.Distinct().ToList(); }
            return res;
        }


        private async Task<ExactDuplicationResult> CheckExactDuplication(int courseId)
        {
            var current = await _contentHashService.GetCourseHashesAsync(courseId);
            var others = await _contentHashService.GetAllCourseHashesAsync();
            return await GetExactDuplicationResult(new ExactDuplicationCommand { CourseExt = current, ExistingCourseExts = others });
        }

        private async Task<AssignAIModeratorsToCourseResult> AssignAIModeratorsToCourseAsync(int courseId, List<AiModelDto> models)
        {
            var thresholds = await _aiModerationService.GetScoreThresholdConfigAsync(SystemConfigKeys.ModerationThreshold);
            var modelIds = models.Select(m => m.ModelId).ToList();

            foreach (var model in models)
            {
                var existing = await _courseQueryService.GetByModelAndCourseAsync(model.ModelId, courseId);
                if (existing == null)
                {
                    var role = $"{model.ModelType}_{model.ProcessType}".ToLower();
                    await _courseCommandService.IntegrateAItoCourseAsync(new CourseAIIntegrationCommand
                    {
                        CourseId = courseId,
                        ModelId = model.ModelId,
                        Role = role,
                        IsEnabled = true,
                        ConfigJson = thresholds
                    });
                }
            }

            return new AssignAIModeratorsToCourseResult
            {
                CourseId = courseId,
                ModelIds = modelIds,
                Thresholds = thresholds
            };
        }



        private async Task<PrepareForCourseAIModerationResult> PrepareForCourseAIModeration(int courseId)
        {
            try
            {
                var (classifiers, emb_generators) = await GetCourseModerationModelsAsync();

                // Get existing course integrations
                var thresholds = await _aiModerationService.GetScoreThresholdConfigAsync(SystemConfigKeys.ModerationThreshold);

                await UpdateCourseAIIntegrationsAsync(courseId, classifiers, emb_generators, thresholds);

                var materialIds = await GetCourseMaterialIdsAsync(courseId);

                await _embeddingService.PrepareMaterialEmbeddingsAsync();

                return new PrepareForCourseAIModerationResult
                {
                    CourseId = courseId,
                    MaterialIds = materialIds,
                    Thresholds = thresholds,
                    SemanticDeDuplicationModels = emb_generators,
                    CourseHarmfulDetectionModels = classifiers,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to prepare for AI moderation");
                throw;
            }
        }

        private async Task<(List<AiModelDto> classifiers, List<AiModelDto> emb_generators)> GetCourseModerationModelsAsync()
        {
            // Query system configs for configured model paths
            var (classifierPath, textGeneratorPath, mediaGeneratorPath) = await GetModelConfigPathsAsync();

            var classifiers = new List<AiModelDto>();
            var emb_generators = new List<AiModelDto>();

            if (!string.IsNullOrEmpty(classifierPath))
            {
                var model = await _aiModelRepository.GetByModelPathAsync(classifierPath);
                if (model != null) classifiers.Add(model);
            }
            if (!string.IsNullOrEmpty(textGeneratorPath))
            {
                var model = await _aiModelRepository.GetByModelPathAsync(textGeneratorPath);
                if (model != null) emb_generators.Add(model);
            }
            if (!string.IsNullOrEmpty(mediaGeneratorPath))
            {
                var model = await _aiModelRepository.GetByModelPathAsync(mediaGeneratorPath);
                if (model != null) emb_generators.Add(model);
            }

            // If models are not configured in system configs, fetch active ones by type as fallback
            if (classifiers.Count == 0) classifiers = await _aiModerationService.GetModelsByTypeAsync(AiModelConst.Classifier);
            if (emb_generators.Count == 0) emb_generators = await _aiModerationService.GetModelsByTypeAsync(AiModelConst.EmbeddingGenerator);

            return (classifiers, emb_generators);
        }

        private async Task<(string? classifierPath, string? textGeneratorPath, string? mediaGeneratorPath)> GetModelConfigPathsAsync()
        {
            var classifierPath = await _systemConfigRepository.GetValueAsync(SystemConfigKeys.CourseHarmfulTextClassifier);
            var textGeneratorPath = await _systemConfigRepository.GetValueAsync(SystemConfigKeys.CourseTextEmbeddingGenerator);
            var mediaGeneratorPath = await _systemConfigRepository.GetValueAsync(SystemConfigKeys.CourseMediaEmbeddingGenerator);

            return (classifierPath, textGeneratorPath, mediaGeneratorPath);
        }

        private async Task UpdateCourseAIIntegrationsAsync(
            int courseId,
            List<AiModelDto> classifiers,
            List<AiModelDto> emb_generators,
            Dictionary<string, float> thresholds)
        {
            var integrations = await _aiIntegrationRepository.GetByCourseIdAsync(courseId);
            var models = classifiers.Concat(emb_generators).ToList();
            if (integrations == null || !integrations.Any())
            {
                await AssignAIModeratorsToCourseAsync(courseId, models);
                return;
            }

            int updateCount = 0;
            foreach (var integration in integrations)
            {
                var role = integration.Role?.ToLower() ?? "";
                var integratedModelId = integration.ModelId;

                var match = models.FirstOrDefault(
                    model =>
                    role.Contains(model.ProcessType?.ToLower() ?? "") &&
                    role.Contains(model.ModelType?.ToLower() ?? "")
                    );

                if (match != null && match.ModelId != integratedModelId)
                {
                    integration.ModelId = match.ModelId;
                    integration.ConfigJson = JsonSerializer.Serialize(thresholds);
                    integration.AssignedAt = DateTime.UtcNow;

                    _aiIntegrationRepository.Update(integration);
                    updateCount++;

                }

            }

            if (updateCount > 0) await SaveCourseAiIntegrationChangesAsync();
        }

        private async Task<int> SaveCourseAiIntegrationChangesAsync()
        {
            try
            {
                int rowsAffected = await _aiIntegrationRepository.SaveChangesAsync();
                if (rowsAffected == 0) throw new InvalidOperationException("Failed to save course AI integration");
                return rowsAffected;
            }
            catch (CourseAiIntegrationException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }

        private async Task<List<int>> GetCourseMaterialIdsAsync(int courseId)
        {
            var course = await GetCourseForModerationAsync(courseId); // Cache course
            var materialIds = course?.Lessons?
                .SelectMany(lesson => lesson.LearningMaterials?.Select(material => material.MaterialId) ?? [])
                .ToList() ?? [];
            return materialIds;
        }


        private async Task ResolveCourseAIModerationResult(CourseModerationResult result)
        {
            _logger.LogInformation("Resolving AI moderation result for course {CourseId}", result.CourseId);

            await _embeddingService.PersistMaterialEmbeddingsAsync(result.CourseId);

            var (threatLevel, feedback) = EvaluateModerationFeedback(result);

            await _courseCommandService.UpdateCourseStatusAndFeedbackAsync(
                result.CourseId,
                CourseStatus.Pending.ToValue(),
                feedback,
                threatLevel);

            var notificationContent = $"Course {result.CourseId} requires manual review following AI Moderation. Threat Level: {threatLevel}.\n\nDetails:\n{feedback}";
            await _courseModerationService.NotifyAdminAsync("Manual Audit Required", notificationContent, UrlConst.AdminCourseModerationURL);
        }

        private (AiThreatLevel ThreatLevel, string Feedback) EvaluateModerationFeedback(CourseModerationResult result)
        {
            string statusStr = result.ModerationStatus;
            AiThreatLevel threatLevel = AiThreatLevel.None;
            string feedback = "";

            if (statusStr == ModerationStatus.Approved.ToValue())
            {
                threatLevel = AiThreatLevel.Approved;
                feedback = "AI moderation passed. Awaiting manual review.";
            }
            else if (statusStr == ModerationStatus.Rejected.ToValue() || statusStr == ModerationStatus.Flagged.ToValue())
            {
                threatLevel = AiThreatLevel.FlaggedOrRejected;

                var flaggedReasons = new List<string>();
                foreach (var log in result.StageLogs)
                {
                    if (log.Result == StageLogResult.Flagged.ToValue() || log.Result == StageLogResult.MatchFound.ToValue())
                    {
                        var fields = string.Join(", ", log.FlaggedFields);
                        var reasonText = log.Reason ?? "AI moderation flag";
                        flaggedReasons.Add($"[{fields}] {reasonText}");
                    }
                }

                if (flaggedReasons.Any())
                {
                    feedback = "AI flagged the following issues:\n- " + string.Join("\n- ", flaggedReasons);
                }
                else
                {
                    feedback = "Course content violates the moderation policy according to AI.";
                }
            }
            else if (statusStr == ModerationStatus.ManualAudit.ToValue())
            {
                threatLevel = AiThreatLevel.ManualAudit;
                feedback = "AI suggested manual audit.";
            }

            return (threatLevel, feedback);
        }


        private async Task LogCourseAiModeration(LogCourseAiModerationCommand command)
        {
            _logger.LogInformation("Logging course AI moderation for course {CourseId}", command.CourseModerationResult.CourseId);
            var result = command.CourseModerationResult;
            foreach (var stage in result.StageLogs)
            {
                _logger.LogInformation("Logging course AI moderation for course {CourseId} and stage {Stage}", command.CourseModerationResult.CourseId, stage.Stage);
                var integration = await _aiIntegrationRepository.GetByModelAndCourseAsync(stage.ModelId, result.CourseId);
                if (integration == null)
                {
                    _logger.LogWarning("Integration not found for course {CourseId} and model {ModelId}", result.CourseId, stage.ModelId);
                    continue;
                }

                var semRq = command.SemanticDuplicationRequest;
                var harmRq = command.CourseHarmfulRequest;

                _logger.LogInformation("Saving Log for course AI moderation for course {CourseId} and integration {IntegrationId}", command.CourseModerationResult.CourseId, integration.Id);
                var inputJson = JsonSerializer.Serialize(new CourseAiUsageLogInput
                {
                    CourseId = result.CourseId,
                    MaterialIds = semRq.MaterialIds,
                    SimilarityScoreThreshold = semRq.SimilarityScoreThreshold,
                    SpamScoreThreshold = harmRq.SpamScoreThreshold,
                    ToxicScoreThreshold = harmRq.ToxicScoreThreshold
                });
                _logger.LogInformation("Input JSON for course AI moderation for course {CourseId} and integration {IntegrationId}: {InputJson}", command.CourseModerationResult.CourseId, integration.Id, inputJson);
                var outputJson = JsonSerializer.Serialize(new CourseAiUsageLogOutput
                {
                    Stage = stage.Stage,
                    Step = stage.Step,
                    Timestamp = stage.Timestamp,
                    Result = stage.Result,
                    Reason = stage.Reason,
                    FlaggedFields = stage.FlaggedFields,
                    Details = stage.Details,
                    ConfidenceScore = stage.ConfidenceScore

                });
                _logger.LogInformation("Output JSON for course AI moderation for course {CourseId} and integration {IntegrationId}: {OutputJson}", command.CourseModerationResult.CourseId, integration.Id, outputJson);
                await _aiModerationService.SaveCourseAiUsageLog(new SaveCourseAiUsageLogCommand
                {
                    IntegrationId = integration.Id,
                    InteractionType = command.InteractionType,
                    InputJson = inputJson,
                    OutputJson = outputJson,
                    LatencyMs = stage.LatencyMs,
                    TokenUsage = 0,
                    ErrorMessage = command.ErrorMessage
                });
                _logger.LogInformation("Saved Log for course AI moderation for course {CourseId} and integration {IntegrationId}", command.CourseModerationResult.CourseId, integration.Id);
            }
        }



        private async Task<CourseModerationResult> HandleCourseModerationWithAIAsync(CourseModerationRequest request)
        {
            try
            {
                var isHealthy = await _aiModerationService.HealthCheckAsync();
                if (!isHealthy)
                {
                    await _courseModerationService.NotifyAdminAsync("AI Service Unhealthy", $"Course {request.CourseId} requires manual review due to AI service being unhealthy.", UrlConst.AdminCourseModerationURL);
                    return new CourseModerationResult { CourseId = request.CourseId, ModerationStatus = ModerationStatus.ManualAudit.ToValue() };
                }

                var prep = await PrepareForCourseAIModeration(request.CourseId);

                var (semanticReq, harmfulReq) = CreateModerationRequests(request.CourseId, prep);

                var result = await _aiModerationService.ModerateCourseFullPipelineAsync(semanticReq, harmfulReq);

                _logger.LogInformation("AI Moderation Result: {result}", JsonSerializer.Serialize(result));
                if (result.ModerationStatus == ModerationStatus.ManualAudit.ToValue())
                {
                    await _courseModerationService.NotifyAdminAsync("Manual Audit Required", $"Course {request.CourseId} flagged for manual review by AI.", UrlConst.AdminCourseModerationURL);
                }
                else
                {
                    await ResolveCourseAIModerationResult(result);
                }

                await LogCourseAiModeration(new LogCourseAiModerationCommand
                {
                    SemanticDuplicationRequest = semanticReq,
                    CourseHarmfulRequest = harmfulReq,
                    CourseModerationResult = result,
                    InteractionType = AIInteractionType.Moderation.ToValue(),
                    ErrorMessage = null
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AI moderation for course {CourseId}", request.CourseId);
                await _courseModerationService.NotifyAdminAsync("Moderation Process Exception", $"Exception during AI moderation for course {request.CourseId}: {ex.Message}", UrlConst.AdminCourseModerationURL);
                throw;
            }
        }

        private (SemanticDuplicationRequest SemanticReq, CourseHarmfulRequest HarmfulReq) CreateModerationRequests(
            int courseId,
            PrepareForCourseAIModerationResult prep)
        {
            var thresholds = prep.Thresholds;
            var materialIds = prep.MaterialIds;
            var semDupModels = prep.SemanticDeDuplicationModels;
            var courseHarmModels = prep.CourseHarmfulDetectionModels;

            var semanticReq = new SemanticDuplicationRequest
            {
                CourseId = courseId,
                MaterialIds = materialIds,
                SimilarityScoreThreshold = thresholds.GetValueOrDefault(
                    AiModelConst.Similarity,
                    AiModelConst.DefaultSimilarityScoreThreshold),
                Models = semDupModels
            };

            var harmfulReq = new CourseHarmfulRequest
            {
                CourseId = courseId,
                SpamScoreThreshold = thresholds.GetValueOrDefault(
                    AiModelConst.Spam,
                    AiModelConst.DefaultSpamScoreThreshold),
                ToxicScoreThreshold = thresholds.GetValueOrDefault(
                    AiModelConst.Toxic,
                    AiModelConst.DefaultToxicScoreThreshold),
                Models = courseHarmModels
            };

            return (semanticReq, harmfulReq);
        }

        public async Task<CourseModerationResult> HandleCourseModerationAsync(CourseModerationRequest request)
        {
            try
            {
                return await HandleCourseModerationWithAIAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleCourseModerationAsync for course {CourseId}", request.CourseId);
                throw;
            }
        }

        // Legacy hash dedup logic in HandleCourseModerationAsync: 
        //      var exactDeDupRes = await HandleExactDeDuplication(request.CourseId);
        //      if(exactDeDupRes != null){
        //          return exactDeDupRes;
        //      }
        private async Task<CourseModerationResult?> HandleExactDeDuplication(int courseId)
        {
            var dupResult = await CheckExactDuplication(courseId);
            if (dupResult.IsDup)
            {
                var items = dupResult.DupFields.Select(f => new RejectCourseItemDto
                {
                    Target = $"course.{f}",
                    Reason = "Exact duplication with an existing course found."
                }).ToList();

                await _courseModerationService.RejectCourseDetailedAsync(
                    new RejectCourseDetailedRequest
                    {
                        CourseId = courseId,
                        Items = items
                    }
                );


                return new CourseModerationResult
                {
                    CourseId = courseId,
                    ModerationStatus = ModerationStatus.Rejected.ToValue(),
                    FlaggedFields = dupResult.DupFields,
                    OverallConfidenceScore = 1.0f,
                    TotalLatencyMs = 0,
                    StageLogs = []
                };
            }
            return null;
        }
    }
}
