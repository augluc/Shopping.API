using Shopping.API.Models;

namespace Shopping.API.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Order> ProcessPayment(int CartId);
    }
}
