// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;
// using StackExchange.Redis;

// namespace CourseMarketplaceBE.Application.Services
// {
//     /// <summary>
//     /// Implementation of Redis cache service.
//     /// Caches course data for FastAPI moderation service access.
//     /// </summary>
//     public class RedisCacheService : IRedisCacheService
//     {
//         private readonly IConnectionMultiplexer _redis;
//         private readonly ILogger<RedisCacheService> _logger;
//         private readonly IDatabase _db;

//         private const string CoursePrefix = "mod:course:";
//         private const string EmbeddingPrefix = "mod:embedding:";
//         private const int CacheTtlSeconds = 86400; // 24 hours

//         public RedisCacheService(
//             IConnectionMultiplexer redis,
//             ILogger<RedisCacheService> logger)
//         {
//             _redis = redis;
//             _logger = logger;
//             _db = redis.GetDatabase();
//         }

//         /// <summary>
//         /// Cache course data for moderation.
//         /// </summary>
//         public async Task CacheCourseForModerationAsync(int courseId)
//         {
//             try
//             {
//                 _logger.LogInformation($"Caching course {courseId} for moderation");

//                 // In production, you would:
//                 // 1. Fetch course + lessons + materials from DB
//                 // 2. Build comprehensive course data object
//                 // 3. Serialize to JSON
//                 // 4. Store in Redis with TTL

//                 // For MVP, storing placeholder
//                 var courseData = new
//                 {
//                     course_id = courseId,
//                     cached_at = DateTime.UtcNow,
//                     hashes = new { /* course hashes */ },
//                     materials = new List<object>(),
//                 };

//                 string key = $"{CoursePrefix}{courseId}";
//                 string json = JsonSerializer.Serialize(courseData);

//                 await _db.StringSetAsync(key, json, TimeSpan.FromSeconds(CacheTtlSeconds));

//                 _logger.LogInformation($"✓ Cached course {courseId}");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error caching course {courseId}: {ex.Message}");
//                 throw;
//             }
//         }

//         /// <summary>
//         /// Invalidate course cache (on update).
//         /// </summary>
//         public async Task InvalidateCourseAsync(int courseId)
//         {
//             try
//             {
//                 _logger.LogInformation($"Invalidating cache for course {courseId}");

//                 string key = $"{CoursePrefix}{courseId}";
//                 await _db.KeyDeleteAsync(key);

//                 _logger.LogInformation($"✓ Invalidated course {courseId}");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error invalidating course {courseId}: {ex.Message}");
//                 throw;
//             }
//         }

//         /// <summary>
//         /// Store material embedding in cache.
//         /// </summary>
//         public async Task CacheMaterialEmbeddingAsync(int materialId, float[] embedding)
//         {
//             try
//             {
//                 _logger.LogDebug($"Caching embedding for material {materialId}");

//                 string key = $"{EmbeddingPrefix}{materialId}";

//                 // Convert float[] to JSON
//                 string json = JsonSerializer.Serialize(embedding);

//                 await _db.StringSetAsync(key, json, TimeSpan.FromSeconds(CacheTtlSeconds));

//                 _logger.LogDebug($"✓ Cached embedding for material {materialId}");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error caching embedding for material {materialId}: {ex.Message}");
//                 throw;
//             }
//         }
//     }
// }
