using System;
using CourseMarketplaceBE.Application.DTOs.Common;

namespace CourseMarketplaceBE.Application.DTOs;

public class PagedReviewModerationRequest : PagedRequestDto
{
    public string? Search { get; set; }
    public string? ModerationStatus { get; set; } = "all";
    public string? RequestType { get; set; } = "both";
    public string? AiModerationStatus { get; set; } = "all";
    public string? SortBy { get; set; } = "priority_desc";
}

public class ReviewModerationRecordDto
{
    public int RecordId { get; set; }
    public int ReviewId { get; set; } // CourseReviewId or LessonReviewId
    public string Type { get; set; } = null!; // "course" or "lesson"
    public bool IsUpdate { get; set; }
    public string TempComment { get; set; } = null!;
    public decimal TempRating { get; set; }
    
    // Original Review Info (for comparison if IsUpdate == true)
    public string? OriginalComment { get; set; }
    public decimal? OriginalRating { get; set; }

    public string AiModerationStatus { get; set; } = null!;
    public string AiModerationNote { get; set; } = null!;
    public string ModerationStatus { get; set; } = null!;
    public string ModerationNote { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Author Info
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    
    // Course/Lesson Info
    public string CourseTitle { get; set; } = null!;
    public string? LessonTitle { get; set; }
}

public class ApproveRejectReviewRequest
{
    public int RecordId { get; set; }
    public string Type { get; set; } = null!; // "course" or "lesson"
    public string ModerationNote { get; set; } = null!;
}
