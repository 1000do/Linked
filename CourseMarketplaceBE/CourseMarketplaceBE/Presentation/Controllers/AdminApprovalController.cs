using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers
{
    [Authorize(Roles = "manager")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminApprovalController : ControllerBase
    {
        private readonly IInstructorApprovalService _service;
        public AdminApprovalController(IInstructorApprovalService service) => _service = service;

        [HttpGet("pending-instructors")]
        public async Task<IActionResult> GetPending() => Ok(await _service.GetPendingListAsync());

        [HttpPost("process")]
        public async Task<IActionResult> Process(UpdateApprovalStatusDto dto)
        {
            var result = await _service.ApproveOrRejectAsync(dto);
            return result ? Ok(new { message = "Thao tác thành công" }) : BadRequest("Không tìm thấy giảng viên");
        }
    }
}
