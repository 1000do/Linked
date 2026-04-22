using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Share.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key!);

        // Lấy thông tin hiển thị
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
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityTokenHandler().CreateToken(tokenDescriptor));

        // Trả về object đầy đủ
        return new LoginResponse    
        {
            Token = token,
            FullName = fullName,
            AvatarUrl = avatar
        };
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
            //TotalSpent = 0,
            //EnrolledCoursesCount = 0
        };

        return await _userRepo.RegisterUserAsync(acc, user) ? "Success" : "Error";
    }
}