using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Review
{
    public int ReviewId { get; set; }

    public int? EnrollmentId { get; set; }

    public float? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsRemoved { get; set; }

    public virtual Enrollment? Enrollment { get; set; }
}
