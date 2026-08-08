using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CourseMarketplaceBE.Presentation.Controllers
{
    [ApiController]
    [Route("api/admin/moderation")]
    [Authorize(Roles = "admin,staff")]
    public class AdminModerationController : ControllerBase
    {
        private readonly ICourseModerationService _courseModerationService;
        private readonly IUserReportModerationService _userReportModerationService;
        private readonly IReportModerationService _reportService;
        private readonly IReviewModerationService _reviewModerationService;
        private readonly IHubService _hubService;
        private readonly IHubContext<AdminModerationHub> _hubContext;

        public AdminModerationController(
            ICourseModerationService courseModerationService,
            IUserReportModerationService userReportModerationService,
            IReportModerationService reportService,
            IReviewModerationService reviewModerationService,
            IHubService hubService,
            IHubContext<AdminModerationHub> hubContext)
        {
            _courseModerationService = courseModerationService;
            _userReportModerationService = userReportModerationService;
            _reportService = reportService;
            _reviewModerationService = reviewModerationService;
            _hubService = hubService;
            _hubContext = hubContext;
        }

        [HttpGet("reports/pending-count")]
        public async Task<IActionResult> GetPendingReportCount()
        {
            var stats = await _reportService.GetReportStatsAsync();
            return Ok(new { count = stats.TotalPending });
        }

        [HttpGet("reviews/pending-count")]
        public async Task<IActionResult> GetPendingReviewCount()
        {
            var stats = await _reviewModerationService.GetModerationStatsAsync();
            var count = stats?.GetType()?.GetProperty("TotalPending")?.GetValue(stats, null) as int? ?? 0;
            return Ok(new { count = count });
        }

        [HttpGet("courses/pending-count")]
        public async Task<IActionResult> GetPendingCourseCount()
        {
            var stats = await _courseModerationService.GetCourseModerationStatsAsync();
            return Ok(new { count = stats?.PendingCount ?? 0 });
        }

        private int? GetUserId()
        {
            var str = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(str, out int id) ? id : null;
        }

        // ── Course Approval Moderation (existing) ──────────────────────────

        [HttpGet("courses/stats")]
        public async Task<IActionResult> GetCourseModerationStats()
        {
            var stats = await _courseModerationService.GetCourseModerationStatsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(stats));
        }

        [HttpGet("courses/pending")]
        public async Task<IActionResult> GetPendingCourses([FromQuery] ModerationFilterDto filter)
        {
            return Ok(await _courseModerationService.GetPendingCoursesAsync(filter));
        }

        [HttpPost("courses/approve/{id}")]
        public async Task<IActionResult> ApproveCourse(int id, [FromBody] string? feedback)
        {
            var result = await _courseModerationService.ApproveCourseAsync(id, feedback);
            if (result)
            {
                await _hubContext.Clients.Group("admin_moderators").SendAsync("ModerationQueueUpdated");
                return Ok();
            }
            return NotFound();
        }

        [HttpPost("courses/reject/{id}")]
        public async Task<IActionResult> RejectCourse(int id, [FromBody] string reason)
        {
            var result = await _courseModerationService.RejectCourseAsync(id, reason);
            if (result)
            {
                await _hubContext.Clients.Group("admin_moderators").SendAsync("ModerationQueueUpdated");
                return Ok();
            }
            return NotFound();
        }

        [HttpPost("courses/reject-detailed")]
        public async Task<IActionResult> RejectCourseDetailed([FromBody] RejectCourseDetailedRequest request)
        {
            var result = await _courseModerationService.RejectCourseDetailedAsync(request);
            if (result)
            {
                await _hubContext.Clients.Group("admin_moderators").SendAsync("ModerationQueueUpdated");
                return Ok();
            }
            return NotFound();
        }

        [HttpPost("courses/flag/{id}")]
        public async Task<IActionResult> FlagCourse(int id, [FromBody] string reason)
        {
            var result = await _courseModerationService.FlagCourseAsync(id, reason);
            if (result)
            {
                await _hubContext.Clients.Group("admin_moderators").SendAsync("ModerationQueueUpdated");
                return Ok();
            }
            return NotFound();
        }

        [HttpPost("courses/unflag/{id}")]
        public async Task<IActionResult> UnflagCourse(int id)
        {
            var result = await _courseModerationService.UnflagCourseAsync(id);
            if (result)
            {
                await _hubContext.Clients.Group("admin_moderators").SendAsync("ModerationQueueUpdated");
                return Ok(ApiResponse<string>.SuccessResponse("Course unflagged successfully."));
            }
            return NotFound(ApiResponse<string>.ErrorResponse("Course not found or could not be unflagged."));
        }

        // ── Legacy report endpoint (kept for backward compatibility) ────────

        [HttpGet("reports")]
        public async Task<IActionResult> GetAllReports([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return Ok(await _userReportModerationService.GetAllReportsAsync(page, pageSize));
        }

        [HttpPost("reports/resolve")]
        public async Task<IActionResult> ResolveReport([FromBody] ResolveReportDto dto)
        {
            var resolverId = GetUserId();
            if (resolverId == null) return Unauthorized();

            try
            {
                var result = await _userReportModerationService.ResolveReportAsync(dto, resolverId.Value);
                return result 
                    ? Ok(ApiResponse<string>.SuccessResponse("Report resolved successfully.")) 
                    : NotFound(ApiResponse<string>.ErrorResponse("Report not found."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        // ── Staff/Admin: Report Management ─────────────────────────────────

        /// <summary>Thống kê tổng quan tất cả reports (pending count, resolved today...)</summary>
        [HttpGet("reports/stats")]
        public async Task<IActionResult> GetReportStats()
        {
            var stats = await _reportService.GetReportStatsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(stats));
        }

        /// <summary>Lấy tất cả course reports (có thể lọc theo status)</summary>
        [HttpGet("reports/courses")]
        public async Task<IActionResult> GetAllCourseReports([FromQuery] PagedReportRequestDto request)
        {
            var reports = await _reportService.GetAllCourseReportsAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(reports));
        }

        /// <summary>Lấy tất cả course review reports (có thể lọc theo status)</summary>
        [HttpGet("reports/course-reviews")]
        public async Task<IActionResult> GetAllCourseReviewReports([FromQuery] PagedReportRequestDto request)
        {
            var reports = await _reportService.GetAllCourseReviewReportsAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(reports));
        }

        /// <summary>Lấy tất cả lesson review reports (có thể lọc theo status)</summary>
        [HttpGet("reports/lesson-reviews")]
        public async Task<IActionResult> GetAllLessonReviewReports([FromQuery] PagedReportRequestDto request)
        {
            var reports = await _reportService.GetAllLessonReviewReportsAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(reports));
        }

        /// <summary>
        /// Xử lý một course report (approve/reject/escalate).
        /// Nếu RemoveContent=true và Status="resolved" thì soft-remove course.
        /// </summary>
        [HttpPatch("reports/courses/{reportId}")]
        public async Task<IActionResult> ResolveCourseReport(int reportId, [FromBody] ResolveReportRequest request)
        {
            var resolverId = GetUserId();
            if (resolverId == null) return Unauthorized();

            try
            {
                var result = await _reportService.ResolveCourseReportAsync(reportId, resolverId.Value, request);
                if (result)
                {
                    await _hubService.SendReportUpdateAsync();
                    return Ok(ApiResponse<string>.SuccessResponse("Report resolved successfully."));
                }
                return NotFound(ApiResponse<string>.ErrorResponse("Report not found."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
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

        /// <summary>
        /// Xử lý một course review report.
        /// Nếu RemoveContent=true và Status="resolved" thì soft-remove review (is_removed=true).
        /// </summary>
        [HttpPatch("reports/course-reviews/{reportId}")]
        public async Task<IActionResult> ResolveCourseReviewReport(int reportId, [FromBody] ResolveReportRequest request)
        {
            var resolverId = GetUserId();
            if (resolverId == null) return Unauthorized();

            try
            {
                var result = await _reportService.ResolveCourseReviewReportAsync(reportId, resolverId.Value, request);
                if (result)
                {
                    await _hubService.SendReportUpdateAsync();
                    return Ok(ApiResponse<string>.SuccessResponse("Course review report resolved successfully."));
                }
                return NotFound(ApiResponse<string>.ErrorResponse("Report not found."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
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

        /// <summary>
        /// Xử lý một lesson review report.
        /// Nếu RemoveContent=true và Status="resolved" thì soft-remove review.
        /// </summary>
        [HttpPatch("reports/lesson-reviews/{reportId}")]
        public async Task<IActionResult> ResolveLessonReviewReport(int reportId, [FromBody] ResolveReportRequest request)
        {
            var resolverId = GetUserId();
            if (resolverId == null) return Unauthorized();

            try
            {
                var result = await _reportService.ResolveLessonReviewReportAsync(reportId, resolverId.Value, request);
                if (result)
                {
                    await _hubService.SendReportUpdateAsync();
                    return Ok(ApiResponse<string>.SuccessResponse("Lesson review report resolved successfully."));
                }
                return NotFound(ApiResponse<string>.ErrorResponse("Report not found."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
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

        // ── Admin only: Hard delete ─────────────────────────────────────────

        /// <summary>
        /// Admin: Gỡ khóa học khỏi nền tảng (xóa mềm/khóa).
        /// Chỉ dành cho Admin, không dành cho Staff.
        /// </summary>
        [HttpDelete("courses/{courseId}/remove")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RemoveCourse(int courseId)
        {
            var adminId = GetUserId();
            if (adminId == null) return Unauthorized();

            try
            {
                // var result = await _reportService.RemoveCourseAsync(courseId, adminId.Value);
                var result = true;
                return result
                    ? Ok(ApiResponse<string>.SuccessResponse("Course removed successfully."))
                    : NotFound(ApiResponse<string>.ErrorResponse("Course not found."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        // ── Review Moderation (UC-119) ──────────────────────────────────────────

        [HttpGet("reviews/stats")]
        public async Task<IActionResult> GetReviewModerationStats()
        {
            var stats = await _reviewModerationService.GetModerationStatsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(stats));
        }

        [HttpGet("reviews/course")]
        public async Task<IActionResult> GetCourseReviewModerationRecords([FromQuery] PagedReviewModerationRequest request)
        {
            var result = await _reviewModerationService.GetCourseReviewModerationRecordsAsync(request);
            return Ok(result);
        }

        [HttpGet("reviews/lesson")]
        public async Task<IActionResult> GetLessonReviewModerationRecords([FromQuery] PagedReviewModerationRequest request)
        {
            var result = await _reviewModerationService.GetLessonReviewModerationRecordsAsync(request);
            return Ok(result);
        }

        [HttpPost("reviews/approve")]
        public async Task<IActionResult> ApproveReview([FromBody] ApproveRejectReviewRequest request)
        {
            try
            {
                await _reviewModerationService.ApproveReviewAsync(request);
                await _hubService.SendReviewUpdateAsync();
                return Ok(ApiResponse<string>.SuccessResponse("Review approved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("reviews/reject")]
        public async Task<IActionResult> RejectReview([FromBody] ApproveRejectReviewRequest request)
        {
            try
            {
                await _reviewModerationService.RejectReviewAsync(request);
                await _hubService.SendReviewUpdateAsync();
                return Ok(ApiResponse<string>.SuccessResponse("Review rejected successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }
}

