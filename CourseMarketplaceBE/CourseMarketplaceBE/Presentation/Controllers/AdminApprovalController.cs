using System.Linq;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers
{
    [Authorize(Roles = "admin,manager")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminApprovalController : ControllerBase
    {
        private readonly IInstructorApprovalService _service;
        public AdminApprovalController(IInstructorApprovalService service) => _service = service;

        [HttpGet("pending-instructors")]
        public async Task<IActionResult> GetPending()
        {
            var list = await _service.GetPendingListAsync();
            Console.WriteLine($"[ADMIN-APPROVAL] Found {list.Count()} pending instructor applications.");
            return Ok(list);
        }

        [HttpPost("process")]
        public async Task<IActionResult> Process(UpdateApprovalStatusDto dto)
        {
            var result = await _service.ApproveOrRejectAsync(dto);
            if (result)
            {
                return Ok(new { status = 200, message = "Thao tác thành công" });
            }
            return BadRequest(new { status = 400, message = "Không tìm thấy giảng viên hoặc có lỗi xảy ra" });
        }
    }
}
