using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Domain.Constants;
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
    private readonly ILockoutRepository _lockoutRepo;
    private readonly JwtSettings _jwtSettings;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepo, ILockoutRepository lockoutRepo, JwtSettings jwtSettings, IOtpService otpService,
    IEmailService emailService, IConfiguration config)
    {
        _userRepo = userRepo;
        _lockoutRepo = lockoutRepo;
        _jwtSettings = jwtSettings;
        _otpService = otpService;
        _emailService = emailService;
        _config = config;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest r)
    {
        var a = await _userRepo.GetAccountByEmailOrUsernameAsync(r.UsernameOrEmail.ToLower());

        if (a == null || !BCrypt.Net.BCrypt.Verify(r.Password, a.PasswordHash))
            return null;

        await EnsureAccountNotLockedOutAsync(a.AccountId);
        await _userRepo.UpdateLastLoginAsync(a.AccountId);

        return await GenerateLoginResponseAsync(a);
    }

    public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken)
    {
        var a = await _userRepo.GetAccountByRefreshTokenAsync(refreshToken);

        if (a == null || a.RefreshTokenExpiryTime == null || a.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return null;

        return await GenerateLoginResponseAsync(a);
    }

    public async Task<bool> LogoutAsync(int accountId)
    {
        await _userRepo.RevokeRefreshTokenAsync(accountId);
        return true;
    }

    public async Task<string> RegisterAsync(RegisterRequest r)
    {
        var validationError = await ValidateRegisterRequestAsync(r);
        if (validationError != null)
            return validationError;

        var acc = new Account
        {
            Email = r.Email.ToLower(),
            Username = r.Username.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(r.Password),
            AccountStatus = AccountStatus.Active.ToValue(),
            AuthProvider = AuthProvider.Local.ToValue(),
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

    public async Task<LoginResponse?> GoogleLoginAsync(string idToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            IssuedAtClockTolerance = TimeSpan.FromMinutes(5)
        };
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        var email = payload.Email.ToLower();
        var account = await _userRepo.GetAccountByEmailAsync(email);

        if (account == null)
        {
            account = await CreateGoogleAccountAsync(email, payload.Name, payload.Picture);
            if (account == null) return null;
        }

        await EnsureAccountNotLockedOutAsync(account.AccountId);
        await _userRepo.UpdateLastLoginAsync(account.AccountId);

        return await GenerateLoginResponseAsync(account, payload.Name, payload.Picture, true);
    }

    public async Task<bool> SendOtpAsync(string email)
    {
        var acc = await _userRepo.GetAccountByEmailAsync(email.ToLower());
        if (acc == null) return false;

        var otp = _otpService.GenerateOtp(email, "verify");
        var body = GenerateOtpEmailBody("Email Verification", otp);

        await _emailService.SendEmailAsync(email, "Email Verification", body);

        return true;
    }

    public async Task<bool> VerifyEmailAsync(string email, string otp)
    {
        var isValid = _otpService.ConsumeOtp(email, otp, "verify");
        if (!isValid) return false;

        return await _userRepo.UpdateEmailVerifiedAsync(email.ToLower());
    }

    public async Task<string> ForgotPasswordAsync(string email)
    {
        var acc = await _userRepo.GetAccountByEmailAsync(email.ToLower());

        if (acc == null)
            return "Email not found";

        if (acc.AuthProvider == AuthProvider.Google.ToValue())
            return "This account uses Google login";

        if (!acc.IsVerified)
            return "Email is not verified";

        var otp = _otpService.GenerateOtp(email, "reset");
        var body = GenerateOtpEmailBody("Reset Password", otp);

        await _emailService.SendEmailAsync(email, "Reset Password", body);

        return "OTP sent";
    }

    public async Task<bool> ResetPasswordAsync(string email, string otp, string newPassword)
    {
        var isValid = _otpService.ConsumeOtp(email, otp, "reset");
        if (!isValid) return false;

        var acc = await _userRepo.GetAccountByEmailAsync(email.ToLower());
        if (acc == null || acc.AuthProvider == AuthProvider.Google.ToValue()) 
            return false;

        acc.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        acc.AccountUpdatedAt = DateTime.Now;

        await _userRepo.UpdateAccountAsync(acc);

        return true;
    }

    public bool VerifyOtpForReset(string email, string otp)
    {
        return _otpService.ValidateOtp(email, otp, "reset");
    }

    public async Task<bool> IsEmailVerifiedAsync(int userId)
    {
        var account = await _userRepo.GetAccountByIdAsync(userId);
        return account?.IsVerified ?? false;
    }

    // ─── PRIVATE HELPERS ──────────────────────────────────────────────────────

    private async Task EnsureAccountNotLockedOutAsync(int accountId)
    {
        var activeLockout = await _lockoutRepo.GetActiveLockoutAsync(accountId, "account");
        if (activeLockout != null)
            throw new UnauthorizedAccessException($"Your account has been suspended until {activeLockout.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss} due to community standards violations.");
    }

    private async Task<LoginResponse> GenerateLoginResponseAsync(Account account, string? fallbackName = null, string? fallbackAvatar = null, bool? isVerifiedOverride = null)
    {
        var role = await _userRepo.GetRoleByAccountIdAsync(account.AccountId);

        var fullName = ResolveFullName(account, role, fallbackName);
        var avatar = ResolveAvatarUrl(account, fallbackAvatar);

        var accessToken = GenerateAccessToken(account, role, fullName, avatar);
        var refreshToken = await CreateAndSaveRefreshTokenAsync(account.AccountId);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccountId = account.AccountId,
            FullName = fullName,
            AvatarUrl = avatar,
            Role = role,
            IsVerified = isVerifiedOverride ?? account.IsVerified
        };
    }

    private string ResolveFullName(Account account, string role, string? fallbackName)
    {
        if (role == "manager")
            return account.Manager?.DisplayName ?? "Manager";
            
        return account.User?.FullName ?? fallbackName ?? "User";
    }

    private string ResolveAvatarUrl(Account account, string? fallbackAvatar)
    {
        return account.AvatarUrl ?? fallbackAvatar ?? "";
    }

    private async Task<string> CreateAndSaveRefreshTokenAsync(int accountId)
    {
        var refreshToken = GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        await _userRepo.SaveRefreshTokenAsync(accountId, refreshToken, expiry);
        return refreshToken;
    }

    private string GenerateAccessToken(Account a, string role, string fullName, string avatar)
    {
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key!);

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

    private async Task<string?> ValidateRegisterRequestAsync(RegisterRequest r)
    {
        if (!r.Email.ToLower().EndsWith("@gmail.com"))
            return "Email must be a @gmail.com address";

        if (!System.Text.RegularExpressions.Regex.IsMatch(r.Password, @"^(?=.*[^a-zA-Z0-9]).+$"))
            return "Password must contain at least 1 special character";

        if (await _userRepo.IsEmailExistsAsync(r.Email.ToLower()))
            return "Email already exists";

        if (await _userRepo.IsUsernameExistsAsync(r.Username))
            return "Username already exists";

        return null;
    }

    private async Task<Account?> CreateGoogleAccountAsync(string email, string? name, string? picture)
    {
        var baseUsername = email.Split('@')[0];
        var username = baseUsername;
        int count = 1;
        while (await _userRepo.IsUsernameExistsAsync(username))
        {
            username = $"{baseUsername}{count}";
            count++;
        }

        var acc = new Account
        {
            Email = email,
            Username = username,
            PasswordHash = null, // GOOGLE → NULL
            AuthProvider = AuthProvider.Google.ToValue(),
            AccountStatus = AccountStatus.Active.ToValue(),
            IsVerified = true, // AUTO VERIFIED
            AvatarUrl = picture,
            AccountCreatedAt = DateTime.Now,
            AccountUpdatedAt = DateTime.Now
        };

        var user = new User
        {
            FullName = name
        };

        var created = await _userRepo.RegisterUserAsync(acc, user);
        if (!created) return null;

        return await _userRepo.GetAccountByEmailAsync(email);
    }

    private string GenerateOtpEmailBody(string title, string otp)
    {
        return $@"
        <h3>{title}</h3>
    <p>Your OTP code is: <b>{otp}</b></p>
    <p>Expires in 2 minutes</p>
    <p>If this was not you, please contact support.</p>
    ";
    }
}
