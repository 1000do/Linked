using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class LearningMaterial
{
    public int MaterialId { get; set; }

    public int? LessonId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? LearningStatus { get; set; }

    public string? MaterialUrl { get; set; }

    public MaterialMetadata? MaterialMetadata { get; set; }

    public string? MaterialHash { get; set; }

    public virtual Lesson? Lesson { get; set; }
}

public class MaterialMetadata
{
    [JsonPropertyName("file_size")]
    public long? FileSize { get; set; }

    [JsonPropertyName("file_type")]
    public string? FileType { get; set; }

    [JsonPropertyName("file_extension")]
    public string? FileExtension { get; set; }

    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    [JsonPropertyName("page_count")]
    public int? PageCount { get; set; }
}
