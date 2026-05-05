using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class CourseAiUsageLog
{
    public int LogId { get; set; }
    public int? AiModelCourseId { get; set; }
    public string? InteractionType { get; set; }
    public string? InputJson { get; set; }
    public string? OutputJson { get; set; }
    public float? LatencyMs { get; set; }
    public float? TokenUsage { get; set; }
    public string? LogStatus { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? LogCreatedAt { get; set; }

    public virtual AiModelsCourse? AiModelCourse { get; set; }
}
