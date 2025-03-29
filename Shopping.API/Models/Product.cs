using System.ComponentModel.DataAnnotations;

namespace Shopping.API.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public int CartId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        public string ProductName { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        public Product(int cartId, string productName, decimal price, int quantity)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Product name cannot be empty or null.");

            if (price <= 0)
                throw new ArgumentException("Price must be greater than zero.");

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be at least 1.");

            CartId = cartId;
            ProductName = productName;
            Price = price;
            Quantity = quantity;
        }
        public Product() { }
    }
}
