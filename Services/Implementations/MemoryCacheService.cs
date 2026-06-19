#nullable enable
using Microsoft.Extensions.Caching.Memory;
using Warehouse.Services.Interfaces;

namespace Warehouse.Services.Implementations
{
    /// <summary>
    /// In-memory cache implementation using IMemoryCache.
    /// To switch to Redis: implement ICacheService using IDistributedCache
    /// and register the new implementation in ServiceRegistration — no consumer changes needed.
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(5);

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            if (_cache.TryGetValue(key, out T? cached))
                return cached;

            var value = await factory();
            _cache.Set(key, value, expiry ?? DefaultExpiry);
            return value;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            _cache.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            _cache.Set(key, value, expiry ?? DefaultExpiry);
            return Task.CompletedTask;
        }

        public void Remove(string key) => _cache.Remove(key);

        public bool Exists(string key) => _cache.TryGetValue(key, out _);
    }

    /// <summary>Well-known cache key constants used across the application.</summary>
    public static class CacheKeys
    {
        public const string AllCategories = "cache:categories:all";
        public const string AllWarehouses  = "cache:warehouses:all";
        public const string LowStockCount  = "cache:inventory:lowstock:count";
    }
}
