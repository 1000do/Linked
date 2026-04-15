namespace CourseMarketplaceBE.Application.DTOs;

public class UserProfileResponse
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Bio { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; } // Thêm trường này để hiện SĐT
}