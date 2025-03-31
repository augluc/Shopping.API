using Microsoft.AspNetCore.Mvc;
using Shopping.API.Application.Services.Interfaces;
using Shopping.API.Domain.Models;
using Shopping.API.Domain.Models.Request;

namespace Shopping.API.Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [HttpGet("{cartId}", Name = "GetCartById")]
        [ProducesResponseType(typeof(Cart), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCart(int cartId)
        {
            var cart = await _cartService.GetCartByIdAsync(cartId);
            if (cart == null)
            {
                _logger.LogWarning("Cart not found - ID: {CartId}", cartId);
                return NotFound("Cart not found");
            }
            return Ok(cart);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Cart), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCart([FromBody] CartRequest request)
        {
            var cart = await _cartService.CreateCartAsync(request);
            return Created("api/cart", cart);
        }

        [HttpDelete("{cartId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCart(int cartId)
        {
            await _cartService.DeleteCartAsync(cartId);
            return NoContent();
        }

        [HttpPost("{cartId}/products")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddProduct(int cartId, [FromBody] Product product)
        {
            var createdProduct = await _cartService.AddProductToCartAsync(cartId, product);
            return Created($"api/{cartId}/products", createdProduct);
        }

        [HttpGet("products/{productId}", Name = "GetProduct")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProduct(int productId)
        {
            var product = await _cartService.GetProductByIdAsync(productId);
            return Ok(product);
        }

        [HttpPut("products/{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] Product product)
        {
            await _cartService.UpdateProductInCartAsync(productId, product);
            return NoContent();
        }

        [HttpDelete("products/{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveProduct(int productId)
        {
            await _cartService.RemoveProductFromCartAsync(productId);
            return NoContent();
        }

        [HttpPut("{cartId}/discount")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProduct(int cartId, decimal discountPercentage)
        {
            await _cartService.ApplyDiscountAsync(cartId, discountPercentage);
            return NoContent();
        }
    }
}
