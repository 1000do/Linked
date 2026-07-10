using System.Security.Claims;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers
{
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
        [Authorize(Roles = "user,instructor")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = int.MaxValue)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            try {
                var data = await _notiService.GetNotificationsForUserAsync(userId, page, pageSize);
                return Ok(ApiResponse<PagedResult<NotificationResponseDto>>.SuccessResponse(data));
            } catch (KeyNotFoundException) {
                return Ok(ApiResponse<PagedResult<NotificationResponseDto>>.SuccessResponse(new PagedResult<NotificationResponseDto> { Items = new List<NotificationResponseDto>(), TotalCount = 0 }));
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "admin,staff")]
        public async Task<IActionResult> GetAllNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = int.MaxValue)
        {
            try {
                var data = await _notiService.GetAllNotificationsForAdminAsync(page, pageSize);
                return Ok(ApiResponse<PagedResult<NotificationAdminResponseDto>>.SuccessResponse(data));
            } catch (KeyNotFoundException) {
                return Ok(ApiResponse<PagedResult<NotificationAdminResponseDto>>.SuccessResponse(new PagedResult<NotificationAdminResponseDto> { Items = new List<NotificationAdminResponseDto>(), TotalCount = 0 }));
            }
        }

        [HttpPut("mark-all-as-read")]
        [Authorize(Roles = "user,instructor")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            try
            {
                var result = await _notiService.MarkAllAsReadAsync(userId);
                return Ok(ApiResponse<string>.SuccessResponse("All notifications marked as read."));
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is CourseMarketplaceBE.Application.Exceptions.BadRequestException)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("mark-as-read/{id}")]
        [Authorize(Roles = "user,instructor")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            try
            {
                var result = await _notiService.MarkAsReadAsync(id, userId);
                return Ok(ApiResponse<string>.SuccessResponse("Notification marked as read successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is CourseMarketplaceBE.Application.Exceptions.BadRequestException)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "user,instructor")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim);

            try
            {
                var result = await _notiService.DeleteNotificationAsync(id, userId);
                return Ok(ApiResponse<string>.SuccessResponse("Deleted successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is CourseMarketplaceBE.Application.Exceptions.BadRequestException)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("search-emails")]
        [Authorize(Roles = "admin,staff")]
        public async Task<IActionResult> SearchEmails([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Ok(new List<string>());

            var senderIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var senderRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(senderIdClaim) || string.IsNullOrEmpty(senderRole))
                return Unauthorized();

            var senderId = int.Parse(senderIdClaim);
            var emails = await _notiService.SearchEmailsAsync(query, senderId, senderRole);
            return Ok(ApiResponse<List<string>>.SuccessResponse(emails));
        }
        [Authorize(Roles = "admin,staff")]
        [HttpPost("send-test")]
        public async Task<IActionResult> SendTest([FromBody] NotificationSendDto dto)
        {
            try
            {
                await _notiService.SendNotificationAsync(dto.ReceiverId, dto.Title, dto.Content, null);
                return Ok(ApiResponse<string>.SuccessResponse("Sent successfully."));
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is CourseMarketplaceBE.Application.Exceptions.BadRequestException)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
        [Authorize(Roles = "admin,staff")]
        [HttpPost("send-advanced")]
        public async Task<IActionResult> SendAdvanced([FromBody] NotificationAdvancedDto dto)
        {
            var senderIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var senderRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(senderIdClaim) || string.IsNullOrEmpty(senderRole))
                return Unauthorized();

            var senderId = int.Parse(senderIdClaim);

            try
            {
                var count = await _notiService.SendAdvancedAsync(dto, senderId, senderRole);
                if (count < 0) return BadRequest(ApiResponse<string>.ErrorResponse("Invalid data."));
                return Ok(ApiResponse<object>.SuccessResponse(new { message = "Sent successfully.", count }));
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is CourseMarketplaceBE.Application.Exceptions.BadRequestException)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("unread-summary")]
        [Authorize(Roles = "user,instructor")]
        public async Task<IActionResult> GetUnreadSummary()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var notiCount = await _notiService.GetUnreadCountAsync(userId);
            var chatCount = await _chatService.GetTotalUnreadCountAsync(userId);

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                NotificationCount = notiCount,
                ChatCount = chatCount
            }));
        }
    }
}
