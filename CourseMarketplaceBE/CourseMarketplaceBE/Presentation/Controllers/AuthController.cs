using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
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
        if (string.IsNullOrEmpty(request.Email) || !request.Email.EndsWith("@gmail.com"))
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
            avatarUrl = result.AvatarUrl
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AuthToken", new CookieOptions { Path = "/" });
        return Ok(new { status = 200, message = "Đã đăng xuất thành công." });
    }
}