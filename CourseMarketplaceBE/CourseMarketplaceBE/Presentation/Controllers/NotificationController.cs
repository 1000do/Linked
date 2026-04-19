using System.Security.Claims;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notiService;
        private readonly AppDbContext _context; // Viết ngắn gọn thế này thôi

        // Nếu AppDbContext vẫn bị gạch đỏ, hãy nhấn Ctrl + . để Visual Studio tự thêm 'using'
        public NotificationController(INotificationService notiService, AppDbContext context)
        {
            _notiService = notiService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized(); // Trả về 401 thay vì crash

            var userId = int.Parse(userIdClaim);
            var data = await _notiService.GetNotificationsForUserAsync(userId);
            return Ok(data);
        }
        [HttpGet("all")] // Route: GET api/Notification/all
        public async Task<IActionResult> GetAllNotifications()
        {
            // Bạn có thể thêm check Role Admin ở đây nếu muốn
            var data = await _notiService.GetAllNotificationsAsync();
            return Ok(data);
        }

        [HttpPut("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            // Tìm thông báo và đảm bảo nó thuộc về người đang đăng nhập
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.ReceiverId == userId);

            if (notification == null)
                return NotFound(new { message = "Không tìm thấy thông báo hoặc bạn không có quyền" });

            notification.IsRead = true;
            await _context.SaveChangesAsync();

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

            // Tìm top 5 email chứa từ khóa (để tránh trả về quá nhiều)
            var emails = await _context.Accounts
                .Where(a => a.Email.Contains(query))
                .Select(a => a.Email)
                .Take(5)
                .ToListAsync();

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
            if (dto.TargetType == "ALL")
            {
                var allUserIds = await _context.Users.Select(u => u.UserId).ToListAsync();
                foreach (var userId in allUserIds)
                {
                    await _notiService.SendNotificationAsync(userId, dto.Title, dto.Content, null);
                }
                return Ok("Đã gửi cho tất cả");
            }

            if (dto.Emails != null && dto.Emails.Any())
            {
                int count = 0;
                foreach (var email in dto.Emails)
                {
                    var account = await _context.Accounts
                        .Include(a => a.User)
                        .FirstOrDefaultAsync(a => a.Email == email.Trim());

                    if (account?.User != null)
                    {
                        await _notiService.SendNotificationAsync(account.User.UserId, dto.Title, dto.Content, null);
                        count++;
                    }
                }
                return Ok($"Đã gửi thành công cho {count} người dùng.");
            }

            return BadRequest("Dữ liệu không hợp lệ");
        }
    }
}