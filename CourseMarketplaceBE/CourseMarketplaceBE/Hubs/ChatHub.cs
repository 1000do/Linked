using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        // Khi kết nối, có thể thực hiện logic nếu cần
        await base.OnConnectedAsync();
    }

    public async Task JoinChat(int chatId)
    {
        var accountId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Kiểm tra quyền truy cập phòng chat
        if (await _chatService.HasAccessToChatAsync(accountId, chatId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
        }
    }

    public async Task SendMessage(SendMessageDto dto)
    {
        var accountId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            // Tin nhắn hệ thống (connection request/accepted) → chỉ broadcast, không lưu DB
            if (dto.Content.StartsWith("__ADMIN_"))
            {
                var systemMsg = new MessageDto
                {
                    ChatId = dto.ChatId,
                    SenderId = accountId,
                    Content = dto.Content,
                    SentAt = DateTime.Now,
                    MessageStatus = "system"
                };
                await Clients.Group($"Chat_{dto.ChatId}").SendAsync("ReceiveMessage", systemMsg);
                return;
            }

            // 1. Lưu tin nhắn thường vào DB
            var messageDto = await _chatService.SaveMessageAsync(accountId, dto);

            // 2. Gửi tin nhắn real-time tới những người trong Group Chat này
            await Clients.Group($"Chat_{dto.ChatId}").SendAsync("ReceiveMessage", messageDto);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }
}
