using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
//[Authorize(Roles = "instructor")]
public class QuestionBankController : ControllerBase
{
    private readonly IQuestionBankService _questionBankService;

    public QuestionBankController(IQuestionBankService questionBankService)
    {
        _questionBankService = questionBankService;
    }

    private int GetCurrentUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(idStr, out int id))
        {
            return id;
        }
        throw new UnauthorizedAccessException("Cannot identify user");
    }

    [HttpPost("courses/{courseId}/questions")]
    public async Task<ActionResult<QuizQuestionResponse>> AddQuestion(int courseId, [FromBody] QuizAddQuestionRequest request)
    {
        try
        {
            var instructorId = GetCurrentUserId();
            var response = await _questionBankService.AddQuestionAsync(courseId, request, instructorId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("questions/{questionId}")]
    public async Task<ActionResult<QuizQuestionResponse>> UpdateQuestion(int questionId, [FromBody] QuizUpdateQuestionRequest request)
    {
        try
        {
            var instructorId = GetCurrentUserId();
            var response = await _questionBankService.UpdateQuestionAsync(questionId, request, instructorId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("questions/{questionId}")]
    public async Task<IActionResult> DeleteQuestion(int questionId)
    {
        try
        {
            var instructorId = GetCurrentUserId();
            await _questionBankService.DeleteQuestionAsync(questionId, instructorId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("courses/{courseId}/questions")]
    public async Task<ActionResult<List<QuizQuestionResponse>>> GetQuestionsByCourse(int courseId)
    {
        try
        {
            var instructorId = GetCurrentUserId();
            var response = await _questionBankService.GetQuestionsByCourseAsync(courseId, instructorId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("courses/{courseId}/lessons-summary")]
    public async Task<ActionResult<List<QuestionBankLessonSummaryResponse>>> GetLessonsSummaryByCourse(int courseId)
    {
        try
        {
            var instructorId = GetCurrentUserId();
            var response = await _questionBankService.GetLessonsSummaryByCourseAsync(courseId, instructorId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("lessons/{lessonId}/questions")]
    public async Task<ActionResult<List<QuizQuestionResponse>>> GetQuestionsByLesson(int lessonId)
    {
        try
        {
            var instructorId = GetCurrentUserId();
            var response = await _questionBankService.GetQuestionsByLessonAsync(lessonId, instructorId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
