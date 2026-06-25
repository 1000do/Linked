using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

/// <summary>Instructor quản lý bộ quiz độc lập của mình.</summary>
[ApiController]
[Route("api/quizzes")]
[Authorize]
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

    // ── GET api/quizzes ────────────────────────────────────────────────────────

    [HttpGet]
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

    // ── GET api/quizzes/{id} ───────────────────────────────────────────────────

    [HttpGet("{id:int}")]
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

    [HttpGet("{id:int}/question-pool")]
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

    // ── POST api/quizzes ───────────────────────────────────────────────────────

    [HttpPost]
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

    // ── PATCH api/quizzes/{id} ─────────────────────────────────────────────────

    [HttpPatch("{id:int}")]
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

    // ── PATCH api/quizzes/{id}/hide ────────────────────────────────────────────

    [HttpPatch("{id:int}/hide")]
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

    // ── DELETE api/quizzes/{id} ────────────────────────────────────────────────

    [HttpDelete("{id:int}")]
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
