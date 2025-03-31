using Shopping.API.Domain.Models;

namespace Shopping.API.Application.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Order> ProcessPayment(int CartId);
    }
}
