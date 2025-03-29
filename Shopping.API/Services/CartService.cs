﻿using Shopping.API.Models.Request;
using Shopping.API.Models;
using Shopping.API.Repositories.Interfaces;
using Shopping.API.Services.Interfaces;

namespace Shopping.API.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CartService> _logger;


        public CartService(
            ICartRepository cartRepository,
            ICacheService cacheService,
            ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ShoppingCart?> GetCartByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid cart ID.");

            return await _cartRepository.GetByIdAsync(id);
        }

        public async Task<ShoppingCart> CreateCartAsync(ShoppingCartRequest shoppingCartRequest)
        {
            if (shoppingCartRequest == null || string.IsNullOrWhiteSpace(shoppingCartRequest.PayerDocument))
                throw new ArgumentException("ShoppingCartRequest cannot be null and PayerDocument is required.");

            return await _cartRepository.CreateAsync(shoppingCartRequest);
        }

        public async Task<bool> DeleteCartAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid cart ID.");

            var existingCart = await _cartRepository.GetByIdAsync(id);
            if (existingCart == null)
                return false;

            return await _cartRepository.DeleteAsync(id);
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid product ID.");

            return await _cartRepository.GetProductByIdAsync(id);
        }

        public async Task<decimal> GetCartTotalAsync(int cartId)
        {
            // Try to get from cache first
            var cachedTotal = await _cacheService.GetCachedCartTotalAsync(cartId);
            if (cachedTotal.HasValue)
            {
                _logger.LogInformation("Retrieved cart {CartId} total from cache", cartId);
                return cachedTotal.Value;
            }

            // Calculate from database if not in cache
            var cart = await _cartRepository.GetByIdAsync(cartId);
            if (cart == null)
            {
                throw new KeyNotFoundException($"Cart with ID {cartId} not found");
            }

            var total = cart.Products.Sum(p => p.Price * p.Quantity);

            // Store in cache
            await _cacheService.CacheCartTotalAsync(cartId, total);
            _logger.LogInformation("Calculated and cached cart {CartId} total", cartId);

            return total;
        }

        public async Task<Product> AddProductToCartAsync(int cartId, Product product)
        {
            if (cartId <= 0 || product == null)
                throw new ArgumentException("Invalid cart ID or product cannot be null.");

            product.CartId = cartId;
            await _cartRepository.AddProductAsync(product);
            await _cacheService.InvalidateCartTotalCacheAsync(cartId);
            return product;
        }

        public async Task<bool> UpdateProductInCartAsync(int productId, Product product)
        {
            if (product == null || productId <= 0 || product.CartId <= 0)
                throw new ArgumentException("Invalid product ID, cart ID, or product cannot be null.");

            var existingProduct = await _cartRepository.GetProductByIdAsync(productId);
            if (existingProduct == null)
                return false;

            if (existingProduct.CartId != product.CartId)
                throw new InvalidOperationException("Product does not belong to the specified cart.");

            await _cacheService.InvalidateCartTotalCacheAsync(product.CartId);
            return await _cartRepository.UpdateProductAsync(product);

        }

        public async Task<bool> RemoveProductFromCartAsync(int productId)
        {
            if (productId <= 0)
                throw new ArgumentException("Invalid product ID.");

            var existingProduct = await _cartRepository.GetProductByIdAsync(productId);
            if (existingProduct == null)
                return false;

            await _cacheService.InvalidateCartTotalCacheAsync(existingProduct.CartId);
            return await _cartRepository.RemoveProductAsync(productId);
        }
    }
}
