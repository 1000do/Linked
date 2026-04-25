using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class MaterialPHash
{
    public int MaterialId { get; set; }

    public long[] PHash { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual LearningMaterial Material { get; set; } = null!;
}
