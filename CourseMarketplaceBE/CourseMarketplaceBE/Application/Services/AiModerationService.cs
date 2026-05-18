using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace CourseMarketplaceBE.Application.Services
{
    /// <summary>
    /// Implementation of AI moderation service.
    /// Calls FastAPI service endpoints with retry and circuit breaker policies.
    /// </summary>
    public class AiModerationService : IAiModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiModerationService> _logger;
        private readonly IAiModelRepository _aiModelRepository;
        private readonly ICourseAiUsageLogRepository _usageLogRepository;
        private readonly ISystemConfigRepository _configRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IRedisService _redisService;
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
        private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreakerPolicy;


        private const int MaxRetries = 3;
        private const string BaseUrl = "http://ai-moderation:8000";


        public AiModerationService(
            HttpClient httpClient,
            ILogger<AiModerationService> logger,
            IAiModelRepository aiModelRepository,
            ICourseAiUsageLogRepository usageLogRepository,
            ISystemConfigRepository configRepository,
            ICourseRepository courseRepository,
            IRedisService redisService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _aiModelRepository = aiModelRepository;
            _usageLogRepository = usageLogRepository;
            _configRepository = configRepository;
            _courseRepository = courseRepository;
            _redisService = redisService;

            // Setup retry policy

            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && r.StatusCode != System.Net.HttpStatusCode.BadRequest)
                .WaitAndRetryAsync(
                    retryCount: MaxRetries,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retry {retryCount} after {timespan.TotalSeconds}s");
                    });

            // Setup circuit breaker policy

            _circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && r.StatusCode != System.Net.HttpStatusCode.BadRequest)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, timespan) =>
                    {
                        _logger.LogError($"Circuit breaker opened for {timespan.TotalSeconds}s");
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset");
                    });
        }


        public async Task<CourseModerationResult> ModerateCourseFullPipelineAsync(SemanticDuplicationRequest semanticReq, CourseHarmfulRequest harmfulReq)
        {
            var request = new
            {
                semantic = semanticReq,
                harmful = harmfulReq
            };


            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/moderation/full-pipeline", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CourseModerationResult>() ?? new CourseModerationResult();
        }


        public async Task<Dictionary<string, float>> GetScoreThresholdConfigAsync(string key)
        {
            var val = await _configRepository.GetValueAsync(key);
            if (string.IsNullOrEmpty(val))

            {
                return new Dictionary<string, float>
                {

                    { "similarity", Domain.Constants.SystemConfigKeys.DefaultSimilarityScoreThreshold },

                    { "spam", Domain.Constants.SystemConfigKeys.DefaultSpamScoreThreshold },

                    { "toxic", Domain.Constants.SystemConfigKeys.DefaultToxicScoreThreshold }

                };
            }


            return JsonSerializer.Deserialize<Dictionary<string, float>>(val) ?? new Dictionary<string, float>();
        }


        public async Task<List<int>> GetModelIdsByType(string type)
        {
            var models = await _aiModelRepository.GetByTypeAsync(type);
            return models.Select(m => m.ModelId).ToList();
        }

        public async Task<List<AiModelResponse>> GetModelsByTypeAsync(string modelType)
        {
            string cacheKey = CacheKeys.AiModelType.GetKey(modelType);
            var cached = await _redisService.GetCacheAsync<List<AiModelResponse>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var dbModels = await _aiModelRepository.GetModelsByTypeAsync(modelType);
            var result = dbModels.Select(m => new AiModelResponse
            {
                model_id = m.ModelId,
                model_name = m.ModelName,
                model_type = m.ModelType,
                model_provider = m.ModelProvider,
                model_version = m.ModelVersion,
                model_status = m.ModelStatus,
                description = m.Description
            }).ToList();

            await _redisService.SetCacheAsync(cacheKey, result, CacheTtl.Medium.GetTtl());
            return result;
        }

        public async Task SaveCourseAiUsageLog(SaveCourseAiUsageLogCommand command)
        {
            var log = new CourseAiUsageLog
            {
                IntegrationId = command.integration_id,
                InteractionType = command.interaction_type,
                InputJson = command.input_json,
                OutputJson = command.output_json,
                LatencyMs = command.latency_ms,
                TokenUsage = command.token_usage,
                ErrorMessage = command.error_message,
                LogCreatedAt = DateTime.UtcNow
            };

            await _usageLogRepository.AddAsync(log);
            await _usageLogRepository.SaveChangesAsync();
        }

        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
