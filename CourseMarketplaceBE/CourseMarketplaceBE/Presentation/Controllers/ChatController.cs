using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet("support-account")]
    public async Task<IActionResult> GetSupportAccount()
    {
        var support = await _chatService.GetSupportAccountAsync();
        if (support == null) return NotFound(new { message = "No support staff available." });
        
        return Ok(support);
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetMyChats()
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var chats = await _chatService.GetMyChatsAsync(accountId);
        return Ok(chats);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetChatHistory([FromQuery] int roomId)
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            var messages = await _chatService.GetChatHistoryAsync(roomId, accountId);
            return Ok(messages);
        }
        catch (System.UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetTotalUnreadCount()
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var count = await _chatService.GetTotalUnreadCountAsync(accountId);
        return Ok(new { Count = count });
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateChat([FromBody] CreateChatDto dto)
    {
        var accountIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(accountIdString))
            return Unauthorized(new { message = "User not logged in." });

        var accountId = int.Parse(accountIdString);

        if (dto.TargetAccountId <= 0)
            return BadRequest(new { message = "Invalid target account." });
            
        if (accountId == dto.TargetAccountId)
            return BadRequest(new { message = "Cannot create chat with yourself." });

        try
        {
            var chatId = await _chatService.GetOrCreateChatAsync(accountId, dto);
            return Ok(new { ChatId = chatId });
        }
        catch (System.UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (System.InvalidOperationException ex)
        {
            return BadRequest(new { message = "Failed to create chat" });
        }
    }

    [Authorize] // Cả User và Instructor đều có thể báo cáo
    [HttpPost("report")]
    public async Task<IActionResult> SubmitReport([FromBody] SubmitReportDto dto)
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            var success = await _chatService.SubmitReportAsync(accountId, dto.ChatId, dto.Reason, dto.Description);
            return success ? Ok() : BadRequest();
        }
        catch (System.InvalidOperationException ex)
        {
            return BadRequest(new { message = "Failed to submit report" });
        }
    }

    [HttpPost("grant-access/{chatId}")]
    public async Task<IActionResult> GrantAccess(int chatId)
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Chỉ participant thực sự mới được cấp quyền cho Admin
        if (!await _chatService.HasAccessToChatAsync(accountId, chatId))
            return Forbid();

        try
        {
            var success = await _chatService.GrantAdminAccessAsync(chatId, 24);
            return success ? Ok() : BadRequest();
        }
        catch (System.InvalidOperationException ex)
        {
            return BadRequest(new { message = "Failed to grant admin access" });
        }
    }

    [HttpDelete("{chatId}/clear")]
    public async Task<IActionResult> ClearChatHistory(int chatId)
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            var success = await _chatService.ClearChatHistoryAsync(chatId, accountId);
            return success ? Ok() : BadRequest("Chat not found or access denied.");
        }
        catch (System.InvalidOperationException ex)
        {
            return BadRequest(new { message = "Failed to clear chat history" });
        }
    }

    [HttpPost("{chatId}/read")]
    public async Task<IActionResult> MarkAsRead(int chatId)
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            var success = await _chatService.MarkChatAsReadAsync(chatId, accountId);
            return success ? Ok() : BadRequest();
        }
        catch (System.InvalidOperationException ex)
        {
            return BadRequest(new { message = "Failed to mark chat as read" });
        }
    }
}
