// CartRepository.cs
using Dapper;
using Shopping.API.Data;
using Shopping.API.Models;
using Shopping.API.Models.Request;
using Shopping.API.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.API.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly DbContext _dbContext;

        public CartRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Cart?> GetByIdAsync(int cartId)
        {
            using var connection = _dbContext.CreateConnection();

            var cart = await connection.QueryFirstOrDefaultAsync<Cart>(
                "SELECT * FROM Cart WHERE CartId = @CartId",
                new { CartId = cartId });

            if (cart == null)
                return null;

            var products = await connection.QueryAsync<Product>(
                "SELECT * FROM Product WHERE CartId = @CartId",
                new { CartId = cartId });

            cart.Products = products.ToList();
            return cart;
        }

        public async Task<Cart> CreateAsync(CartRequest cartRequest)
        {
            using var connection = _dbContext.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var id = await connection.QuerySingleAsync<int>(
                    @"INSERT INTO Cart (PayerDocument, CreatedAt) 
                      VALUES (@PayerDocument, @CreatedAt);
                      SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new { cartRequest.PayerDocument, cartRequest.CreatedAt },
                    transaction);

                transaction.Commit();

                return new Cart(id, cartRequest.PayerDocument, 0, cartRequest.CreatedAt);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateCartDiscountPercentage(Cart cart)
        {
            using var connection = _dbContext.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var affectedRows = await connection.ExecuteAsync(
                    "UPDATE Cart SET DiscountPercentage = @DiscountPercentage WHERE CartId = @CartId",
                    new { DiscountPercentage = cart.DiscountPercentage, CartId = cart.CartId },
                    transaction);
                transaction.Commit();

                return affectedRows > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int cartId)
        {
            using var connection = _dbContext.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(
                    "DELETE FROM Product WHERE CartId = @CartId",
                    new { CartId = cartId },
                    transaction);

                var affectedRows = await connection.ExecuteAsync(
                    "DELETE FROM Cart WHERE CartId = @CartId",
                    new { CartId = cartId },
                    transaction);

                transaction.Commit();

                return affectedRows > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Product>(
                "SELECT * FROM Product WHERE ProductId = @ProductId",
                new { ProductId = productId });
        }

        public async Task AddProductAsync(Product product)
        {
            using var connection = _dbContext.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var existingProduct = await connection.QueryFirstOrDefaultAsync<Product>(
                    "SELECT * FROM Product WHERE CartId = @CartId AND ProductName = @ProductName",
                    new { product.CartId, product.ProductName },
                    transaction);

                if (existingProduct != null)
                {
                    throw new InvalidOperationException("This product already exists in the cart.");
                }

                await connection.ExecuteAsync(
                    @"INSERT INTO Product (CartId, ProductName, Quantity, Price)
                      VALUES (@CartId, @ProductName, @Quantity, @Price)",
                    product,
                    transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            using var connection = _dbContext.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var affectedRows = await connection.ExecuteAsync(
                    @"UPDATE Product 
                      SET ProductName = @ProductName, 
                          Quantity = @Quantity, 
                          Price = @Price
                      WHERE ProductId = @ProductId",
                    product,
                    transaction);

                transaction.Commit();

                return affectedRows > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> RemoveProductAsync(int productId)
        {
            using var connection = _dbContext.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var affectedRows = await connection.ExecuteAsync(
                    "DELETE FROM Product WHERE ProductId = @ProductId",
                    new { ProductId = productId },
                    transaction);

                transaction.Commit();

                return affectedRows > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}