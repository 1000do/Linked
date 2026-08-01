using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class LearningMaterialAiFeedback
{
    public int FeedbackId { get; set; }
    public int MaterialId { get; set; }
    public string FieldName { get; set; } = null!;
    public string FeedbackText { get; set; } = null!;
    public string ModerationStatus { get; set; } = "PENDING";
    public DateTime? DateAdded { get; set; }

    public virtual LearningMaterial Material { get; set; } = null!;
}
