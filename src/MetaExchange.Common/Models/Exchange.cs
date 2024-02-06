namespace MetaExchange.Common.Models
{
    public class Exchange
    {
        public string Identifier { get; set; } // A unique identifier for the exchange
        public Balances Balances { get; set; }
        public OrderBook OrderBook { get; set; }
    }
}
