using System;
using System.Text.Json.Serialization;
using CourseMarketplaceBE.Domain.Constants;


namespace CourseMarketplaceBE.Application.DTOs
{
    public class CourseModerationDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string InstructorName { get; set; } = null!;
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CourseStatus { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("course_thumbnail_url")]
        public string? CourseThumbnailUrl { get; set; }

        // NEW: Urgency calculation
        public string UrgencyLevel { get; set; } = "Normal";
        public string UrgencyColor { get; set; } = "slate";

        // NEW: Flagging tracking
        public int FlagCount { get; set; }
    }

    public class ModerationFilterDto
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; } = "pending";
        public string? SortBy { get; set; } = "oldest"; // oldest (urgent), newest
    }

    public class UserReportModerationDto
    {
        public int ReportId { get; set; }
        public string ReporterName { get; set; } = null!;
        public string? Reason { get; set; }
        public string? Description { get; set; }
        public int? ChatId { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class ResolveReportDto
    {
        public int ReportId { get; set; }
        public string Status { get; set; } = null!; // resolved, rejected
        public string? ResolutionNote { get; set; }
    }

    public class RejectCourseItemDto
    {
        public string Target { get; set; } = null!; // "course.title", "course.description", "course.thumbnail", "course.what_you_will_learn", "course.requirements", "lesson.title", "file"
        public int? LessonId { get; set; }
        public int? MaterialId { get; set; }
        public string? LessonTitle { get; set; }
        public string? MaterialTitle { get; set; }
        public string Reason { get; set; } = null!;
    }

    public class RejectCourseDetailedRequest
    {
        public int CourseId { get; set; }
        public List<RejectCourseItemDto> Items { get; set; } = new();
    }

    // ===== New DTOs for AI Moderation Pipeline =====

    /// <summary>
    /// DTO for exact duplication check command
    /// </summary>
    public class ExactDuplicationCommand
    {
        public CourseExtDto CourseExt { get; set; } = new();
        public List<CourseExtDto> ExistingCourseExts { get; set; } = [];
    }

    /// <summary>
    /// DTO for exact duplication check result
    /// </summary>
    public class ExactDuplicationResult
    {
        public int CourseId { get; set; }
        public bool IsDup { get; set; }
        public List<string> DupFields { get; set; } = [];
    }

    /// <summary>
    /// DTO for course AI integration command
    /// </summary>
    public class CourseAIIntegrationCommand
    {
        public int CourseId { get; set; }
        public int ModelId { get; set; }
        public string? Role { get; set; }
        public bool IsEnabled { get; set; } = true;
        public Dictionary<string, float>? ConfigJson { get; set; }
    }

    /// <summary>
    /// DTO for semantic duplication check request to FastAPI
    /// </summary>
    public class SemanticDuplicationRequest
    {
        [JsonPropertyName("course_id")]
        public int CourseId { get; set; }

        [JsonPropertyName("material_ids")]
        public List<int> MaterialIds { get; set; } = [];

        [JsonPropertyName("similarity_score_threshold")]
        public float SimilarityScoreThreshold { get; set; } = 0.8f;

        [JsonPropertyName("models")]
        public List<AiModelDto> Models { get; set; } = [];
    }

    /// <summary>
    /// DTO for course harmful content check request to FastAPI
    /// </summary>
    public class CourseHarmfulRequest
    {
        [JsonPropertyName("course_id")]
        public int CourseId { get; set; }

        [JsonPropertyName("spam_score_threshold")]
        public float SpamScoreThreshold { get; set; } = 0.7f;

        [JsonPropertyName("toxic_score_threshold")]
        public float ToxicScoreThreshold { get; set; } = 0.7f;

        [JsonPropertyName("models")]
        public List<AiModelDto> Models { get; set; } = [];
    }

    /// <summary>
    /// DTO for AI moderation result
    /// </summary>
    public class FullPipelineRequest
    {
        [JsonPropertyName("course_id")]
        public int CourseId { get; set; }

        [JsonPropertyName("material_ids")]
        public List<int> MaterialIds { get; set; } = [];

        [JsonPropertyName("similarity_score_threshold")]
        public float SimilarityScoreThreshold { get; set; } = 0.8f;

        [JsonPropertyName("spam_score_threshold")]
        public float SpamScoreThreshold { get; set; } = 0.7f;

        [JsonPropertyName("toxic_score_threshold")]
        public float ToxicScoreThreshold { get; set; } = 0.7f;
    }

    /// <summary>
    /// DTO for saving course hashes
    /// </summary>
    public class SaveCourseHashesCommand
    {
        public int CourseId { get; set; }
        public string? TitleHash { get; set; }
        public string? DescriptionHash { get; set; }
        public string? WhatYouWillLearnHash { get; set; }
        public string? RequirementsHash { get; set; }
        public string? ThumbnailHash { get; set; }
    }

    /// <summary>
    /// DTO for course moderation request
    /// </summary>
    public class CouresModerationRequest
    {
        public int CourseId { get; set; }
        public int InstructorId { get; set; }
    }



    public class CourseModerationResult
    {
        [JsonPropertyName("course_id")]
        public int CourseId { get; set; }
        [JsonPropertyName("moderation_status")]
        public string ModerationStatus { get; set; } = Domain.Constants.ModerationStatus.Pending.ToValue(); // APPROVED, FLAGGED, MANUAL_AUDIT, PENDING
        [JsonPropertyName("flagged_fields")]
        public List<string> FlaggedFields { get; set; } = [];
        [JsonPropertyName("overall_confidence_score")]
        public float OverallConfidenceScore { get; set; } = 0f;
        [JsonPropertyName("total_latency_ms")]
        public float TotalLatencyMs { get; set; } = 0f;
        [JsonPropertyName("stage_logs")]
        public List<StageLog> StageLogs { get; set; } = [];
    }

    /// <summary>
    /// DTO for stage log in AI moderation pipeline
    /// </summary>
    public class StageLog
    {
        [JsonPropertyName("stage")]
        public int Stage { get; set; } // 1, 2, 3, etc.
        [JsonPropertyName("step")]
        public int Step { get; set; } // Step within stage
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
        [JsonPropertyName("result")]
        public string? Result { get; set; } // NO_MATCH, MATCH_FOUND, FLAGGED, APPROVED, etc.
        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
        [JsonPropertyName("flagged_fields")]
        public List<string> FlaggedFields { get; set; } = [];
        [JsonPropertyName("details")]
        public Dictionary<string, object>? Details { get; set; }
        [JsonPropertyName("latency_ms")]
        public float LatencyMs { get; set; }
        [JsonPropertyName("confidence_score")]
        public float ConfidenceScore { get; set; } // 0-1
        [JsonPropertyName("model_id")]
        public int ModelId { get; set; }
    }

    /// <summary>
    /// DTO for saving course AI usage log
    /// </summary>
    public class SaveCourseAiUsageLogCommand
    {
        public int? IntegrationId { get; set; }
        public string? InteractionType { get; set; }
        public string? InputJson { get; set; }
        public string? OutputJson { get; set; }
        public float LatencyMs { get; set; }
        public float TokenUsage { get; set; }
        public string? ErrorMessage { get; set; }
    }



    public class AiModelDto
    {
        public int ModelId { get; set; }
        public string ModelName { get; set; } = null!;
        public string? ModelType { get; set; }
        public string? ModelProvider { get; set; }
        public string? ModelVersion { get; set; }
        public string? ModelStatus { get; set; }
        public string? Description { get; set; }
        public string? ModelPath { get; set; }
        public string? ProcessType { get; set; }
    }

    public class MaterialEmbeddingResponse
    {
        public int EmbeddingId { get; set; }
        public int? MaterialId { get; set; }
        public string? Embedding { get; set; }
    }

    public class LogCourseAiModerationCommand
    {
        public SemanticDuplicationRequest SemanticDuplicationRequest { get; set; } = null!;
        public CourseHarmfulRequest CourseHarmfulRequest { get; set; } = null!;
        public CourseModerationResult CourseModerationResult { get; set; } = null!;
        public string InteractionType { get; set; } = AIInteractionType.Moderation.ToValue();
        public string? ErrorMessage { get; set; }
    }


    public class CourseAiUsageLogInput
    {
        [JsonPropertyName("course_id")]
        public int CourseId { get; set; }
        [JsonPropertyName("material_ids")]
        public List<int> MaterialIds { get; set; } = [];
        [JsonPropertyName("similarity_score_threshold")]
        public float SimilarityScoreThreshold { get; set; }
        [JsonPropertyName("spam_score_threshold")]
        public float SpamScoreThreshold { get; set; }
        [JsonPropertyName("toxic_score_threshold")]
        public float ToxicScoreThreshold { get; set; }
    }

    public class CourseAiUsageLogOutput
    {
        [JsonPropertyName("stage")]
        public int Stage { get; set; }
        [JsonPropertyName("step")]
        public int Step { get; set; }
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
        [JsonPropertyName("result")]
        public string? Result { get; set; } = "";
        [JsonPropertyName("reason")]
        public string? Reason { get; set; } = "";
        [JsonPropertyName("flagged_fields")]
        public List<string>? FlaggedFields { get; set; } = [];
        [JsonPropertyName("details")]
        public Dictionary<string, object>? Details { get; set; } = [];
        [JsonPropertyName("confidence_score")]
        public float ConfidenceScore { get; set; }

    }



    /// <summary>
    /// DTO for course hashes (used internally)
    /// </summary>
    public class CourseExtDto
    {
        public int CourseId { get; set; } = 0;
        public string TitleHash { get; set; } = string.Empty;
        public string DescriptionHash { get; set; } = string.Empty;
        public string WhatYouWillLearnHash { get; set; } = string.Empty;
        public string RequirementsHash { get; set; } = string.Empty;
        public string ThumbnailHash { get; set; } = string.Empty;
    }

    public class CourseAIIntegrationResult
    {
        public int CourseId { get; set; }
        public int ModelId { get; set; }
        public bool IsEnabled { get; set; } = true;
        public Dictionary<string, float> ConfigJson { get; set; } = new();
        public string? Role { get; set; }

    }

    public class AssignAIModeratorsToCourseResult
    {
        public int CourseId { get; set; }
        public List<int> ModelIds { get; set; } = [];
        public Dictionary<string, float> Thresholds { get; set; } = new();


    }

    public class PrepareForCourseAIModerationResult
    {
        public int CourseId { get; set; }
        public List<int> MaterialIds { get; set; } = [];
        public List<int> ModelIds { get; set; } = [];
        public Dictionary<string, float> Thresholds { get; set; } = new();
    }

    public class CourseAiIntegrationResponse
    {
        public int CourseId { get; set; }
        public int ModelId { get; set; }
        public bool IsEnabled { get; set; } = true;
        public Dictionary<string, float> ConfigJson { get; set; } = new();
        public string? Role { get; set; }
    }
}

