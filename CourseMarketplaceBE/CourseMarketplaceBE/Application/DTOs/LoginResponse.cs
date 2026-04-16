namespace CourseMarketplaceBE.Application.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
}