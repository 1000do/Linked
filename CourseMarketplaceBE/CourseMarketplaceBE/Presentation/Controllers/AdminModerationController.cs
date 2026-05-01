using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Presentation.Controllers
{
    [Authorize(Roles = "manager")] // Manager role as Admin
    [ApiController]
    [Route("api/admin/moderation")]
    public class AdminModerationController : ControllerBase
    {
        private readonly IModerationService _moderationService;

        public AdminModerationController(IModerationService moderationService)
        {
            _moderationService = moderationService;
        }

        [HttpGet("courses/pending")]
        public async Task<IActionResult> GetPendingCourses()
        {
            return Ok(await _moderationService.GetPendingCoursesAsync());
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

        [HttpPost("courses/flag/{id}")]
        public async Task<IActionResult> FlagCourse(int id, [FromBody] string reason)
        {
            var result = await _moderationService.FlagCourseAsync(id, reason);
            return result ? Ok() : NotFound();
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetAllReports()
        {
            return Ok(await _moderationService.GetAllReportsAsync());
        }

        [HttpPost("reports/resolve")]
        public async Task<IActionResult> ResolveReport([FromBody] ResolveReportDto dto)
        {
            var result = await _moderationService.ResolveReportAsync(dto);
            return result ? Ok() : NotFound();
        }
    }
}
