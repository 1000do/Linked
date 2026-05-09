using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices
{
    /// <summary>
    /// Service for caching course data in Redis.
    /// Used by C# backend to pre-load data for FastAPI moderation service.
    /// </summary>
    public interface IRedisCacheService
    {
        /// <summary>
        /// Cache course data for moderation.
        /// Called after course creation, before moderation check.
        /// </summary>
        /// <param name="courseId">Course to cache</param>
        Task CacheCourseForModerationAsync(int courseId);

        /// <summary>
        /// Invalidate course cache (on update).
        /// </summary>
        /// <param name="courseId">Course to invalidate</param>
        Task InvalidateCourseAsync(int courseId);

        /// <summary>
        /// Store material embedding in cache.
        /// </summary>
        /// <param name="materialId">Material identifier</param>
        /// <param name="embedding">768-dim embedding vector</param>
        Task CacheMaterialEmbeddingAsync(int materialId, float[] embedding);
    }
}
