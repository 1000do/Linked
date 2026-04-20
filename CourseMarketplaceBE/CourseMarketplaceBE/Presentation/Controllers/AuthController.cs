using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
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

    // ─── REGISTER ─────────────────────────────────────────────────────
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result == "Success")
            return Ok(new { status = 200, message = "Đăng ký tài khoản thành công!" });

        return BadRequest(new { status = 400, message = result });
    }

    // ─── LOGIN ────────────────────────────────────────────────────────
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || !request.Email.EndsWith("@gmail.com"))
            return BadRequest(new { status = 400, message = "Tài khoản phải có định dạng @gmail.com." });

        var result = await _authService.LoginAsync(request);

        if (result == null)
            return Unauthorized(new { status = 401, message = "Email hoặc mật khẩu không chính xác." });

        // Access token: HttpOnly cookie, ngắn hạn (15 phút)
        Response.Cookies.Append("AccessToken", result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,          // HTTPS trên production
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15),
            Path = "/"
        });

        // Refresh token: HttpOnly cookie, dài hạn (7 ngày)
        Response.Cookies.Append("RefreshToken", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/api/auth/refresh" // Chỉ gửi đến endpoint refresh
        });

        return Ok(new
        {
            status = 200,
            message = "Đăng nhập thành công",
            accessToken = result.AccessToken,   // Trả về để FE lưu in-memory
            fullName = result.FullName,
            avatarUrl = result.AvatarUrl
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
            return BadRequest(new { status = 400, message = "Refresh token không được để trống." });

        var result = await _authService.RefreshTokenAsync(refreshToken);

        if (result == null)
            return Unauthorized(new { status = 401, message = "Refresh token không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại." });

        // Cấp lại cả 2 cookie (access + refresh mới, do đã rotate)
        Response.Cookies.Append("AccessToken", result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15),
            Path = "/"
        });

        Response.Cookies.Append("RefreshToken", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/api/auth/refresh"
        });

        return Ok(new
        {
            status = 200,
            message = "Token đã được làm mới",
            accessToken = result.AccessToken
        });
    }

    // ─── LOGOUT ───────────────────────────────────────────────────────
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(accountIdClaim, out var accountId))
            await _authService.LogoutAsync(accountId); // Thu hồi refresh token khỏi DB

        // Xoá cả 2 cookie
        Response.Cookies.Delete("AccessToken", new CookieOptions { Path = "/" });
        Response.Cookies.Delete("RefreshToken", new CookieOptions { Path = "/api/auth/refresh" });

        return Ok(new { status = 200, message = "Đã đăng xuất thành công." });
    }
}