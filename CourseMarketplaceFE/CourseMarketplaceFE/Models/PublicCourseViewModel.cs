using System;

namespace CourseMarketplaceFE.Models
{
    public class PublicCourseViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? CourseThumbnailUrl { get; set; }
        public string? InstructorName { get; set; }
        public int StudentsCount { get; set; } // We'll mock this for now or get from view_course_stats later
        public double Rating { get; set; } // Mocked
        public DateTime? CreatedAt { get; set; }
    }

    public class CourseDetailViewModel : PublicCourseViewModel
    {
        public List<LessonViewModel> Lessons { get; set; } = new List<LessonViewModel>();
    }

    public class LessonViewModel
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public List<MaterialViewModel> LearningMaterials { get; set; } = new List<MaterialViewModel>();
    }

    public class MaterialViewModel
    {
        public int MaterialId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? MaterialUrl { get; set; }
        public MaterialMetadata? MaterialMetadata { get; set; }
    }

    public class MaterialMetadata
    {
        public string? FileType { get; set; }
        public int? Duration { get; set; }
        public int? PageCount { get; set; }
    }
}
