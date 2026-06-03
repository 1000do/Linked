using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

/// <summary>
/// Endpoints dành cho User và Instructor:
/// - Tạo report (khóa học, course review, lesson review)
/// - Xem lịch sử report của mình
/// - Instructor: xem reports nhận được trên khóa học của mình
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    private int? GetUserId()
    {
        var str = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(str, out int id) ? id : null;
    }

    private bool IsInstructor()
        => User.IsInRole("instructor") || User.IsInRole("admin") || User.IsInRole("staff");

    // ── Tạo report khóa học ─────────────────────────────────────────────────

    /// <summary>
    /// Tạo báo cáo cho một khóa học.
    /// User: cần đã enrolled. Instructor: không cần enrolled (không thể report course của mình).
    /// </summary>
    [HttpPost("courses")]
    public async Task<IActionResult> ReportCourse([FromBody] CreateCourseReportRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            bool isInstructor = IsInstructor();
            await _reportService.CreateCourseReportAsync(userId.Value, request, isInstructor);
            return Ok(ApiResponse<string>.SuccessResponse("Your report has been submitted and will be reviewed shortly."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to report course"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    // ── Tạo report review khóa học ──────────────────────────────────────────

    /// <summary>Tạo báo cáo cho một đánh giá khóa học.</summary>
    [HttpPost("course-reviews")]
    public async Task<IActionResult> ReportCourseReview([FromBody] CreateCourseReviewReportRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            await _reportService.CreateCourseReviewReportAsync(userId.Value, request);
            return Ok(ApiResponse<string>.SuccessResponse("Your course review report has been submitted."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to report course review"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    // ── Tạo report review bài học ────────────────────────────────────────────

    /// <summary>Tạo báo cáo cho một đánh giá bài học.</summary>
    [HttpPost("lesson-reviews")]
    public async Task<IActionResult> ReportLessonReview([FromBody] CreateLessonReviewReportRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            await _reportService.CreateLessonReviewReportAsync(userId.Value, request);
            return Ok(ApiResponse<string>.SuccessResponse("Your lesson review report has been submitted."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to report lesson review"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    // ── Xem lịch sử report của bản thân ────────────────────────────────────

    /// <summary>Lấy tất cả report khóa học mà user đã tạo.</summary>
    [HttpGet("my/courses")]
    public async Task<IActionResult> GetMyCourseReports()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var reports = await _reportService.GetMyCourseReportsAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(reports));
    }

    /// <summary>Lấy tất cả report review (course + lesson) mà user đã tạo.</summary>
    [HttpGet("my/reviews")]
    public async Task<IActionResult> GetMyReviewReports()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var reports = await _reportService.GetMyReviewReportsAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(reports));
    }

    // ── Instructor: Xem reports nhận được trên khóa học của mình ───────────

    /// <summary>
    /// Instructor xem danh sách reports nhắm vào một khóa học cụ thể của mình.
    /// </summary>
    [HttpGet("instructor/courses/{courseId}")]
    [Authorize(Roles = "instructor,admin")]
    public async Task<IActionResult> GetReportsOnMyCourse(int courseId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var reports = await _reportService.GetReportsOnMyCourseAsync(userId.Value, courseId);
            return Ok(ApiResponse<object>.SuccessResponse(reports));
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
}
