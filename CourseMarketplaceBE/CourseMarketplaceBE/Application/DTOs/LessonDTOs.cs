using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Application.DTOs;

public class LessonCreateRequest
{
    public int CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public IFormFile? ThumbnailFile { get; set; }
}

public class MaterialCreateRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? MaterialUrl { get; set; }
    public MaterialMetadata? MaterialMetadata { get; set; }
    public IFormFile? MaterialFile { get; set; }
}

public class LessonResponse
{
    public int LessonId { get; set; }
    public int? CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? LessonStatus { get; set; }
    public string? CourseStatus { get; set; }
    public string? ModerationFeedback { get; set; }
    
    public List<MaterialResponse> LearningMaterials { get; set; } = new List<MaterialResponse>();
}

public class MaterialResponse
{
    public int MaterialId { get; set; }
    public int? LessonId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? MaterialUrl { get; set; }
    public MaterialMetadata? MaterialMetadata { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CourseStatus { get; set; }
    public string? LearningStatus { get; set; }
    public string? ModerationFeedback { get; set; }
}

public class MaterialTrashResponse
{
    public int MaterialId { get; set; }
    public string Title { get; set; } = null!;
    public string? LessonTitle { get; set; }
    public string? CourseTitle { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? FileType { get; set; }
    public string? CloudPublicId { get; set; }
}

public class LessonUpdateTitleRequest
{
    public string Title { get; set; } = null!;
}

public class MaterialUpdateRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
}

