using System.Text.Json.Serialization;

namespace CartServiceApi.Model
{
    public class CartItemModel
    {
        [JsonPropertyName("cart_id")]
        public Guid CartId { get; set; }
        [JsonPropertyName("product_id")]
        public Guid ProductId { get; set; }
        [JsonPropertyName("count")]
        public int Count { get; set; }
        [JsonPropertyName("price")]
        public int Price { get; set; }
    }
}
