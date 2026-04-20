using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<string> RegisterAsync(RegisterRequest request);
    Task<LoginResponse?> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(int accountId);
}
