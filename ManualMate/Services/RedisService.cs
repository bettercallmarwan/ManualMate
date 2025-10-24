using StackExchange.Redis;
using System.Text.Json;

namespace ManualMate.Services
{
    public class RedisService
    {
        private readonly IDatabase _redis;

        public RedisService(IConnectionMultiplexer redis, IConfiguration configuration)
        {
            _redis = redis.GetDatabase(1);
        }

        public async Task<T?> GetAsync<T>(string cacheKey)
        {
            var cached = await _redis.StringGetAsync(cacheKey);

            if (cached.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(cached!);
        }

        public async Task SetAsync<T>(string cacheKey, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _redis.StringSetAsync(cacheKey, json, expiry);
        }

        public async Task DeleteAsync(string cacheKey)
        {
            await _redis.KeyDeleteAsync(cacheKey);
        }
    }
}
