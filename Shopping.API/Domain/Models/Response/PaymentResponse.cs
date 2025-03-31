using System.Text.Json.Serialization;

namespace Shopping.API.Domain.Models.Response
{
    public record PaymentResponse
    {
        [JsonPropertyName("payment_id")]
        public Guid PaymentId { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
