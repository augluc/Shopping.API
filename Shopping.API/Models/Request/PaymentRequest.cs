namespace Shopping.API.Models.Request
{
    public record PaymentRequest
    {
        public int CartId { get; set; }
        public decimal Amount { get; set; }
        public string PayerDocument { get; set; } = string.Empty;
    }
}
