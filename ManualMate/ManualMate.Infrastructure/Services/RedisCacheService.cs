using System.Text.Json;
using ManualMate.Application.Interfaces.Services;
using StackExchange.Redis;

namespace ManualMate.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _redis;
        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase(1);
        }
        public async Task<T?> GetAsync<T>(string key)
        {
            var cached = await _redis.StringGetAsync(key);

            if (cached.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>((string)cached!);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _redis.StringSetAsync(key, json, expiration.HasValue ? (Expiration)expiration.Value : default(Expiration));
        }

        public async Task RemoveAsync<T>(string key)
        {
            await _redis.KeyDeleteAsync(key);
        }

    }
}
