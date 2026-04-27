using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceFE.Models;

/// <summary>
/// ViewModel cho form nộp đơn Giảng viên (Apply.cshtml).
/// </summary>
public class InstructorApplyViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập chức danh chuyên môn.")]
    [StringLength(255)]
    [Display(Name = "Chức danh chuyên môn")]
    public string ProfessionalTitle { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập lĩnh vực chuyên môn.")]
    [StringLength(255)]
    [Display(Name = "Lĩnh vực chuyên môn")]
    public string ExpertiseCategories { get; set; } = null!;

    [Display(Name = "LinkedIn URL")]
    public string? LinkedinUrl { get; set; }

    [Display(Name = "Tài liệu CV / CMND")]
    public IFormFile? DocumentFile { get; set; }

    /// <summary>URL tài liệu cũ (khi nộp lại sau khi bị Rejected)</summary>
    public string? ExistingDocumentUrl { get; set; }

    /// <summary>True nếu đang nộp lại (status cũ = Rejected)</summary>
    public bool IsResubmit { get; set; }
}

/// <summary>
/// ViewModel cho Dashboard Giảng viên.
/// </summary>
public class InstructorDashboardViewModel
{
    public int InstructorId { get; set; }
    public string? ProfessionalTitle { get; set; }
    public string? ExpertiseCategories { get; set; }
    public string? ApprovalStatus { get; set; }
    public string? StripeOnboardingStatus { get; set; }
    public bool PayoutsEnabled { get; set; }
    public bool ChargesEnabled { get; set; }
    public string? FullName { get; set; }
}
