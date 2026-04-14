using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _userService.RegisterAsync(request);
        if (result == "Success")
            return Ok(new { status = 200, message = "Đăng ký thành công!" });

        return BadRequest(new { status = 400, message = result });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 1. Chỉ chấp nhận Gmail
        if (string.IsNullOrEmpty(request.Email) || !request.Email.EndsWith("@gmail.com"))
        {
            return BadRequest(new { status = 400, message = "Tài khoản phải là định dạng @gmail.com." });
        }

        // 2. Thực hiện đăng nhập
        var token = await _userService.LoginAsync(request);
        if (token == null)
        {
            return Unauthorized(new { status = 401, message = "Email hoặc mật khẩu không chính xác." });
        }

        // 3. Lấy FullName để FE hiển thị
        var fullName = await _userService.GetFullNameByEmailAsync(request.Email);

        // 4. Lưu JWT vào HttpOnly Cookie (Server-side)
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Để false nếu dùng http localhost
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/"
        };
        Response.Cookies.Append("AuthToken", token, cookieOptions);

        return Ok(new
        {
            status = 200,
            message = "Đăng nhập thành công",
            fullName = fullName,
            email = request.Email.ToLower()
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AuthToken", new CookieOptions { Path = "/" });
        return Ok(new { status = 200, message = "Đã đăng xuất thành công." });
    }

    [HttpGet("check-auth")]
    [Authorize]
    public IActionResult CheckAuth()
    {
        return Ok(new
        {
            isAuthenticated = true,
            email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            fullName = User.FindFirst("FullName")?.Value
        });
    }
}