using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Review
{
    public int ReviewId { get; set; }

    public int? EnrollmentId { get; set; }

    public float? Rating { get; set; }

    public string? Comment { get; set; }

    /// <summary>'detail' = review tổng (trang chi tiết), 'learn' = review lesson cụ thể</summary>
    public string? ReviewSource { get; set; }

    /// <summary>NULL = review tổng, có giá trị = review cho lesson cụ thể</summary>
    public int? LessonId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsRemoved { get; set; }

    public virtual Enrollment? Enrollment { get; set; }

    public virtual Lesson? Lesson { get; set; }
}
