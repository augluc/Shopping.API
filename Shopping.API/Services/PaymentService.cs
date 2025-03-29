using Shopping.API.Models;
using Shopping.API.Models.Request;
using Shopping.API.Models.Response;
using Shopping.API.Repositories.Interfaces;
using Shopping.API.Services.Interfaces;

namespace Shopping.API.Services
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
            {
                throw new ArgumentException("Invalid cart ID");
            }

            var cart = await _cartService.GetCartByIdAsync(cartId);
            if (cart == null || !cart.Products.Any())
            {
                throw new ArgumentException("Cart is empty or doesn't exist");
            }

            var cartTotal = await _cacheService.GetCachedCartTotalAsync(cartId) ??
                          cart.Products.Sum(p => p.Price * p.Quantity);

            var order = await _orderRepository.GetByCartIdAsync(cartId) ??
                       await _orderRepository.CreateAsync(cartId);

            if (order.PaymentStatus == "INLINE")
            {
                throw new ArgumentException("Payment already processed");
            }

            var paymentRequest = new PaymentRequest
            {
                CartId = cartId,
                Amount = cartTotal,
                PayerDocument = cart.PayerDocument
            };

            if (string.IsNullOrWhiteSpace(paymentRequest.PayerDocument))
            {
                throw new ArgumentException("Payer document is required");
            }

            PaymentResponse paymentResponse;
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "/payments",
                    new
                    {
                        cart_id = paymentRequest.CartId,
                        amount = paymentRequest.Amount,
                        payer_document = paymentRequest.PayerDocument
                    });

                response.EnsureSuccessStatusCode();
                paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>() ??
                    throw new InvalidOperationException("Invalid payment response");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Payment API error for cart {CartId}", cartId);
                throw new ApplicationException("Payment service unavailable", ex);
            }

            return await _orderRepository.UpdatePaymentStatusAsync(
                order.OrderId,
                paymentResponse.PaymentId,
                paymentResponse.Status);
        }
    }
}