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
    private readonly IRedisService _redisService;
    private readonly INotificationService _notificationService;

    public ChatHub(
        IChatService chatService, 
        IRedisService redisService,
        INotificationService notificationService)
    {
        _chatService = chatService;
        _redisService = redisService;
        _notificationService = notificationService;
    }

    public override async Task OnConnectedAsync()
    {
        var accountIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(accountIdStr, out int accountId))
        {
            await _redisService.SetUserOnlineAsync(accountId, Context.ConnectionId);

            // Tự động join vào tất cả các group chat mà user tham gia
            var myChats = await _chatService.GetMyChatsAsync(accountId);
            foreach (var chat in myChats)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Chat_{chat.ChatId}");
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var accountIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(accountIdStr, out int accountId))
        {
            await _redisService.SetUserOfflineAsync(accountId, Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChat(int chatId)
    {
        var accountId = GetAccountId();
        
        if (await _chatService.HasAccessToChatAsync(accountId, chatId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
            // Thông báo người dùng đã join (tùy chọn)
        }
    }

    public async Task SendMessage(SendMessageDto dto)
    {
        var accountId = GetAccountId();

        try
        {
            // 1. Lưu tin nhắn vào DB
            var messageDto = await _chatService.SaveMessageAsync(accountId, dto);

            // 2. Gửi tin nhắn real-time tới những người trong Group Chat
            await Clients.Group($"Chat_{dto.ChatId}").SendAsync("ReceiveMessage", messageDto);

            // 3. Tăng unread count trong Redis cho các thành viên khác
            // (Logic này có thể chuyển vào ChatService để đồng bộ hơn)
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task BroadcastMessage(string title, string content)
    {
        var accountId = GetAccountId();
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "admin")
        {
            await Clients.Caller.SendAsync("Error", "Chỉ Admin mới có quyền broadcast.");
            return;
        }

        // Ghi Audit Log
        await _chatService.LogActionAsync(accountId, "broadcast", "system", null, $"Tiêu đề: {title}");

        await Clients.All.SendAsync("ReceiveBroadcast", new { Title = title, Content = content, SentAt = DateTime.Now });
    }

    public async Task Typing(int chatId, bool isTyping)
    {
        var accountId = GetAccountId();
        var name = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Ai đó";
        
        await Clients.Group($"Chat_{chatId}").SendAsync("UserTyping", new { ChatId = chatId, AccountId = accountId, Name = name, IsTyping = isTyping });
    }

    private int GetAccountId()
    {
        return int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
