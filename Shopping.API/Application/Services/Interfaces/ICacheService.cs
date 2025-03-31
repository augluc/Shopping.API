namespace Shopping.API.Application.Services.Interfaces
{
    public interface ICacheService
    {
        Task CacheCartTotalAsync(int cartId, decimal total);
        Task<decimal?> GetCachedCartTotalAsync(int cartId);
        Task InvalidateCartTotalCacheAsync(int cartId);
    }
    public class CacheException : Exception
    {
        public CacheException() { }
        public CacheException(string message) : base(message) { }
        public CacheException(string message, Exception inner) : base(message, inner) { }
    }
}