using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Shopping.API.Models
{
    public class ShoppingCart
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
        public decimal? Ammount => Products?.Sum(p => p.Price * p.Quantity) ?? 0;

        public ShoppingCart(int cartId, string payerDocument, DateTime? createdAt = null)
        {
            CartId = cartId;
            PayerDocument = payerDocument;
            CreatedAt = createdAt ?? DateTime.UtcNow;
        }
    }
}
