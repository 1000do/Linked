using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceFE.Models;

/// <summary>
/// ViewModel cho form đăng ký trở thành Giảng viên (UC-48).
/// Dùng Data Annotations để validate phía client trước khi gửi lên API.
/// </summary>
public class BecomeInstructorViewModel
{
    /// <summary>Chức danh chuyên môn</summary>
    [Required(ErrorMessage = "Vui lòng nhập chức danh chuyên môn.")]
    [Display(Name = "Chức danh chuyên môn")]
    [StringLength(200, ErrorMessage = "Chức danh không được quá 200 ký tự.")]
    public string ProfessionalTitle { get; set; } = null!;

    /// <summary>Giới thiệu bản thân</summary>
    [Required(ErrorMessage = "Vui lòng nhập phần giới thiệu.")]
    [Display(Name = "Giới thiệu bản thân")]
    [StringLength(2000, ErrorMessage = "Giới thiệu không được quá 2000 ký tự.")]
    public string Bio { get; set; } = null!;

    /// <summary>Lĩnh vực chuyên môn</summary>
    [Required(ErrorMessage = "Vui lòng nhập lĩnh vực chuyên môn.")]
    [Display(Name = "Lĩnh vực chuyên môn")]
    [StringLength(500, ErrorMessage = "Lĩnh vực chuyên môn không được quá 500 ký tự.")]
    public string Expertise { get; set; } = null!;
}
