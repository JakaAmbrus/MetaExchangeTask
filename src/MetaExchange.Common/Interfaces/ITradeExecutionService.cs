using MetaExchange.Common.Models;

namespace MetaExchange.Common.Interfaces
{
    public interface ITradeExecutionService
    {
        Task<TradeOrderResult> ExecuteBuyOrderAsync(decimal amountBTC, IEnumerable<Exchange> exchanges);
        Task<TradeOrderResult> ExecuteSellOrderAsync(decimal amountBTC, IEnumerable<Exchange> exchanges);
    }
}
