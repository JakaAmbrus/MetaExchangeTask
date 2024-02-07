using System.Text.Json.Serialization;

namespace MetaExchange.Common.Models
{
    public class Exchange
    {
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; } // A unique identifier for the exchange
        [JsonPropertyName("balances")]
        public Balances Balances { get; set; }
        [JsonPropertyName("orderBook")]
        public OrderBook OrderBook { get; set; }
    }
}
