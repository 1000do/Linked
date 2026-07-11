using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CourseMarketplaceBE.Application.DTOs;

/// <summary>
/// DTO nhận từ Learner khi nộp đơn đăng ký Giảng viên.
/// Endpoint: POST /api/instructor/apply
/// </summary>
public class InstructorApplicationRequest
{
    [Required(ErrorMessage = "Please enter your professional title.")]
    [StringLength(255)]
    public string ProfessionalTitle { get; set; } = null!;

    [Required(ErrorMessage = "Please enter your areas of expertise.")]
    [StringLength(255)]
    public string ExpertiseCategories { get; set; } = null!;

    public string? LinkedinUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? FacebookUrl { get; set; }

    /// <summary>File CV/CMND upload lên Cloudinary</summary>
    public IFormFile? DocumentFile { get; set; }

    /// <summary>Danh sách file CV/chứng chỉ/tài liệu upload (tối đa 3)</summary>
    public List<IFormFile>? DocumentFiles { get; set; }

    /// <summary>Danh sách URL tài liệu cũ được giữ lại</summary>
    public List<string>? RetainedDocumentUrls { get; set; }

    [Required(ErrorMessage = "Please select the country for your Stripe account.")]
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
    public string? YoutubeUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? DocumentUrl { get; set; }
    public string? ApprovalStatus { get; set; }
    public string? RejectionReason { get; set; }
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
    public int PendingCoursesCount { get; set; }
    public int DraftCoursesCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public double EnrollmentGrowthPercentage { get; set; }
    public int InstructorRankPercentage { get; set; }
}

public class InstructorPayoutDto
{
    public int PayoutId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PayoutDate { get; set; }
    public bool IsPaid { get; set; }
    public string CourseTitle { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string? PayoutStatus { get; set; }
    public DateTime? PaidToBankAt { get; set; }
    public string? StripeTransferId { get; set; }
    public string? StripePayoutId { get; set; }
}



/// <summary>
/// DTO cho trang Instructor Public Profile.
/// </summary>
public class InstructorPublicProfileDto
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

    // Statistics
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }

    // Courses list
    public List<InstructorCourseDto> Courses { get; set; } = new();
}

public class InstructorCourseDto
{
    public int CourseId { get; set; }
    public string? Title { get; set; }
    public string? ThumbnailUrl { get; set; }
    public decimal Price { get; set; }
    public decimal Rating { get; set; }
    public int TotalStudents { get; set; }
}
