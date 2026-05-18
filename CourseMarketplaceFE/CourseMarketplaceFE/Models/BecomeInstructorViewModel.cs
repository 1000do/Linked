using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceFE.Models;

/// <summary>
/// ViewModel cho form đăng ký trở thành Giảng viên (UC-48).
/// Dùng Data Annotations để validate phía client trước khi gửi lên API.
/// </summary>
public class BecomeInstructorViewModel
{
    /// <summary>Chức danh chuyên môn</summary>
    [Required(ErrorMessage = "Please enter your professional title.")]
    [Display(Name = "Professional Title")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string ProfessionalTitle { get; set; } = null!;

    /// <summary>Giới thiệu bản thân</summary>
    [Required(ErrorMessage = "Please enter your bio.")]
    [Display(Name = "Bio")]
    [StringLength(2000, ErrorMessage = "Bio cannot exceed 2000 characters.")]
    public string Bio { get; set; } = null!;

    /// <summary>Lĩnh vực chuyên môn</summary>
    [Required(ErrorMessage = "Please enter your expertise categories.")]
    [Display(Name = "Expertise Categories")]
    [StringLength(500, ErrorMessage = "Expertise categories cannot exceed 500 characters.")]
    public string Expertise { get; set; } = null!;
}
