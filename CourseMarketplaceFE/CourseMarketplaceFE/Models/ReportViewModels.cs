using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CourseMarketplaceFE.Models
{
    // ═══════════════════════════════════════════════════════════════════════════════
    // REQUEST VIEW MODELS (User / Instructor tạo report)
    // ═══════════════════════════════════════════════════════════════════════════════

    // Model gửi yêu cầu báo cáo khóa học
    public class CreateCourseReportViewModel
    {
        public int CourseId { get; set; }
        public string Reason { get; set; } = null!;
        public string? Description { get; set; }
    }

    // Model gửi yêu cầu báo cáo đánh giá khóa học / bài học (dùng chung)
    public class CreateReviewReportViewModel
    {
        public int ReviewId { get; set; }
        public string ReviewType { get; set; } = "course_review"; // "course_review" hoặc "lesson_review"
        public string Reason { get; set; } = null!;
        public string? Description { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // STAFF / ADMIN: Xử lý report
    // ═══════════════════════════════════════════════════════════════════════════════

    // Model gửi yêu cầu giải quyết report
    public class ResolveReportViewModel
    {
        public string Status { get; set; } = null!; // resolved | rejected | escalated | under_review
        public string? ResolutionNote { get; set; }
        public bool RemoveContent { get; set; } = false;
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // RESPONSE VIEW MODELS
    // ═══════════════════════════════════════════════════════════════════════════════

    // Lịch sử báo cáo khóa học của cá nhân
    public class MyCourseReportViewModel
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

    // Lịch sử báo cáo đánh giá của cá nhân
    public class MyReviewReportViewModel
    {
        public int ReportId { get; set; }
        public string ReviewType { get; set; } = null!; // "course_review" | "lesson_review"
        public int? ReviewId { get; set; }
        public string? ReviewComment { get; set; }
        public string? Reason { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? ResolutionNote { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    // Chi tiết báo cáo khóa học dành cho Staff/Admin
    public class CourseReportDetailViewModel
    {
        public int ReportId { get; set; }
        public int? ReporterId { get; set; }
        public string? ReporterEmail { get; set; }
        public string? ReporterName { get; set; }
        public int? CourseId { get; set; }
        public string? CourseTitle { get; set; }
        public string? InstructorEmail { get; set; }
        public string? InstructorName { get; set; }
        public int CourseFlagCount { get; set; }
        public string? Reason { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? ResolutionNote { get; set; }
        public string? ResolverEmail { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? AccessGrantedUntil { get; set; }
    }

    // Chi tiết báo cáo đánh giá dành cho Staff/Admin
    public class ReviewReportDetailViewModel
    {
        public int ReportId { get; set; }
        public string ReviewType { get; set; } = null!; // "course_review" | "lesson_review"
        public int? ReporterId { get; set; }
        public string? ReporterEmail { get; set; }
        public string? ReporterName { get; set; }
        public int? ReviewId { get; set; }
        public string? ReviewComment { get; set; }
        public float? ReviewRating { get; set; }
        public string? ReviewAuthorEmail { get; set; }
        public string? ReviewAuthorName { get; set; }
        public string? Reason { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? ResolutionNote { get; set; }
        public string? ResolverEmail { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    // Thống kê báo cáo dành cho dashboard
    public class ReportStatsViewModel
    {
        public int TotalPendingCourseReports { get; set; }
        public int TotalPendingCourseReviewReports { get; set; }
        public int TotalPendingLessonReviewReports { get; set; }
        public int TotalPending { get; set; }
        public int TotalResolvedToday { get; set; }
        public int TotalRejectedToday { get; set; }
    }

    // Kết quả báo cáo mà Instructor xem trên khóa học của mình
    public class InstructorCourseReportSummaryViewModel
    {
        public int CourseId { get; set; }
        public string? CourseTitle { get; set; }
        public int CourseFlagCount { get; set; }
        public List<MyCourseReportViewModel> Reports { get; set; } = new();
    }
}
