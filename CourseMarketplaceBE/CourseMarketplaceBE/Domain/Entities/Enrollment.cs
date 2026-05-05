using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int? UserId { get; set; }

    public int? CourseId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateOnly? CompletedDate { get; set; }

    public bool? IsCompleted { get; set; }

    public DateOnly? EnrollDate { get; set; }

    public DateTime? LastAccessedAt { get; set; }

    public string? EnrollmentStatus { get; set; }

    public virtual Course? Course { get; set; }

    public virtual EnrollmentProgress? Progress { get; set; }

    public virtual ICollection<CourseReview> CourseReviews { get; set; } = new List<CourseReview>();

    public virtual ICollection<LessonReview> LessonReviews { get; set; } = new List<LessonReview>();

    public virtual User? User { get; set; }
}
