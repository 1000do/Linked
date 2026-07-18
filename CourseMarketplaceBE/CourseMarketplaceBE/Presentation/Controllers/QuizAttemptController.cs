using System;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CourseMarketplaceBE.Application.DTOs.Common;

namespace CourseMarketplaceBE.Presentation.Controllers;

[ApiController]
[Route("api/quiz-attempts")]
public class QuizAttemptController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizAttemptController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpGet("quiz/{quizId}")]
    [Authorize(Roles = "user,admin,staff")]
    public async Task<IActionResult> GetQuizForStudent(int quizId)
    {
        try
        {
            var userIdStr = (User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId"))?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Invalid token payload." });

            var result = await _quizService.GetQuizForStudentAsync(quizId, userId);
            return Ok(new { success = true, data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> SubmitAttempt([FromBody] QuizAttemptSubmitRequest request)
    {
        try
        {
            var userIdStr = (User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId"))?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Invalid token payload." });

            var result = await _quizService.SubmitAttemptAsync(request, userId);
            return Ok(new { success = true, data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("quiz/{quizId}/history")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> GetMyQuizHistory(int quizId, [FromQuery] PagedRequestDto request)
    {
        try
        {
            var userIdStr = (User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId"))?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Invalid token payload." });

            var result = await _quizService.GetMyQuizAttemptsAsync(quizId, userId, request);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("{attemptId}/details")]
    [Authorize(Roles = "user,admin,staff")]
    public async Task<IActionResult> GetAttemptDetail(int attemptId)
    {
        try
        {
            var userIdStr = (User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId"))?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Invalid token payload." });

            var result = await _quizService.GetAttemptDetailAsync(attemptId, userId);
            return Ok(new { success = true, data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
