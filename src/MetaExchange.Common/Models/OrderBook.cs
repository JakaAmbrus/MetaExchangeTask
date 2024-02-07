using System.Text.Json.Serialization;

namespace MetaExchange.Common.Models
{
    public class OrderBook
    {
        public DateTime AcqTime { get; set; }
        [JsonPropertyName("Bids")]
        public List<OrderWrapper> Bids { get; set; } // Using IEnumerable because the data will only be iterated over and not modified
        [JsonPropertyName("Asks")]
        public List<OrderWrapper> Asks { get; set; }
    }
}
