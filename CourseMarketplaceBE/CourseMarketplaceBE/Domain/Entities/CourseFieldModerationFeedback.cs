using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class CourseFieldModerationFeedback
{
    public int FeedbackId { get; set; }

    public int CourseId { get; set; }

    public string FieldName { get; set; } = null!;

    public string FeedbackText { get; set; } = null!;

    public DateTime? DateAdded { get; set; }

    public virtual Course Course { get; set; } = null!;
}
