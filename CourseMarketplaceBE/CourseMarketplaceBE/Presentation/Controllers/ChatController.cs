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

    [HttpGet("list")]
    public async Task<IActionResult> GetMyChats()
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var chats = await _chatService.GetMyChatsAsync(accountId);
        return Ok(chats);
    }

    [HttpGet("history/{chatId}")]
    public async Task<IActionResult> GetChatHistory(int chatId)
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            var messages = await _chatService.GetChatHistoryAsync(chatId, accountId);
            return Ok(messages);
        }
        catch (System.UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateChat([FromBody] CreateChatDto dto)
    {
        var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var chatId = await _chatService.GetOrCreateChatAsync(accountId, dto);
        return Ok(new { ChatId = chatId });
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
