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
    public async Task<IActionResult> CreateLesson([FromForm] LessonCreateRequest request)
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to create lesson"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{lessonId}/materials")]
    public async Task<IActionResult> AddMaterial(int lessonId, [FromForm] MaterialCreateRequest request)
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to add material"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPatch("{lessonId}/title")]
    public async Task<IActionResult> UpdateLessonTitle(int lessonId, [FromBody] LessonUpdateTitleRequest request)
    {
        try
        {
            var instructorId = GetInstructorId();
            var result = await _lessonService.UpdateLessonTitleAsync(lessonId, request, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Lesson title updated successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to update lesson title"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPatch("materials/{materialId}")]
    public async Task<IActionResult> UpdateMaterialDetails(int materialId, [FromBody] MaterialUpdateRequest request)
    {
        try
        {
            var instructorId = GetInstructorId();
            var result = await _lessonService.UpdateMaterialDetailsAsync(materialId, request, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Material details updated successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to update material details"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPatch("materials/{materialId}/remove")]
    public async Task<IActionResult> RemoveMaterial(int materialId)
    {
        try
        {
            var instructorId = GetInstructorId();
            await _lessonService.RemoveMaterialAsync(materialId, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Material removed successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to remove material"));
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to delete lesson"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("materials/trash")]
    public async Task<IActionResult> GetTrashMaterials()
    {
        try
        {
            var instructorId = GetInstructorId();
            var result = await _lessonService.GetTrashMaterialsAsync(instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Trash materials retrieved successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("materials/{materialId}/permanent")]
    public async Task<IActionResult> PermanentDeleteMaterial(int materialId)
    {
        try
        {
            var instructorId = GetInstructorId();
            await _lessonService.PermanentDeleteMaterialAsync(materialId, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Material permanently deleted."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to permanently delete material"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("materials/{materialId}/restore")]
    public async Task<IActionResult> RestoreMaterial(int materialId)
    {
        try
        {
            var instructorId = GetInstructorId();
            await _lessonService.RestoreMaterialAsync(materialId, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Material restored successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to restore material"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}
