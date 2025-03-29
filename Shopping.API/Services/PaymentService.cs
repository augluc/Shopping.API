using Shopping.API.Models.Request;
using Shopping.API.Models.Response;
using Shopping.API.Services.Interfaces;

namespace Shopping.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(HttpClient httpClient, ILogger<PaymentService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<PaymentResponse> ProcessPayment(PaymentRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "https://rendimentopay.free.beeceptor.com/payments",
                    new
                    {
                        cart_id = request.CartId,
                        amount = request.Amount,
                        payer_document = request.PayerDocument
                    });

                response.EnsureSuccessStatusCode();

                var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();

                if (paymentResponse == null)
                {
                    throw new InvalidOperationException("Failed to deserialize payment response");
                }

                return paymentResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling payment API");
                throw new ApplicationException("Payment service unavailable", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                throw;
            }
        }
    }
}
