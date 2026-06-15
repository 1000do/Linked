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
        private readonly IChatService _chatService;

        public NotificationController(INotificationService notiService, IChatService chatService)
        {
            _notiService = notiService;
            _chatService = chatService;
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
        [Authorize(Roles = "admin,staff")]
        public async Task<IActionResult> GetAllNotifications()
        {
            //var data = await _notiService.GetAllNotificationsAsync();
            var data = await _notiService.GetAllNotificationsForAdminAsync();
            return Ok(data);
        }

        [HttpPut("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            try
            {
                var result = await _notiService.MarkAllAsReadAsync(userId);
                return Ok(new { message = "All notifications marked as read." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to mark all notifications as read"));
            }
        }

        [HttpPut("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            try
            {
                var result = await _notiService.MarkAsReadAsync(id, userId);

                if (!result)
                    return NotFound(new { message = "Notification not found or access denied." });

                return Ok(new { message = "Notification marked as read successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to mark notification as read"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            try
            {
                var result = await _notiService.DeleteNotificationAsync(id, userId);

                if (!result) return BadRequest("Could not delete notification.");
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to delete notification"));
            }
        }

        [HttpGet("search-emails")]
        public async Task<IActionResult> SearchEmails([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Ok(new List<string>());

            var senderIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var senderRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(senderIdClaim) || string.IsNullOrEmpty(senderRole))
                return Unauthorized();

            var senderId = int.Parse(senderIdClaim);
            var emails = await _notiService.SearchEmailsAsync(query, senderId, senderRole);
            return Ok(emails);
        }
        [Authorize(Roles = "admin,staff")]
        [HttpPost("send-test")]
        public async Task<IActionResult> SendTest([FromBody] NotificationSendDto dto)
        {
            try
            {
                await _notiService.SendNotificationAsync(dto.ReceiverId, dto.Title, dto.Content, null);
                return Ok("Sent successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to send notification"));
            }
        }
        [Authorize(Roles = "admin,staff")]
        [HttpPost("send-advanced")]
        public async Task<IActionResult> SendAdvanced([FromBody] NotificationAdvancedDto dto)
        {
            if (dto.Title.Length > 255)
                return BadRequest("Title maximum 255 characters.");

            var senderIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var senderRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(senderIdClaim) || string.IsNullOrEmpty(senderRole))
                return Unauthorized();

            var senderId = int.Parse(senderIdClaim);

            try
            {
                var count = await _notiService.SendAdvancedAsync(dto, senderId, senderRole);
                if (count < 0) return BadRequest("Invalid data.");
                return Ok(new { message = "Sent successfully.", count });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("unread-summary")]
        public async Task<IActionResult> GetUnreadSummary()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var notiCount = await _notiService.GetUnreadCountAsync(userId);
            var chatCount = await _chatService.GetTotalUnreadCountAsync(userId);

            return Ok(new
            {
                NotificationCount = notiCount,
                ChatCount = chatCount
            });
        }
    }
}
