using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourseMarketplaceBE.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserProfileService _profileService;

    public ProfileController(IUserProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized(new { status = 401, message = "Phiên đăng nhập không hợp lệ." });

        var profile = await _profileService.GetUserProfileAsync(userId);

        if (profile == null)
            return NotFound(new { status = 404, message = "Không tìm thấy thông tin người dùng." });

        return Ok(new { status = 200, data = profile });
    }

    [HttpPut("update")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized(new { status = 401, message = "Phiên đăng nhập không hợp lệ." });

        var result = await _profileService.UpdateProfileAsync(userId, request);

        if (result)
            return Ok(new { status = 200, message = "Cập nhật thông tin hồ sơ thành công!" });

        return BadRequest(new { status = 400, message = "Cập nhật thất bại." });
    }
}