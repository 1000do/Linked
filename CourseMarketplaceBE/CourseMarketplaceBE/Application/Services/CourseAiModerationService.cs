using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
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
        private readonly ICourseModerationService _courseModerationService;
        private readonly ILogger<CourseAiModerationService> _logger;

        public CourseAiModerationService(
            IContentHashService contentHashService,
            IAiModerationService aiModerationService,
            ICourseQueryService courseQueryService,
            ICourseCommandService courseCommandService,
            ILessonService lessonService,
            IRedisService redisService,
            ICourseAiIntegrationRepository aiIntegrationRepository,
            ICourseModerationService courseModerationService,
            ILogger<CourseAiModerationService> logger)
        {
            _contentHashService = contentHashService;
            _aiModerationService = aiModerationService;
            _courseQueryService = courseQueryService;
            _courseCommandService = courseCommandService;
            _lessonService = lessonService;
            _redisService = redisService;
            _aiIntegrationRepository = aiIntegrationRepository;
            _courseModerationService = courseModerationService;
            _logger = logger;
        }

        public async Task<ExactDuplicationResult> GetExactDuplicationResult(ExactDuplicationCommand command)
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

        public async Task<ExactDuplicationResult> CheckExactDuplication(int courseId)
        {
            var current = await _contentHashService.GetCourseHashesAsync(courseId);
            var others = await _contentHashService.GetAllCourseHashesAsync();
            return await GetExactDuplicationResult(new ExactDuplicationCommand { CourseExt = current, ExistingCourseExts = others });
        }

        public async Task PrepareMaterialEmbeddingsAsync()
        {
            var isInitialized = await _redisService.GetCacheAsync<bool>(CacheKeys.MaterialEmbeddingInitialized.GetKey());
            if (isInitialized) return;

            var allEmbeddings = await _lessonService.GetAllMaterialEmbeddingsAsync();
            foreach (var e in allEmbeddings)
            {
                string cacheKey = CacheKeys.MaterialEmbedding.GetKey(e.EmbeddingId);
                var cached = await _redisService.GetCacheAsync<MaterialEmbedding>(cacheKey);
                if (cached == null)
                {
                    await _redisService.SetCacheAsync(cacheKey, e, CacheTtl.Medium.GetTtl());
                }
            }

            await _redisService.SetCacheAsync(CacheKeys.MaterialEmbeddingInitialized.GetKey(), true, CacheTtl.Medium.GetTtl());
        }

        public async Task<AssignAIModeratorsToCourseResult> AssignAIModeratorsToCourseAsync(int courseId)
        {
            var thresholds = await _aiModerationService.GetScoreThresholdConfigAsync(SystemConfigKeys.ModerationThreshold);
            var classifiers = await _aiModerationService.GetModelsByTypeAsync(AiModelConst.Classifier);
            var generators = await _aiModerationService.GetModelsByTypeAsync(AiModelConst.Generator);
            var modelIds = classifiers.Concat(generators).Select(m => m.ModelId).ToList();

            foreach (var mid in modelIds)
            {
                var existing = await _courseQueryService.GetByModelAndCourseAsync(mid, courseId);
                if (existing == null)
                {
                    await _courseCommandService.IntegrateAItoCourseAsync(new CourseAIIntegrationCommand
                    {
                        CourseId = courseId,
                        ModelId = mid,
                        Role = AiModelConst.Moderator,
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

        public async Task<PrepareForCourseAIModerationResult> PrepareForCourseAIModeration(int courseId)
        {
            try
            {
                var assignmentResult = await AssignAIModeratorsToCourseAsync(courseId);
                var course = await _courseQueryService.GetCourseWithDetailsAsync(courseId, 0);
                var materialIds = course?.Lessons?
                    .SelectMany(lesson => lesson.LearningMaterials?.Select(material => material.MaterialId) ?? [])
                    .ToList() ?? [];

                await PrepareMaterialEmbeddingsAsync();
                return new PrepareForCourseAIModerationResult
                {
                    CourseId = courseId,
                    MaterialIds = materialIds,
                    ModelIds = assignmentResult.ModelIds,
                    Thresholds = assignmentResult.Thresholds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to prepare for AI moderation");
                throw;
            }
        }

        public async Task ResolveCourseAIModerationResult(CourseModerationResult result)
        {
            var courseId = result.CourseId;
            var course = await _courseQueryService.GetCourseWithDetailsAsync(courseId, 0);
            if (course == null) return;

            var materialIds = course.Lessons?
                .SelectMany(l => l.LearningMaterials?.Select(m => m.MaterialId) ?? [])
                .ToList() ?? [];

            foreach (var matId in materialIds)
            {
                string cacheKey = CacheKeys.MaterialEmbedding.GetKey(matId);
                var cachedEmbedding = await _redisService.GetCacheAsync<List<float>>(cacheKey);
                if (cachedEmbedding != null && cachedEmbedding.Any())
                {
                    await _lessonService.SaveMaterialEmbeddingsAsync(matId, cachedEmbedding);
                }
            }

            if (result.ModerationStatus == ModerationStatus.Approved.ToValue())
            {
                await _courseModerationService.ApproveCourseAsync(courseId, "AI moderation passed.");
            }
            else if (result.ModerationStatus == ModerationStatus.Rejected.ToValue())
            {
                var items = new List<RejectCourseItemDto>();
                foreach (var log in result.StageLogs)
                {
                    if (log.Result == StageLogResult.Flagged.ToValue() || log.Result == StageLogResult.MatchFound.ToValue())
                    {
                        foreach (var field in log.FlaggedFields)
                        {
                            items.Add(new RejectCourseItemDto
                            {
                                Target = field,
                                Reason = log.Reason ?? "AI moderation flag"
                            });
                        }
                    }
                }

                if (!items.Any())
                {
                    items.Add(new RejectCourseItemDto
                    {
                        Target = "course.description",
                        Reason = "Course content violates the moderation policy."
                    });
                }

                await _courseModerationService.RejectCourseDetailedAsync(new RejectCourseDetailedRequest
                {
                    CourseId = courseId,
                    Items = items
                });
            }
            else if (result.ModerationStatus == ModerationStatus.Flagged.ToValue())
            {
                var items = new List<RejectCourseItemDto>();
                foreach (var log in result.StageLogs)
                {
                    if (log.Result == StageLogResult.Flagged.ToValue() || log.Result == StageLogResult.MatchFound.ToValue())
                    {
                        foreach (var field in log.FlaggedFields)
                        {
                            items.Add(new RejectCourseItemDto
                            {
                                Target = field,
                                Reason = log.Reason ?? "AI moderation flag"
                            });
                        }
                    }
                }

                if (!items.Any())
                {
                    items.Add(new RejectCourseItemDto
                    {
                        Target = "course.description",
                        Reason = "Course content violates the moderation policy."
                    });
                }

                await _courseModerationService.FlagCourseDetailedAsync(new RejectCourseDetailedRequest
                {
                    CourseId = courseId,
                    Items = items
                });
            }
            else if (result.ModerationStatus == ModerationStatus.ManualAudit.ToValue())
            {
                await _courseCommandService.UpdateCourseStatusAndFeedbackAsync(courseId, CourseStatus.Pending.ToValue(), "AI suggested manual audit.");
                await _courseModerationService.NotifyAdminAsync("Manual Audit Required", $"Course {courseId} requires manual review by AI.", UrlConst.AdminCourseModerationURL);
            }
        }

        public async Task LogCourseAiModeration(LogCourseAiModerationCommand command)
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

        public async Task<CourseModerationResult> HandleCourseModerationWithAIAsync(CouresModerationRequest request)
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

                var thresholds = prep.Thresholds;
                var materialIds = prep.MaterialIds;

                var semanticReq = new SemanticDuplicationRequest
                {
                    CourseId = request.CourseId,
                    MaterialIds = materialIds,
                    SimilarityScoreThreshold = thresholds.GetValueOrDefault(AiModelConst.Similarity,
                                                                            AiModelConst.DefaultSimilarityScoreThreshold)
                };

                var harmfulReq = new CourseHarmfulRequest
                {
                    CourseId = request.CourseId,
                    SpamScoreThreshold = thresholds.GetValueOrDefault(
                                                                    AiModelConst.Spam,
                                                                    AiModelConst.DefaultSpamScoreThreshold
                                                                     ),
                    ToxicScoreThreshold = thresholds.GetValueOrDefault(
                                                                    AiModelConst.Toxic,
                                                                    AiModelConst.DefaultToxicScoreThreshold
                                                                     )
                };

                var result = await _aiModerationService.ModerateCourseFullPipelineAsync(semanticReq, harmfulReq);

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

        public async Task<CourseModerationResult> HandleCourseModerationAsync(CouresModerationRequest request)
        {
            try
            {
                await _courseCommandService.UpdateCourseStatusAsync(request.CourseId, CourseStatus.Pending.ToValue(), request.InstructorId);

                var dupResult = await CheckExactDuplication(request.CourseId);
                if (dupResult.IsDup)
                {
                    var items = dupResult.DupFields.Select(f => new RejectCourseItemDto
                    {
                        Target = $"course.{f}",
                        Reason = "Exact duplication with an existing course found."
                    }).ToList();

                    await _courseModerationService.RejectCourseDetailedAsync(new RejectCourseDetailedRequest
                    {
                        CourseId = request.CourseId,
                        Items = items
                    });

                    return new CourseModerationResult
                    {
                        CourseId = request.CourseId,
                        ModerationStatus = ModerationStatus.Rejected.ToValue(),
                        FlaggedFields = dupResult.DupFields,
                        OverallConfidenceScore = 1.0f,
                        TotalLatencyMs = 0,
                        StageLogs = []
                    };
                }

                return await HandleCourseModerationWithAIAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleCourseModerationAsync for course {CourseId}", request.CourseId);
                throw;
            }
        }
    }
}
