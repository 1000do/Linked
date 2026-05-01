using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;

    public ChatService(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }

    public async Task<List<ChatListDto>> GetMyChatsAsync(int accountId)
    {
        var participants = await _chatRepository.GetParticipantsByAccountIdAsync(accountId);

        return participants.Select(p => new ChatListDto
        {
            ChatId = p.ChatId,
            ChatName = p.Chat.ChatName,
            ChatType = p.Chat.ChatType,
            LastMessage = p.Chat.Messages
                .Where(m => !m.Content.StartsWith("__ADMIN_"))
                .FirstOrDefault()?.Content,
            LastMessageAt = p.Chat.LastMessageAt,
            ContextType = p.Chat.ContextType,
            ContextId = p.Chat.ContextId,
            PartnerName = p.Chat.ChatParticipants
                .Where(cp => cp.AccountId != accountId)
                .Select(cp => cp.Account.User != null ? cp.Account.User.FullName : 
                             (cp.Account.Manager != null ? cp.Account.Manager.DisplayName : cp.Account.Email))
                .FirstOrDefault(),
            PartnerAvatar = p.Chat.ChatParticipants
                .Where(cp => cp.AccountId != accountId)
                .Select(cp => cp.Account.AvatarUrl)
                .FirstOrDefault()
        }).ToList();
    }

    public async Task<List<MessageDto>> GetChatHistoryAsync(int chatId, int accountId)
    {
        if (!await HasAccessToChatAsync(accountId, chatId))
            throw new UnauthorizedAccessException("Bạn không có quyền xem cuộc hội thoại này.");

        var messages = await _chatRepository.GetMessagesByChatIdAsync(chatId);

        return messages.Select(m => new MessageDto
        {
            MessageId = m.MessageId,
            ChatId = m.ChatId,
            SenderId = m.SenderId,
            Content = m.Content,
            IsSeen = m.IsSeen,
            MessageStatus = m.MessageStatus,
            SentAt = m.SentAt,
            SenderName = m.Sender?.User?.FullName ?? m.Sender?.Manager?.DisplayName ?? m.Sender?.Email ?? "Unknown",
            SenderAvatar = m.Sender?.AvatarUrl
        }).ToList();
    }

    public async Task<MessageDto> SaveMessageAsync(int senderId, SendMessageDto dto)
    {
        // Kiểm tra quyền gửi tin nhắn
        var isParticipant = await _chatRepository.IsParticipantAsync(dto.ChatId, senderId);
        
        if (!isParticipant)
        {
            // Nếu không phải participant, phải có report và còn trong thời hạn được cấp quyền (Admin/Staff)
            var hasActiveReport = await _chatRepository.HasActiveReportForChatAsync(dto.ChatId);
            if (!hasActiveReport)
                throw new UnauthorizedAccessException("Bạn không có quyền gửi tin nhắn trong cuộc hội thoại này.");

            // Ở đây bạn có thể thêm logic kiểm tra AccessGrantedUntil từ DB nếu muốn khắt khe hơn
        }

        var message = new Message
        {
            ChatId = dto.ChatId,
            SenderId = senderId,
            Content = dto.Content,
            SentAt = DateTime.Now,
            MessageStatus = "ok"
        };

        await _chatRepository.AddMessageAsync(message);

        var chat = await _chatRepository.GetChatByIdAsync(dto.ChatId);
        if (chat != null)
        {
            chat.LastMessageAt = message.SentAt;
            await _chatRepository.UpdateChatAsync(chat);
        }

        return new MessageDto
        {
            MessageId = message.MessageId,
            ChatId = message.ChatId,
            SenderId = message.SenderId,
            Content = message.Content,
            SentAt = message.SentAt,
            MessageStatus = message.MessageStatus
        };
    }

    public async Task<int> GetOrCreateChatAsync(int senderId, CreateChatDto dto)
    {
        var existingParticipant = await _chatRepository.FindPrivateChatAsync(senderId, dto.TargetAccountId, dto.ContextType, dto.ContextId);
        if (existingParticipant != null) return existingParticipant.ChatId;

        var chat = new Chat
        {
            ChatType = "private",
            ContextType = dto.ContextType,
            ContextId = dto.ContextId,
            CreatedAt = DateTime.Now
        };

        var createdChat = await _chatRepository.CreateChatAsync(chat);

        await _chatRepository.AddChatParticipantsAsync(new List<ChatParticipant>
        {
            new ChatParticipant { ChatId = createdChat.ChatId, AccountId = senderId, Role = "member", JoinedAt = DateTime.Now },
            new ChatParticipant { ChatId = createdChat.ChatId, AccountId = dto.TargetAccountId, Role = "member", JoinedAt = DateTime.Now }
        });

        return createdChat.ChatId;
    }

    public async Task<bool> HasAccessToChatAsync(int accountId, int chatId)
    {
        // 1. Nếu là người tham gia trực tiếp -> Luôn có quyền
        if (await _chatRepository.IsParticipantAsync(chatId, accountId)) return true;

        // 2. Nếu không phải participant, kiểm tra xem có report nào đang xử lý không (Quyền xem lịch sử)
        return await _chatRepository.HasActiveReportForChatAsync(chatId);
    }

    public async Task<bool> GrantAdminAccessAsync(int chatId, int hours)
    {
        // Cấp quyền cho Admin trong X giờ tính từ hiện tại
        var expiry = DateTime.Now.AddHours(hours);
        await _chatRepository.UpdateAdminAccessAsync(chatId, expiry);
        return true;
    }

    public async Task<bool> SubmitReportAsync(int reporterId, int chatId, string reason, string description)
    {
        var report = new UserReport
        {
            ReporterId = reporterId,
            ChatId = chatId,
            Reason = reason,
            Description = description,
            UserReportsStatus = "pending",
            CreatedAt = DateTime.Now
        };

        await _chatRepository.AddReportAsync(report);
        return true;
    }
}
