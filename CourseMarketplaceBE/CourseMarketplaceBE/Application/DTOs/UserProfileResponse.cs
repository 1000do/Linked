namespace CourseMarketplaceBE.Application.DTOs;

public class UserProfileResponse
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Bio { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; } // Thêm trường này để hiện SĐT
    public bool IsVerified { get; set; }

    // ── Thông tin Giảng viên (null nếu chưa đăng ký) ──────────────
    /// <summary>true nếu user đã có bản ghi trong bảng instructors</summary>
    public bool IsInstructor { get; set; }
    public string? ProfessionalTitle { get; set; }
    public string? ExpertiseCategories { get; set; }
    public string? LinkedinUrl { get; set; }
    /// <summary>Pending | Approved | Rejected (null nếu chưa đăng ký)</summary>
    public string? ApprovalStatus { get; set; }
}