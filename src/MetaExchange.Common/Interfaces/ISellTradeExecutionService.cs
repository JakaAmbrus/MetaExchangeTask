using MetaExchange.Common.Models;

namespace MetaExchange.Common.Interfaces
{
    public interface ISellTradeExecutionService
    {
        Task<TradeOrderResult> ExecuteSellOrderAsync(decimal amountBTC);
    }
}
