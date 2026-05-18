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
    public string? ModelPath { get; set; }

    public virtual ICollection<CourseAiIntegration> CourseAiIntegrations { get; set; } = new List<CourseAiIntegration>();
    
    // Legacy support
    
    public virtual ICollection<CourseReviewModerationLog> CourseReviewModerationLogs { get; set; } = new List<CourseReviewModerationLog>();
    public virtual ICollection<LessonReviewModerationLog> LessonReviewModerationLogs { get; set; } = new List<LessonReviewModerationLog>();

    // Added to match SQL relationship for message_moderation_logs
    public virtual ICollection<MessageModerationLog> MessageModerationLogs { get; set; } = new List<MessageModerationLog>();
}
