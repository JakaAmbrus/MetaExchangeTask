using MetaExchange.Common.Interfaces;
using MetaExchange.Common.Models;

namespace MetaExchange.Common.Services
{
    public class BuyTradeExecutionService : IBuyTradeExecutionService
    {
        private readonly IExchangeDataContextService _seedExecutionData;

        public BuyTradeExecutionService(IExchangeDataContextService seedExecutionData)
        {
            _seedExecutionData = seedExecutionData;
        }
        public async Task<TradeOrderResult> ExecuteBuyOrderAsync(decimal requestedBTCAmount)
        {
            var exchanges = await _seedExecutionData.GetExchangeDataAsync();

            // At first only the type and amount of BTC are set, with the success flag set to true, that changes to true if the order is fulfilled
            var tradeResult = new TradeOrderResult
            {
                OrderType = "Buy",
                RequestedBTC = requestedBTCAmount,
                Execution = new List<TradeDetails>(),
                Summary = new Summary { BTCVolume = 0, TotalEUR = 0 },
                Success = false,
                ErrorMessage = ""
            };

            if (requestedBTCAmount <= 0)
            {
                tradeResult.ErrorMessage = "Invalid BTC amount requested.";
                return tradeResult;
            }

            if (exchanges is null || !exchanges.Any())
            {
                tradeResult.ErrorMessage = "No exchanges available.";
                return tradeResult;
            }

            decimal amountTracker = requestedBTCAmount;
            decimal totalCostEUR = 0;

            var sortedAsks = exchanges
                .Where(exchange => exchange.OrderBook?.Asks != null && exchange.OrderBook.Asks.Any())
                .SelectMany(exchange => exchange.OrderBook.Asks.Select(askWrapper => new { Exchange = exchange.Identifier, Ask = askWrapper.Order }))
                .OrderBy(ask => ask.Ask.Price)
                .ToList();

            if (!sortedAsks.Any())
            {
                tradeResult.ErrorMessage = "No asks available.";
                return tradeResult;
            }

            foreach (var ask in sortedAsks)
            {
                if (amountTracker == 0)
                {
                    break; // Order fulfilled
                }

                var exchange = exchanges.First(e => e.Identifier == ask.Exchange);

                var affordableBTC = exchange.Balances.EUR / ask.Ask.Price; // checks how much BTC can be bought with the available EUR balance of the exchange
                var maxBTC = Math.Min(amountTracker, Math.Min(ask.Ask.Amount, affordableBTC));
                var costEUR = maxBTC * ask.Ask.Price;

                if (costEUR <= exchange.Balances.EUR && maxBTC > 0)
                {
                    exchange.Balances.EUR -= costEUR;
                    exchange.Balances.BTC += maxBTC;
                    amountTracker -= maxBTC;
                    totalCostEUR += costEUR;

                    // Adding the trade details to execution
                    tradeResult.Execution.Add(new TradeDetails
                    {
                        Exchange = exchange.Identifier,
                        Action = "Buy",
                        RequestedBTCAmount = maxBTC,
                        PricePerBTC = Math.Round(ask.Ask.Price, 2),
                        TotalCostEUR = Math.Round(costEUR, 2)
                    });

                    tradeResult.Success = true;
                }
            }

            // Summary
            tradeResult.Summary.BTCVolume = Math.Round(requestedBTCAmount - amountTracker, 8);
            tradeResult.Summary.TotalEUR = Math.Round(totalCostEUR, 2);
            // will keep the average price more specific
            tradeResult.Summary.AverageBTCPrice = tradeResult.Summary.BTCVolume > 0 ? totalCostEUR / tradeResult.Summary.BTCVolume : 0;

            if (amountTracker > 0)
            {
                if (amountTracker == requestedBTCAmount)
                {
                    // Complete failure, no changes made 
                    tradeResult.ErrorMessage = "Could not fulfill the order.";
                }
                else
                {
                    // Partial success, some changes made, but could not fulfill the entire order
                    tradeResult.Success = false;
                    tradeResult.ErrorMessage = $"Could only fulfill the order partially. only {tradeResult.Summary.BTCVolume}BTC acquired.";
                }
            }

            return tradeResult;
        }
    }
}
