using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class MaterialPHash
{
    public int MaterialId { get; set; }

    public string? PHash { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual LearningMaterial Material { get; set; } = null!;
}
