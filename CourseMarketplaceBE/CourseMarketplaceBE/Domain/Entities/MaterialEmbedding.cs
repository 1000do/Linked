using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class MaterialEmbedding
{
    public int EmbeddingId { get; set; }

    public int? MaterialId { get; set; }

    public string? Embedding { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual LearningMaterial? Material { get; set; }
}
