using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.Extensions.Configuration;
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
    private readonly IConfiguration _configuration;

    public ChatService(
        IChatRepository chatRepository, 
        IUserRepository userRepository,
        ICourseRepository courseRepository,
        IRedisService redisService,
        IConfiguration configuration)
    {
        _chatRepository = chatRepository;
        _userRepository = userRepository;
        _courseRepository = courseRepository;
        _redisService = redisService;
        _configuration = configuration;
    }

    public async Task<List<ChatListDto>> GetMyChatsAsync(int accountId)
    {
        var participants = await _chatRepository.GetParticipantsByAccountIdAsync(accountId);
        return await ProcessAndMapChatParticipantsAsync(accountId, participants);
    }

    public async Task<List<ChatListDto>> SearchChatsAsync(int accountId, string query)
    {
        var participants = await _chatRepository.SearchParticipantsByAccountIdAsync(accountId, query);
        return await ProcessAndMapChatParticipantsAsync(accountId, participants);
    }

    public async Task<List<MessageDto>> GetChatHistoryAsync(int chatId, int accountId)
    {
        await ValidateChatAccessAsync(accountId, chatId);
        await MarkChatAsReadAsync(chatId, accountId);

        var participant = await _chatRepository.GetParticipantAsync(chatId, accountId);
        var messages = await _chatRepository.GetMessagesByChatIdAsync(chatId);

        var filteredMessages = FilterMessagesByClearedAt(messages, participant?.ClearedAt);

        return filteredMessages.Select(MapToMessageDto).ToList();
    }

    public async Task<MessageDto> SaveMessageAsync(int senderId, SendMessageDto dto)
    {
        await ValidateMessageSendingAccessAsync(senderId, dto.ChatId);

        var message = CreateMessageEntity(senderId, dto);
        ProcessMessageAttachments(message, dto.Attachments);

        await _chatRepository.AddMessageAsync(message);
        await UpdateChatAndNotifyRecipientsAsync(senderId, dto.ChatId, message.SentAt);
        await _chatRepository.SaveChangesAsync();

        var sender = await _userRepository.GetAccountByIdAsync(senderId);
        message.Sender = sender;

        return MapToMessageDto(message);
    }

    public async Task<int> GetOrCreateChatAsync(int senderId, CreateChatDto dto)
    {
        await ValidateChatCreationAccessAsync(senderId, dto);

        var existingChatId = await TryGetExistingPrivateChatAsync(senderId, dto.TargetAccountId, dto.ContextType, dto.ContextId);
        if (existingChatId.HasValue) return existingChatId.Value;

        return await CreateNewPrivateChatAsync(senderId, dto);
    }

    public async Task<bool> HasAccessToChatAsync(int accountId, int chatId)
    {
        if (await _chatRepository.IsParticipantAsync(chatId, accountId)) return true;

        var role = await _userRepository.GetRoleByAccountIdAsync(accountId);
        if (role == "admin" || role == "staff")
        {
             return await _chatRepository.HasActiveReportForChatAsync(chatId);
        }

        return false;
    }

    public async Task<List<int>> GetParticipantIdsAsync(int chatId)
    {
        return await _chatRepository.GetParticipantIdsAsync(chatId);
    }

    public async Task<bool> GrantAdminAccessAsync(int chatId, int hours)
    {
        var expiry = DateTime.Now.AddHours(hours);
        await _chatRepository.UpdateAdminAccessAsync(chatId, expiry);
        await _chatRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearChatHistoryAsync(int chatId, int accountId)
    {
        var participant = await _chatRepository.GetParticipantAsync(chatId, accountId);
        if (participant == null) return false;

        participant.ClearedAt = DateTime.UtcNow;
        participant.UnreadCount = 0;
        await _chatRepository.UpdateParticipantAsync(participant);
        await _chatRepository.SaveChangesAsync();
            
        await _redisService.ClearUnreadCountAsync(accountId, chatId);
        return true;
    }

    public async Task<bool> MarkChatAsReadAsync(int chatId, int accountId)
    {
        await _chatRepository.MarkAsReadAsync(chatId, accountId);
        await _chatRepository.SaveChangesAsync();
        await _redisService.ClearUnreadCountAsync(accountId, chatId);
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
        await _chatRepository.SaveChangesAsync();
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
        await _chatRepository.SaveChangesAsync();
    }

    public async Task<SupportAccountDto?> GetSupportAccountAsync()
    {
        var staffId = await _userRepository.GetStaffAccountIdAsync();
        if (staffId == null) return null;
        
        var staff = await _userRepository.GetAccountByIdAsync(staffId.Value);
        return new SupportAccountDto
        { 
            AccountId = staffId.Value,
            FullName = staff?.User?.FullName ?? "Support Team",
            AvatarUrl = staff?.AvatarUrl ?? ""
        };
    }

    public async Task<SupportAccountDto?> GetAdminAccountAsync()
    {
        var adminId = await _userRepository.GetAdminIdAsync();
        if (adminId == null) return null;
        
        var admin = await _userRepository.GetAccountByIdAsync(adminId.Value);
        return new SupportAccountDto
        { 
            AccountId = adminId.Value,
            FullName = admin?.User?.FullName ?? admin?.Manager?.DisplayName ?? "Administrator",
            AvatarUrl = admin?.AvatarUrl ?? ""
        };
    }

    // --- Ticket-Based Support Flow ---

    public async Task<SupportTicketDto> CreateSupportRequestAsync(int senderId, SupportRequestDto dto)
    {
        var sender = await _userRepository.GetAccountByIdAsync(senderId);
        if (sender == null) throw new InvalidOperationException("Sender not found");

        var ticket = CreateTicketFromDto(senderId, sender, dto);

        await AddTicketToCacheAsync(ticket);
        
        return ticket;
    }

    public async Task<int> AcceptSupportRequestAsync(int acceptorId, string ticketId)
    {
        var tickets = await _redisService.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets") ?? new List<SupportTicketDto>();
        
        var ticket = await ValidateAndExtractSupportTicketAsync(tickets, ticketId, acceptorId);
        
        await RemoveSupportTicketFromCacheAsync(tickets, ticketId);

        var chatId = await InitiateSupportChatAsync(ticket.SenderId, acceptorId);
        await SaveInitialSupportMessageAsync(chatId, ticket);

        return chatId;
    }

    public async Task<List<SupportTicketDto>> GetPendingRequestsAsync(int accountId, string currentRole)
    {
        var tickets = await _redisService.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets") ?? new List<SupportTicketDto>();
        
        tickets = await CleanupExpiredTicketsAsync(tickets);

        return FilterTicketsByRole(tickets, accountId, currentRole);
    }

    // --- Private Helper Methods ---

    private async Task<List<ChatListDto>> ProcessAndMapChatParticipantsAsync(int accountId, IEnumerable<ChatParticipant> participants)
    {
        var result = new List<ChatListDto>();
        foreach (var p in participants)
        {
            var dto = await MapToChatListDtoAsync(accountId, p);
            if (dto != null)
            {
                result.Add(dto);
            }
        }
        return result;
    }

    private async Task<ChatListDto?> MapToChatListDtoAsync(int accountId, ChatParticipant p)
    {
        if (p.ClearedAt.HasValue && p.Chat.LastMessageAt.HasValue && p.Chat.LastMessageAt.Value <= p.ClearedAt.Value)
        {
            return null;
        }

        if (!p.Chat.Messages.Any())
        {
            return null;
        }

        var unreadFromRedis = await _redisService.GetUnreadCountAsync(accountId, p.ChatId);
        
        var partner = p.Chat.ChatParticipants.FirstOrDefault(cp => cp.AccountId != accountId);
        var partnerId = partner?.AccountId;
        var isOnline = partnerId.HasValue && await _redisService.IsUserOnlineAsync(partnerId.Value);

        return new ChatListDto
        {
            ChatId = p.ChatId,
            ChatName = p.Chat.ChatName,
            ChatType = p.Chat.ChatType,
            LastMessage = p.Chat.Messages
                .Where(m => m.Content != null && !m.Content.StartsWith("__ADMIN_"))
                .FirstOrDefault()?.Content,
            LastMessageAt = p.Chat.LastMessageAt,
            ContextType = p.Chat.ContextType,
            ContextId = p.Chat.ContextId,
            UnreadCount = unreadFromRedis > 0 ? unreadFromRedis : p.UnreadCount,
            PartnerName = partner?.Account.User?.FullName 
                ?? partner?.Account.Manager?.DisplayName 
                ?? partner?.Account.Username 
                ?? partner?.Account.Email 
                ?? "Support Team",
            PartnerAvatar = partner?.Account.AvatarUrl,
            PartnerId = partnerId,
            IsOnline = isOnline
        };
    }

    private async Task ValidateChatAccessAsync(int accountId, int chatId)
    {
        if (!await HasAccessToChatAsync(accountId, chatId))
            throw new UnauthorizedAccessException("You do not have permission to view this chat.");
    }

    private List<Message> FilterMessagesByClearedAt(List<Message> messages, DateTime? clearedAt)
    {
        if (clearedAt != null)
        {
            return messages.Where(m => m.SentAt > clearedAt.Value).ToList();
        }
        return messages;
    }

    private MessageDto MapToMessageDto(Message m)
    {
        return new MessageDto
        {
            MessageId = m.MessageId,
            ChatId = m.ChatId,
            SenderId = m.SenderId,
            Content = m.Content,
            IsSeen = m.IsSeen,
            MessageStatus = m.MessageStatus,
            SentAt = m.SentAt,
            SenderName = m.Sender?.User?.FullName ?? m.Sender?.Manager?.DisplayName ?? m.Sender?.Email ?? "Unknown",
            SenderAvatar = m.Sender?.AvatarUrl,
            Attachments = m.Attachments?.Select(a => new AttachmentDto
            {
                AttachmentId = a.AttachmentId,
                FileUrl = a.FileUrl,
                FileName = a.FileName,
                FileType = a.FileType,
                FileSize = a.FileSize
            }).ToList() ?? new List<AttachmentDto>()
        };
    }

    private async Task ValidateMessageSendingAccessAsync(int senderId, int chatId)
    {
        var isParticipant = await _chatRepository.IsParticipantAsync(chatId, senderId);
        if (!isParticipant)
        {
            var hasActiveReport = await _chatRepository.HasActiveReportForChatAsync(chatId);
            if (!hasActiveReport)
                throw new UnauthorizedAccessException("You do not have permission to send messages in this chat.");
        }
    }

    private Message CreateMessageEntity(int senderId, SendMessageDto dto)
    {
        return new Message
        {
            ChatId = dto.ChatId,
            SenderId = senderId,
            Content = dto.Content,
            SentAt = DateTime.Now,
            MessageStatus = "ok",
            Attachments = new List<MessageAttachment>()
        };
    }

    private void ProcessMessageAttachments(Message message, List<AttachmentInputDto>? attachments)
    {
        if (_configuration.GetValue<bool>("ChatSettings:EnableAttachments") && attachments != null && attachments.Any())
        {
            message.Attachments = attachments.Select(a => new MessageAttachment
            {
                FileUrl = a.FileUrl,
                FileName = a.FileName,
                FileType = a.FileType,
                FileSize = a.FileSize,
                CreatedAt = DateTime.Now
            }).ToList();
        }
    }

    private async Task UpdateChatAndNotifyRecipientsAsync(int senderId, int chatId, DateTime? sentAt)
    {
        var chat = await _chatRepository.GetChatByIdAsync(chatId);
        if (chat != null)
        {
            chat.LastMessageAt = sentAt;
            await _chatRepository.UpdateChatAsync(chat);

            var participantIds = await _chatRepository.GetParticipantIdsAsync(chatId);
            foreach (var accountId in participantIds)
            {
                if (accountId != senderId)
                {
                    await _redisService.IncrementUnreadCountAsync(accountId, chatId);
                }
            }
        }
    }

    private async Task ValidateChatCreationAccessAsync(int senderId, CreateChatDto dto)
    {
        var senderRole = await _userRepository.GetRoleByAccountIdAsync(senderId);
        var targetRole = await _userRepository.GetRoleByAccountIdAsync(dto.TargetAccountId);

        bool canCreate = false;

        if (senderRole == "user" || senderRole == "instructor") 
        {
            if (targetRole == "admin" || targetRole == "staff") canCreate = true;
            else if (dto.ContextType == "course" && dto.ContextId.HasValue)
            {
                var isEnrolled = await _courseRepository.IsEnrolledAsync(senderId, dto.ContextId.Value) 
                              || await _courseRepository.IsEnrolledAsync(dto.TargetAccountId, dto.ContextId.Value);
                if (isEnrolled) canCreate = true;
            }
        }
        else if (senderRole == "admin" || senderRole == "staff") 
        {
            canCreate = true;
        }

        if (!canCreate)
            throw new UnauthorizedAccessException("You do not have permission to initiate this conversation.");
    }

    private async Task<int?> TryGetExistingPrivateChatAsync(int senderId, int targetAccountId, string? contextType, int? contextId)
    {
        var existingParticipant = await _chatRepository.FindPrivateChatAsync(senderId, targetAccountId, contextType, contextId);
        return existingParticipant?.ChatId;
    }

    private async Task<int> CreateNewPrivateChatAsync(int senderId, CreateChatDto dto)
    {
        var chat = new Chat
        {
            ChatType = "private",
            ContextType = dto.ContextType,
            ContextId = dto.ContextId,
            CreatedAt = DateTime.Now,
            ChatParticipants = new List<ChatParticipant>
            {
                new ChatParticipant { AccountId = senderId, Role = "member", JoinedAt = DateTime.Now },
                new ChatParticipant { AccountId = dto.TargetAccountId, Role = "member", JoinedAt = DateTime.Now }
            }
        };

        var createdChat = await _chatRepository.CreateChatAsync(chat);
        await _chatRepository.SaveChangesAsync();

        return createdChat.ChatId;
    }

    private SupportTicketDto CreateTicketFromDto(int senderId, Account sender, SupportRequestDto dto)
    {
        var ticketId = Guid.NewGuid().ToString("N");
        return new SupportTicketDto
        {
            TicketId = ticketId,
            SenderId = senderId,
            SenderName = sender.User?.FullName ?? sender.Manager?.DisplayName ?? sender.Username ?? sender.Email ?? "Unknown",
            SenderAvatar = sender.AvatarUrl,
            InitialMessage = dto.Content,
            RequestedAt = DateTime.UtcNow,
            TargetRole = dto.TargetRole
        };
    }

    private async Task AddTicketToCacheAsync(SupportTicketDto ticket)
    {
        var tickets = await _redisService.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets") ?? new List<SupportTicketDto>();
        tickets.Add(ticket);
        await _redisService.SetCacheAsync("ActiveSupportTickets", tickets, TimeSpan.FromDays(7));
    }

    private async Task<SupportTicketDto> ValidateAndExtractSupportTicketAsync(List<SupportTicketDto> tickets, string ticketId, int acceptorId)
    {
        var ticket = tickets.FirstOrDefault(t => t.TicketId == ticketId);
        
        if (ticket == null) throw new InvalidOperationException("This support request has already been accepted or expired.");

        var role = await _userRepository.GetRoleByAccountIdAsync(acceptorId);
        if (ticket.TargetRole == "admin" && role != "admin")
            throw new UnauthorizedAccessException("Only admins can accept this request.");
        if (ticket.TargetRole == "staff" && role != "staff" && role != "admin")
            throw new UnauthorizedAccessException("Only staff or admins can accept this request.");

        return ticket;
    }

    private async Task RemoveSupportTicketFromCacheAsync(List<SupportTicketDto> tickets, string ticketId)
    {
        tickets.RemoveAll(t => t.TicketId == ticketId);
        await _redisService.SetCacheAsync("ActiveSupportTickets", tickets, TimeSpan.FromDays(7));
    }

    private async Task<int> InitiateSupportChatAsync(int senderId, int acceptorId)
    {
        var createDto = new CreateChatDto
        {
            TargetAccountId = acceptorId,
            ContextType = "system"
        };
        return await GetOrCreateChatAsync(senderId, createDto);
    }

    private async Task SaveInitialSupportMessageAsync(int chatId, SupportTicketDto ticket)
    {
        var msgDto = new SendMessageDto
        {
            ChatId = chatId,
            Content = ticket.InitialMessage
        };
        await SaveMessageAsync(ticket.SenderId, msgDto);
    }

    private async Task<List<SupportTicketDto>> CleanupExpiredTicketsAsync(List<SupportTicketDto> tickets)
    {
        var originalCount = tickets.Count;
        tickets.RemoveAll(t => (DateTime.UtcNow - t.RequestedAt).TotalHours > 24);
        if (tickets.Count < originalCount)
        {
            await _redisService.SetCacheAsync("ActiveSupportTickets", tickets, TimeSpan.FromDays(7));
        }
        return tickets;
    }

    private List<SupportTicketDto> FilterTicketsByRole(List<SupportTicketDto> tickets, int accountId, string currentRole)
    {
        var targetRole = currentRole.ToLower();
        if (targetRole == "admin")
            return tickets.Where(t => t.TargetRole == "admin").ToList();
            
        if (targetRole == "staff")
            return tickets.Where(t => t.TargetRole == "staff").ToList();
            
        return tickets.Where(t => t.SenderId == accountId).ToList();
    }
}
