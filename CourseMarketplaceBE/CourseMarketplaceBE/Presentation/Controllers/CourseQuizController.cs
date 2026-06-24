using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

/// <summary>Instructor gán và quản lý quiz trong một course cụ thể.</summary>
[ApiController]
[Route("api/courses/{courseId:int}/quizzes")]
[Authorize]
public class CourseQuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public CourseQuizController(IQuizService quizService)
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

    // ── GET api/courses/{courseId}/quizzes ────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetCourseQuizzes(int courseId)
    {
        try
        {
            var instructorId = GetInstructorId();
            var result = await _quizService.GetCourseQuizzesAsync(courseId, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (Exception ex) { return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message)); }
    }

    // ── POST api/courses/{courseId}/quizzes ───────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> AddQuizToCourse(int courseId, [FromBody] AddQuizToCourseRequest request)
    {
        try
        {
            Console.WriteLine($"[DEBUG] AddQuizToCourse - Route courseId: {courseId}, Request CourseId: {request.CourseId}, QuizId: {request.QuizId}");
            request.CourseId = courseId; // Override từ route
            var instructorId = GetInstructorId();
            var result = await _quizService.AddQuizToCourseAsync(request, instructorId);
            return StatusCode(201, ApiResponse<object>.SuccessResponse(result, "Quiz added to course."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (Exception ex) { return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message)); }
    }

    // ── DELETE api/courses/{courseId}/quizzes/{quizId} ────────────────────────

    [HttpDelete("{quizId:int}")]
    public async Task<IActionResult> RemoveQuizFromCourse(int courseId, int quizId)
    {
        try
        {
            var instructorId = GetInstructorId();
            await _quizService.RemoveQuizFromCourseAsync(courseId, quizId, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Quiz removed from course."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (Exception ex) { return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message)); }
    }

    // ── PATCH api/courses/{courseId}/quizzes/{quizId}/hide ────────────────────

    [HttpPatch("{quizId:int}/hide")]
    public async Task<IActionResult> SetCourseQuizHidden(int courseId, int quizId, [FromQuery] bool hide)
    {
        try
        {
            var instructorId = GetInstructorId();
            await _quizService.SetCourseQuizHiddenAsync(courseId, quizId, hide, instructorId);
            return Ok(ApiResponse<object>.SuccessResponse(null, hide ? "Quiz hidden in course." : "Quiz visible in course."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (Exception ex) { return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message)); }
    }
}
