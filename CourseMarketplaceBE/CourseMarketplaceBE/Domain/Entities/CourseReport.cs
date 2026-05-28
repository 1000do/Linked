using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class CourseReport
{
    public int CourseReportId { get; set; }

    public int? ReporterId { get; set; }

    public int? CourseId { get; set; }

    public int? ResolverId { get; set; }

    public string? Reason { get; set; }

    public string? Description { get; set; }

    /// <summary>pending | under_review | resolved | rejected | escalated</summary>
    public string? CourseReportsStatus { get; set; }

    public string? ResolutionNote { get; set; }

    public DateTime? ResolvedAt { get; set; }

    /// <summary>Cho phép reviewer xem nội dung bị ẩn đến thời điểm này</summary>
    public DateTime? AccessGrantedUntil { get; set; }

    public DateTime? CreatedAt { get; set; }

    // ── Navigation ──────────────────────────────────────────────────────────

    public virtual Account? Reporter { get; set; }

    public virtual Account? Resolver { get; set; }

    public virtual Course? Course { get; set; }
}
