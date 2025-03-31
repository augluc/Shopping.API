using Shopping.API.Application.Services.Interfaces;
using Shopping.API.Domain.Models;
using Shopping.API.Domain.Models.Request;
using Shopping.API.Domain.Models.Response;
using Shopping.API.Infrastructure.Repositories.Interfaces;

namespace Shopping.API.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly ICartService _cartService;
        private readonly ICacheService _cacheService;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            HttpClient httpClient,
            ICartService cartService,
            ICacheService cacheService,
            IOrderRepository orderRepository,
            ILogger<PaymentService> logger)
        {
            _httpClient = httpClient;
            _cartService = cartService;
            _cacheService = cacheService;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<Order> ProcessPayment(int cartId)
        {
            if (cartId <= 0)
                throw new ArgumentException("Invalid cart ID");

            var cart = await _cartService.GetCartByIdAsync(cartId);

            if (cart == null || !cart.Products.Any())
                throw new ArgumentException("Cart is empty or doesn't exist");

            if (string.IsNullOrWhiteSpace(cart.PayerDocument))
                throw new ArgumentException("Payer document is required");

            var order = await _orderRepository.GetByCartIdAsync(cartId);

            if (order != null && order.PaymentStatus == "INLINE")
                throw new ArgumentException("Payment already processed");

            var paymentRequest = new PaymentRequest
            {
                CartId = cartId,
                Amount = cart.Amount,
                PayerDocument = cart.PayerDocument
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "payments",
                    new
                    {
                        cart_id = paymentRequest.CartId,
                        amount = paymentRequest.Amount,
                        payer_document = paymentRequest.PayerDocument
                    });

                response.EnsureSuccessStatusCode();

                var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>() ??
                    throw new InvalidOperationException("Invalid payment response");

                return await _orderRepository.CreateAsync(
                    cartId,
                    paymentResponse.PaymentId,
                    paymentResponse.Status,
                    paymentResponse.CreatedAt);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Payment API error for cart {CartId}", cartId);
                throw new ApplicationException("Payment service unavailable", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing payment for cart {CartId}", cartId);
                throw;
            }
        }
    }
}