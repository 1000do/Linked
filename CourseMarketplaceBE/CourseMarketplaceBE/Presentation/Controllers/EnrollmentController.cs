using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    private int? GetUserId()
    {
        var str = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(str, out int id) ? id : null;
    }

    [HttpPost("free-enroll/{courseId}")]
    public async Task<IActionResult> FreeEnroll(int courseId)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.ErrorResponse("Phiên đăng nhập không hợp lệ."));

        try
        {
            await _enrollmentService.EnrollFreeAsync(userId.Value, courseId);
            return Ok(ApiResponse<string>.SuccessResponse("Ghi danh thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi server: {ex.Message}"));
        }
    }

    [HttpGet("progress/{courseId}")]
    public async Task<IActionResult> GetProgress(int courseId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var progress = await _enrollmentService.GetProgressAsync(userId.Value, courseId);
        if (progress == null) return NotFound(ApiResponse<object>.ErrorResponse("Enrollment not found."));

        return Ok(ApiResponse<object>.SuccessResponse(progress));
    }

    [HttpPost("progress")]
    public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            await _enrollmentService.UpdateProgressAsync(userId.Value, request.CourseId, request.MaterialId);
            return Ok(ApiResponse<string>.SuccessResponse("Progress updated."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }
}
