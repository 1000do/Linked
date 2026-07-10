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

    // ── GET danh sách reviews ──────────────────────────────────────────

    /// <summary>Lấy danh sách reviews của khóa học (public). Hỗ trợ starFilter server-side.</summary>
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

    /// <summary>Lấy danh sách reviews của lesson (public)</summary>
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

    // ── GET thống kê sao ──────────────────────────────────────────────

    /// <summary>Thống kê phân bổ sao của course (public)</summary>
    [HttpGet("stats/{courseId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviewStats(int courseId)
    {
        var stats = await _reviewService.GetReviewStatsAsync(courseId);
        return Ok(ApiResponse<object>.SuccessResponse(stats));
    }

    /// <summary>Thống kê phân bổ sao của lesson (public)</summary>
    [HttpGet("lesson-stats/{lessonId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLessonReviewStats(int lessonId)
    {
        var stats = await _reviewService.GetLessonReviewStatsAsync(lessonId);
        return Ok(ApiResponse<object>.SuccessResponse(stats));
    }

    /// <summary>Trả về avg rating cho tất cả lesson của 1 course — dùng để render sidebar Learn.</summary>
    [HttpGet("lesson-ratings/{courseId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLessonRatings(int courseId)
    {
        var data = await _reviewService.GetLessonRatingsForCourseAsync(courseId);
        return Ok(ApiResponse<object>.SuccessResponse(data));
    }

    // ── GET eligibility ───────────────────────────────────────────────

    /// <summary>Kiểm tra trạng thái enrollment + quyền review của user</summary>
    [HttpGet("eligibility/{courseId}")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> GetReviewEligibility(int courseId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var status = await _reviewService.GetEnrollmentStatusAsync(userId.Value, courseId);
        return Ok(ApiResponse<object>.SuccessResponse(status));
    }

    // ── POST submit ───────────────────────────────────────────────────

    /// <summary>
    /// Gửi review mới — luôn tạo record mới (nhiều comment được phép).
    /// source=detail → yêu cầu hoàn thành khóa học
    /// source=learn  → yêu cầu đã học ít nhất 1 bài
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
            return Ok(ApiResponse<string>.SuccessResponse("Review submitted successfully!"));
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

    // ── PUT edit ──────────────────────────────────────────────────────

    /// <summary>Chỉnh sửa review (chỉ chủ review được phép)</summary>
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
            return Ok(ApiResponse<string>.SuccessResponse("Review updated successfully."));
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

    // ── DELETE soft-delete ────────────────────────────────────────────

    /// <summary>Xóa mềm review (chỉ chủ review được phép). type = course | lesson</summary>
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

    // ── POST report ───────────────────────────────────────────────────

    /// <summary>Báo cáo review vi phạm</summary>
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
