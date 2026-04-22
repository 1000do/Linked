using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class LearningMaterial
{
    public int MaterialId { get; set; }

    public int? LessonId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? LearningStatus { get; set; }

    public string? MaterialUrl { get; set; }

    public string? MaterialMetadata { get; set; }

    public string? MaterialHash { get; set; }

    public virtual Lesson? Lesson { get; set; }

    public virtual MaterialPHash? MaterialPHash { get; set; }

    public virtual ICollection<MaterialEmbedding> MaterialEmbeddings { get; set; } = new List<MaterialEmbedding>();
}
