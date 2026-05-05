using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class UserReport
{
    public int ReportId { get; set; }

    public int? ReporterId { get; set; }

    public int? TargetId { get; set; }

    public int? ResolverId { get; set; }

    public string? Reason { get; set; }

    public string? Description { get; set; }

    public string? UserReportsStatus { get; set; }

    public string? ResolutionNote { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public int? ChatId { get; set; }

    public DateTime? AccessGrantedUntil { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Chat? Chat { get; set; }

    public virtual Account? Reporter { get; set; }

    public virtual Account? Resolver { get; set; }

    public virtual Account? Target { get; set; }
}
