using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class MaterialCompletion
{
    public int Id { get; set; }

    public int EnrollmentId { get; set; }

    public int MaterialId { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;

    public virtual LearningMaterial Material { get; set; } = null!;
}
