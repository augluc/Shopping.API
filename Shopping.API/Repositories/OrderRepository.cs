using Dapper;
using Microsoft.AspNetCore.Mvc;
using Shopping.API.Data;
using Shopping.API.Models;
using Shopping.API.Repositories.Interfaces;
using System.Data;

namespace Shopping.API.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbContext _dbContext;

        public OrderRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Order?> GetByCartIdAsync(int cartId)
        {
            using var connection = _dbContext.CreateConnection();

            return await connection.QueryFirstOrDefaultAsync<Order>(
                "SELECT * FROM Orders WHERE CartId = @CartId", 
                new { CartId = cartId });
        }

        public async Task<Order> CreateAsync(int cartId, Guid paymentId, string status, DateTime createdAt)
        {
            using var connection = _dbContext.CreateConnection();

            var sql = @"
                INSERT INTO Orders (CartId, PaymentId, PaymentStatus, CreatedAt)
                VALUES (@CartId, @PaymentId, @PaymentStatus, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            var orderId = await connection.QuerySingleAsync<int>(sql, new
            {
                CartId = cartId,
                PaymentId = paymentId,
                PaymentStatus = status,
                CreatedAt = createdAt
            });

            return new Order
            {
                OrderId = orderId,
                CartId = cartId,
                PaymentId = paymentId,
                PaymentStatus = status,
                CreatedAt = createdAt
            };
        }
    }
}
