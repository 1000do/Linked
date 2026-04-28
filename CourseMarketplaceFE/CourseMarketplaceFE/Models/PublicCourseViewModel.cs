using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        public int TotalStudents { get; set; }
        public decimal RatingAverage { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? WhatYouWillLearn { get; set; }
        public string? Requirements { get; set; }
        public string? CategoryName { get; set; }
        public string? InstructorAvatarUrl { get; set; }
        public string? InstructorBio { get; set; }
        public string? InstructorProfessionalTitle { get; set; }
        public int InstructorReviewCount { get; set; }
        public int InstructorStudentsCount { get; set; }
        public int InstructorCoursesCount { get; set; }
        public bool IsInWishlist { get; set; }
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
        public int TotalSeconds => LearningMaterials.Sum(m => m.MaterialMetadata?.Duration ?? 0);
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
        [JsonPropertyName("file_type")]
        public string? FileType { get; set; }

        [JsonPropertyName("duration")]
        public int? Duration { get; set; }

        [JsonPropertyName("page_count")]
        public int? PageCount { get; set; }
        
        [JsonPropertyName("file_size")]
        public long? FileSize { get; set; }
        
        [JsonPropertyName("file_extension")]
        public string? FileExtension { get; set; }
    }

    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoriesName { get; set; } = null!;
    }
}
