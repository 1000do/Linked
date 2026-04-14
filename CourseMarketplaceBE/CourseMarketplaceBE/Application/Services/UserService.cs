using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Share.Helpers;
using Microsoft.IdentityModel.Tokens;

namespace CourseMarketplaceBE.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly JwtSettings _jwtSettings;

    public UserService(IUserRepository userRepo, JwtSettings jwtSettings)
    {
        _userRepo = userRepo;
        _jwtSettings = jwtSettings;
    }

    public async Task<string> GetFullNameByEmailAsync(string email)
    {
        // Chuyển về chữ thường để query chính xác
        var account = await _userRepo.GetAccountByEmailAsync(email.ToLower());
        return account?.User?.FullName ?? "User";
    }

    public async Task<string> RegisterAsync(RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return "Mật khẩu xác nhận không khớp.";

        if (await _userRepo.IsEmailExistsAsync(request.Email.ToLower()))
            return "Email đã tồn tại.";

        var account = new Account
        {
            Email = request.Email.ToLower(), // Luôn lưu email chữ thường
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            AccountStatus = "active",
            AccountFlagCount = 0,
            AuthProvider = "local",
            AccountCreatedAt = DateTime.Now,
            AccountUpdatedAt = DateTime.Now
        };

        var user = new User
        {
            FullName = request.FullName,
            TotalSpent = 0.00m,
            EnrolledCoursesCount = 0,
            Bio = ""
        };

        var success = await _userRepo.RegisterUserAsync(account, user);
        return success ? "Success" : "Lỗi hệ thống khi lưu dữ liệu.";
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        // 1. Tìm tài khoản (Chuyển email về chữ thường trước khi tìm)
        var account = await _userRepo.GetAccountByEmailAsync(request.Email.ToLower());

        // Nếu không tìm thấy Account hoặc Account không có thông tin User đi kèm
        if (account == null) return null;

        // 2. Verify mật khẩu (Hash trong DB vs Password người dùng nhập)
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash);
        if (!isPasswordValid) return null;

        // 3. Cập nhật thời gian đăng nhập cuối
        await _userRepo.UpdateLastLoginAsync(account.AccountId);

        // 4. Tạo JWT Token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key ?? "Key_Mac_Dinh_Sieu_Bao_Mat_32_Ky_Tu");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim("FullName", account.User?.FullName ?? "User")
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}