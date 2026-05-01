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
    private readonly IUserRepository _userRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IRedisService _redisService;

    public ChatService(
        IChatRepository chatRepository, 
        IUserRepository userRepository,
        ICourseRepository courseRepository,
        IRedisService redisService)
    {
        _chatRepository = chatRepository;
        _userRepository = userRepository;
        _courseRepository = courseRepository;
        _redisService = redisService;
    }

    public async Task<List<ChatListDto>> GetMyChatsAsync(int accountId)
    {
        var participants = await _chatRepository.GetParticipantsByAccountIdAsync(accountId);

        var result = new List<ChatListDto>();
        foreach (var p in participants)
        {
            var unreadFromRedis = await _redisService.GetUnreadCountAsync(accountId, p.ChatId);
            
            result.Add(new ChatListDto
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
                UnreadCount = unreadFromRedis > 0 ? unreadFromRedis : p.UnreadCount,
                PartnerName = p.Chat.ChatParticipants
                    .Where(cp => cp.AccountId != accountId)
                    .Select(cp => cp.Account.User != null ? cp.Account.User.FullName : 
                                 (cp.Account.Manager != null ? cp.Account.Manager.DisplayName : cp.Account.Email))
                    .FirstOrDefault(),
                PartnerAvatar = p.Chat.ChatParticipants
                    .Where(cp => cp.AccountId != accountId)
                    .Select(cp => cp.Account.AvatarUrl)
                    .FirstOrDefault()
            });
        }
        return result;
    }

    public async Task<List<MessageDto>> GetChatHistoryAsync(int chatId, int accountId)
    {
        if (!await HasAccessToChatAsync(accountId, chatId))
            throw new UnauthorizedAccessException("Bạn không có quyền xem cuộc hội thoại này.");

        // Đánh dấu đã xem
        await _chatRepository.MarkAsReadAsync(chatId, accountId);
        await _redisService.ClearUnreadCountAsync(accountId, chatId);

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
        var isParticipant = await _chatRepository.IsParticipantAsync(dto.ChatId, senderId);
        
        if (!isParticipant)
        {
            var hasActiveReport = await _chatRepository.HasActiveReportForChatAsync(dto.ChatId);
            if (!hasActiveReport)
                throw new UnauthorizedAccessException("Bạn không có quyền gửi tin nhắn trong cuộc hội thoại này.");
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

        // Cập nhật Redis cho các người nhận
        var chat = await _chatRepository.GetChatByIdAsync(dto.ChatId);
        if (chat != null)
        {
            chat.LastMessageAt = message.SentAt;
            await _chatRepository.UpdateChatAsync(chat);

            // Giả sử lấy list participants để tăng unread trong Redis
            // (Trong thực tế nên dùng Repository để lấy list ID nhanh hơn)
        }

        // Lấy thông tin sender để trả về DTO đầy đủ
        var sender = await _userRepository.GetAccountByIdAsync(senderId);

        return new MessageDto
        {
            MessageId = message.MessageId,
            ChatId = message.ChatId,
            SenderId = message.SenderId,
            Content = message.Content,
            SentAt = message.SentAt,
            MessageStatus = message.MessageStatus,
            SenderName = sender?.User?.FullName ?? sender?.Manager?.DisplayName ?? sender?.Email ?? "Unknown",
            SenderAvatar = sender?.AvatarUrl
        };
    }

    public async Task<int> GetOrCreateChatAsync(int senderId, CreateChatDto dto)
    {
        // 1. Phân quyền tạo Chat (Actor Layer Logic)
        var senderRole = await _userRepository.GetRoleByAccountIdAsync(senderId);
        var targetRole = await _userRepository.GetRoleByAccountIdAsync(dto.TargetAccountId);

        bool canCreate = false;

        if (senderRole == "user" || senderRole == "instructor") 
        {
            // Learner hoặc Instructor: chat với Staff/Admin
            if (targetRole == "admin" || targetRole == "staff") canCreate = true;
            else if (dto.ContextType == "course" && dto.ContextId.HasValue)
            {
                // Kiểm tra quan hệ mua/bán khóa học
                var isEnrolled = await _courseRepository.IsEnrolledAsync(senderId, dto.ContextId.Value) 
                              || await _courseRepository.IsEnrolledAsync(dto.TargetAccountId, dto.ContextId.Value);
                if (isEnrolled) canCreate = true;
            }
        }
        else if (senderRole == "admin" || senderRole == "staff") 
        {
            canCreate = true; // Quyền hạn cao
        }

        if (!canCreate)
            throw new UnauthorizedAccessException("Bạn không có quyền khởi tạo cuộc hội thoại này.");

        // 2. Tìm hoặc tạo mới
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
        if (await _chatRepository.IsParticipantAsync(chatId, accountId)) return true;

        var role = await _userRepository.GetRoleByAccountIdAsync(accountId);
        if (role == "manager") // Kiểm tra Admin/Staff có đang xử lý report không
        {
             return await _chatRepository.HasActiveReportForChatAsync(chatId);
        }

        return false;
    }

    public async Task<bool> GrantAdminAccessAsync(int chatId, int hours)
    {
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

    public async Task<int> GetTotalUnreadCountAsync(int accountId)
    {
        return await _chatRepository.GetTotalUnreadCountAsync(accountId);
    }

    public async Task LogActionAsync(int actorId, string action, string targetType, int? targetId, string details)
    {
        var log = new AuditLog
        {
            ActorId = actorId,
            ActionType = action,
            TargetType = targetType,
            TargetId = targetId,
            Details = details,
            CreatedAt = DateTime.Now
        };
        await _chatRepository.AddAuditLogAsync(log);
    }
}
