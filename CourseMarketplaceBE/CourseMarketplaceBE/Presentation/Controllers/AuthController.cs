using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
//using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result == "Success")
            return Ok(new { status = 200, message = "Đăng ký tài khoản thành công!" });

        return BadRequest(new { status = 400, message = result });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email))
        {
            return BadRequest(new { status = 400, message = "Tài khoản phải có định dạng @gmail.com." });
        }

        // result bây giờ chứa cả Token, FullName và AvatarUrl
        var result = await _authService.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized(new { status = 401, message = "Email hoặc mật khẩu không chính xác." });
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Đổi thành true nếu chạy HTTPS thực tế
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/"
        };

        Response.Cookies.Append("AuthToken", result.Token, cookieOptions);

        return Ok(new
        {
            status = 200,
            message = "Đăng nhập thành công",
            token = result.Token,
            fullName = result.FullName,
            avatarUrl = result.AvatarUrl,
            isVerified = result.IsVerified
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AuthToken", new CookieOptions { Path = "/" });
        return Ok(new { status = 200, message = "Đã đăng xuất thành công." });
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var result = await _authService.GoogleLoginAsync(request.IdToken);

        if (result == null)
            return Unauthorized(new { message = "Google login failed" });

        return Ok(new
        {
            token = result.Token,
            fullName = result.FullName,
            avatarUrl = result.AvatarUrl,
            isVerified = result.IsVerified
        });
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromQuery] string email)
    {
        var result = await _authService.SendOtpAsync(email);

        if (!result) return BadRequest("Email not found");

        return Ok("OTP sent");
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyOtpRequest request)
    {
        var result = await _authService.VerifyEmailAsync(request.Email, request.Otp);

        if (!result)
            return BadRequest("Invalid or expired OTP");

        return Ok("Email verified successfully");
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromQuery] string email)
    {
        var result = await _authService.ForgotPasswordAsync(email);

        if (result != "OTP sent")
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("reset-password")]
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
}