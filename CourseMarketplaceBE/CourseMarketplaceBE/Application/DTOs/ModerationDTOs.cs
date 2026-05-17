using System;
using System.Text.Json.Serialization;


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
        public float similarityScoreThreshold { get; set; } = 0.8f;
    }

    /// <summary>
    /// DTO for course harmful content check request to FastAPI
    /// </summary>
    public class CourseHarmfulRequest
    {
        [JsonPropertyName("course_id")]
        public int CourseId { get; set; }

        [JsonPropertyName("spam_score_threshold")]
        public float spamScoreThreshold { get; set; } = 0.7f;

        [JsonPropertyName("toxic_score_threshold")]
        public float toxicScoreThreshold { get; set; } = 0.7f;
    }

    /// <summary>
    /// DTO for saving course hashes
    /// </summary>
    public class SaveCourseHashesCommand
    {
        public int CourseId { get; set; }
        public string? title_hash { get; set; }
        public string? description_hash { get; set; }
        public string? what_you_will_learn_hash { get; set; }
        public string? requirements_hash { get; set; }
        public string? thumbnail_hash { get; set; }
    }

    /// <summary>
    /// DTO for course moderation request
    /// </summary>
    public class CouresModerationRequest
    {
        public int CourseId { get; set; }
        public int InstructorId { get; set; }
    }

    /// <summary>
    /// DTO for saving course AI usage log
    /// </summary>
    public class SaveCourseAiUsageLogCommand
    {
        public int? integration_id { get; set; }
        public string? interaction_type { get; set; }
        public Dictionary<string, object>? input_json { get; set; }
        public Dictionary<string, object>? output_json { get; set; }
        public float latency_ms { get; set; }
        public float token_usage { get; set; }
        public string? error_message { get; set; }
    }

    /// <summary>
    /// DTO for stage log in AI moderation pipeline
    /// </summary>
    public class StageLog
    {
        public int stage { get; set; } // 1, 2, 3, etc.
        public int step { get; set; } // Step within stage
        public DateTime timestamp { get; set; }
        public string? result { get; set; } // NO_MATCH, MATCH_FOUND, FLAGGED, APPROVED, etc.
        public string? reason { get; set; }
        public List<string> flaggedFields { get; set; } = [];
        public Dictionary<string, object>? details { get; set; }
        public float latency_ms { get; set; }
        public float confidence_score { get; set; } // 0-1
        public int model_id { get; set; }
    }

    public class AiModelResponse
    {
        public int model_id { get; set; }
        public string model_name { get; set; } = null!;
        public string? model_type { get; set; }
        public string? model_provider { get; set; }
        public string? model_version { get; set; }
        public string? model_status { get; set; }
        public string? description { get; set; }
    }

    public class LogCourseAiModerationCommand
    {
        public SemanticDuplicationRequest SemanticDuplicationRequest { get; set; } = null!;
        public CourseHarmfulRequest CourseHarmfulRequest { get; set; } = null!;
        public CourseAIModerationResult CourseAIModerationResult { get; set; } = null!;
        public string InteractionType { get; set; } = "moderation";
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// DTO for AI moderation result
    /// </summary>
    public class FullPipelineRequest
    {
        public int course_id { get; set; }
        public List<int> material_ids { get; set; } = [];
        public float similarity_score_threshold { get; set; }
        public float spam_score_threshold { get; set; }
        public float toxic_score_threshold { get; set; }
    }

    public class CourseAIModerationResult
    {
        public int CourseId { get; set; }
        public string ModerationStatus { get; set; } = null!; // APPROVED, FLAGGED, MANUAL_AUDIT, PENDING
        public List<string> flaggedFields { get; set; } = [];
        public float overall_confidence_score { get; set; } // 0-1
        public float total_latency_ms { get; set; }
        public List<StageLog> stageLogs { get; set; } = [];
    }

    /// <summary>
    /// DTO for course hashes (used internally)
    /// </summary>
    public class CourseExtDto
    {
        public int CourseId { get; set; } = 0;
        public string title_hash { get; set; } = string.Empty;
        public string description_hash { get; set; } = string.Empty;
        public string what_you_will_learn_hash { get; set; } = string.Empty;
        public string requirements_hash { get; set; } = string.Empty;
        public string thumbnail_hash { get; set; } = string.Empty;
    }

    public class CourseAIIntegrationResult{
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
        public Dictionary<string,float> Thresholds { get; set; } = new();
        
        
    }

    public class PrepareForCourseAIModerationResult
    {
        public int CourseId { get; set; }
        public List<int> MaterialIds { get; set; } = [];
        public List<int> ModelIds { get; set; } = [];
        public Dictionary<string,float> Thresholds { get; set; } = new();
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
    
