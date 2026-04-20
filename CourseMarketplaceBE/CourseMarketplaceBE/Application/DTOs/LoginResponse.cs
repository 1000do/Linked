namespace CourseMarketplaceBE.Application.DTOs;

public class LoginResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    /// <summary>"user" | "manager" — dùng để FE redirect đúng trang sau login</summary>
    public string Role { get; set; } = "user";
}