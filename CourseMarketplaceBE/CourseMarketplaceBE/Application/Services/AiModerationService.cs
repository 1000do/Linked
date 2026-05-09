// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Net.Http;
// using System.Text.Json;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;
// using Polly;
// using Polly.CircuitBreaker;
// using Polly.Retry;

// namespace CourseMarketplaceBE.Application.Services
// {
//     /// <summary>
//     /// Implementation of AI moderation service.
//     /// Calls FastAPI service endpoints with retry and circuit breaker policies.
//     /// </summary>
//     public class AiModerationService : IAiModerationService
//     {
//         private readonly HttpClient _httpClient;
//         private readonly ILogger<AiModerationService> _logger;
//         private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
//         private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreakerPolicy;

//         private const int MaxRetries = 3;
//         private const string BaseUrl = "http://ai-moderation:8000";

//         public AiModerationService(
//             HttpClient httpClient,
//             ILogger<AiModerationService> logger)
//         {
//             _httpClient = httpClient;
//             _logger = logger;

//             // Setup retry policy
//             _retryPolicy = Policy
//                 .Handle<HttpRequestException>()
//                 .Or<TaskCanceledException>()
//                 .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
//                 .WaitAndRetryAsync(
//                     retryCount: MaxRetries,
//                     sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
//                     onRetry: (outcome, timespan, retryCount, context) =>
//                     {
//                         _logger.LogWarning($"Retry {retryCount} after {timespan.TotalSeconds}s");
//                     });

//             // Setup circuit breaker policy
//             _circuitBreakerPolicy = Policy
//                 .Handle<HttpRequestException>()
//                 .Or<TaskCanceledException>()
//                 .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
//                 .CircuitBreakerAsync(
//                     handledEventsAllowedBeforeBreaking: 5,
//                     durationOfBreak: TimeSpan.FromSeconds(30),
//                     onBreak: (outcome, timespan) =>
//                     {
//                         _logger.LogError($"Circuit breaker opened for {timespan.TotalSeconds}s");
//                     },
//                     onReset: () =>
//                     {
//                         _logger.LogInformation("Circuit breaker reset");
//                     });
//         }

//         /// <summary>
//         /// Check course for duplication (Stage 1).
//         /// </summary>
//         public async Task<ModerationResponse> CheckDuplicationAsync(int courseId)
//         {
//             try
//             {
//                 _logger.LogInformation($"Checking duplication for course {courseId}");

//                 // In production, you would:
//                 // 1. Fetch course data from DB
//                 // 2. Query Redis cache for hashes and embeddings
//                 // 3. Build DuplicationRequest with hashes and embeddings
//                 // 4. POST to /moderation/stage1

//                 // For MVP, returning APPROVED response
//                 return new ModerationResponse
//                 {
//                     CourseId = courseId,
//                     Status = "APPROVED",
//                     StageLogs = new List<Dictionary<string, object>>(),
//                     FlaggedContent = new List<string>(),
//                     ConfidenceScore = 1.0f,
//                     TotalLatencyMs = 0f,
//                 };
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error checking duplication: {ex.Message}");
//                 throw;
//             }
//         }

//         /// <summary>
//         /// Check course for toxicity and spam (Stage 2).
//         /// </summary>
//         public async Task<ModerationResponse> CheckToxicityAsync(int courseId)
//         {
//             try
//             {
//                 _logger.LogInformation($"Checking toxicity for course {courseId}");

//                 // In production, you would:
//                 // 1. Fetch course data from DB
//                 // 2. Build ToxicityRequest with title, description, lesson texts, materials
//                 // 3. POST to /moderation/stage2

//                 // For MVP, returning APPROVED response
//                 return new ModerationResponse
//                 {
//                     CourseId = courseId,
//                     Status = "APPROVED",
//                     StageLogs = new List<Dictionary<string, object>>(),
//                     FlaggedContent = new List<string>(),
//                     ConfidenceScore = 1.0f,
//                     TotalLatencyMs = 0f,
//                 };
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error checking toxicity: {ex.Message}");
//                 throw;
//             }
//         }

