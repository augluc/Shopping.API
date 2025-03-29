using Shopping.API.Models.Request;
using Shopping.API.Models.Response;

namespace Shopping.API.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessPayment(PaymentRequest request);
    }
}
