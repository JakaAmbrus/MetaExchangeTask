﻿using MetaExchange.Common.Interfaces;
using MetaExchange.Common.Models;

namespace MetaExchange.Common.Services
{
    public class SellTradeExecutionService : ISellTradeExecutionService
    {
        public TradeOrderResult ExecuteSellOrder(decimal amountBTC, IEnumerable<Exchange> exchanges)
        {
            // Since this is very similar to buying, I could have made it a single method with a parameter for the order type,
            // but I decided to keep them separate for clarity and to make the tests separate tests for buying and selling.
            var tradeResult = new TradeOrderResult
            {
                OrderType = "Sell",
                RequestedBTC = amountBTC,
                Execution = new List<TradeDetails>(),
                Summary = new Summary { BTCVolume = 0, TotalEUR = 0 },
                Success = false,
                ErrorMessage = ""
            };

            if (amountBTC <= 0)
            {
                tradeResult.ErrorMessage = "Invalid BTC amount requested.";
                return tradeResult;
            }

            if (exchanges is null || !exchanges.Any())
            {
                tradeResult.ErrorMessage = "No exchanges available.";
                return tradeResult;
            }

            decimal amountTracker = amountBTC; // just like in the buy order, this variable will keep track of the amount of BTC left to sell
            decimal totalReceivedEUR = 0;

            var sortedBids = exchanges
              .Where(exchange => exchange.OrderBook?.Bids != null && exchange.OrderBook.Bids.Any())
              .SelectMany(exchange => exchange.OrderBook.Bids.Select(bid => new { Exchange = exchange.Identifier, Bid = bid }))
              .OrderByDescending(bid => bid.Bid.Price)
              .ToList();

            if (!sortedBids.Any())
            {
                tradeResult.ErrorMessage = "No bids available.";
                return tradeResult;
            }

            foreach (var bid in sortedBids)
            {
                if (amountTracker == 0)
                {
                    break; // Order fulfilled
                }

                var exchange = exchanges.First(e => e.Identifier == bid.Exchange);

                if (exchange.Balances.BTC >= amountTracker) // if the exchange has enough BTC to sell
                {
                    var sellableBTC = Math.Min(amountTracker, bid.Bid.Amount);
                    var receivedEUR = sellableBTC * bid.Bid.Price;

                    exchange.Balances.BTC -= sellableBTC;
                    exchange.Balances.EUR += receivedEUR;
                    amountTracker -= sellableBTC;
                    totalReceivedEUR += receivedEUR;

                    tradeResult.Execution.Add(new TradeDetails
                    {
                        Exchange = exchange.Identifier,
                        Action = "Sell",
                        RequestedBTCAmount = sellableBTC,
                        PricePerBTC = bid.Bid.Price,
                        TotalCostEUR = receivedEUR
                    });

                    tradeResult.Success = true;
                }
            }

            tradeResult.Summary.BTCVolume = amountBTC - amountTracker;
            tradeResult.Summary.TotalEUR = Math.Round(totalReceivedEUR, 2);
            tradeResult.Summary.AverageBTCPrice = tradeResult.Summary.BTCVolume > 0 ? totalReceivedEUR / tradeResult.Summary.BTCVolume : 0;

            if (amountTracker > 0)
            {
                if (amountTracker == amountBTC)
                {
                    tradeResult.Success = false;
                    tradeResult.ErrorMessage = "Could not fulfill the order.";
                }
                else
                {
                    tradeResult.Success = false;
                    tradeResult.ErrorMessage = $"Could only fulfill the order partially. only {tradeResult.Summary.BTCVolume}BTC sold.";
                }
            }

            return tradeResult;
        }
    }
}
