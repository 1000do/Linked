using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class AuditLog
{
    public int LogId { get; set; }

    public int? ActorId { get; set; }

    public string ActionType { get; set; } = null!;

    public string? TargetType { get; set; }

    public int? TargetId { get; set; }

    public string? Details { get; set; }

    public string? IpAddress { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account? Actor { get; set; }
}
