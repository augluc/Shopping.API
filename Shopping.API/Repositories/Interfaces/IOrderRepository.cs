using Shopping.API.Models;

namespace Shopping.API.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByCartIdAsync(int cartId);
        Task<Order> CreateAsync(int cartId);
        Task<Order> UpdatePaymentStatusAsync(int orderId, Guid paymentId, string status);
    }
}
