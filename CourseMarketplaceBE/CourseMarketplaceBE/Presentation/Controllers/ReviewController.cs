using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
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

    /// <summary>Lấy danh sách reviews của khóa học (public)</summary>
    [HttpGet("course/{courseId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourseReviews(int courseId)
    {
        var reviews = await _reviewService.GetCourseReviewsAsync(courseId);
        return Ok(ApiResponse<object>.SuccessResponse(reviews, "Retrieved reviews successfully."));
    }

    /// <summary>Thống kê phân bổ sao (public)</summary>
    [HttpGet("stats/{courseId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviewStats(int courseId)
    {
        var stats = await _reviewService.GetReviewStatsAsync(courseId);
        return Ok(ApiResponse<object>.SuccessResponse(stats));
    }

    /// <summary>Kiểm tra trạng thái enrollment + quyền review của user</summary>
    [HttpGet("eligibility/{courseId}")]
    [Authorize]
    public async Task<IActionResult> GetReviewEligibility(int courseId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var status = await _reviewService.GetEnrollmentStatusAsync(userId.Value, courseId);
        return Ok(ApiResponse<object>.SuccessResponse(status));
    }

    /// <summary>
    /// Gửi review — chia thành 2 context:
    /// - source=detail → yêu cầu hoàn thành khóa học
    /// - source=learn  → yêu cầu đã học ít nhất 1 bài
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SubmitReview([FromBody] ReviewRequest request, [FromQuery] string source = "learn")
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            bool requireCompletion = string.Equals(source, "detail", StringComparison.OrdinalIgnoreCase);
            await _reviewService.SubmitReviewAsync(userId.Value, request, requireCompletion);
            return Ok(ApiResponse<string>.SuccessResponse("Đánh giá đã được gửi thành công!"));
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
