using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceFE.Models
{
    public class CourseListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int Students { get; set; }
        public double Rating { get; set; }
        public string Status { get; set; } = null!;
        public string ThumbnailUrl { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
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
    }
}
