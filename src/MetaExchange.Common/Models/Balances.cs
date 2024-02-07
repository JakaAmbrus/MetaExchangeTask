using System.Text.Json.Serialization;

namespace MetaExchange.Common.Models
{
    public class Balances
    {
        [JsonPropertyName("EUR")]
        public decimal EUR { get; set; }
        [JsonPropertyName("BTC")]
        public decimal BTC { get; set; }
    }
}
