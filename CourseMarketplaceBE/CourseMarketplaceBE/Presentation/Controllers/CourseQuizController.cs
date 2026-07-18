using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

/// <summary>Instructor gÃ¡n vÃ  quáº£n lÃ½ quiz trong má»™t course cá»¥ thá»ƒ.</summary>
[ApiController]
[Route("api/courses/{courseId:int}/quizzes")]
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

    // â”€â”€ GET api/courses/{courseId}/quizzes â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [HttpGet]
    [Authorize(Roles = "instructor,admin,staff")]
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

    // â”€â”€ POST api/courses/{courseId}/quizzes â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [HttpPost]
    [Authorize(Roles = "instructor")]
    public async Task<IActionResult> AddQuizToCourse(int courseId, [FromBody] AddQuizToCourseRequest request)
    {
        try
        {
            Console.WriteLine($"[DEBUG] AddQuizToCourse - Route courseId: {courseId}, Request CourseId: {request.CourseId}, QuizId: {request.QuizId}");
            request.CourseId = courseId; // Override tá»« route
            var instructorId = GetInstructorId();
            var result = await _quizService.AddQuizToCourseAsync(request, instructorId);
            return StatusCode(201, ApiResponse<object>.SuccessResponse(result, "Quiz added to course."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message)); }
        catch (Exception ex) { return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message)); }
    }

    // â”€â”€ DELETE api/courses/{courseId}/quizzes/{quizId} â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [HttpDelete("{quizId:int}")]
    [Authorize(Roles = "instructor")]
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

    // â”€â”€ PATCH api/courses/{courseId}/quizzes/{quizId}/hide â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [HttpPatch("{quizId:int}/hide")]
    [Authorize(Roles = "instructor")]
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
