using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Share.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;

namespace CourseMarketplaceBE.Application.IServices;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly JwtSettings _jwtSettings;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepo, JwtSettings jwtSettings, IOtpService otpService,
    IEmailService emailService, IConfiguration config)
    {
        _userRepo = userRepo;
        _jwtSettings = jwtSettings;
        _otpService = otpService;
        _emailService = emailService;
        _config = config;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest r)
    {
        var a = await _userRepo.GetAccountByEmailAsync(r.Email.ToLower());

        if (a == null || !BCrypt.Net.BCrypt.Verify(r.Password, a.PasswordHash))
            return null;

        await _userRepo.UpdateLastLoginAsync(a.AccountId);

        // ── Detect role ───────────────────────────────────────────────────────
        var role = await _userRepo.GetRoleByAccountIdAsync(a.AccountId);

        var fullName = role == "manager"
            ? (a.Manager?.DisplayName ?? "Manager")
            : (a.User?.FullName ?? "User");
        var avatar = a.AvatarUrl ?? "";

        var accessToken = GenerateAccessToken(a, role);
        var refreshToken = GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        await _userRepo.SaveRefreshTokenAsync(a.AccountId, refreshToken, expiry);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            FullName = fullName,
            AvatarUrl = avatar,
            Role = role
        };
    }

    public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken)
    {
        var a = await _userRepo.GetAccountByRefreshTokenAsync(refreshToken);

        if (a == null || a.RefreshTokenExpiryTime == null || a.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return null;

        var role = await _userRepo.GetRoleByAccountIdAsync(a.AccountId);

        var newAccessToken = GenerateAccessToken(a, role);
        var newRefreshToken = GenerateRefreshToken();
        var newExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        await _userRepo.SaveRefreshTokenAsync(a.AccountId, newRefreshToken, newExpiry);

        var fullName = role == "manager"
            ? (a.Manager?.DisplayName ?? "Manager")
            : (a.User?.FullName ?? "User");

        return new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            FullName = fullName,
          
            Role = role
            AvatarUrl = avatar,
            IsVerified = a.IsVerified
        };
    }

    public async Task<bool> LogoutAsync(int accountId)
    {
        await _userRepo.RevokeRefreshTokenAsync(accountId);
        return true;
    }

    public async Task<string> RegisterAsync(RegisterRequest r)
    {
        if (await _userRepo.IsEmailExistsAsync(r.Email.ToLower()))
            return "Email already exists";

        var acc = new Account
        {
            Email = r.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(r.Password),
            AccountStatus = "active",
            AuthProvider = "local",
            IsVerified = false,
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

    private string GenerateAccessToken(Account a, string role)
    {
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key!);
        var fullName = role == "manager"
            ? (a.Manager?.DisplayName ?? "Manager")
            : (a.User?.FullName ?? "User");
        var avatar = a.AvatarUrl ?? "";

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, a.AccountId.ToString()),
                new Claim(ClaimTypes.Email, a.Email),
                new Claim(ClaimTypes.Role, role),          // ← dùng [Authorize(Roles="manager")]
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

    //Google login
    public async Task<LoginResponse?> GoogleLoginAsync(string idToken)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

        var email = payload.Email.ToLower();

        var account = await _userRepo.GetAccountByEmailAsync(email);

        if (account == null)
        {
            var acc = new Account
            {
                Email = email,
                PasswordHash = null, // GOOGLE → NULL
                AuthProvider = "google",
                IsVerified = true, // AUTO VERIFIED
                AvatarUrl = payload.Picture,
                AccountCreatedAt = DateTime.Now,
                AccountUpdatedAt = DateTime.Now
            };

            var user = new User
            {
                FullName = payload.Name
            };

            var created = await _userRepo.RegisterUserAsync(acc, user);

            if (!created) return null;

            account = await _userRepo.GetAccountByEmailAsync(email);
        }

        // GENERATE JWT (reuse code cũ)
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key!);

        var fullName = account!.User?.FullName ?? payload.Name;
        var avatar = account.AvatarUrl ?? payload.Picture;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim("FullName", fullName),
            new Claim("AvatarUrl", avatar ?? "")
        }),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = new JwtSecurityTokenHandler()
            .WriteToken(new JwtSecurityTokenHandler().CreateToken(tokenDescriptor));

        return new LoginResponse
        {
            Token = token,
            FullName = fullName,
            AvatarUrl = avatar,
            IsVerified = true
        };
    }

    //Send OTP
    public async Task<bool> SendOtpAsync(string email)
    {
        var acc = await _userRepo.GetAccountByEmailAsync(email.ToLower());
        if (acc == null) return false;

        var otp = _otpService.GenerateOtp(email, "verify");

        var body = $@"
        <h3>Xác Thực Email</h3>
    <p>Mã OTP của bạn là: <b>{otp}</b></p>
    <p>Sẽ hết hạn trong 2 phút</p>
    <p>Nếu đây không phải là bạn, vui lòng liên hệ đội ngũ hỗ trợ để được giải quyết.</p>
    ";

        await _emailService.SendEmailAsync(email, "Email Verification", body);

        return true;
    }

    public async Task<bool> VerifyEmailAsync(string email, string otp)
    {
        var isValid = _otpService.ConsumeOtp(email, otp, "verify");

        if (!isValid) return false;

        return await _userRepo.UpdateEmailVerifiedAsync(email.ToLower());
    }

    //forgot password
    public async Task<string> ForgotPasswordAsync(string email)
    {
        var acc = await _userRepo.GetAccountByEmailAsync(email.ToLower());

        if (acc == null)
            return "Email not found";

        if (acc.AuthProvider == "google")
            return "This account uses Google login";

        var otp = _otpService.GenerateOtp(email, "reset");

        var body = $@"
        <h3>Reset Password</h3>
         <p>Mã OTP của bạn là: <b>{otp}</b></p>
    <p>Sẽ hết hạn trong 2 phút</p>
    <p>Nếu đây không phải là bạn, vui lòng liên hệ đội ngũ hỗ trợ để được giải quyết.</p>
    ";

        await _emailService.SendEmailAsync(email, "Reset Password", body);

        return "OTP sent";
    }

    public async Task<bool> ResetPasswordAsync(string email, string otp, string newPassword)
    {
        var isValid = _otpService.ConsumeOtp(email, otp, "reset");

        if (!isValid) return false;

        var acc = await _userRepo.GetAccountByEmailAsync(email.ToLower());

        if (acc == null) return false;

        if (acc.AuthProvider == "google") return false;

        acc.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        acc.AccountUpdatedAt = DateTime.Now;

        await _userRepo.UpdateAccountAsync(acc);

        return true;
    }

    public bool VerifyOtpForReset(string email, string otp)
    {
        return _otpService.ValidateOtp(email, otp, "reset");
    }
}