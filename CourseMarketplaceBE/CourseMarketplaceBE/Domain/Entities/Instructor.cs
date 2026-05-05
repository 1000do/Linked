using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

/// <summary>
/// Entity ánh xạ bảng instructors.
/// Gộp cả thông tin đơn đăng ký + trạng thái duyệt + Stripe Connect.
/// </summary>
public partial class Instructor
{
    public int InstructorId { get; set; }

    // ── Thông tin đơn đăng ký (Learner nộp) ──────────────────────────
    public string? ProfessionalTitle { get; set; }
    public string? ExpertiseCategories { get; set; }
    public string? LinkedinUrl { get; set; }
    public string? DocumentUrl { get; set; }  // Link CV/ID trên Cloudinary

    // ── Trạng thái duyệt (Admin quản lý) ────────────────────────────
    /// <summary>Pending | Approved | Rejected</summary>
    public string? ApprovalStatus { get; set; }

    // ── Stripe Connect Onboarding ────────────────────────────────────
    public string? StripeAccountId { get; set; }
    public string? StripeOnboardingStatus { get; set; }

    /// <summary>
    /// Stripe PayoutsEnabled – true khi giảng viên đã hoàn tất xác thực
    /// tài chính và Stripe cho phép rút tiền (payouts) về tài khoản ngân hàng.
    /// </summary>
    public bool? PayoutsEnabled { get; set; }

    /// <summary>
    /// Stripe ChargesEnabled – true khi Stripe cho phép tài khoản giảng viên
    /// nhận thanh toán (charges) từ học viên mua khóa học.
    /// </summary>
    public bool? ChargesEnabled { get; set; }

    /// <summary>
    /// Mã quốc gia ISO 2 ký tự mà giảng viên chọn khi đăng ký Stripe Connect.
    /// VD: "SG", "AU", "US", "JP". Phải nằm trong danh sách StripeCountries ở system_configs.
    /// </summary>
    public string? StripeCountry { get; set; }

    // instructor_rating & total_revenue đã bị xóa khỏi bảng instructors trong SQL v2
    // Dùng VIEW view_instructor_stats để lấy các giá trị này (tính động qua SQL)

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<InstructorPayout> InstructorPayouts { get; set; } = new List<InstructorPayout>();

    public virtual User InstructorNavigation { get; set; } = null!;
}
