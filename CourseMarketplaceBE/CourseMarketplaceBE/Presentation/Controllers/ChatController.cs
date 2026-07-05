using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using CourseMarketplaceBE.Hubs;

namespace CourseMarketplaceBE.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
    {
        _chatService = chatService;
        _hubContext = hubContext;
    }

    [HttpGet("support-account")]
    [Authorize(Roles = "user,instructor")]
    public async Task<IActionResult> GetSupportAccount()
    {
        var support = await _chatService.GetSupportAccountAsync();
        if (support == null) return NotFound(new { message = "No support staff available." });
        
        return Ok(support);
    }

    [HttpGet("admin-account")]
    [Authorize(Roles = "admin,staff")]
    public async Task<IActionResult> GetAdminAccount()
    {
        var admin = await _chatService.GetAdminAccountAsync();
        if (admin == null) return NotFound(new { message = "No admin available." });
        
        return Ok(admin);
    }

    [HttpGet("list")]
    [Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> GetMyChats()
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var chats = await _chatService.GetMyChatsAsync(accountId);
        return Ok(chats);
    }

    [HttpGet("search")]
    [Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> SearchChats([FromQuery] string q)
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (string.IsNullOrWhiteSpace(q))
        {
            var chats = await _chatService.GetMyChatsAsync(accountId);
            return Ok(chats);
        }
        var searchResults = await _chatService.SearchChatsAsync(accountId, q);
        return Ok(searchResults);
    }

    [HttpGet("history")]
    [Authorize(Roles = "user,instructor,admin,staff")]
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
    [Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> GetTotalUnreadCount()
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var count = await _chatService.GetTotalUnreadCountAsync(accountId);
        return Ok(new { Count = count });
    }

    [HttpPost("create")]
    [Authorize(Roles = "user,instructor,admin,staff")]
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

    [Authorize(Roles = "user,instructor")] // Cả User và Instructor đều có thể báo cáo
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
    [Authorize(Roles = "user,instructor")]
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
    [Authorize(Roles = "user,instructor,admin,staff")]
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
    [Authorize(Roles = "user,instructor,admin,staff")]
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

    [HttpPost("request-support")]
    [Authorize(Roles = "user,instructor")]
    public async Task<IActionResult> RequestSupport([FromBody] SupportRequestDto dto)
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            var ticket = await _chatService.CreateSupportRequestAsync(accountId, dto);
            
            // Broadcast to the target role group
            await _hubContext.Clients.Group($"Role_{dto.TargetRole.ToLower()}").SendAsync("NewSupportRequest", ticket);
            
            return Ok(ticket);
        }
        catch (System.InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("accept-support/{ticketId}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<IActionResult> AcceptSupport(string ticketId)
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            // Get ticket details to know the sender before we accept (and consume) it
            var tickets = await _chatService.GetPendingRequestsAsync(accountId, "admin");
            if (tickets == null || tickets.Count == 0) tickets = await _chatService.GetPendingRequestsAsync(accountId, "staff");
            
            var ticket = tickets?.FirstOrDefault(t => t.TicketId == ticketId);
            
            // Broadcast to remove from lists
            await _hubContext.Clients.Group("Role_staff").SendAsync("RemoveSupportRequest", ticketId);
            await _hubContext.Clients.Group("Role_admin").SendAsync("RemoveSupportRequest", ticketId);

            var chatId = await _chatService.AcceptSupportRequestAsync(accountId, ticketId);
            
            if (ticket != null)
            {
                // Notify the sender that their request was accepted
                await _hubContext.Clients.Group($"User_{ticket.SenderId}").SendAsync("SupportRequestAccepted", new { ChatId = chatId, TicketId = ticketId });
            }
            
            return Ok(new { ChatId = chatId });
        }
        catch (System.UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (System.InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("pending-requests")]
    [Authorize(Roles = "user,instructor,admin,staff")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        
        var tickets = await _chatService.GetPendingRequestsAsync(accountId, role);
        return Ok(tickets);
    }
}
