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
}
