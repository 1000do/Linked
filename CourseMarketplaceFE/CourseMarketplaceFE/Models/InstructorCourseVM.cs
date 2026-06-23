using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CourseMarketplaceFE.Models
{
    public class CourseListViewModel
    {
        [JsonPropertyName("courseId")]
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int Students { get; set; }
        public double Rating { get; set; }
        public string Status { get; set; } = null!;
        public string ThumbnailUrl { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
        public bool IsRemoved { get; set; }
        public int FlagCount { get; set; }
    }

    public class CreateCourseViewModel
    {
        [Required(ErrorMessage = "Course title is required")]
        [MinLength(5, ErrorMessage = "Course title must be at least 5 characters long")]
        [MaxLength(100, ErrorMessage = "Course title cannot exceed 100 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        public string CourseThumbnailUrl { get; set; } = null!;

        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, 10000, ErrorMessage = "Price must be between 0 and 10000")]
        public decimal Price { get; set; }       

        [Required(ErrorMessage = "What you will learn is required")]
        [MinLength(10, ErrorMessage = "What you will learn must be at least 10 characters long")]
        public string WhatYouWillLearn { get; set; } = null!;

        [Required(ErrorMessage = "Requirements are required")]
        [MinLength(10, ErrorMessage = "Requirements must be at least 10 characters long")]
        public string Requirements { get; set; } = null!;

        public int? CouponId { get; set; }
        public List<CategoryViewModel> AvailableCategories { get; set; } = new();
    }
    public class InstructorStudioViewModel
    {
        public List<CourseListViewModel> Courses { get; set; } = new();
        public int TotalStudents { get; set; }
        public double AverageRating { get; set; }
        public int ActiveCoursesCount { get; set; }
        public decimal TotalRevenue { get; set; }
        
        // Pagination & Search
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public string? SearchTerm { get; set; }
        public string? FilterStatus { get; set; }
    }

    public class MaterialTrashViewModel
    {
        public int MaterialId { get; set; }
        public string Title { get; set; } = null!;
        public string? LessonTitle { get; set; }
        public string? CourseTitle { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? FileType { get; set; }
        public string? CloudPublicId { get; set; }
    }

    public class AddMaterialViewModel
    {
        [Required(ErrorMessage = "Lesson ID is required")]
        public int LessonId { get; set; }

        [Required(ErrorMessage = "Material title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Material URL is required")]
        public string MaterialUrl { get; set; } = null!;

        [Required(ErrorMessage = "Material type is required")]
        public string Type { get; set; } = null!; // "video" or "document"

        public int? Duration { get; set; }
        public long? FileSize { get; set; }
        public string? FileExtension { get; set; }
    }

    public class UpdateCourseDetailsViewModel
    {
        [Required(ErrorMessage = "Course ID is required")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MinLength(5, ErrorMessage = "Title must be at least 5 characters long")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = null!;

        [Range(0, 10000, ErrorMessage = "Price must be between 0 and 10000")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "What you will learn is required")]
        [MinLength(10, ErrorMessage = "What you will learn must be at least 10 characters long")]
        public string WhatYouWillLearn { get; set; } = null!;

        [Required(ErrorMessage = "Requirements are required")]
        [MinLength(10, ErrorMessage = "Requirements must be at least 10 characters long")]
        public string Requirements { get; set; } = null!;

        [Required(ErrorMessage = "Thumbnail URL is required")]
        public string ThumbnailUrl { get; set; } = null!;
    }
}
