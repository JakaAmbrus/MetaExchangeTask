using MetaExchange.Common.Models;

namespace MetaExchange.Common.Interfaces
{
    public interface IBuyTradeExecutionService
    {
        TradeOrderResult ExecuteBuyOrder(decimal requestedBTCAmount, IEnumerable<Exchange> exchanges);
    }
}
