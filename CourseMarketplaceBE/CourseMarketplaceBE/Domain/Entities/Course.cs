using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Course
{
    public int CourseId { get; set; }

    public int? InstructorId { get; set; }

    public int? CategoryId { get; set; }

    public int? CouponId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? CourseThumbnailUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CourseStatus { get; set; }

    public int? CourseFlagCount { get; set; }

    public int? TotalLessons { get; set; }

    public float? RatingAverage { get; set; }

    public int? TotalStudents { get; set; }

    public virtual ICollection<AiModelsCourse> AiModelsCourses { get; set; } = new List<AiModelsCourse>();

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category? Category { get; set; }

    public virtual Coupon? Coupon { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual Instructor? Instructor { get; set; }

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
