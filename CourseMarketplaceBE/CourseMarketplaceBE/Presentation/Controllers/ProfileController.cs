using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CourseMarketplaceBE.Presentation.Filters;

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
    [AllowAnonymous] // Override default Authorize if any
    [CustomAuthorize(requireAuth: true, "user", "instructor")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized(new { status = 401, message = "Invalid login session." });

        var profile = await _profileService.GetUserProfileAsync(userId);

        if (profile == null)
            return NotFound(new { status = 404, message = "User profile not found." });

        return Ok(new { status = 200, data = profile });
    }

    [HttpPut("update")]
    [Consumes("multipart/form-data")]
    [AllowAnonymous] // Override default Authorize if any
    [CustomAuthorize(requireAuth: true, "user", "instructor")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { status = 400, message = string.Join(" ", errors) });
        }

        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized(new { status = 401, message = "Invalid login session." });

        try
        {
            var result = await _profileService.UpdateProfileAsync(userId, request);

            if (result)
                return Ok(new { status = 200, message = "Profile updated successfully!" });

            return BadRequest(new { status = 400, message = "Profile update failed." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { status = 400, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"An unexpected error occurred: {ex.Message}" });
        }
    }

    [HttpPost("change-password")]
    [AllowAnonymous] // Override default Authorize if any
    [CustomAuthorize(requireAuth: true, "user", "instructor")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { status = 400, message = "Invalid data.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized(new { status = 401, message = "Invalid login session." });

        var (success, message) = await _profileService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

        if (success)
        {
            return Ok(new { status = 200, message = message });
        }
        else
        {
            return BadRequest(new { status = 400, message = message });
        }
    }
}
