using MetaExchange.Common.Interfaces;
using MetaExchange.Common.Models;
using System.Text.Json;

namespace MetaExchange.Common.Services
{
    // This class is used to read the exchange data from the DB.json file, purely for demonstration purposes
    public class ExchangeDataContextService : IExchangeDataContextService
    {
        public async Task<IEnumerable<Exchange>> GetExchangeDataAsync()
        {
            var fullPath = Path.Combine(AppContext.BaseDirectory, "Data", "DB.json");

            await using var stream = File.OpenRead(fullPath);
            var exchanges = await JsonSerializer.DeserializeAsync<List<Exchange>>(stream);

            return exchanges;
        }
    }
}
