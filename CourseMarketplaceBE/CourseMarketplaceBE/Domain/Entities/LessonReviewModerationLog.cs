using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class LessonReviewModerationLog
{
    public int LogId { get; set; }
    public int? ModelId { get; set; }
    public int? LessonReviewId { get; set; }
    public string? InputJson { get; set; }
    public string? OutputJson { get; set; }
    public float? LatencyMs { get; set; }
    public string? LogStatus { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? LogCreatedAt { get; set; }

    public virtual AiModel? Model { get; set; }
    public virtual LessonReview? LessonReview { get; set; }
}
