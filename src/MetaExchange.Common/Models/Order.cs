namespace MetaExchange.Common.Models
{
    public class Order
    {
        public string Id { get; set; } // can be null based on the data file
        public DateTime? Time { get; set; } // default value naturally set to DateTime.MinValue
        public string Type { get; set; } // Buy or Sell
        public string Kind { get; set; } = "Limit"; // default value, I iterated over the orders and all of them are "Limit" Kinds so I will proceed with this assumption
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
    }
}
