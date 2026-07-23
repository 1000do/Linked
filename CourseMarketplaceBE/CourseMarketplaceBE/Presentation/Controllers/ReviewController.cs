using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    private int? GetUserId()
    {
        var str = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(str, out int id) ? id : null;
    }

    // â”€â”€ GET danh sÃ¡ch reviews â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Láº¥y danh sÃ¡ch reviews cá»§a khÃ³a há»c (public). Há»— trá»£ starFilter server-side.</summary>
    [HttpGet("course/{courseId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourseReviews(
        int courseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? starFilter = null)
    {
        var result = await _reviewService.GetCourseReviewsAsync(courseId, page, pageSize, starFilter);
        return Ok(ApiResponse<object>.SuccessResponse(result, "Retrieved course reviews successfully."));
    }

    /// <summary>Láº¥y danh sÃ¡ch reviews cá»§a lesson (public)</summary>
    [HttpGet("lesson/{lessonId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLessonReviews(
        int lessonId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetLessonReviewsAsync(lessonId, page, pageSize);
        return Ok(ApiResponse<object>.SuccessResponse(result, "Retrieved lesson reviews successfully."));
    }

    // â”€â”€ GET thá»‘ng kÃª sao â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Thá»‘ng kÃª phÃ¢n bá»• sao cá»§a course (public)</summary>
    [HttpGet("stats/{courseId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviewStats(int courseId)
    {
        var stats = await _reviewService.GetReviewStatsAsync(courseId);
        return Ok(ApiResponse<object>.SuccessResponse(stats));
    }

    /// <summary>Thá»‘ng kÃª phÃ¢n bá»• sao cá»§a lesson (public)</summary>
    [HttpGet("lesson-stats/{lessonId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLessonReviewStats(int lessonId)
    {
        var stats = await _reviewService.GetLessonReviewStatsAsync(lessonId);
        return Ok(ApiResponse<object>.SuccessResponse(stats));
    }

    /// <summary>Tráº£ vá» avg rating cho táº¥t cáº£ lesson cá»§a 1 course â€” dÃ¹ng Ä‘á»ƒ render sidebar Learn.</summary>
    [HttpGet("lesson-ratings/{courseId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLessonRatings(int courseId)
    {
        var data = await _reviewService.GetLessonRatingsForCourseAsync(courseId);
        return Ok(ApiResponse<object>.SuccessResponse(data));
    }

    // â”€â”€ GET eligibility â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Kiá»ƒm tra tráº¡ng thÃ¡i enrollment + quyá»n review cá»§a user</summary>
    [HttpGet("eligibility/{courseId}")]
    [Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> GetReviewEligibility(int courseId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var status = await _reviewService.GetEnrollmentStatusAsync(userId.Value, courseId);
        return Ok(ApiResponse<object>.SuccessResponse(status));
    }

    // â”€â”€ POST submit â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// Gá»­i review má»›i â€” luÃ´n táº¡o record má»›i (nhiá»u comment Ä‘Æ°á»£c phÃ©p).
    /// source=detail â†’ yÃªu cáº§u hoÃ n thÃ nh khÃ³a há»c
    /// source=learn  â†’ yÃªu cáº§u Ä‘Ã£ há»c Ã­t nháº¥t 1 bÃ i
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> SubmitReview(
        [FromBody] ReviewRequest request,
        [FromQuery] string source = "learn")
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            bool requireCompletion = string.Equals(source, "detail", StringComparison.OrdinalIgnoreCase);
            await _reviewService.SubmitReviewAsync(userId.Value, request, requireCompletion);
            return Ok(ApiResponse<string>.SuccessResponse("Review has been sent for auditing and will appear when approved."));
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to submit review"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    // â”€â”€ PUT edit â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Chá»‰nh sá»­a review (chá»‰ chá»§ review Ä‘Æ°á»£c phÃ©p)</summary>
    [HttpPut("{reviewId}")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        request.ReviewId = reviewId;

        try
        {
            await _reviewService.UpdateReviewAsync(userId.Value, request);
            return Ok(ApiResponse<string>.SuccessResponse("Review has been sent for auditing and will appear when approved."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    // â”€â”€ DELETE soft-delete â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>XÃ³a má»m review (chá»‰ chá»§ review Ä‘Æ°á»£c phÃ©p). type = course | lesson</summary>
    [HttpDelete("{reviewId}")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> DeleteReview(int reviewId, [FromQuery] string type = "course")
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            await _reviewService.DeleteReviewAsync(userId.Value, new DeleteReviewRequest
            {
                ReviewId = reviewId,
                Type = type
            });
            return Ok(ApiResponse<string>.SuccessResponse("Review deleted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    // â”€â”€ POST report â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>BÃ¡o cÃ¡o review vi pháº¡m</summary>
    [HttpPost("report")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> Report([FromBody] ReportRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            await _reviewService.ReportReviewAsync(userId.Value, request.ReviewId, request.Type, request.Reason);
            return Ok(ApiResponse<string>.SuccessResponse("Report submitted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }
}
