using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace CourseMarketplaceBE.Infrastructure.Services
{
    public class AiModerationService : IAiModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiModerationService> _logger;
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
        private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreakerPolicy;

        private const int MaxRetries = 3;
        private const string BaseUrl = UrlConst.AIModerationBaseURL;

        public AiModerationService(
            HttpClient httpClient,
            ILogger<AiModerationService> logger)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromMinutes(30);
            _logger = logger;

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

            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/{UrlConst.FullPipelineURL}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CourseModerationResult>() ?? new CourseModerationResult { CourseId = semanticReq.CourseId, ModerationStatus = ModerationStatus.ManualAudit.ToValue() };
        }



        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{UrlConst.HealthCheckURL}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
