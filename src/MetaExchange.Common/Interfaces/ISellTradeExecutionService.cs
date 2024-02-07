using MetaExchange.Common.Models;

namespace MetaExchange.Common.Interfaces
{
    public interface ISellTradeExecutionService
    {
        TradeOrderResult ExecuteSellOrder(decimal amountBTC, IEnumerable<Exchange> exchanges);
    }
}
