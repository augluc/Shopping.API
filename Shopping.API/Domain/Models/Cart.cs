using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Shopping.API.Domain.Models
{
    public class Cart
    {
        public int CartId { get; set; }
        public string PayerDocument { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        private List<Product> _products = new List<Product>();
        public List<Product> Products
        {
            get => _products;
            set => _products = value ?? new List<Product>();
        }
        public decimal Amount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public Cart(int cartId, string payerDocument, decimal? discountPercentage, DateTime? createdAt = null)
        {
            CartId = cartId;
            PayerDocument = payerDocument;
            CreatedAt = createdAt ?? DateTime.UtcNow;
            Amount = 0;
            DiscountPercentage = discountPercentage ?? 0;
        }
    }
}
