using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CourseMarketplaceBE.Application.DTOs;

/// <summary>
/// DTO nhận từ Learner khi nộp đơn đăng ký Giảng viên.
/// Endpoint: POST /api/instructor/apply
/// </summary>
public class InstructorApplicationRequest
{
    [Required(ErrorMessage = "Vui lòng nhập chức danh chuyên môn.")]
    [StringLength(255)]
    public string ProfessionalTitle { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập lĩnh vực chuyên môn.")]
    [StringLength(255)]
    public string ExpertiseCategories { get; set; } = null!;

    public string? LinkedinUrl { get; set; }

    /// <summary>File CV/CMND upload lên Cloudinary</summary>
    public IFormFile? DocumentFile { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn quốc gia cho tài khoản Stripe.")]
    [StringLength(2)]
    public string StripeCountry { get; set; } = null!;
}

/// <summary>
/// Response trả về URL Onboarding của Stripe cho Instructor đã được Approved.
/// </summary>
public class StripeSetupResponse
{
    public string OnboardingUrl { get; set; } = null!;
    public string StripeAccountId { get; set; } = null!;
}

/// <summary>
/// DTO thông tin Instructor hiển thị trên Dashboard.
/// </summary>
public class InstructorDashboardDto
{
    public int InstructorId { get; set; }
    public string? ProfessionalTitle { get; set; }
    public string? ExpertiseCategories { get; set; }
    public string? LinkedinUrl { get; set; }
    public string? DocumentUrl { get; set; }
    public string? ApprovalStatus { get; set; }
    public string? StripeAccountId { get; set; }
    public string? StripeOnboardingStatus { get; set; }
    public bool PayoutsEnabled { get; set; }
    public bool ChargesEnabled { get; set; }
    public string? StripeCountry { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }

    // Statistics from Views
    public int TotalStudents { get; set; }
    public decimal AverageRating { get; set; }
    public int ActiveCoursesCount { get; set; }
    public decimal TotalRevenue { get; set; }
}
