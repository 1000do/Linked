using System;

namespace CourseMarketplaceFE.Models
{
    public class EnrolledCourseViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string? CourseThumbnailUrl { get; set; }
        public string? InstructorName { get; set; }
        public double ProgressPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? LastAccessedAt { get; set; }
    }
}
