using System;
using System.Text.Json.Serialization;

namespace CourseMarketplaceFE.Models
{
    public enum AiThreatLevel
    {
        None = 1,
        Approved = 2,
        ManualAudit = 3,
        FlaggedOrRejected = 4
    }

    public class CourseModerationViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string InstructorName { get; set; } = null!;
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CourseStatus { get; set; }
        [JsonPropertyName("course_thumbnail_url")]
        public string? CourseThumbnailUrl { get; set; }
        public string UrgencyLevel { get; set; } = "Normal";
        public string UrgencyColor { get; set; } = "slate";
        public int FlagCount { get; set; }
        public bool IsRemoved { get; set; }
        public AiThreatLevel ThreatLevel { get; set; } = AiThreatLevel.None;
    }

    public class UserReportModerationViewModel
    {
        public int ReportId { get; set; }
        public string ReporterName { get; set; } = null!;
        public string? Reason { get; set; }
        public string? Description { get; set; }
        public int? ChatId { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class ReviewModerationRecordViewModel
    {
        public int RecordId { get; set; }
        public int ReviewId { get; set; }
        public string Type { get; set; } = null!; // "course" or "lesson"
        public bool IsUpdate { get; set; }
        public string TempComment { get; set; } = null!;
        public decimal TempRating { get; set; }
        public string? OriginalComment { get; set; }
        public decimal? OriginalRating { get; set; }
        public string AiModerationStatus { get; set; } = null!;
        public string AiModerationNote { get; set; } = null!;
        public string ModerationStatus { get; set; } = null!;
        public string ModerationNote { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        
        public string CourseTitle { get; set; } = null!;
        public string? LessonTitle { get; set; }
    }

    public class ReviewModerationListViewModel
    {
        public PagedResult<ReviewModerationRecordViewModel> Records { get; set; } = new PagedResult<ReviewModerationRecordViewModel> { Items = new System.Collections.Generic.List<ReviewModerationRecordViewModel>(), TotalCount = 0, Page = 1, PageSize = 10 };
        public string Search { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = "all";
        public string SortBy { get; set; } = "priority_desc";
    }
}
