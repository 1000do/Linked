using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class AiModel
{
    public int ModelId { get; set; }

    public string ModelName { get; set; } = null!;

    public string? ModelType { get; set; }

    public string? ModelProvider { get; set; }

    public string? ModelVersion { get; set; }

    public string? ModelStatus { get; set; }

    public string? Description { get; set; }

    public DateTime? ModelCreatedAt { get; set; }

    public DateTime? ModelUpdatedAt { get; set; }

    public virtual ICollection<AiModelsCourse> AiModelsCourses { get; set; } = new List<AiModelsCourse>();
    public virtual ICollection<CourseReviewModerationLog> CourseReviewModerationLogs { get; set; } = new List<CourseReviewModerationLog>();
    public virtual ICollection<LessonReviewModerationLog> LessonReviewModerationLogs { get; set; } = new List<LessonReviewModerationLog>();
}
