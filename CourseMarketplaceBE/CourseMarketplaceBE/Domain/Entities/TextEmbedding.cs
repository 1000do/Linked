using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class TextEmbedding
{
    public int TextEmbeddingId { get; set; }

    public int? MaterialId { get; set; }

    public List<float>? Embedding { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual LearningMaterial? Material { get; set; }
}
