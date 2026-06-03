using System;
using System.Collections.Generic;
using CourseMarketplaceBE.Domain.Constants;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class CourseReview
{
    public int CourseReviewId { get; set; }

    public int EnrollmentId { get; set; }

    public float? Rating { get; set; }

    public string? Comment { get; set; }

    public string CourseReviewStatus { get; set; } = ReviewStatus.Ok.ToValue();

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsRemoved { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;
}
