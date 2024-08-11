using System.Text.Json.Serialization;

namespace CartServiceApi.Model
{
    public class CarProfileModel
    {
        [JsonPropertyName("cart")]
        public CartModel Cart { get; set; }
        [JsonPropertyName("total")]
        public decimal Total { get; set; }
    }
}
