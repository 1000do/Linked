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
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value?.ToLower() ?? "user";

        if (int.TryParse(accountIdStr, out int accountId))
        {
            await _redisService.SetUserOnlineAsync(accountId, Context.ConnectionId);
            await Clients.Others.SendAsync("UserOnlineStatus", new { AccountId = accountId, IsOnline = true });

            // Tự động join vào tất cả các group chat mà user tham gia
            var myChats = await _chatService.GetMyChatsAsync(accountId);
            foreach (var chat in myChats)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Chat_{chat.ChatId}");
            }

            // Ticket-based support groups
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{role}");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{accountId}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var accountIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(accountIdStr, out int accountId))
        {
            await _redisService.SetUserOfflineAsync(accountId, Context.ConnectionId);
            
            // Check if user has other active connections before broadcasting offline
            var isStillOnline = await _redisService.IsUserOnlineAsync(accountId);
            if (!isStillOnline)
            {
                await Clients.Others.SendAsync("UserOnlineStatus", new { AccountId = accountId, IsOnline = false });
            }
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

            // 2. Lấy danh sách participants
            var participantIds = await _chatService.GetParticipantIdsAsync(dto.ChatId);

            // 3. Gửi tin nhắn real-time tới từng người thông qua User_{id}
            foreach (var pId in participantIds)
            {
                await Clients.Group($"User_{pId}").SendAsync("ReceiveMessage", messageDto);
            }

            // (Tuỳ chọn: vẫn giữ gửi cho group chat để đảm bảo đồng bộ nếu cần, 
            // nhưng gửi qua User_{pId} đã đủ để trigger loadChats() ở FE)
            // await Clients.Group($"Chat_{dto.ChatId}").SendAsync("ReceiveMessage", messageDto);
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
            await Clients.Caller.SendAsync("Error", "Only Admin is authorized to broadcast.");
            return;
        }

        // Ghi Audit Log
        await _chatService.LogActionAsync(accountId, "broadcast", "system", null, $"Title: {title}");

        await Clients.All.SendAsync("ReceiveBroadcast", new { Title = title, Content = content, SentAt = DateTime.Now });
    }

    public async Task Typing(int chatId, bool isTyping)
    {
        var accountId = GetAccountId();
        var name = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Someone";
        
        var participantIds = await _chatService.GetParticipantIdsAsync(chatId);
        foreach (var pId in participantIds)
        {
            await Clients.Group($"User_{pId}").SendAsync("UserTyping", new { ChatId = chatId, AccountId = accountId, Name = name, IsTyping = isTyping });
        }
    }

    public async Task MarkAsRead(int chatId)
    {
        var accountId = GetAccountId();
        
        var participantIds = await _chatService.GetParticipantIdsAsync(chatId);
        foreach (var pId in participantIds)
        {
            await Clients.Group($"User_{pId}").SendAsync("MessageRead", new { ChatId = chatId, AccountId = accountId });
        }
    }

    private int GetAccountId()
    {
        return int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
