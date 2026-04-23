using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<string> RegisterAsync(RegisterRequest request);
    Task<LoginResponse?> GoogleLoginAsync(string idToken);
    Task<bool> SendOtpAsync(string email);
    Task<bool> VerifyEmailAsync(string email, string otp);
    Task<string> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string otp, string newPassword);
    bool VerifyOtpForReset(string email, string otp);

    Task<LoginResponse?> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(int accountId);
}
