using System.Text.Json.Serialization;

namespace MetaExchange.Common.Models
{
    public class Order
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; } // can be null based on the data file
        [JsonPropertyName("Time")]
        public DateTime? Time { get; set; } // default value naturally set to DateTime.MinValue
        [JsonPropertyName("Type")]
        public string Type { get; set; } // Buy or Sell
        [JsonPropertyName("Kind")]
        public string Kind { get; set; } = "Limit"; // default value, I iterated over the orders and all of them are "Limit" Kinds so I will proceed with this assumption
        [JsonPropertyName("Amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("Price")]
        public decimal Price { get; set; }
    }
}
