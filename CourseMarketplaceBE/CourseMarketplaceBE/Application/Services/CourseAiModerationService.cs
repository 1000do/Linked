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
        private readonly IAiModerationService _aiModerationService;
        private readonly IAiModelManagementService _aiModelManagementService;
        private readonly IAiModerationLogService _aiModerationLogService;
        private readonly IAiConfigurationService _aiConfigurationService;
        private readonly ICourseQueryService _courseQueryService;
        private readonly ICourseCommandService _courseCommandService;
        private readonly IRedisService _redisService;
        private readonly ICourseAiIntegrationRepository _aiIntegrationRepository;
        private readonly IAiModelRepository _aiModelRepository;
        private readonly ISystemConfigRepository _systemConfigRepository;
        private readonly ILogger<CourseAiModerationService> _logger;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly IHtmlTextManipulationService _htmlTextManipulationService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IUserRepository _userRepo;
        private readonly INotificationService _notificationService;

        public CourseAiModerationService(
            IAiModerationService aiModerationService,
            IAiModelManagementService aiModelManagementService,
            IAiModerationLogService aiModerationLogService,
            IAiConfigurationService aiConfigurationService,
            ICourseQueryService courseQueryService,
            ICourseCommandService courseCommandService,
            IRedisService redisService,
            ICourseAiIntegrationRepository aiIntegrationRepository,
            IAiModelRepository aiModelRepository,
            ISystemConfigRepository systemConfigRepository,
            ILogger<CourseAiModerationService> logger,
            ICourseRepository courseRepository,
            IMapper mapper,
            IHtmlTextManipulationService htmlTextManipulationService,
            IEmbeddingService embeddingService,
            IBackgroundTaskQueue taskQueue,
            IUserRepository userRepo,
            INotificationService notificationService)
        {
            _aiModerationService = aiModerationService;
            _aiModelManagementService = aiModelManagementService;
            _aiModerationLogService = aiModerationLogService;
            _aiConfigurationService = aiConfigurationService;
            _courseQueryService = courseQueryService;
            _courseCommandService = courseCommandService;
            _redisService = redisService;
            _aiIntegrationRepository = aiIntegrationRepository;
            _aiModelRepository = aiModelRepository;
            _systemConfigRepository = systemConfigRepository;
            _logger = logger;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _htmlTextManipulationService = htmlTextManipulationService;
            _embeddingService = embeddingService;
            _taskQueue = taskQueue;
            _userRepo = userRepo;
            _notificationService = notificationService;
        }

        public async Task<bool> StartCourseModerationAsync(CourseModerationRequest request, int instructorId)
        {
            // 1. Enforce all business validation checks, lockouts, and reset rejected statuses synchronously
            await UpdateCourseStatusAndClearCacheAsync(request.CourseId, CourseStatus.Pending.ToValue(), instructorId);

            // 2. Queue AI moderation for background processing
            await _taskQueue.QueueBackgroundWorkItemAsync<ICourseAiModerationService>(async (moderationService, token) =>
            {
                try
                {
                    await moderationService.HandleCourseModerationWithAIAsync(request);
                }
                catch (Exception)
                {
                    // Exceptions should be logged internally by the moderation service
                }
            });

            return true;
        }

        private async Task<CourseModerationDetailResponse?> GetCourseForModerationAsync(int courseId)
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

        private async Task UpdateCourseStatusAndClearCacheAsync(int courseId, string status, int instructorId)
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

       

        private async Task<Dictionary<string, float>> GetModerationThresholdsAsync()
        {
            var config = await _aiConfigurationService.GetConfigurationsAsync();
            return new Dictionary<string, float>
            {
                { AiModelConst.Similarity, config.SimilarityScoreThreshold },
                { AiModelConst.Spam, config.SpamConfidenceThreshold },
                { AiModelConst.Toxic, config.ToxicityConfidenceThreshold }
            };
        }

        private async Task<AssignAIModeratorsToCourseResult> AssignAIModeratorsToCourseAsync(int courseId, List<AiModelDto> models)
        {
            var thresholds = await GetModerationThresholdsAsync();
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
                var thresholds = await GetModerationThresholdsAsync();

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
            if (classifiers.Count == 0) classifiers = await _aiModelManagementService.GetModelsByTypeAsync(AiModelConst.Classifier);
            if (emb_generators.Count == 0) emb_generators = await _aiModelManagementService.GetModelsByTypeAsync(AiModelConst.EmbeddingGenerator);

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
                /* zero rows exception removed */
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

            var (threatLevel, feedback) = EvaluateModerationFeedback(result);

            await _courseCommandService.UpdateCourseStatusAndFeedbackAsync(
                result.CourseId,
                null,
                feedback,
                threatLevel);

            var notificationContent = $"Course {result.CourseId} requires manual review following AI Moderation. Threat Level: {threatLevel}.\n\nDetails:\n{feedback}";
            await NotifyManagersAsync("Manual Review Required", notificationContent, UrlConst.AdminCourseModerationURL);
        }

        private (AiThreatLevel ThreatLevel, string Feedback) EvaluateModerationFeedback(CourseModerationResult result)
        {
            string statusStr = result.ModerationStatus;
            AiThreatLevel threatLevel = AiThreatLevel.None;
            string feedback = "";

            var flaggedReasons = new List<string>();
            var manualAuditReasons = new List<string>();

            foreach (var log in result.StageLogs)
            {
                if (log.Result == StageLogResult.Flagged.ToValue() || log.Result == StageLogResult.MatchFound.ToValue())
                {
                    var fields = string.Join(", ", log.FlaggedFields);
                    var reasonText = log.Reason ?? "AI moderation flag";
                    flaggedReasons.Add($"[{fields}] {reasonText}");
                }
                
                if (log.ManualAuditFields != null && log.ManualAuditFields.Any())
                {
                    var fields = string.Join(", ", log.ManualAuditFields);
                    var reasonText = log.Reason ?? "AI suggested manual audit";
                    manualAuditReasons.Add($"[{fields}] {reasonText}");
                }
            }

            if (statusStr == ModerationStatus.Approved.ToValue())
            {
                threatLevel = AiThreatLevel.Approved;
                feedback = "AI moderation passed. Awaiting manual review.";
            }
            else if (statusStr == ModerationStatus.Rejected.ToValue() || statusStr == ModerationStatus.Flagged.ToValue())
            {
                threatLevel = AiThreatLevel.FlaggedOrRejected;

                if (flaggedReasons.Any())
                {
                    feedback = "AI flagged the following issues:\n- " + string.Join("\n- ", flaggedReasons);
                }
                else
                {
                    feedback = "Course content violates the moderation policy according to AI.";
                }

                if (manualAuditReasons.Any())
                {
                    feedback += "\n\nAdditionally, AI suggested manual audit for the following:\n- " + string.Join("\n- ", manualAuditReasons);
                }
            }
            else if (statusStr == ModerationStatus.ManualAudit.ToValue())
            {
                threatLevel = AiThreatLevel.ManualAudit;
                
                if (manualAuditReasons.Any())
                {
                    feedback = "AI suggested manual audit for the following:\n- " + string.Join("\n- ", manualAuditReasons);
                }
                else
                {
                    feedback = "AI suggested manual audit due to low confidence in its moderation result.";
                }
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
                    ManualAuditFields = stage.ManualAuditFields,
                    Details = stage.Details,
                    ConfidenceScore = stage.ConfidenceScore

                });
                _logger.LogInformation("Output JSON for course AI moderation for course {CourseId} and integration {IntegrationId}: {OutputJson}", command.CourseModerationResult.CourseId, integration.Id, outputJson);
                await _aiModerationLogService.SaveCourseAiUsageLog(new SaveCourseAiUsageLogCommand
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



        public async Task<CourseModerationResult> HandleCourseModerationWithAIAsync(CourseModerationRequest request)
        {
            try
            {
                var isHealthy = await _aiModerationService.HealthCheckAsync();
                if (!isHealthy)
                {
                    await NotifyManagersAsync("AI Service Unhealthy", $"Course {request.CourseId} requires manual review due to AI service being unhealthy.", UrlConst.AdminCourseModerationURL);
                    return new CourseModerationResult { CourseId = request.CourseId, ModerationStatus = ModerationStatus.ManualAudit.ToValue() };
                }

                var prep = await PrepareForCourseAIModeration(request.CourseId);

                var (semanticReq, harmfulReq) = CreateModerationRequests(request.CourseId, prep);

                var result = await _aiModerationService.ModerateCourseFullPipelineAsync(semanticReq, harmfulReq);

                _logger.LogInformation("AI Moderation Result: {result}", JsonSerializer.Serialize(result));
                
                await ResolveCourseAIModerationResult(result);

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
                await NotifyManagersAsync("Moderation Process Exception", $"Exception during AI moderation for course {request.CourseId}: {ex.Message}", UrlConst.AdminCourseModerationURL);
                return new CourseModerationResult { CourseId = request.CourseId, ModerationStatus = ModerationStatus.ManualAudit.ToValue() };
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

        private async Task NotifyManagersAsync(string title, string content, string? linkAction)
        {
            var managerIds = await _userRepo.GetAllManagerIdsAsync();
            if (managerIds.Any())
            {
                var dtos = managerIds.Select(id => new NotificationBulkDto
                {
                    ReceiverId = id,
                    Title = title,
                    Content = content,
                    LinkAction = linkAction
                }).ToList();

                await _notificationService.SendBulkNotificationsAsync(dtos);
            }
        }


        // Legacy hash dedup logic in HandleCourseModerationAsync: 
        //      var exactDeDupRes = await HandleExactDeDuplication(request.CourseId);
        //      if(exactDeDupRes != null){
        //          return exactDeDupRes;
        //      }
        // private async Task<CourseModerationResult?> HandleExactDeDuplication(int courseId)
        // {
        //     var dupResult = await CheckExactDuplication(courseId);
        //     if (dupResult.IsDup)
        //     {
        //         var items = dupResult.DupFields.Select(f => new RejectCourseItemDto
        //         {
        //             Target = $"course.{f}",
        //             Reason = "Exact duplication with an existing course found."
        //         }).ToList();

        //         await _courseModerationService.RejectCourseDetailedAsync(
        //             new RejectCourseDetailedRequest
        //             {
        //                 CourseId = courseId,
        //                 Items = items
        //             }
        //         );


        //         return new CourseModerationResult
        //         {
        //             CourseId = courseId,
        //             ModerationStatus = ModerationStatus.Rejected.ToValue(),
        //             FlaggedFields = dupResult.DupFields,
        //             OverallConfidenceScore = 1.0f,
        //             TotalLatencyMs = 0,
        //             StageLogs = []
        //         };
        //     }
        //     return null;
        // }

        // private async Task<ExactDuplicationResult> GetExactDuplicationResult(ExactDuplicationCommand command)
        // {
        //     var res = new ExactDuplicationResult { CourseId = command.CourseExt.CourseId, IsDup = false };
        //     foreach (var ext in command.ExistingCourseExts)
        //     {
        //         if (ext.CourseId == command.CourseExt.CourseId) continue;
        //         if (ext.TitleHash == command.CourseExt.TitleHash) res.DupFields.Add("title");
        //         if (ext.DescriptionHash == command.CourseExt.DescriptionHash) res.DupFields.Add("description");
        //         if (ext.WhatYouWillLearnHash == command.CourseExt.WhatYouWillLearnHash) res.DupFields.Add("what_you_will_learn");
        //         if (ext.RequirementsHash == command.CourseExt.RequirementsHash) res.DupFields.Add("requirements");
        //         if (ext.ThumbnailHash == command.CourseExt.ThumbnailHash && !string.IsNullOrEmpty(ext.ThumbnailHash)) res.DupFields.Add("thumbnail");
        //     }
        //     if (res.DupFields.Any()) { res.IsDup = true; res.DupFields = res.DupFields.Distinct().ToList(); }
        //     return res;
        // }


        // private async Task<ExactDuplicationResult> CheckExactDuplication(int courseId)
        // {
        //     var current = await _contentHashService.GetCourseHashesAsync(courseId);
        //     var others = await _contentHashService.GetAllCourseHashesAsync();
        //     return await GetExactDuplicationResult(new ExactDuplicationCommand { CourseExt = current, ExistingCourseExts = others });
        // }
    }
}
