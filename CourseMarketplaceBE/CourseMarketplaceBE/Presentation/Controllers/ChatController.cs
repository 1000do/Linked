using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.IRepositories;
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
    private readonly IUserRepository _userRepository;

    public ChatController(IChatService chatService, IUserRepository userRepository)
    {
        _chatService = chatService;
        _userRepository = userRepository;
    }

    [HttpGet("support-account")]
    public async Task<IActionResult> GetSupportAccount()
    {
        // Giả sử lấy Admin/Staff đầu tiên làm hỗ trợ
        // Trong thực tế nên lấy manager có role 'staff'
        var staffId = await _userRepository.GetStaffAccountIdAsync();
        return Ok(new { AccountId = staffId });
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
        try
        {
            var chatId = await _chatService.GetOrCreateChatAsync(accountId, dto);
            return Ok(new { ChatId = chatId });
        }
        catch (System.UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [Authorize] // Cả User và Instructor đều có thể báo cáo
    [HttpPost("report")]
    public async Task<IActionResult> SubmitReport([FromBody] SubmitReportDto dto)
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var success = await _chatService.SubmitReportAsync(accountId, dto.ChatId, dto.Reason, dto.Description);
        return success ? Ok() : BadRequest();
    }

    [HttpPost("grant-access/{chatId}")]
    public async Task<IActionResult> GrantAccess(int chatId)
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Chỉ participant thực sự mới được cấp quyền cho Admin
        if (!await _chatService.HasAccessToChatAsync(accountId, chatId))
            return Forbid();

        var success = await _chatService.GrantAdminAccessAsync(chatId, 24);
        return success ? Ok() : BadRequest();
    }
}
