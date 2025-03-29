namespace Shopping.API.Services.Interfaces
{
    public interface ICacheService
    {
        Task CacheCartTotalAsync(int cartId, decimal total);
        Task<decimal?> GetCachedCartTotalAsync(int cartId);
        Task InvalidateCartTotalCacheAsync(int cartId);
    }
}
