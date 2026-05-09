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

    // Generic Object Caching
    Task SetCacheAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<T?> GetCacheAsync<T>(string key);
    Task RemoveCacheAsync(string key);
}
