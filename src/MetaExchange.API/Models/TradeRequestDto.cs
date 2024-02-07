using System.ComponentModel.DataAnnotations;

namespace MetaExchange.API.Models
{
    // In my API usually I would use fluent validation and pre validate the inputs
    // such as trimming and putting inputs like this to lower case, but I did not want to
    // overcomplicate the task, since it is only one endpoint.
    public class TradeRequestDto
    {
        [Required(ErrorMessage = "Order type is required.")]
        [RegularExpression("^(buy|sell)$", ErrorMessage = "Order type must be either 'buy' or 'sell'.")]
        public string OrderType { get; set; }

        [Required(ErrorMessage = "BTC amount is required.")]
        [Range(0.1, double.MaxValue, ErrorMessage = "BTC amount must be greater than 0.")]
        public decimal Amount { get; set; }
    }
}
