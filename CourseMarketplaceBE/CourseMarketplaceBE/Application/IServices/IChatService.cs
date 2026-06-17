using CourseMarketplaceBE.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices;

public interface IChatService
{
    Task<List<ChatListDto>> GetMyChatsAsync(int accountId);
    Task<List<ChatListDto>> SearchChatsAsync(int accountId, string query);
    Task<List<MessageDto>> GetChatHistoryAsync(int chatId, int accountId);
    Task<MessageDto> SaveMessageAsync(int senderId, SendMessageDto dto);
    Task<int> GetOrCreateChatAsync(int senderId, CreateChatDto dto);
    Task<bool> HasAccessToChatAsync(int accountId, int chatId);
    Task<bool> ClearChatHistoryAsync(int chatId, int accountId);
    Task<bool> MarkChatAsReadAsync(int chatId, int accountId);
    Task<bool> GrantAdminAccessAsync(int chatId, int hours);
    Task<bool> SubmitReportAsync(int reporterId, int chatId, string reason, string description);
    Task<int> GetTotalUnreadCountAsync(int accountId);
    Task LogActionAsync(int actorId, string action, string targetType, int? targetId, string details);
    Task<SupportAccountDto?> GetSupportAccountAsync();

    // Ticket-Based Support Flow
    Task<SupportTicketDto> CreateSupportRequestAsync(int senderId, SupportRequestDto dto);
    Task<int> AcceptSupportRequestAsync(int acceptorId, string ticketId);
    Task<List<SupportTicketDto>> GetPendingRequestsAsync(int accountId, string currentRole);
}
