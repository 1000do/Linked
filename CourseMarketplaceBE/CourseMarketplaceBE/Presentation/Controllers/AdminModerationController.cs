using System.Security.Claims;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Presentation.Controllers
{
    [Authorize(Roles = "admin,staff")]
    [ApiController]
    [Route("api/admin/moderation")]
    public class AdminModerationController : ControllerBase
    {
        private readonly ICourseModerationService _courseModerationService;
        private readonly IUserReportModerationService _userReportModerationService;
        private readonly IReportService _reportService;

        public AdminModerationController(
            ICourseModerationService courseModerationService,
            IUserReportModerationService userReportModerationService,
            IReportService reportService)
        {
            _courseModerationService = courseModerationService;
            _userReportModerationService = userReportModerationService;
            _reportService = reportService;
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
            return result ? Ok() : NotFound();
        }

        [HttpPost("courses/reject/{id}")]
        public async Task<IActionResult> RejectCourse(int id, [FromBody] string reason)
        {
            var result = await _courseModerationService.RejectCourseAsync(id, reason);
            return result ? Ok() : NotFound();
        }

        [HttpPost("courses/reject-detailed")]
        public async Task<IActionResult> RejectCourseDetailed([FromBody] RejectCourseDetailedRequest request)
        {
            var result = await _courseModerationService.RejectCourseDetailedAsync(request);
            return result ? Ok() : NotFound();
        }

        [HttpPost("courses/flag/{id}")]
        public async Task<IActionResult> FlagCourse(int id, [FromBody] string reason)
        {
            var result = await _courseModerationService.FlagCourseAsync(id, reason);
            return result ? Ok() : NotFound();
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
        public async Task<IActionResult> GetAllCourseReports([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var reports = await _reportService.GetAllCourseReportsAsync(status, page, pageSize);
            return Ok(ApiResponse<object>.SuccessResponse(reports));
        }

        /// <summary>Lấy tất cả course review reports (có thể lọc theo status)</summary>
        [HttpGet("reports/course-reviews")]
        public async Task<IActionResult> GetAllCourseReviewReports([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var reports = await _reportService.GetAllCourseReviewReportsAsync(status, page, pageSize);
            return Ok(ApiResponse<object>.SuccessResponse(reports));
        }

        /// <summary>Lấy tất cả lesson review reports (có thể lọc theo status)</summary>
        [HttpGet("reports/lesson-reviews")]
        public async Task<IActionResult> GetAllLessonReviewReports([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var reports = await _reportService.GetAllLessonReviewReportsAsync(status, page, pageSize);
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
                return result
                    ? Ok(ApiResponse<string>.SuccessResponse("Report resolved successfully."))
                    : NotFound(ApiResponse<string>.ErrorResponse("Report not found."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to resolve course report"));
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
                return result
                    ? Ok(ApiResponse<string>.SuccessResponse("Course review report resolved successfully."))
                    : NotFound(ApiResponse<string>.ErrorResponse("Report not found."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to resolve course review report"));
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
                return result
                    ? Ok(ApiResponse<string>.SuccessResponse("Lesson review report resolved successfully."))
                    : NotFound(ApiResponse<string>.ErrorResponse("Report not found."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to resolve lesson review report"));
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
                var result = await _reportService.RemoveCourseAsync(courseId, adminId.Value);
                return result
                    ? Ok(ApiResponse<string>.SuccessResponse("Course removed successfully."))
                    : NotFound(ApiResponse<string>.ErrorResponse("Course not found."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to remove course"));
            }
        }
    }
}

