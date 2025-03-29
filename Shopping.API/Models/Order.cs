namespace Shopping.API.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CartId { get; set; }
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed
        public Guid? PaymentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Order(int cartId)
        {
            CartId = cartId;
        }
    }
}
