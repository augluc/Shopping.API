using Microsoft.AspNetCore.Mvc;
using Shopping.API.Models;
using Shopping.API.Models.Request;
using Shopping.API.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace Shopping.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _repository;

        public CartController(ICartRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Invalid cart ID.");

                var cart = await _repository.GetByIdAsync(id);
                return cart == null ? NotFound($"Cart with ID {id} not found.") : Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ShoppingCartRequest shoppingCartRequest)
        {
            try
            {
                if (shoppingCartRequest == null || string.IsNullOrWhiteSpace(shoppingCartRequest.PayerDocument))
                    return BadRequest("ShoppingCartRequest cannot be null and PayerDocument is required.");

                var newCart = await _repository.CreateAsync(shoppingCartRequest);
                return Created($"/api/cart/{newCart.CartId}", newCart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Invalid cart ID.");

                var existingCart = await _repository.GetByIdAsync(id);
                if (existingCart == null)
                    return NotFound($"Cart with ID {id} not found.");

                await _repository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("prodcts/{id}")]
        public async Task<IActionResult> GetProductByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Invalid product ID.");

                var product = await _repository.GetProductByIdAsync(id);
                return product == null ? NotFound($"Product with ID {id} not found.") : Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{cartId}/products")]
        public async Task<IActionResult> AddItemAsync(int cartId, [FromBody] Product product)
        {
            try
            {
                if (cartId <= 0 || product == null)
                    return BadRequest("Invalid cart ID or product cannot be null.");

                product.CartId = cartId;
                await _repository.AddProductAsync(product);

                return Created($"api/cart/{cartId}/products" ,product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("products/{productId}")]
        public async Task<IActionResult> UpdateItemAsync(int productId, [FromBody] Product product)
        {
            try
            {
                if (product == null || productId <= 0 || product.CartId <= 0)
                    return BadRequest("Invalid product ID, cart ID, or product cannot be null.");

                var existingProduct = await _repository.GetProductByIdAsync(productId);
                if (existingProduct == null)
                    return NotFound($"Product with ID {productId} not found.");

                if (existingProduct.CartId != product.CartId)
                    return BadRequest("Product does not belong to the specified cart.");

                await _repository.UpdateProductAsync(product);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("products/{productId}")]
        public async Task<IActionResult> RemoveItemAsync(int productId)
        {
            try
            {
                if (productId <= 0)
                    return BadRequest("Invalid product ID.");

                var existingProduct = await _repository.GetProductByIdAsync(productId);
                if (existingProduct == null)
                    return NotFound($"Product with ID {productId} not found.");

                await _repository.RemoveProductAsync(productId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
