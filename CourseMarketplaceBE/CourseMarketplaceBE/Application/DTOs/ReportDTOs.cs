using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CourseMarketplaceBE.Application.DTOs.Common;

namespace CourseMarketplaceBE.Application.DTOs;

// ═══════════════════════════════════════════════════════════════════════════════
// REQUEST DTOs (User / Instructor tạo report)
// ═══════════════════════════════════════════════════════════════════════════════

public class PagedReportRequestDto : PagedRequestDto
{
    [MaxLength(50)]
    public string? Status { get; set; }
}

/// <summary>User/Instructor tạo report khóa học</summary>
public class CreateCourseReportRequest
{
    [Required]
    public int CourseId { get; set; }

    /// <summary>VD: "Nội dung sai lệch", "Vi phạm bản quyền", "Spam", "Nội dung người lớn"</summary>
    [Required]
    [MaxLength(255)]
    public string Reason { get; set; } = null!;

    [MaxLength(1000)]
    public string? Description { get; set; }
}

/// <summary>User/Instructor tạo report review khóa học</summary>
public class CreateCourseReviewReportRequest
{
    [Required]
    public int CourseReviewId { get; set; }
    [Required]
    [MaxLength(255)]
    public string Reason { get; set; } = null!;
    [MaxLength(1000)]
    public string? Description { get; set; }
}

/// <summary>User/Instructor tạo report review bài học</summary>
public class CreateLessonReviewReportRequest
{
    [Required]
    public int LessonReviewId { get; set; }
    [Required]
    [MaxLength(255)]
    public string Reason { get; set; } = null!;
    [MaxLength(1000)]
    public string? Description { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════════
// STAFF / ADMIN: Xử lý report
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>Staff/Admin xử lý 1 report (bất kỳ loại nào)</summary>
public class ResolveReportRequest
{
    /// <summary>resolved | rejected | escalated | under_review</summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = null!;

    [MaxLength(1000)]
    public string ResolutionNote { get; set; } = "Report has been resolved";

    /// <summary>
    /// Chỉ dùng khi Status = "resolved" và cần gỡ nội dung vi phạm.
    /// true = soft-remove review / course bị report
    /// </summary>
    public bool RemoveContent { get; set; } = false;
}

// ═══════════════════════════════════════════════════════════════════════════════
// RESPONSE DTOs
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>Response khi User xem lịch sử report khóa học của mình</summary>
public class MyCourseReportResponse
{
    public int ReportId { get; set; }
    public int? CourseId { get; set; }
    public string? CourseTitle { get; set; }
    public string? Reason { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? ResolutionNote { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

/// <summary>Response khi User xem lịch sử report review của mình</summary>
public class MyReviewReportResponse
{
    public int ReportId { get; set; }
    public string ReviewType { get; set; } = null!;  // "course_review" | "lesson_review"
    public int? ReviewId { get; set; }
    public string? ReviewComment { get; set; }
    public string? Reason { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? ResolutionNote { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

/// <summary>Staff/Admin xem chi tiết 1 course report</summary>
public class CourseReportDetailResponse
{
    public int ReportId { get; set; }

    // Reporter info
    public int? ReporterId { get; set; }
    public string? ReporterEmail { get; set; }
    public string? ReporterName { get; set; }

    // Course info
    public int? CourseId { get; set; }
    public string? CourseTitle { get; set; }
    public string? InstructorEmail { get; set; }
    public string? InstructorName { get; set; }
    public int CourseFlagCount { get; set; }

    // Report details
    public string? Reason { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? ResolutionNote { get; set; }
    public string? ResolverEmail { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? AccessGrantedUntil { get; set; }
}

/// <summary>Staff/Admin xem chi tiết 1 review report (course hoặc lesson)</summary>
public class ReviewReportDetailResponse
{
    public int ReportId { get; set; }
    public string ReviewType { get; set; } = null!;  // "course_review" | "lesson_review"

    // Reporter info
    public int? ReporterId { get; set; }
    public string? ReporterEmail { get; set; }
    public string? ReporterName { get; set; }

    // Review info
    public int? ReviewId { get; set; }
    public string? ReviewComment { get; set; }
    public float? ReviewRating { get; set; }
    public string? ReviewAuthorEmail { get; set; }
    public string? ReviewAuthorName { get; set; }

    // Target content info
    public int? CourseId { get; set; }
    public string? CourseTitle { get; set; }
    public int? LessonId { get; set; }
    public string? LessonTitle { get; set; }

    // Report details
    public string? Reason { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? ResolutionNote { get; set; }
    public string? ResolverEmail { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

/// <summary>Thống kê reports cho Staff/Admin dashboard</summary>
public class ReportStatsResponse
{
    public int TotalPendingCourseReports { get; set; }
    public int TotalPendingCourseReviewReports { get; set; }
    public int TotalPendingLessonReviewReports { get; set; }
    public int TotalPending => TotalPendingCourseReports + TotalPendingCourseReviewReports + TotalPendingLessonReviewReports;
    public int TotalResolvedToday { get; set; }
    public int TotalRejectedToday { get; set; }
}

/// <summary>Kết quả kết hợp: Instructor xem reports nhận được trên 1 khóa học</summary>
public class InstructorCourseReportSummary
{
    public int CourseId { get; set; }
    public string? CourseTitle { get; set; }
    public int CourseFlagCount { get; set; }
    public List<MyCourseReportResponse> Reports { get; set; } = new();
}
