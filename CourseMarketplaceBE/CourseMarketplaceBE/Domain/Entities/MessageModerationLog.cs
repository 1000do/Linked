using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class MessageModerationLog
{
    public int LogId { get; set; }
    public int? ModelId { get; set; }
    public int? MessageId { get; set; }
    public string? InputJson { get; set; }
    public string? OutputJson { get; set; }
    public float? LatencyMs { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? LogCreatedAt { get; set; }

    public virtual AiModel? Model { get; set; }
    public virtual Message? Message { get; set; }
}
