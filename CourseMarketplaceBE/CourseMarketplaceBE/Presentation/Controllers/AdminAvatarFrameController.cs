using CourseMarketplaceBE.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers
{
    [Route("api/admin/avatar-frames")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminAvatarFrameController : ControllerBase
    {
        private readonly IAvatarFrameService _frameService;

        public AdminAvatarFrameController(IAvatarFrameService frameService)
        {
            _frameService = frameService;
        }

        [HttpPost("grant")]
        public async Task<IActionResult> GrantFrame([FromBody] GrantFrameRequest request)
        {
            var success = await _frameService.GrantFrameToUserAsync(request.UserId, request.FrameId);
            if (success) return Ok(new { message = "Avatar frame granted successfully!" });
            return BadRequest(new { message = "Cannot grant avatar frame. Please verify UserId or FrameId." });
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllFrames()
        {
            var frames = await _frameService.GetAllFramesAsync();
            return Ok(new { data = frames });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateFrame([FromBody] dynamic frameDto)
        {
            var success = await _frameService.CreateFrameAsync(frameDto);
            if (success) return Ok(new { message = "Avatar frame created successfully!" });
            return BadRequest(new { message = "Error creating avatar frame." });
        }
    }

    public class GrantFrameRequest
    {
        public string UserId { get; set; } = null!;
        public int FrameId { get; set; }
    }
}
