using CourseMarketplaceBE.Application.IServices;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Infrastructure.Services
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = redis.GetDatabase();
        }

        public Task<bool> IsHealthyAsync()
        {
            return Task.FromResult(_redis.IsConnected);
        }

        public async Task SetUserOnlineAsync(int accountId, string connectionId)
        {
            if (!_redis.IsConnected) return;
            try { await _db.HashSetAsync($"user:{accountId}:online", connectionId, DateTime.Now.ToString()); } catch { }
        }

        public async Task SetUserOfflineAsync(int accountId, string connectionId)
        {
            if (!_redis.IsConnected) return;
            try { await _db.HashDeleteAsync($"user:{accountId}:online", connectionId); } catch { }
        }

        public async Task<bool> IsUserOnlineAsync(int accountId)
        {
            if (!_redis.IsConnected) return false;
            try { return await _db.KeyExistsAsync($"user:{accountId}:online"); } catch { return false; }
        }

        public async Task IncrementUnreadCountAsync(int accountId, int chatId)
        {
            if (!_redis.IsConnected) return;
            try { await _db.HashIncrementAsync($"user:{accountId}:unread", chatId.ToString()); } catch { }
        }

        public async Task ClearUnreadCountAsync(int accountId, int chatId)
        {
            if (!_redis.IsConnected) return;
            try { await _db.HashDeleteAsync($"user:{accountId}:unread", chatId.ToString()); } catch { }
        }

        public async Task<int> GetUnreadCountAsync(int accountId, int chatId)
        {
            if (!_redis.IsConnected) return 0;
            try
            {
                var count = await _db.HashGetAsync($"user:{accountId}:unread", chatId.ToString());
                return count.HasValue ? (int)count : 0;
            }
            catch { return 0; }
        }

        public async Task SetCacheAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (!_redis.IsConnected) return;
            try
            {
                var json = JsonSerializer.Serialize(value);
                if (expiry.HasValue)
                    await _db.StringSetAsync(key, json, expiry.Value);
                else
                    await _db.StringSetAsync(key, json);
            }
            catch { }
        }

        public async Task<T?> GetCacheAsync<T>(string key)
        {
            if (!_redis.IsConnected) return default;
            try
            {
                var json = await _db.StringGetAsync(key);
                if (json.IsNullOrEmpty) return default;
                return JsonSerializer.Deserialize<T>(json!);
            }
            catch { return default; }
        }

        public async Task RemoveCacheAsync(string key)
        {
            if (!_redis.IsConnected) return;
            try { await _db.KeyDeleteAsync(key); } catch { }
        }
    }
}
