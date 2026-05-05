using CourseMarketplaceBE.Application.IServices;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.Services;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task SetUserOnlineAsync(int accountId, string connectionId)
    {
        await _db.HashSetAsync($"user:{accountId}:online", connectionId, DateTime.Now.ToString());
    }

    public async Task SetUserOfflineAsync(int accountId, string connectionId)
    {
        await _db.HashDeleteAsync($"user:{accountId}:online", connectionId);
    }

    public async Task<bool> IsUserOnlineAsync(int accountId)
    {
        return await _db.KeyExistsAsync($"user:{accountId}:online");
    }

    public async Task IncrementUnreadCountAsync(int accountId, int chatId)
    {
        await _db.HashIncrementAsync($"user:{accountId}:unread", chatId.ToString());
    }

    public async Task ClearUnreadCountAsync(int accountId, int chatId)
    {
        await _db.HashDeleteAsync($"user:{accountId}:unread", chatId.ToString());
    }

    public async Task<int> GetUnreadCountAsync(int accountId, int chatId)
    {
        var count = await _db.HashGetAsync($"user:{accountId}:unread", chatId.ToString());
        return count.HasValue ? (int)count : 0;
    }
}
