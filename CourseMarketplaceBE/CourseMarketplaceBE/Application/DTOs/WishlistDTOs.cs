using System;

namespace CourseMarketplaceBE.Application.DTOs
{
    public class WishlistResponse
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string? CourseThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public string? InstructorName { get; set; }
        public decimal RatingAverage { get; set; }
        public int TotalStudents { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
