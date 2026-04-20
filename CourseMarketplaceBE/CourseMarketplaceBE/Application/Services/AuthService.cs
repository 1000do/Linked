using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Share.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CourseMarketplaceBE.Application.IServices;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IUserRepository userRepo, JwtSettings jwtSettings)
    {
        _userRepo = userRepo;
        _jwtSettings = jwtSettings;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest r)
    {
        var a = await _userRepo.GetAccountByEmailAsync(r.Email.ToLower());

        if (a == null || !BCrypt.Net.BCrypt.Verify(r.Password, a.PasswordHash))
            return null;

        await _userRepo.UpdateLastLoginAsync(a.AccountId);

        var fullName = a.User?.FullName ?? "User";
        var avatar = a.AvatarUrl ?? "";

        var accessToken = GenerateAccessToken(a);
        var refreshToken = GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        await _userRepo.SaveRefreshTokenAsync(a.AccountId, refreshToken, expiry);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            FullName = fullName,
            AvatarUrl = avatar
        };
    }

    public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken)
    {
        var a = await _userRepo.GetAccountByRefreshTokenAsync(refreshToken);

        // Token không tồn tại hoặc đã hết hạn → yêu cầu đăng nhập lại
        if (a == null || a.RefreshTokenExpiryTime == null || a.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return null;

        // Phát access token mới + rotate refresh token (bảo mật: mỗi lần dùng đổi token mới)
        var newAccessToken = GenerateAccessToken(a);
        var newRefreshToken = GenerateRefreshToken();
        var newExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        await _userRepo.SaveRefreshTokenAsync(a.AccountId, newRefreshToken, newExpiry);

        return new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            FullName = a.User?.FullName ?? "User",
            AvatarUrl = a.AvatarUrl ?? ""
        };
    }

    public async Task<bool> LogoutAsync(int accountId)
    {
        await _userRepo.RevokeRefreshTokenAsync(accountId);
        return true;
    }

    public async Task<string> RegisterAsync(RegisterRequest r)
    {
        var acc = new Account
        {
            Email = r.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(r.Password),
            AccountStatus = "active",
            AuthProvider = "local",
            AccountCreatedAt = DateTime.Now,
            AccountUpdatedAt = DateTime.Now
        };

        var user = new User
        {
            FullName = r.FullName,
         
        };

        return await _userRepo.RegisterUserAsync(acc, user) ? "Success" : "Error";
    }

    // ─── PRIVATE HELPERS ──────────────────────────────────────────────────────

    private string GenerateAccessToken(Account a)
    {
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key!);
        var fullName = a.User?.FullName ?? "User";
        var avatar = a.AvatarUrl ?? "";

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, a.AccountId.ToString()),
                new Claim(ClaimTypes.Email, a.Email),
                new Claim("FullName", fullName),
                new Claim("AvatarUrl", avatar)
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        return new JwtSecurityTokenHandler()
            .WriteToken(new JwtSecurityTokenHandler().CreateToken(tokenDescriptor));
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}