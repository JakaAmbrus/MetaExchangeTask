using MetaExchange.Common.Models;

namespace MetaExchange.Common.Interfaces
{
    public interface IExchangeDataContextService
    {
        Task<IEnumerable<Exchange>> GetExchangeDataAsync();
    }
}
