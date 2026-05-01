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
}
