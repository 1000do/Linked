using System;

namespace CourseMarketplaceBE.Application.DTOs;

public class ReviewRequest
{
    public int CourseId { get; set; }
    public float Rating { get; set; }
    public string Comment { get; set; } = null!;
    /// <summary>ID của lesson nếu review cho lesson cụ thể. Nếu NULL thì là review khóa học.</summary>
    public int? LessonId { get; set; }
}

public class ReviewResponse
{
    public int ReviewId { get; set; }
    public string UserFullName { get; set; } = null!;
    public string? UserAvatarUrl { get; set; }
    public float Rating { get; set; }
    public string Comment { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    /// <summary>Tên lesson nếu là review lesson cụ thể</summary>
    public string? LessonTitle { get; set; }
    public int? LessonId { get; set; }
    /// <summary>true nếu người viết là chủ khóa học</summary>
    public bool IsInstructor { get; set; }
}

/// <summary>
/// Thông tin trạng thái enrollment + quyền review của user cho 1 khóa học.
/// </summary>
public class EnrollmentStatusResponse
{
    public bool IsEnrolled { get; set; }
    public bool IsCompleted { get; set; }
    public double ProgressPercentage { get; set; }
    public int LearnedMaterialCount { get; set; }
    public int TotalMaterialCount { get; set; }
    public bool CanReview { get; set; }
    public string? ReviewBlockedReason { get; set; }
    public bool HasReviewed { get; set; }
    /// <summary>true nếu user là chủ nhân khóa học (instructor)</summary>
    public bool IsOwner { get; set; }
}

/// <summary>
/// Thống kê phân bổ sao.
/// </summary>
public class ReviewStatsResponse
{
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int Star5Count { get; set; }
    public int Star4Count { get; set; }
    public int Star3Count { get; set; }
    public int Star2Count { get; set; }
    public int Star1Count { get; set; }
    public double Star5Pct => TotalReviews > 0 ? Math.Round((double)Star5Count / TotalReviews * 100, 1) : 0;
    public double Star4Pct => TotalReviews > 0 ? Math.Round((double)Star4Count / TotalReviews * 100, 1) : 0;
    public double Star3Pct => TotalReviews > 0 ? Math.Round((double)Star3Count / TotalReviews * 100, 1) : 0;
    public double Star2Pct => TotalReviews > 0 ? Math.Round((double)Star2Count / TotalReviews * 100, 1) : 0;
    public double Star1Pct => TotalReviews > 0 ? Math.Round((double)Star1Count / TotalReviews * 100, 1) : 0;
}
