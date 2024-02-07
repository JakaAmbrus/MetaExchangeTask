using System.Text.Json.Serialization;

namespace MetaExchange.Common.Models
{
    public class RootDataObject
    {
        [JsonPropertyName("cryptoExchanges")]
        public List<Exchange> CryptoExchanges { get; set; }
    }
}
