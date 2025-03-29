using Microsoft.AspNetCore.Mvc;
using Shopping.API.Models;
using Shopping.API.Services.Interfaces;

namespace Shopping.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("{cartId}")]
        [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessPayment(int cartId)
        {
            try
            {
                var order = await _paymentService.ProcessPayment(cartId);
                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error for cart {CartId}", cartId);
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Payment processing error - CartId: {CartId}", cartId);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during payment processing - CartId: {CartId}", cartId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your payment");
            }
        }
    }
}