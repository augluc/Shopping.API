namespace Shopping.API.Controllers
{
    using global::Shopping.API.Models.Request;
    using global::Shopping.API.Repositories.Interfaces;
    using global::Shopping.API.Services.Interfaces;
    // PaymentController.cs
    using Microsoft.AspNetCore.Mvc;
    using Shopping.API.Models.Request;
    using Shopping.API.Models.Response;
    using Shopping.API.Services;

    namespace Shopping.API.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class PaymentController : ControllerBase
        {
            private readonly IPaymentService _paymentService;
            private readonly IOrderRepository _orderRepository;
            private readonly ILogger<PaymentController> _logger;

            public PaymentController(
                IPaymentService paymentService,
                IShoppingCartService shoppingCartService,
                IOrderRepository orderRepository,
                ILogger<PaymentController> logger)
            {
                _paymentService = paymentService;
                _shoppingCartService = shoppingCartService;
                _orderRepository = orderRepository;
                _logger = logger;
            }

            [HttpPost]
            public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
            {
                try
                {
                    // Validate cart exists and has items
                    var cart = await _shoppingCartService.GetCartAsync(request.CartId);
                    if (cart == null || !cart.Products.Any())
                    {
                        return BadRequest("Cart is empty or doesn't exist");
                    }

                    // Verify the amount matches the cart total
                    var cartTotal = cart.Ammount ?? 0;
                    if (cartTotal != request.Amount)
                    {
                        return BadRequest($"Payment amount {request.Amount} doesn't match cart total {cartTotal}");
                    }

                    // Create or get existing order
                    var order = await _orderRepository.GetByCartIdAsync(request.CartId) ??
                        await _orderRepository.CreateAsync(request.CartId);

                    // Process payment
                    var paymentResponse = await _paymentService.ProcessPayment(request);

                    // Update order with payment information
                    order = await _orderRepository.UpdatePaymentStatusAsync(
                        order.OrderId,
                        paymentResponse.PaymentId,
                        paymentResponse.Status);

                    return Ok(new
                    {
                        paymentResponse.PaymentId,
                        paymentResponse.Status,
                        paymentResponse.CreatedAt,
                        OrderId = order.OrderId
                    });
                }
                catch (ApplicationException ex)
                {
                    _logger.LogError(ex, "Payment processing error");
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during payment processing");
                    return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your payment");
                }
            }
        }
    }
}
