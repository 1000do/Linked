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
        private readonly IModerationService _moderationService;
        private readonly IReportService _reportService;

        public AdminModerationController(IModerationService moderationService, IReportService reportService)
        {
            _moderationService = moderationService;
            _reportService = reportService;
        }

        private int? GetUserId()
        {
            var str = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(str, out int id) ? id : null;
        }

        // ── Course Approval Moderation (existing) ──────────────────────────

        [HttpGet("courses/pending")]
        public async Task<IActionResult> GetPendingCourses([FromQuery] ModerationFilterDto filter)
        {
            return Ok(await _moderationService.GetPendingCoursesAsync(filter));
        }

        [HttpPost("courses/approve/{id}")]
        public async Task<IActionResult> ApproveCourse(int id, [FromBody] string? feedback)
        {
            var result = await _moderationService.ApproveCourseAsync(id, feedback);
            return result ? Ok() : NotFound();
        }

        [HttpPost("courses/reject/{id}")]
        public async Task<IActionResult> RejectCourse(int id, [FromBody] string reason)
        {
            var result = await _moderationService.RejectCourseAsync(id, reason);
            return result ? Ok() : NotFound();
        }

        [HttpPost("courses/reject-detailed")]
        public async Task<IActionResult> RejectCourseDetailed([FromBody] RejectCourseDetailedRequest request)
        {
            var result = await _moderationService.RejectCourseDetailedAsync(request);
            return result ? Ok() : NotFound();
        }

        [HttpPost("courses/flag/{id}")]
        public async Task<IActionResult> FlagCourse(int id, [FromBody] string reason)
        {
            var result = await _moderationService.FlagCourseAsync(id, reason);
            return result ? Ok() : NotFound();
        }

        // ── Legacy report endpoint (kept for backward compatibility) ────────

        [HttpGet("reports")]
        public async Task<IActionResult> GetAllReports()
        {
            return Ok(await _moderationService.GetAllReportsAsync());
        }

        [HttpPost("reports/resolve")]
        public async Task<IActionResult> ResolveReport([FromBody] ResolveReportDto dto)
        {
            var resolverId = GetUserId();
            if (resolverId == null) return Unauthorized();

            try
            {
                var result = await _moderationService.ResolveReportAsync(dto, resolverId.Value);
                return result 
                    ? Ok(ApiResponse<string>.SuccessResponse("Báo cáo đã được xử lý thành công.")) 
                    : NotFound(ApiResponse<string>.ErrorResponse("Không tìm thấy báo cáo."));
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
        public async Task<IActionResult> GetAllCourseReports([FromQuery] string? status = null)
        {
            var reports = await _reportService.GetAllCourseReportsAsync(status);
            return Ok(ApiResponse<object>.SuccessResponse(reports));
        }

        /// <summary>Lấy tất cả course review reports (có thể lọc theo status)</summary>
        [HttpGet("reports/course-reviews")]
        public async Task<IActionResult> GetAllCourseReviewReports([FromQuery] string? status = null)
        {
            var reports = await _reportService.GetAllCourseReviewReportsAsync(status);
            return Ok(ApiResponse<object>.SuccessResponse(reports));
        }

        /// <summary>Lấy tất cả lesson review reports (có thể lọc theo status)</summary>
        [HttpGet("reports/lesson-reviews")]
        public async Task<IActionResult> GetAllLessonReviewReports([FromQuery] string? status = null)
        {
            var reports = await _reportService.GetAllLessonReviewReportsAsync(status);
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
                    ? Ok(ApiResponse<string>.SuccessResponse("Báo cáo đã được xử lý thành công."))
                    : NotFound(ApiResponse<string>.ErrorResponse("Không tìm thấy báo cáo."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<string>.ErrorResponse(ex.Message));
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

            var result = await _reportService.ResolveCourseReviewReportAsync(reportId, resolverId.Value, request);
            return result
                ? Ok(ApiResponse<string>.SuccessResponse("Báo cáo đánh giá đã được xử lý thành công."))
                : NotFound(ApiResponse<string>.ErrorResponse("Không tìm thấy báo cáo."));
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

            var result = await _reportService.ResolveLessonReviewReportAsync(reportId, resolverId.Value, request);
            return result
                ? Ok(ApiResponse<string>.SuccessResponse("Báo cáo đánh giá bài học đã được xử lý thành công."))
                : NotFound(ApiResponse<string>.ErrorResponse("Không tìm thấy báo cáo."));
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

            var result = await _reportService.RemoveCourseAsync(courseId, adminId.Value);
            return result
                ? Ok(ApiResponse<string>.SuccessResponse("Khóa học đã được gỡ thành công."))
                : NotFound(ApiResponse<string>.ErrorResponse("Không tìm thấy khóa học."));
        }
    }
}

