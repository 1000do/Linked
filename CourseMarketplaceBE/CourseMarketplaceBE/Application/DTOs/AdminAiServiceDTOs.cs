using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceBE.Application.DTOs
{
    public class AiModelAdminDto
    {
        public int ModelId { get; set; }
        public string ModelName { get; set; } = null!;
        public string ModelVersion { get; set; } = null!;
        public string ModelPath { get; set; } = null!;
        public string ModelProvider { get; set; } = null!;
        public string ModelType { get; set; } = null!;
        public string ProcessType { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string ModelStatus { get; set; } = "active";
        public DateTime? ModelUpdatedAt { get; set; }
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

    public class AiConfigurationDto
    {
        public float SpamConfidenceThreshold { get; set; }
        public float ToxicityConfidenceThreshold { get; set; }
        public float SimilarityScoreThreshold { get; set; }

        public string? CourseHarmfulTextClassifierPath { get; set; }
        public string? CourseTextEmbeddingGeneratorPath { get; set; }
        public string? CourseMediaEmbeddingGeneratorPath { get; set; }
        public string? ReviewHarmfulTextClassifierPath { get; set; }
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

    public class BaseAiLogAdminDto
    {
        public int LogId { get; set; }
        public int ModelId { get; set; }
        public string ModelName { get; set; } = null!;
        public string? ErrorMessage { get; set; }
        public int LatencyMs { get; set; }
        public DateTime LogCreatedAt { get; set; }
        public string? ResultStatus { get; set; } 
        public string? OutputJson { get; set; }
        public string? InputJson { get; set; }
    }

    public class CourseModerationLogAdminDto : BaseAiLogAdminDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public int TokenUsage { get; set; }
        public string? InteractionType { get; set; }
    }

    public class ReviewModerationLogAdminDto : BaseAiLogAdminDto
    {
        public int ReviewId { get; set; }
        public string Comment { get; set; } = null!;
    }
}
