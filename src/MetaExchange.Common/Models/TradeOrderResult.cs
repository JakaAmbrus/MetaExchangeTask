namespace MetaExchange.Common.Models
{
    public class TradeOrderResult
    {
        public string OrderType { get; set; }
        public decimal RequestedBTC { get; set; }
        public List<TradeDetails> Execution { get; set; } = new List<TradeDetails>();
        public Summary Summary { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } // null if Success is true
    }
}
