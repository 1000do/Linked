using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class CourseReviewReport
{
    public int CourseReviewReportId { get; set; }

    public int? ReporterId { get; set; }

    public int? CourseReviewId { get; set; }

    public int? ResolverId { get; set; }

    public string? Reason { get; set; }

    public string? Description { get; set; }

    /// <summary>pending | under_review | resolved | rejected | escalated</summary>
    public string? UserReportsStatus { get; set; }

    public string? ResolutionNote { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime? AccessGrantedUntil { get; set; }

    public DateTime? CreatedAt { get; set; }

    // ── Navigation ──────────────────────────────────────────────────────────

    public virtual Account? Reporter { get; set; }

    public virtual Account? Resolver { get; set; }

    public virtual CourseReview? CourseReview { get; set; }
}
