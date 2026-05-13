using System;

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
}
