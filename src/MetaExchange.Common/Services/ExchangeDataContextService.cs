using MetaExchange.Common.Interfaces;
using MetaExchange.Common.Models;
using System.Text.Json;

namespace MetaExchange.Common.Services
{
    public class ExchangeDataContextService : IExchangeDataContextService
    {
        private const string _dataFilePath = "Data/DB.json";

        public async Task<IEnumerable<Exchange>> GetExchangeDataAsync()
        {
            var fullPath = Path.GetFullPath(_dataFilePath, Directory.GetCurrentDirectory());

            using var stream = File.OpenRead(fullPath);
            return await JsonSerializer.DeserializeAsync<IEnumerable<Exchange>>(stream) ?? new List<Exchange>();
        }
    }
}
