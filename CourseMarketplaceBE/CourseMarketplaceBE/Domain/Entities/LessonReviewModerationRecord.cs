using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class LessonReviewModerationRecord
{
    public int RecordId { get; set; }
    public int LessonReviewId { get; set; }
    public bool IsUpdate { get; set; }
    public string TempComment { get; set; } = null!;
    public decimal TempRating { get; set; }
    public string AiModerationStatus { get; set; } = null!;
    public string AiModerationNote { get; set; } = null!;
    public string ModerationStatus { get; set; } = null!;
    public string ModerationNote { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual LessonReview LessonReview { get; set; } = null!;
}
