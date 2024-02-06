using MetaExchange.Common.Models;

namespace MetaExchange.Common.Interfaces
{
    public interface ISellTradeExecutionService
    {
        Task<TradeOrderResult> ExecuteBuyOrderAsync(decimal amountBTC, IEnumerable<Exchange> exchanges);
    }
}
