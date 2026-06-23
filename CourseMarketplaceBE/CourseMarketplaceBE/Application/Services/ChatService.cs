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

        var result = new List<ChatListDto>();
        foreach (var p in participants)
        {
            // Bỏ qua nếu tin nhắn cuối cùng nhỏ hơn hoặc bằng ClearedAt (đã xóa)
            if (p.ClearedAt.HasValue && p.Chat.LastMessageAt.HasValue && p.Chat.LastMessageAt.Value <= p.ClearedAt.Value)
            {
                continue;
            }

            // Bỏ qua các chat chưa có tin nhắn nào để tránh hiện rỗng cho người nhận
            if (!p.Chat.Messages.Any())
            {
                continue;
            }

            var unreadFromRedis = await _redisService.GetUnreadCountAsync(accountId, p.ChatId);
            
                var partner = p.Chat.ChatParticipants.FirstOrDefault(cp => cp.AccountId != accountId);
                var partnerId = partner?.AccountId;
                var isOnline = partnerId.HasValue ? await _redisService.IsUserOnlineAsync(partnerId.Value) : false;

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
                    PartnerName = partner?.Account.User?.FullName 
                        ?? partner?.Account.Manager?.DisplayName 
                        ?? partner?.Account.Username 
                        ?? partner?.Account.Email 
                        ?? "Support Team",
                    PartnerAvatar = partner?.Account.AvatarUrl,
                    PartnerId = partnerId,
                    IsOnline = isOnline
                });
        }
        return result;
    }

    public async Task<List<ChatListDto>> SearchChatsAsync(int accountId, string query)
    {
        var participants = await _chatRepository.SearchParticipantsByAccountIdAsync(accountId, query);

        var result = new List<ChatListDto>();
        foreach (var p in participants)
        {
            // Bỏ qua nếu tin nhắn cuối cùng nhỏ hơn hoặc bằng ClearedAt (đã xóa)
            if (p.ClearedAt.HasValue && p.Chat.LastMessageAt.HasValue && p.Chat.LastMessageAt.Value <= p.ClearedAt.Value)
            {
                continue;
            }

            // Bỏ qua các chat chưa có tin nhắn nào
            if (!p.Chat.Messages.Any())
            {
                continue;
            }

            var unreadFromRedis = await _redisService.GetUnreadCountAsync(accountId, p.ChatId);
            
            var partner = p.Chat.ChatParticipants.FirstOrDefault(cp => cp.AccountId != accountId);
            var partnerId = partner?.AccountId;
            var isOnline = partnerId.HasValue ? await _redisService.IsUserOnlineAsync(partnerId.Value) : false;

            result.Add(new ChatListDto
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
            });
        }
        return result;
    }

    public async Task<List<MessageDto>> GetChatHistoryAsync(int chatId, int accountId)
    {
        if (!await HasAccessToChatAsync(accountId, chatId))
            throw new UnauthorizedAccessException("You do not have permission to view this chat.");

        // Đánh dấu đã xem
        await MarkChatAsReadAsync(chatId, accountId);

        var participant = await _chatRepository.GetParticipantAsync(chatId, accountId);
        
        var messages = await _chatRepository.GetMessagesByChatIdAsync(chatId);

        if (participant?.ClearedAt != null)
        {
            messages = messages.Where(m => m.SentAt > participant.ClearedAt.Value).ToList();
        }

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
            SenderAvatar = m.Sender?.AvatarUrl,
            Attachments = m.Attachments.Select(a => new AttachmentDto
            {
                AttachmentId = a.AttachmentId,
                FileUrl = a.FileUrl,
                FileName = a.FileName,
                FileType = a.FileType,
                FileSize = a.FileSize
            }).ToList()
        }).ToList();
    }

    public async Task<MessageDto> SaveMessageAsync(int senderId, SendMessageDto dto)
    {
        var isParticipant = await _chatRepository.IsParticipantAsync(dto.ChatId, senderId);
        
        if (!isParticipant)
        {
            var hasActiveReport = await _chatRepository.HasActiveReportForChatAsync(dto.ChatId);
            if (!hasActiveReport)
                throw new UnauthorizedAccessException("You do not have permission to send messages in this chat.");
        }

        var message = new Message
        {
            ChatId = dto.ChatId,
            SenderId = senderId,
            Content = dto.Content,
            SentAt = DateTime.Now,
            MessageStatus = "ok"
        };

        // DLC Attachment Logic
        if (_configuration.GetValue<bool>("ChatSettings:EnableAttachments") && dto.Attachments != null && dto.Attachments.Any())
        {
            message.Attachments = dto.Attachments.Select(a => new MessageAttachment
            {
                FileUrl = a.FileUrl,
                FileName = a.FileName,
                FileType = a.FileType,
                FileSize = a.FileSize,
                CreatedAt = DateTime.Now
            }).ToList();
        }

        await _chatRepository.AddMessageAsync(message);

        // Cập nhật Redis cho các người nhận
        var chat = await _chatRepository.GetChatByIdAsync(dto.ChatId);
        if (chat != null)
        {
            chat.LastMessageAt = message.SentAt;
            await _chatRepository.UpdateChatAsync(chat);

            // Tăng unread count trong Redis cho các người nhận
            var participantIds = await _chatRepository.GetParticipantIdsAsync(dto.ChatId);
            foreach (var accountId in participantIds)
            {
                if (accountId != senderId)
                {
                    await _redisService.IncrementUnreadCountAsync(accountId, dto.ChatId);
                }
            }
        }

        int rows = await _chatRepository.SaveChangesAsync();
        if (rows <= 0)
            throw new InvalidOperationException("Failed to save message");

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
            SenderAvatar = sender?.AvatarUrl,
            Attachments = message.Attachments.Select(a => new AttachmentDto
            {
                AttachmentId = a.AttachmentId,
                FileUrl = a.FileUrl,
                FileName = a.FileName,
                FileType = a.FileType,
                FileSize = a.FileSize
            }).ToList()
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
            throw new UnauthorizedAccessException("You do not have permission to initiate this conversation.");

        // 2. Tìm hoặc tạo mới
        var existingParticipant = await _chatRepository.FindPrivateChatAsync(senderId, dto.TargetAccountId, dto.ContextType, dto.ContextId);
        if (existingParticipant != null) return existingParticipant.ChatId;

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

        int rows = await _chatRepository.SaveChangesAsync();
        if (rows <= 0)
            throw new InvalidOperationException("Failed to create chat");

        return createdChat.ChatId;
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
        int rows = await _chatRepository.SaveChangesAsync();
        if (rows <= 0)
            throw new InvalidOperationException("Failed to grant admin access");
        return true;
    }

    public async Task<bool> ClearChatHistoryAsync(int chatId, int accountId)
    {
        var participant = await _chatRepository.GetParticipantAsync(chatId, accountId);
        if (participant == null) return false;

        participant.ClearedAt = DateTime.UtcNow;
        participant.UnreadCount = 0;
        await _chatRepository.UpdateParticipantAsync(participant);
        int rows = await _chatRepository.SaveChangesAsync();
        if (rows <= 0)
            throw new InvalidOperationException("Failed to clear chat history");
            
        await _redisService.ClearUnreadCountAsync(accountId, chatId);
        return true;
    }

    public async Task<bool> MarkChatAsReadAsync(int chatId, int accountId)
    {
        await _chatRepository.MarkAsReadAsync(chatId, accountId);
        int rows = await _chatRepository.SaveChangesAsync();
        if (rows <= 0)
            throw new InvalidOperationException("Failed to mark chat as read");
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
        int rows = await _chatRepository.SaveChangesAsync();
        if (rows <= 0)
            throw new InvalidOperationException("Failed to submit report");
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
        int rows = await _chatRepository.SaveChangesAsync();
        if (rows <= 0)
            throw new InvalidOperationException("Failed to log action");
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

        var ticketId = Guid.NewGuid().ToString("N");
        var ticket = new SupportTicketDto
        {
            TicketId = ticketId,
            SenderId = senderId,
            SenderName = sender.User?.FullName ?? sender.Manager?.DisplayName ?? sender.Username ?? sender.Email ?? "Unknown",
            SenderAvatar = sender.AvatarUrl,
            InitialMessage = dto.Content,
            RequestedAt = DateTime.UtcNow,
            TargetRole = dto.TargetRole
        };

        // Manage a global list of active tickets
        var tickets = await _redisService.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets") ?? new List<SupportTicketDto>();
        tickets.Add(ticket);
        await _redisService.SetCacheAsync("ActiveSupportTickets", tickets, TimeSpan.FromDays(7));
        
        return ticket;
    }

    public async Task<int> AcceptSupportRequestAsync(int acceptorId, string ticketId)
    {
        var tickets = await _redisService.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets") ?? new List<SupportTicketDto>();
        var ticket = tickets.FirstOrDefault(t => t.TicketId == ticketId);
        
        if (ticket == null) throw new InvalidOperationException("This support request has already been accepted or expired.");

        // Check acceptor role
        var role = await _userRepository.GetRoleByAccountIdAsync(acceptorId);
        if (ticket.TargetRole == "admin" && role != "admin")
            throw new UnauthorizedAccessException("Only admins can accept this request.");
        if (ticket.TargetRole == "staff" && role != "staff" && role != "admin")
            throw new UnauthorizedAccessException("Only staff or admins can accept this request.");

        // Remove the ticket from the list
        tickets.RemoveAll(t => t.TicketId == ticketId);
        await _redisService.SetCacheAsync("ActiveSupportTickets", tickets, TimeSpan.FromDays(7));

        // Create Chat using GetOrCreateChatAsync
        var createDto = new CreateChatDto
        {
            TargetAccountId = acceptorId,
            ContextType = "system"
        };
        var chatId = await GetOrCreateChatAsync(ticket.SenderId, createDto);

        // Save the initial message from the sender
        var msgDto = new SendMessageDto
        {
            ChatId = chatId,
            Content = ticket.InitialMessage
        };
        await SaveMessageAsync(ticket.SenderId, msgDto);

        return chatId;
    }

    public async Task<List<SupportTicketDto>> GetPendingRequestsAsync(int accountId, string currentRole)
    {
        var tickets = await _redisService.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets") ?? new List<SupportTicketDto>();
        var targetRole = currentRole.ToLower();
        
        // Remove old tickets (> 24 hours) as a cleanup measure
        var originalCount = tickets.Count;
        tickets.RemoveAll(t => (DateTime.UtcNow - t.RequestedAt).TotalHours > 24);
        if (tickets.Count < originalCount)
        {
            await _redisService.SetCacheAsync("ActiveSupportTickets", tickets, TimeSpan.FromDays(7));
        }

        if (targetRole == "admin")
            return tickets.Where(t => t.TargetRole == "admin").ToList();
            
        if (targetRole == "staff")
            return tickets.Where(t => t.TargetRole == "staff").ToList();
            
        // For User/Instructor, show only their own requests
        return tickets.Where(t => t.SenderId == accountId).ToList();
    }
}
