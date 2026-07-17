using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

/// <summary>Instructor quáº£n lÃ½ bá»™ quiz Ä‘á»™c láº­p cá»§a mÃ¬nh.</summary>
[ApiController]
[Route("api/quizzes")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    private int GetInstructorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("InstructorId");
        if (claim == null || !int.TryParse(claim.Value, out var id))
            throw new UnauthorizedAccessException("Instructor ID not found in token.");
        return id;
    }

    // â”€â”€ GET api/quizzes â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [HttpGet]
    [Authorize(Roles = "instructor")]
    public async Task<IActionResult> GetMyQuizzes()
    {
        try
        {
            var instructorId = GetInstructorId();
            var result = await _quizService.GetMyQuizzesAsync(instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (Exception ex) { return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message)); }
    }

    // â”€â”€ GET api/quizzes/{id} â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [HttpGet("{id:int}")]
    [Authorize(Roles = "instructor,admin,staff")]
    public async Task<IActionResult> GetQuizDetail(int id)
    {
        try
        {
            var instructorId = GetInstructorId();
            var result = await _quizService.GetQuizDetailAsync(id, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (Exception ex) { return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message)); }
    }

    [HttpGet("{quizId}/attempts")]
    [Authorize(Roles = "instructor")]
    public async Task<IActionResult> GetStudentAttempts(int quizId, [FromQuery] CourseMarketplaceBE.Application.DTOs.Common.PagedRequestDto request)
    {
        try
        {
            var instructorIdStr = (User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId"))?.Value;
            if (!int.TryParse(instructorIdStr, out int instructorId))
                return Unauthorized(new { message = "Invalid token payload." });

            var result = await _quizService.GetStudentQuizAttemptsAsync(quizId, instructorId, request);
            return Ok(new { success = true, data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("{id:int}/question-pool")]
    [Authorize(Roles = "instructor,admin,staff")]
    public async Task<IActionResult> GetQuizQuestionPool(int id)
    {
        try
        {
            var instructorId = GetInstructorId();
            var result = await _quizService.GetQuizQuestionPoolAsync(id, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (Exception ex) { return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message)); }
    }


    // â”€â”€ POST api/quizzes â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [HttpPost]
    [Authorize(Roles = "instructor")]
    public async Task<IActionResult> CreateQuiz([FromBody] QuizCreateRequest request)
    {
        try
        {
            Console.WriteLine("DEBUG - Received CreateQuiz request. CourseId: " + request.CourseId + " Title: " + request.Title);
            var instructorId = GetInstructorId();
            var result = await _quizService.CreateQuizAsync(request, instructorId);
            return StatusCode(201, ApiResponse<object>.SuccessResponse(result, "Quiz created successfully."));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (Exception ex) { return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message)); }
    }

    // â”€â”€ PATCH api/quizzes/{id} â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "instructor")]
    public async Task<IActionResult> UpdateQuizSettings(int id, [FromBody] QuizUpdateRequest request)
    {
        try
        {
            var instructorId = GetInstructorId();
            var result = await _quizService.UpdateQuizSettingsAsync(id, request, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Quiz updated successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (Exception ex) { return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message)); }
    }

    // â”€â”€ PATCH api/quizzes/{id}/hide â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [HttpPatch("{id:int}/hide")]
    [Authorize(Roles = "instructor")]
    public async Task<IActionResult> SetHidden(int id, [FromQuery] bool hide)
    {
        try
        {
            var instructorId = GetInstructorId();
            await _quizService.SetQuizHiddenAsync(id, hide, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(null, hide ? "Quiz hidden." : "Quiz visible."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (Exception ex) { return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message)); }
    }

    // â”€â”€ DELETE api/quizzes/{id} â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "instructor")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        try
        {
            var instructorId = GetInstructorId();
            await _quizService.SoftDeleteQuizAsync(id, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Quiz deleted successfully."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (Exception ex) { return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message)); }
    }

}
