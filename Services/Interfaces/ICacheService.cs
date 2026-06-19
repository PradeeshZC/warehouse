#nullable enable
namespace Warehouse.Services.Interfaces
{
    /// <summary>
    /// Generic cache abstraction.
    /// Default implementation uses IMemoryCache.
    /// Structure is Redis-ready — swap the implementation without changing consumers.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>Returns cached value or calls factory to populate and cache it.</summary>
        Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);

        /// <summary>Returns the cached value, or default if not present.</summary>
        Task<T?> GetAsync<T>(string key);

        /// <summary>Sets a value in cache with optional expiry.</summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>Removes a cache entry by key.</summary>
        void Remove(string key);

        /// <summary>Returns true if the key exists in cache.</summary>
        bool Exists(string key);
    }
}
