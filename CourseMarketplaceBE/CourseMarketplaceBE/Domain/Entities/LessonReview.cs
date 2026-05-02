using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class LessonReview
{
    public int LessonReviewId { get; set; }

    public int EnrollmentId { get; set; }

    public int? LessonId { get; set; }

    public float? Rating { get; set; }

    public string? Comment { get; set; }

    public string LessonReviewStatus { get; set; } = "ok";

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsRemoved { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;

    public virtual Lesson? Lesson { get; set; }
}
