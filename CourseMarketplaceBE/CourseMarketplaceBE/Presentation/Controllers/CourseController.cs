using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

[ApiController]
[Route("api/courses")]
[Authorize] // Requires authentication
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    private int GetInstructorId()
    {
        // Assuming InstructorId is stored in NameIdentifier or a custom claim "InstructorId"
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("InstructorId");
        if (claim == null || !int.TryParse(claim.Value, out var instructorId))
        {
            throw new UnauthorizedAccessException("Instructor ID not found in token.");
        }
        return instructorId;
    }

    [HttpGet("my-courses")]
    public async Task<IActionResult> GetMyCourses()
    {
        try
        {
            var instructorId = GetInstructorId();
            var courses = await _courseService.GetInstructorCoursesAsync(instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(courses, "Retrieved courses successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourse(int id)
    {
        try
        {
            var instructorId = GetInstructorId();
            var course = await _courseService.GetCourseWithDetailsAsync(id, instructorId);
            if (course == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Course not found."));
            }
            return Ok(ApiResponse<object>.SuccessResponse(course, "Retrieved course successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody] CourseCreateRequest request)
    {
        try
        {
            var instructorId = GetInstructorId();
            var result = await _courseService.CreateCourseAsync(request, instructorId);
            return StatusCode(201, ApiResponse<object>.SuccessResponse(result, "Course created successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseUpdateRequest request)
    {
        try
        {
            var instructorId = GetInstructorId();
            var result = await _courseService.UpdateCourseAsync(id, request, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Course updated successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateCourseStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var instructorId = GetInstructorId();
            await _courseService.UpdateCourseStatusAsync(id, request.Status, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Course status updated successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        try
        {
            var instructorId = GetInstructorId();
            await _courseService.DeleteCourseAsync(id, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Course deleted successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
