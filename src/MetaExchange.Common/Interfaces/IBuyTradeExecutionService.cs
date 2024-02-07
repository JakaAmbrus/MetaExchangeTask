using MetaExchange.Common.Models;

namespace MetaExchange.Common.Interfaces
{
    public interface IBuyTradeExecutionService
    {
        Task<TradeOrderResult> ExecuteBuyOrderAsync(decimal requestedBTCAmount);
    }
}
