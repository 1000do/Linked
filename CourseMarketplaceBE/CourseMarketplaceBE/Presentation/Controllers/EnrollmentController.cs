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
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IAuthService _authService;

    public EnrollmentController(IEnrollmentService enrollmentService, IAuthService authService)
    {
        _enrollmentService = enrollmentService;
        _authService = authService;
    }

    private int? GetUserId()
    {
        var str = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(str, out int id) ? id : null;
    }

    [HttpPost("free-enroll/{courseId}")]
    [Authorize(Roles = "user,instructor")]
    public async Task<IActionResult> FreeEnroll(int courseId)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.ErrorResponse("Invalid login session."));

        if (!await _authService.IsEmailVerifiedAsync(userId.Value))
            return BadRequest(ApiResponse<string>.ErrorResponse("Please verify your email address."));

        try
        {
            await _enrollmentService.EnrollFreeAsync(userId.Value, courseId);
            return Ok(ApiResponse<string>.SuccessResponse("Enrolled successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to enroll in free course"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Server error: {ex.Message}"));
        }
    }

    [HttpGet("progress/{courseId}")]
   [Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> GetProgress(int courseId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var progress = await _enrollmentService.GetProgressAsync(userId.Value, courseId);
        if (progress == null) return NotFound(ApiResponse<object>.ErrorResponse("Enrollment not found."));

        return Ok(ApiResponse<object>.SuccessResponse(progress));
    }

    [HttpPost("progress")]
[Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            await _enrollmentService.UpdateProgressAsync(userId.Value, request.CourseId, request.MaterialId);
            return Ok(ApiResponse<string>.SuccessResponse("Progress updated."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to update progress"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("my-courses")]
   [Authorize(Roles = "user,instructor")]
    public async Task<IActionResult> GetMyCourses()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.ErrorResponse("Invalid login session."));

        try
        {
            var courses = await _enrollmentService.GetMyEnrolledCoursesAsync(userId.Value);
            return Ok(ApiResponse<object>.SuccessResponse(courses, "Enrolled courses retrieved successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }
}
