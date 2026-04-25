namespace CourseMarketplaceBE.Application.DTOs;

/// <summary>
/// DTO chứa thông tin Học viên gửi lên khi muốn trở thành Giảng viên (UC-48).
/// </summary>
public class BecomeInstructorRequest
{
    /// <summary>Chức danh chuyên môn, ví dụ: "Senior .NET Developer"</summary>
    public string ProfessionalTitle { get; set; } = null!;

    /// <summary>Giới thiệu bản thân ngắn gọn</summary>
    public string Bio { get; set; } = null!;

    /// <summary>Lĩnh vực chuyên môn, ví dụ: "C#, Cloud, DevOps"</summary>
    public string Expertise { get; set; } = null!;
}
