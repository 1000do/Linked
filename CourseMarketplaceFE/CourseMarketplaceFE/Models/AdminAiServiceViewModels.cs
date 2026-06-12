using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceFE.Models;

public class AiModelViewModel
{
    public int ModelId { get; set; }
    public string ModelName { get; set; } = null!;
    public string ModelVersion { get; set; } = null!;
    public string ModelPath { get; set; } = null!;
    public string ModelProvider { get; set; } = null!;
    public string ModelType { get; set; } = null!; // classifier, embedding_generator
    public string ProcessType { get; set; } = null!; // text, media
    public string Description { get; set; } = null!;
    public string ModelStatus { get; set; } = "active";
    public DateTime? ModelUpdatedAt { get; set; }
}

public class AiConfigurationViewModel
{
    // Thresholds
    public float SpamConfidenceThreshold { get; set; }
    public float ToxicityConfidenceThreshold { get; set; }
    public float SimilarityScoreThreshold { get; set; }

    // Integrations (Paths)
    public string? CourseHarmfulTextClassifierPath { get; set; }
    public string? CourseTextEmbeddingGeneratorPath { get; set; }
    public string? CourseMediaEmbeddingGeneratorPath { get; set; }
    public string? ReviewHarmfulTextClassifierPath { get; set; }
}

public class BaseAiLogViewModel
{
    public int LogId { get; set; }
    public int ModelId { get; set; }
    public string ModelName { get; set; } = null!;
    public string? ErrorMessage { get; set; }
    public int LatencyMs { get; set; }
    public DateTime LogCreatedAt { get; set; }
    
    // For rendering result in table without parsing JSON each time
    public string? ResultStatus { get; set; } 
    
    // Raw JSON strings for details
    public string? OutputJson { get; set; }
    public string? InputJson { get; set; }
}

public class CourseModerationLogViewModel : BaseAiLogViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = null!;
    public int TokenUsage { get; set; }
    public string? InteractionType { get; set; }
}

public class ReviewModerationLogViewModel : BaseAiLogViewModel
{
    public int ReviewId { get; set; }
    public string Comment { get; set; } = null!;
}

public class AdminAiServicePageViewModel
{
    public List<AiModelViewModel> AiModels { get; set; } = new();
    public List<AiModelViewModel> AllActiveModels { get; set; } = new();
    public AiConfigurationViewModel Config { get; set; } = new();
    
    public List<CourseModerationLogViewModel> CourseLogs { get; set; } = new();
    public List<ReviewModerationLogViewModel> CourseReviewLogs { get; set; } = new();
    public List<ReviewModerationLogViewModel> LessonReviewLogs { get; set; } = new();
}

public class CreateAiModelRequest
{
    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string ModelName { get; set; } = null!;

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^\d+\.\d+\.\d+$", ErrorMessage = "Must be semantic versioning (e.g. 1.0.0)")]
    public string ModelVersion { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string ModelPath { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string ModelProvider { get; set; } = null!;

    [Required]
    public string ModelType { get; set; } = null!;

    [Required]
    public string ProcessType { get; set; } = null!;

    [Required]
    [MinLength(20)]
    public string Description { get; set; } = null!;

    public string ModelStatus { get; set; } = "active";
}

public class UpdateAiModelRequest
{
    [Required]
    [StringLength(50)]
    public string ModelProvider { get; set; } = null!;

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^\d+\.\d+\.\d+$", ErrorMessage = "Must be semantic versioning (e.g. 1.0.0)")]
    public string ModelVersion { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string ModelPath { get; set; } = null!;

    [Required]
    [MinLength(20)]
    public string Description { get; set; } = null!;
}

public class UpdateThresholdsRequest
{
    [Required]
    [Range(0.01, 1.0)]
    public float SpamConfidenceThreshold { get; set; }

    [Required]
    [Range(0.01, 1.0)]
    public float ToxicityConfidenceThreshold { get; set; }

    [Required]
    [Range(0.01, 1.0)]
    public float SimilarityScoreThreshold { get; set; }
}

public class UpdateIntegrationRequest
{
    [Required]
    public string ConfigKey { get; set; } = null!;

    [Required]
    public string ConfigValue { get; set; } = null!;
}
