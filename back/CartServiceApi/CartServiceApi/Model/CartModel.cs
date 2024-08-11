using CartService.Domain.Entity;
using System.Text.Json.Serialization;

namespace CartServiceApi.Model
{
    public class CartModel
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
        [JsonPropertyName("status")]
        public CartStatus Status { get; set; }
        [JsonPropertyName("items")]
        public ICollection<CartItemModel> CartItems { get; set; }
    }
}
