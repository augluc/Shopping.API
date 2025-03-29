using Dapper;
using Shopping.API.Data;
using Shopping.API.Models;
using Shopping.API.Models.Request;
using Shopping.API.Repositories.Interfaces;
using System;
using System.Collections.Generic;
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

        public async Task<ShoppingCart?> GetByIdAsync(int cartId)
        {
            try
            {
                using var connection = _dbContext.CreateConnection();

                var cart = await connection.QueryFirstOrDefaultAsync<ShoppingCart>(
                    "SELECT * FROM ShoppingCart WHERE CartId = @CartId",
                    new { CartId = cartId });

                if (cart == null)
                    return null;

                var products = await connection.QueryAsync<Product>(
                    "SELECT * FROM Product WHERE CartId = @CartId",
                    new { CartId = cartId });

                cart.Products = products.ToList();
                return cart;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ShoppingCart> CreateAsync(ShoppingCartRequest shoppingCartRequest)
        {
            try
            {
                using var connection = _dbContext.CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                var id = await connection.QuerySingleAsync<int>(
                    @"INSERT INTO ShoppingCart (PayerDocument, CreatedAt) 
                        VALUES (@PayerDocument, @CreatedAt);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new { shoppingCartRequest.PayerDocument, shoppingCartRequest.CreatedAt },
                    transaction);

                var cart = new ShoppingCart(id, shoppingCartRequest.PayerDocument, shoppingCartRequest.CreatedAt);

                transaction.Commit();

                return cart;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating ShoppingCart: {ex.Message}");
                throw new Exception("Error creating ShoppingCart", ex);
            }
        }


        public async Task<bool> DeleteAsync(int cartId)
        {
            try
            {
                using var connection = _dbContext.CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                await connection.ExecuteAsync(
                    "DELETE FROM Product WHERE CartId = @CartId",
                    new { CartId = cartId },
                    transaction);

                var affectedRows = await connection.ExecuteAsync(
                    "DELETE FROM ShoppingCart WHERE CartId = @CartId",
                    new { CartId = cartId },
                    transaction);

                if (affectedRows == 0)
                    return false;

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            try
            {
                using var connection = _dbContext.CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<Product>(
                    "SELECT * FROM Product WHERE ProductId = @ProductId",
                    new { ProductId = productId });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task AddProductAsync(Product product)
        {
            try
            {
                using var connection = _dbContext.CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                var existingProduct = await connection.QueryFirstOrDefaultAsync<Product>(
                    "SELECT * FROM Product WHERE CartId = @CartId AND ProductName = @ProductName",
                    new { product.CartId, product.ProductName });

                if (existingProduct != null)
                {
                    throw new InvalidOperationException("This product already exists in the cart.");
                }

                await connection.ExecuteAsync(
                    @"INSERT INTO Product (CartId, ProductName, Quantity, Price)
                             VALUES (@CartId, @ProductName, @Quantity, @Price)",
                    product);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            try
            {
                using var connection = _dbContext.CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                var existingProduct = await connection.QueryFirstOrDefaultAsync<Product>(
                    "SELECT * FROM Product WHERE ProductId = @ProductId",
                    new { product.ProductId });

                if (existingProduct == null)
                    throw new InvalidOperationException($"Product with ID {product.ProductId} not found.");

                if (existingProduct.CartId != product.CartId)
                    throw new InvalidOperationException("Product does not belong to the specified cart.");

                var affectedRows = await connection.ExecuteAsync(
                    @"UPDATE Product 
                        SET ProductName = @ProductName, 
                            Quantity = @Quantity, 
                            Price = @Price
                        WHERE ProductId = @ProductId",
                    product);

                transaction.Commit();

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> RemoveProductAsync(int productId)
        {
            try
            {
                using var connection = _dbContext.CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                var existingProduct = await connection.QueryFirstOrDefaultAsync<Product>(
                    "SELECT * FROM Product WHERE ProductId = @ProductId",
                    new { ProductId = productId });

                if (existingProduct == null)
                    throw new InvalidOperationException($"Product with ID {productId} not found.");

                var affectedRows = await connection.ExecuteAsync(
                    "DELETE FROM Product WHERE ProductId = @ProductId",
                    new { ProductId = productId });

                transaction.Commit();

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
