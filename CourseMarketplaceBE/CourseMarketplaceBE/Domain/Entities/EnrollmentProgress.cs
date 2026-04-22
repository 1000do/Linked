using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class EnrollmentProgress
{
    public int EnrollmentId { get; set; }

    public int? LearnedMaterialCount { get; set; } = 0;

    public DateTime? LastModifiedAt { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;
}
