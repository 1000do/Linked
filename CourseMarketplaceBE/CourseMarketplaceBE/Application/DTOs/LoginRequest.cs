namespace CourseMarketplaceBE.Application.DTOs;

public class LoginRequest
{
    public string UsernameOrEmail { get; set; } = null!;
    public string Password { get; set; } = null!;
}
