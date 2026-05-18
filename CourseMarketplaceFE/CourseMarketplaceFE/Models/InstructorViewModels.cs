using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceFE.Models;

public class ApiResponse<T>
{
    public int Status { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}


/// <summary>
/// ViewModel cho form nộp đơn Giảng viên (Apply.cshtml).
/// </summary>
public class InstructorApplyViewModel
{
    [Required(ErrorMessage = "Please enter your professional title.")]
    [StringLength(255)]
    [Display(Name = "Professional Title")]
    public string ProfessionalTitle { get; set; } = null!;

    [Required(ErrorMessage = "Please enter your expertise categories.")]
    [StringLength(255)]
    [Display(Name = "Expertise Categories")]
    public string ExpertiseCategories { get; set; } = null!;

    [Display(Name = "LinkedIn URL")]
    public string? LinkedinUrl { get; set; }

    [Display(Name = "YouTube URL")]
    public string? YoutubeUrl { get; set; }

    [Display(Name = "Facebook URL")]
    public string? FacebookUrl { get; set; }

    [Display(Name = "CV / ID Document")]
    public IFormFile? DocumentFile { get; set; }

    /// <summary>URL tài liệu cũ (khi nộp lại sau khi bị Rejected)</summary>
    public string? ExistingDocumentUrl { get; set; }

    /// <summary>True nếu đang nộp lại (status cũ = Rejected)</summary>
    public bool IsResubmit { get; set; }

    [Required(ErrorMessage = "Please select a country for your Stripe account.")]
    [Display(Name = "Stripe Country")]
    public string StripeCountry { get; set; } = null!;

    /// <summary>Danh sách quốc gia để bind vào dropdown</summary>
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> AvailableCountries { get; set; } = new();

    /// <summary>Danh sách lĩnh vực chuyên môn từ categories</summary>
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> AvailableCategories { get; set; } = new();
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

public class InstructorPublicProfileViewModel
{
    public int InstructorId { get; set; }
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? ProfessionalTitle { get; set; }
    public string? ExpertiseCategories { get; set; }
    public string? Bio { get; set; }
    public string? LinkedinUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<InstructorCourseViewModel> Courses { get; set; } = new();
}

public class InstructorCourseViewModel
{
    public int CourseId { get; set; }
    public string? Title { get; set; }
    public string? ThumbnailUrl { get; set; }
    public decimal Price { get; set; }
    public decimal Rating { get; set; }
    public int TotalStudents { get; set; }
}
