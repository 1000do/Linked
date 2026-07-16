using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Presentation.Filters;

namespace CourseMarketplaceBE.Presentation.Controllers
{
    [Route("api/admin/profile")]
    [ApiController]
    [Authorize(Roles = "admin,staff")]
    public class AdminProfileController : ControllerBase
    {
        private readonly IManagerProfileService _profileService;

        public AdminProfileController(IManagerProfileService profileService)
        {
            _profileService = profileService;
        }

        // UC-75: View Profile (For Manager) — Staff và Admin
        [HttpGet]
        [CustomAuthorize(requireAuth: true, "staff", "admin")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { status = 401, message = "Invalid login session." });

            var profile = await _profileService.GetProfileAsync(userId);
            if (profile == null)
                return NotFound(new { status = 404, message = "Manager profile not found." });

            return Ok(new { status = 200, data = profile });
        }

        // UC-76: Edit Profile (For Manager) — Staff và Admin
        [HttpPut("update")]
        [Consumes("multipart/form-data")]
        [CustomAuthorize(requireAuth: true, "staff", "admin")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateManagerProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(new { status = 400, message = errors });
            }

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { status = 401, message = "Invalid login session." });

            var success = await _profileService.UpdateProfileAsync(userId, request);
            if (success)
                return Ok(new { status = 200, message = "Profile updated successfully!" });

            return BadRequest(new { status = 400, message = "Failed to update manager profile." });
        }

        // UC-75/76: Change Password (For Manager) — Staff và Admin
        [HttpPost("change-password")]
        [CustomAuthorize(requireAuth: true, "staff", "admin")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { status = 401, message = "Invalid login session." });

            var (success, message) = await _profileService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            if (success)
                return Ok(new { status = 200, message });

            return BadRequest(new { status = 400, message });
        }
    }
}
