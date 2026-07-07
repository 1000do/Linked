using CourseMarketplaceBE.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface IChatRepository
{
    Task<List<ChatParticipant>> GetParticipantsByAccountIdAsync(int accountId);
    Task<List<ChatParticipant>> SearchParticipantsByAccountIdAsync(int accountId, string query);
    Task<List<Message>> GetMessagesByChatIdAsync(int chatId);
    Task AddMessageAsync(Message message);
    Task<Chat?> GetChatByIdAsync(int chatId);
    Task UpdateChatAsync(Chat chat);
    Task<Chat> CreateChatAsync(Chat chat);
    Task AddChatParticipantsAsync(IEnumerable<ChatParticipant> participants);
    Task<ChatParticipant?> GetParticipantAsync(int chatId, int accountId);
    Task UpdateParticipantAsync(ChatParticipant participant);
    Task<ChatParticipant?> FindPrivateChatAsync(int accountId1, int accountId2, string? contextType, int? contextId);
    Task<bool> IsParticipantAsync(int chatId, int accountId);
    Task<bool> HasActiveReportForChatAsync(int chatId);
    Task UpdateAdminAccessAsync(int chatId, DateTime expiry);
    Task AddReportAsync(UserReport report);
    Task AddAuditLogAsync(AuditLog log);
    Task MarkAsReadAsync(int chatId, int accountId);
    Task<int> GetTotalUnreadCountAsync(int accountId);
    Task<List<int>> GetParticipantIdsAsync(int chatId);
    Task<(List<UserReport> Items, int TotalCount)> GetAllReportsAsync(int page = 1, int pageSize = 10);
    Task<UserReport?> GetReportByIdAsync(int reportId);
    Task<Message?> GetMessageByIdAsync(int messageId);
    Task<int> SaveChangesAsync();
    Task<int> GetActiveChatsCountAsync(int accountId);
}
