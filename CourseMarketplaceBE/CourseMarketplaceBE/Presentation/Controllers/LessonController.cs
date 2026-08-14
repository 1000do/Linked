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
public class LessonController : ControllerBase
{
    private readonly ILessonService _lessonService;
    private readonly IMaterialStreamService _streamService;

    public LessonController(ILessonService lessonService, IMaterialStreamService streamService)
    {
        _lessonService = lessonService;
        _streamService = streamService;
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

    [HttpGet("check-material-hash")]
    [Authorize(Roles = "instructor")]
    public async Task<IActionResult> CheckMaterialHash([FromQuery] string hash)
    {
        try
        {
            var isDuplicate = await _lessonService.CheckMaterialDuplicateAsync(hash);
            return Ok(new CourseMarketplaceBE.Application.DTOs.ApiResponse<bool> { Success = true, Data = isDuplicate });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CourseMarketplaceBE.Application.DTOs.ApiResponse<bool> { Success = false, Message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "instructor")]
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
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{lessonId}/materials")]
    [Authorize(Roles = "instructor")]
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
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPatch("{lessonId}/title")]
    [Authorize(Roles = "instructor")]
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
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPatch("materials/{materialId}")]
    [Authorize(Roles = "instructor")]
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
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPatch("materials/{materialId}/remove")]
    [Authorize(Roles = "instructor")]
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
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "instructor")]
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
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("materials/trash")]
    [Authorize(Roles = "instructor")]
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
    [Authorize(Roles = "instructor")]
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
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("materials/{materialId}/restore")]
    [Authorize(Roles = "instructor")]
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
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("materials/{materialId}/stream")]
    [AllowAnonymous]
    public async Task<IActionResult> StreamMaterial(int materialId)
    {
        try
        {
            int userId = 0;
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdStr))
            {
                int.TryParse(userIdStr, out userId);
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var rangeHeader = Request.Headers["Range"].ToString();

            var result = await _streamService.GetMaterialStreamAsync(materialId, userId, userRole, rangeHeader);

            if (!string.IsNullOrEmpty(result.ContentRangeHeader))
            {
                Response.Headers["Accept-Ranges"] = "bytes";
                Response.Headers["Content-Range"] = result.ContentRangeHeader;
            }

            if (result.ContentLength.HasValue)
            {
                Response.ContentLength = result.ContentLength.Value;
            }

            if (!string.IsNullOrEmpty(result.FileName))
            {
                var contentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") 
                { 
                    FileName = "\"" + result.FileName + "\"",
                    FileNameStar = result.FileName
                };
                Response.Headers["Content-Disposition"] = contentDisposition.ToString();
            }

            Response.StatusCode = result.StatusCode;
            return File(result.Stream, result.ContentType);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}
