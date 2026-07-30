using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class LessonAiFeedback
{
    public int FeedbackId { get; set; }
    public int LessonId { get; set; }
    public string FieldName { get; set; } = null!;
    public string FeedbackText { get; set; } = null!;
    public string ModerationStatus { get; set; } = "PENDING";
    public DateTime? DateAdded { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;
}
