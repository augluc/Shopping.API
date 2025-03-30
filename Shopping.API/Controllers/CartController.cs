using Microsoft.AspNetCore.Mvc;
using Shopping.API.Models;
using Shopping.API.Models.Request;
using Shopping.API.Services.Interfaces;

namespace Shopping.API.Controllers
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
            try
            {
                var cart = await _cartService.GetCartByIdAsync(cartId);
                if (cart == null)
                {
                    _logger.LogWarning("Cart not found - ID: {CartId}", cartId);
                    return NotFound("Cart not found");
                }
                return Ok(cart);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request - ID: {CartId}", cartId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart - ID: {CartId}", cartId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Cart), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCart([FromBody] CartRequest request)
        {
            try
            {
                var cart = await _cartService.CreateCartAsync(request);
                return Created("api/cart", cart);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid cart creation request");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cart");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the cart");
            }
        }

        [HttpDelete("{cartId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCart(int cartId)
        {
            try
            {
                await _cartService.DeleteCartAsync(cartId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Cart not found for deletion - ID: {CartId}", cartId);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid cart deletion request - ID: {CartId}", cartId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cart - ID: {CartId}", cartId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the cart");
            }
        }

        [HttpPost("{cartId}/products")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddProduct(int cartId, [FromBody] Product product)
        {
            try
            {
                var createdProduct = await _cartService.AddProductToCartAsync(cartId, product);
                return Created($"api/{cartId}/products", createdProduct);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Cart not found for product addition - ID: {CartId}", cartId);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid product addition request - Cart ID: {CartId}", cartId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product to cart - Cart ID: {CartId}", cartId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the product");
            }
        }

        [HttpGet("products/{productId}", Name = "GetProduct")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProduct(int productId)
        {
            try
            {
                var product = await _cartService.GetProductByIdAsync(productId);
                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product not found - ID: {ProductId}", productId);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid product request - ID: {ProductId}", productId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product - ID: {ProductId}", productId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the product");
            }
        }

        [HttpPut("products/{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] Product product)
        {
            try
            {
                await _cartService.UpdateProductInCartAsync(productId, product);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product not found for update - ID: {ProductId}", productId);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid product update request - ID: {ProductId}", productId);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Product update conflict - ID: {ProductId}", productId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product - ID: {ProductId}", productId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the product");
            }
        }

        [HttpDelete("products/{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveProduct(int productId)
        {
            try
            {
                await _cartService.RemoveProductFromCartAsync(productId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product not found for removal - ID: {ProductId}", productId);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid product removal request - ID: {ProductId}", productId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing product - ID: {ProductId}", productId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing the product");
            }
        }

        [HttpPut("{cartId}/discount")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProduct(int cartId, decimal discountPercentage)
        {
            try
            {
                await _cartService.ApplyDiscountAsync(cartId, discountPercentage);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Cart not found for update - ID: {CartId}", cartId);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid cart update request - ID: {CartId}", cartId);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cart update conflict - ID: {CartId}", cartId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart - ID: {CartId}", cartId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the cart");
            }
        }
    }
}