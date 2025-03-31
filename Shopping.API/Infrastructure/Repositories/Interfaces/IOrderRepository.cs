using Shopping.API.Domain.Models;

namespace Shopping.API.Infrastructure.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByCartIdAsync(int cartId);
        Task<Order> CreateAsync(int cartId, Guid paymentId, string status, DateTime createdAt);
    }
}