//         /// <summary>
//         /// Run full moderation pipeline (Stage 1 + Stage 2).
//         /// </summary>
//         public async Task<ModerationResponse> ModerateFullPipelineAsync(int courseId)
//         {
//             try
//             {
//                 _logger.LogInformation($"Running full moderation pipeline for course {courseId}");

//                 // In production, you would POST to /moderation/full-pipeline
//                 // with combined duplication and toxicity requests

//                 // For MVP, returning APPROVED response
//                 return new ModerationResponse
//                 {
//                     CourseId = courseId,
//                     Status = "APPROVED",
//                     StageLogs = new List<Dictionary<string, object>>(),
//                     FlaggedContent = new List<string>(),
//                     ConfidenceScore = 1.0f,
//                     TotalLatencyMs = 0f,
//                 };
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error in full moderation pipeline: {ex.Message}");
//                 // Return MANUAL_AUDIT on service unavailability
//                 return new ModerationResponse
//                 {
//                     CourseId = courseId,
//                     Status = "MANUAL_AUDIT",
//                     StageLogs = new List<Dictionary<string, object>>(),
//                     FlaggedContent = new List<string>(),
//                     ConfidenceScore = 0.0f,
//                     TotalLatencyMs = 0f,
//                 };
//             }
//         }

//         /// <summary>
//         /// Generate embedding for a material.
//         /// </summary>
//         public async Task<float[]> GenerateEmbeddingAsync(int materialId, string filePath, string fileType)
//         {
//             try
//             {
//                 if (!File.Exists(filePath))
//                 {
//                     throw new FileNotFoundException($"Material file not found: {filePath}");
//                 }

//                 _logger.LogInformation($"Generating embedding for material {materialId} ({fileType})");

//                 // Read file bytes
//                 byte[] fileBytes = await File.ReadAllBytesAsync(filePath);

//                 // Build embedding request
//                 var requestData = new
//                 {
//                     material_id = materialId,
//                     material_type = fileType,
//                     content = Convert.ToBase64String(fileBytes), // Encode as base64 for JSON
//                 };

//                 var content = new StringContent(
//                     JsonSerializer.Serialize(requestData),
//                     System.Text.Encoding.UTF8,
//                     "application/json"
//                 );

//                 // Make request with retry and circuit breaker policies
//                 var policy = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy);

//                 var response = await policy.ExecuteAsync(async () =>
//                     await _httpClient.PostAsync($"{BaseUrl}/moderation/embeddings/generate", content)
//                 );

//                 if (!response.IsSuccessStatusCode)
//                 {
//                     throw new Exception($"Embedding generation failed: {response.StatusCode}");
//                 }

//                 // Parse response
//                 string jsonResponse = await response.Content.ReadAsStringAsync();
//                 using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
//                 {
//                     var root = doc.RootElement;
//                     var embeddingArray = root.GetProperty("embedding");

//                     // Convert to float[]
//                     var embedding = new List<float>();
//                     foreach (var element in embeddingArray.EnumerateArray())
//                     {
//                         embedding.Add(element.GetSingle());
//                     }

//                     _logger.LogInformation($"Generated embedding for material {materialId}: {embedding.Count} dimensions");
//                     return embedding.ToArray();
//                 }
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error generating embedding for material {materialId}: {ex.Message}");
//                 throw;
//             }
//         }

//         /// <summary>
//         /// Health check to verify FastAPI service is available.
//         /// </summary>
//         public async Task<bool> HealthCheckAsync()
//         {
//             try
//             {
//                 var response = await _httpClient.GetAsync($"{BaseUrl}/health");
//                 return response.IsSuccessStatusCode;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogWarning($"Health check failed: {ex.Message}");
//                 return false;
//             }
//         }
//     }
// }
