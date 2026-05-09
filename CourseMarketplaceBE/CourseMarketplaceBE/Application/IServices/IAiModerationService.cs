using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices
{
    /// <summary>
    /// Response model from AI moderation service.
    /// </summary>
    public class ModerationResponse
    {
        public int CourseId { get; set; }
        public string Status { get; set; } // APPROVED, FLAGGED, MANUAL_AUDIT, PENDING
        public List<Dictionary<string, object>> StageLogs { get; set; }
        public List<string> FlaggedContent { get; set; }
        public float ConfidenceScore { get; set; }
        public float TotalLatencyMs { get; set; }
    }

    /// <summary>
    /// Service for calling AI moderation pipeline.
    /// Communicates with FastAPI service via HTTP.
    /// </summary>
    public interface IAiModerationService
    {
        /// <summary>
        /// Check course for duplication (Stage 1).
        /// </summary>
        Task<ModerationResponse> CheckDuplicationAsync(int courseId);

        /// <summary>
        /// Check course for toxicity and spam (Stage 2).
        /// </summary>
        Task<ModerationResponse> CheckToxicityAsync(int courseId);

        /// <summary>
        /// Run full moderation pipeline (Stage 1 + Stage 2).
        /// </summary>
        Task<ModerationResponse> ModerateFullPipelineAsync(int courseId);

        /// <summary>
        /// Generate embedding for a material.
        /// Used by C# backend to pre-compute embeddings during course creation.
        /// </summary>
        /// <param name="materialId">Material identifier</param>
        /// <param name="filePath">Path to material file (for reading content)</param>
        /// <param name="fileType">Type of material (text, image, video, pdf, word)</param>
        /// <returns>768-dimension embedding vector</returns>
        Task<float[]> GenerateEmbeddingAsync(int materialId, string filePath, string fileType);

        /// <summary>
        /// Health check to verify FastAPI service is available.
        /// </summary>
        Task<bool> HealthCheckAsync();
    }
}
