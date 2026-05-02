using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class AiModelsCourse
{
    public int Id { get; set; }

    public int? ModelId { get; set; }

    public int? CourseId { get; set; }

    public string? Role { get; set; }

    public bool? IsEnabled { get; set; }

    public string? ConfigJson { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual ICollection<CourseAiUsageLog> CourseAiUsageLogs { get; set; } = new List<CourseAiUsageLog>();

    public virtual Course? Course { get; set; }

    public virtual AiModel? Model { get; set; }
}
