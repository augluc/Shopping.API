using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Shopping.API.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace Shopping.API.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly CacheOptions _options;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(
            IDistributedCache cache,
            IOptions<CacheOptions> options)
        {
            _cache = cache;
            _options = options.Value;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task CacheCartTotalAsync(int cartId, decimal total)
        {
            var cacheKey = GetCartTotalCacheKey(cartId);
            var serializedValue = JsonSerializer.Serialize(total, _jsonOptions);
            await _cache.SetStringAsync(
                cacheKey,
                serializedValue,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _options.CartTotalExpiration
                });
        }

        public async Task<decimal?> GetCachedCartTotalAsync(int cartId)
        {
            var cacheKey = GetCartTotalCacheKey(cartId);
            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(cachedValue))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<decimal>(cachedValue, _jsonOptions);
            }
            catch (JsonException)
            {
                await _cache.RemoveAsync(cacheKey);
                return null;
            }
        }

        public async Task InvalidateCartTotalCacheAsync(int cartId)
        {
            var cacheKey = GetCartTotalCacheKey(cartId);
            await _cache.RemoveAsync(cacheKey);
        }

        private static string GetCartTotalCacheKey(int cartId) => $"cart:{cartId}:total";
    }

    public class CacheOptions
    {
        public TimeSpan CartTotalExpiration { get; set; } = TimeSpan.FromMinutes(180);
    }
}