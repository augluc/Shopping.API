using Dapper;
using Shopping.API.Models;
using Shopping.API.Repositories.Interfaces;
using System.Data;

namespace Shopping.API.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbConnection _dbConnection;

        public OrderRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<Order?> GetByCartIdAsync(int cartId)
        {
            var sql = @"
                SELECT * FROM Orders 
                WHERE CartId = @CartId";

            return await _dbConnection.QueryFirstOrDefaultAsync<Order>(sql, new { CartId = cartId });
        }

        public async Task<Order> CreateAsync(int cartId)
        {
            var sql = @"
                INSERT INTO Orders (CartId, PaymentStatus, CreatedAt)
                VALUES (@CartId, @PaymentStatus, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            var orderId = await _dbConnection.QuerySingleAsync<int>(sql, new
            {
                CartId = cartId,
                PaymentStatus = "Pending",
                CreatedAt = DateTime.UtcNow
            });

            return new Order(cartId)
            {
                OrderId = orderId,
                PaymentStatus = "Pending"
            };
        }

        public async Task<Order> UpdatePaymentStatusAsync(int orderId, Guid paymentId, string status)
        {
            var sql = @"
                UPDATE Orders 
                SET PaymentId = @PaymentId, 
                    PaymentStatus = @Status,
                    UpdatedAt = @UpdatedAt
                WHERE OrderId = @OrderId;
                
                SELECT * FROM Orders WHERE OrderId = @OrderId";

            return await _dbConnection.QuerySingleAsync<Order>(sql, new
            {
                OrderId = orderId,
                PaymentId = paymentId,
                Status = status,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }
}
