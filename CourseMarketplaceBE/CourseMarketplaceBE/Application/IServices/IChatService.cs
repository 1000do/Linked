using CourseMarketplaceBE.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices;

public interface IChatService
{
    Task<List<ChatListDto>> GetMyChatsAsync(int accountId);
    Task<List<MessageDto>> GetChatHistoryAsync(int chatId, int accountId);
    Task<MessageDto> SaveMessageAsync(int senderId, SendMessageDto dto);
    Task<int> GetOrCreateChatAsync(int senderId, CreateChatDto dto);
    Task<bool> HasAccessToChatAsync(int accountId, int chatId);
    Task<bool> GrantAdminAccessAsync(int chatId, int hours);
    Task<bool> SubmitReportAsync(int reporterId, int chatId, string reason, string description);
}
