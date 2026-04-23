using System;

namespace CourseMarketplaceBE.Domain.Entities;

/// <summary>
/// Theo dõi tiến độ học của từng enrollment.
/// learned_material_count / course.total_materials = progress bar %
/// </summary>
public partial class EnrollmentProgress
{
    public int EnrollmentId { get; set; }

    public int LearnedMaterialCount { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;
}
