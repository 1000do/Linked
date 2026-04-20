using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

[ApiController]
[Route("api/lessons")]
[Authorize]
public class LessonController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    private int GetInstructorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("InstructorId");
        if (claim == null || !int.TryParse(claim.Value, out var instructorId))
        {
            throw new UnauthorizedAccessException("Instructor ID not found in token.");
        }
        return instructorId;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLesson([FromBody] LessonCreateRequest request)
    {
        try
        {
            var instructorId = GetInstructorId();
            var result = await _lessonService.CreateLessonAsync(request, instructorId);
            return StatusCode(201, ApiResponse<object>.SuccessResponse(result, "Lesson created successfully."));
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

    [HttpPost("{lessonId}/materials")]
    public async Task<IActionResult> AddMaterial(int lessonId, [FromBody] MaterialCreateRequest request)
    {
        try
        {
            var instructorId = GetInstructorId();
            var result = await _lessonService.AddMaterialToLessonAsync(lessonId, request, instructorId);
            return StatusCode(201, ApiResponse<object>.SuccessResponse(result, "Material added successfully."));
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        try
        {
            var instructorId = GetInstructorId();
            await _lessonService.DeleteLessonAsync(id, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Lesson deleted successfully."));
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
}
