using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices;

public interface IRedisService
{
    Task SetUserOnlineAsync(int accountId, string connectionId);
    Task SetUserOfflineAsync(int accountId, string connectionId);
    Task<bool> IsUserOnlineAsync(int accountId);
    Task IncrementUnreadCountAsync(int accountId, int chatId);
    Task ClearUnreadCountAsync(int accountId, int chatId);
    Task<int> GetUnreadCountAsync(int accountId, int chatId);
}
