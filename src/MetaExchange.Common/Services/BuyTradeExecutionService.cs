using MetaExchange.Common.Interfaces;
using MetaExchange.Common.Models;

namespace MetaExchange.Common.Services
{
    public class BuyTradeExecutionService : IBuyTradeExecutionService
    {
        public TradeOrderResult ExecuteBuyOrder(decimal requestedBTCAmount, IEnumerable<Exchange> exchanges)
        {
            // At first only the type and amount of BTC are set, with the success flag set to true, that changes to true if the order is fulfilled
            var tradeResult = new TradeOrderResult
            {
                OrderType = "Buy",
                RequestedBTC = requestedBTCAmount,
                Execution = new List<TradeDetails>(),
                Summary = new Summary { BTCAcquired = 0, TotalEURCost = 0 },
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

            var sortedBids = exchanges.SelectMany(exchange => exchange.OrderBook.Bids.Select(bid => new { Exchange = exchange.Identifier, Bid = bid })) // give each bid an exchange identifier
                    .OrderBy(bid => bid.Bid.Price).ToList();

            foreach (var bid in sortedBids)
            {
                if (amountTracker == 0)
                {
                    break; // Order fulfilled
                }

                var exchange = exchanges.First(e => e.Identifier == bid.Exchange);

                var affordableBTC = exchange.Balances.EUR / bid.Bid.Price; // checks how much BTC can be bought with the available EUR balance of the exchange
                var maxBTC = Math.Min(amountTracker, Math.Min(bid.Bid.Amount, affordableBTC));
                var costEUR = maxBTC * bid.Bid.Price;

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
                        PricePerBTC = bid.Bid.Price,
                        TotalCostEUR = costEUR
                    });

                    tradeResult.Success = true;
                }
            }

            // Summary
            tradeResult.Summary.BTCAcquired = requestedBTCAmount - amountTracker;
            tradeResult.Summary.TotalEURCost = totalCostEUR;
            tradeResult.Summary.AverageBTCPrice = tradeResult.Summary.BTCAcquired > 0 ? totalCostEUR / tradeResult.Summary.BTCAcquired : 0;

            if (amountTracker > 0)
            {
                if (amountTracker == requestedBTCAmount)
                {
                    // Partial failure, some changes made, but could not fulfill the entire order
                    tradeResult.ErrorMessage = "Could not fulfill the order.";
                }
                else
                {
                    // Partial success, some changes made, but could not fulfill the entire order
                    tradeResult.ErrorMessage = $"Could only fulfill the order partially. only {tradeResult.Summary.BTCAcquired}BTC acquired.";
                }
            }

            return tradeResult;
        }
    }
}
