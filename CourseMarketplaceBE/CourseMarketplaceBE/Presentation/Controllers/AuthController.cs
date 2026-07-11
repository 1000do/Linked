using CloudinaryDotNet.Actions;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CourseMarketplaceBE.Presentation.Filters;

namespace CourseMarketplaceBE.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // ─── REGISTER ─────────────────────────────────────────────────────
    [HttpPost("register")]
    [AllowAnonymous]
    [CustomAuthorize(requireAuth: false)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result == "Success")
            return Ok(new { status = 200, message = "Account registered successfully!" });

        return BadRequest(new { status = 400, message = result });
    }

    // ─── LOGIN ────────────────────────────────────────────────────────
    [HttpPost("login")]
    [AllowAnonymous]
    [CustomAuthorize(requireAuth: false)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.UsernameOrEmail))
        {
            return BadRequest(new { status = 400, message = "Username or Email is required." });
        }
        var result = await _authService.LoginAsync(request);
    
        if (result == null)
            return Unauthorized(new { status = 401, message = "Incorrect username/email or password." });

        // Access token: HttpOnly cookie, dài hạn (24 giờ — khớp JWT)
        Response.Cookies.Append("AccessToken", result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(24),
            Path = "/"
        });

        // Refresh token: HttpOnly cookie, dài hạn (7 ngày)
        Response.Cookies.Append("RefreshToken", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/api/auth/refresh" // Chỉ gửi đến endpoint refresh
        });

        return Ok(new
        {
            status = 200,
            message = "Login successful",
            accessToken = result.AccessToken,   // Trả về để FE lưu in-memory
            refreshToken = result.RefreshToken,
            fullName = result.FullName,
            avatarUrl = result.AvatarUrl,
            role = result.Role
        });
    }

    // ─── REFRESH TOKEN ────────────────────────────────────────────────
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        // Lấy refresh token từ cookie (ưu tiên) hoặc body
        var refreshToken = Request.Cookies["RefreshToken"]
                        ?? Request.Headers["X-Refresh-Token"].ToString();

        if (string.IsNullOrWhiteSpace(refreshToken))
            return BadRequest(new { status = 400, message = "Refresh token is required." });

        var result = await _authService.RefreshTokenAsync(refreshToken);

        if (result == null)
            return Unauthorized(new { status = 401, message = "Invalid or expired refresh token. Please login again." });

        // Cấp lại cả 2 cookie (access + refresh mới, do đã rotate)
        Response.Cookies.Append("AccessToken", result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(24),
            Path = "/"
        });

        Response.Cookies.Append("RefreshToken", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/api/auth/refresh"
        });

        return Ok(new
        {
            status = 200,
            message = "Token refreshed",
            accessToken = result.AccessToken,
            avatarUrl = result.AvatarUrl,
            isVerified = result.IsVerified
        });
    }

    // ─── LOGOUT ───────────────────────────────────────────────────────
    [AllowAnonymous]
    [CustomAuthorize(requireAuth: true)]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(accountIdClaim, out var accountId))
            await _authService.LogoutAsync(accountId); // Thu hồi refresh token khỏi DB

        // Xoá cả 2 cookie
        Response.Cookies.Delete("AccessToken", new CookieOptions { Path = "/" });
        Response.Cookies.Delete("RefreshToken", new CookieOptions { Path = "/api/auth/refresh" });

        return Ok(new { status = 200, message = "Logged out successfully." });
    }

    [HttpPost("google-login")]
    [AllowAnonymous]
    [CustomAuthorize(requireAuth: false)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var result = await _authService.GoogleLoginAsync(request.IdToken);

        if (result == null)
            return Unauthorized(new { message = "Google login failed" });

        return Ok(new
        {
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            role = result.Role,
            fullName = result.FullName,
            avatarUrl = result.AvatarUrl,
            isVerified = result.IsVerified
        });
    }

    [HttpPost("send-otp")]
    [AllowAnonymous]
    [CustomAuthorize(requireAuth: false)]
    public async Task<IActionResult> SendOtp([FromQuery] string email)
    {
        var result = await _authService.SendOtpAsync(email);

        if (!result) return BadRequest("Email not found");

        return Ok("OTP sent");
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [CustomAuthorize(requireAuth: false)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyOtpRequest request)
    {
        var result = await _authService.VerifyEmailAsync(request.Email, request.Otp);

        if (!result)
            return BadRequest("Invalid or expired OTP");

        return Ok("Email verified successfully");
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [CustomAuthorize(requireAuth: false)]
    public async Task<IActionResult> ForgotPassword([FromQuery] string email)
    {
        var result = await _authService.ForgotPasswordAsync(email);

        if (result != "OTP sent")
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [CustomAuthorize(requireAuth: false)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(
            request.Email,
            request.Otp,
            request.NewPassword
        );

        if (!result)
            return BadRequest("Invalid OTP or email");

        return Ok("Password reset successfully");
    }

    [HttpPost("verify-otp")]
    [AllowAnonymous]
    [CustomAuthorize(requireAuth: false)]
    public IActionResult VerifyOtpForReset([FromBody] VerifyOtpRequest request)
    {
        var isValid = _authService.VerifyOtpForReset(request.Email, request.Otp);

        if (!isValid)
            return BadRequest("Invalid or expired OTP");

        return Ok("OTP valid");
    }
}
