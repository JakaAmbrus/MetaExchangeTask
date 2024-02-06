namespace MetaExchange.Common.Models
{
    public class TradeDetails
    {
        public string Exchange { get; set; }
        public string Action { get; set; }
        public decimal RequestedBTCAmount { get; set; }
        public decimal PricePerBTC { get; set; }
        public decimal TotalCostEUR { get; set; }
    }
}
