namespace MetaExchange.Common.Models
{
    public class OrderBook
    {
        public DateTime AcqTime { get; set; }
        public IEnumerable<Order> Bids { get; set; } // Using IEnumerable because the data will only be iterated over and not modified
        public IEnumerable<Order> Asks { get; set; }
    }
}
