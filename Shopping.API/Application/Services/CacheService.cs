﻿using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Diagnostics;
using Shopping.API.Application.Services.Interfaces;

namespace Shopping.API.Application.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly RedisCacheOptions _options;
        private readonly ILogger<CacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly SemaphoreSlim _cacheLock = new(1, 1);
        private readonly TimeSpan _defaultLockTimeout = TimeSpan.FromSeconds(5);

        public CacheService(
            IDistributedCache cache,
            IOptions<RedisCacheOptions> options,
            ILogger<CacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };
        }

        public async Task CacheCartTotalAsync(int cartId, decimal total)
        {
            var cacheKey = GetCacheKey(cartId);
            using var activity = DiagnosticService.StartActivity("CacheCartTotal");

            try
            {
                if (!await _cacheLock.WaitAsync(_defaultLockTimeout))
                {
                    throw new CacheException("Cache operation timeout");
                }

                var serializedValue = JsonSerializer.Serialize(total, _jsonOptions);

                await _cache.SetStringAsync(
                    cacheKey,
                    serializedValue,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _options.CartTotalExpiration,
                        SlidingExpiration = _options.SlidingExpiration
                    });

                activity?.SetTag("cartId", cartId);
                _logger.LogDebug("Successfully cached total for cart {CartId}", cartId);
            }
            catch (Exception ex) when (ex is not CacheException)
            {
                _logger.LogError(ex, "Failed to cache total for cart {CartId}", cartId);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw new CacheException("Cache operation failed", ex);
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        public async Task<decimal?> GetCachedCartTotalAsync(int cartId)
        {
            var cacheKey = GetCacheKey(cartId);
            using var activity = DiagnosticService.StartActivity("GetCachedCartTotal");

            try
            {
                var cachedValue = await _cache.GetStringAsync(cacheKey);
                if (string.IsNullOrEmpty(cachedValue))
                {
                    activity?.SetTag("cache.hit", false);
                    return null;
                }

                try
                {
                    var result = JsonSerializer.Deserialize<decimal>(cachedValue, _jsonOptions);
                    activity?.SetTag("cache.hit", true);
                    return result;
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Invalid cache data for cart {CartId}", cartId);
                    await SafeRemoveAsync(cacheKey);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cached total for cart {CartId}", cartId);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return null;
            }
        }

        public async Task InvalidateCartTotalCacheAsync(int cartId)
        {
            var cacheKey = GetCacheKey(cartId);
            using var activity = DiagnosticService.StartActivity("InvalidateCartCache");

            try
            {
                await SafeRemoveAsync(cacheKey);
                activity?.SetTag("cartId", cartId);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw new CacheException("Cache invalidation failed", ex);
            }
        }

        private async Task SafeRemoveAsync(string key)
        {
            try
            {
                if (!await _cacheLock.WaitAsync(_defaultLockTimeout))
                {
                    throw new CacheException("Cache operation timeout");
                }

                await _cache.RemoveAsync(key);
                _logger.LogDebug("Removed cache entry for key {CacheKey}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache entry for key {CacheKey}", key);
                throw;
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        private static string GetCacheKey(int cartId) => $"cart:{cartId}:total:v2";
    }

    public class RedisCacheOptions
    {
        public TimeSpan CartTotalExpiration { get; set; } = TimeSpan.FromMinutes(30);
        public TimeSpan? SlidingExpiration { get; set; } = TimeSpan.FromMinutes(10);
        public int RetryCount { get; set; } = 3;
    }

    public static class DiagnosticService
    {
        public static Activity? StartActivity(string name)
        {
            return new ActivitySource("Shopping.API.Cache").StartActivity(name);
        }
    }
}