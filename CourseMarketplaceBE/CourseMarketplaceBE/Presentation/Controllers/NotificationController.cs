using System.Security.Claims;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notiService;

        public NotificationController(INotificationService notiService)
        {
            _notiService = notiService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var data = await _notiService.GetNotificationsForUserAsync(userId);
            return Ok(data);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllNotifications()
        {
            var data = await _notiService.GetAllNotificationsAsync();
            return Ok(data);
        }

        [HttpPut("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var result = await _notiService.MarkAsReadAsync(id, userId);

            if (!result)
                return NotFound(new { message = "Không tìm thấy thông báo hoặc bạn không có quyền" });

            return Ok(new { message = "Đã đánh dấu đã đọc thành công" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _notiService.DeleteNotificationAsync(id, userId);

            if (!result) return BadRequest("Không thể xóa thông báo");
            return Ok();
        }

        [HttpGet("search-emails")]
        public async Task<IActionResult> SearchEmails([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Ok(new List<string>());

            var emails = await _notiService.SearchEmailsAsync(query);
            return Ok(emails);
        }

        [HttpPost("send-test")]
        public async Task<IActionResult> SendTest([FromBody] NotificationSendDto dto)
        {
            await _notiService.SendNotificationAsync(dto.ReceiverId, dto.Title, dto.Content, null);
            return Ok("Đã gửi thành công");
        }

        [HttpPost("send-advanced")]
        public async Task<IActionResult> SendAdvanced([FromBody] NotificationAdvancedDto dto)
        {
            var count = await _notiService.SendAdvancedAsync(dto);
            if (count < 0) return BadRequest("Dữ liệu không hợp lệ");
            if (dto.TargetType == "ALL") return Ok("Đã gửi cho tất cả");
            return Ok($"Đã gửi thành công cho {count} người dùng.");
        }
    }
}