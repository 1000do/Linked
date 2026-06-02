using System.Linq;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers
{
    [Authorize(Roles = "admin,staff")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminApprovalController : ControllerBase
    {
        private readonly IInstructorApprovalService _service;
        public AdminApprovalController(IInstructorApprovalService service) => _service = service;

        [HttpGet("pending-instructors")]
        public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var list = await _service.GetPendingListAsync(page, pageSize);
            Console.WriteLine($"[ADMIN-APPROVAL] Found {list.TotalCount} pending instructor applications.");
            return Ok(list);
        }

        [HttpPost("process")]
        public async Task<IActionResult> Process(UpdateApprovalStatusDto dto)
        {
            try
            {
                var result = await _service.ApproveOrRejectAsync(dto);
                if (result)
                {
                    return Ok(new { status = 200, message = "Operation successful" });
                }
                return BadRequest(new { status = 400, message = "Instructor not found or an error occurred" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { status = 400, message = "Failed to process instructor approval" });
            }
        }
    }
}
