using MetaExchange.Common.Interfaces;
using MetaExchange.Common.Models;
using MetaExchange.Common.Services;
using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();

serviceCollection.AddTransient<IExchangeDataContextService, ExchangeDataContextService>();
serviceCollection.AddTransient<IBuyTradeExecutionService, BuyTradeExecutionService>();
serviceCollection.AddTransient<ISellTradeExecutionService, SellTradeExecutionService>();

var serviceProvider = serviceCollection.BuildServiceProvider();

string orderType = string.Empty;
bool validOrderTypeEntered = false;

while (!validOrderTypeEntered)
{
    Console.WriteLine("Enter the order type (buy/sell):");
    orderType = Console.ReadLine().Trim().ToUpper();

    if (orderType == "BUY" || orderType == "SELL")
    {
        validOrderTypeEntered = true;
    }
    else
    {
        Console.WriteLine("Invalid order type. Please enter 'buy' or 'sell'.");
    }
}


bool validBtcAmountEntered = false;
decimal btcAmount = 0;

while (!validBtcAmountEntered)
{
    Console.WriteLine("Enter the amount of BTC to trade(positive decimal):");
    string btcAmountString = Console.ReadLine().Trim();

    if (decimal.TryParse(btcAmountString, out btcAmount) && btcAmount > 0)
    {
        validBtcAmountEntered = true;
    }
    else
    {
        Console.WriteLine("Invalid BTC amount. Please enter a valid positive decimal.");
    }
}

TradeOrderResult result = null;

if (orderType == "BUY")
{
    var buyService = serviceProvider.GetService<IBuyTradeExecutionService>();
    result = await buyService.ExecuteBuyOrderAsync(btcAmount);
}
else if (orderType == "SELL")
{
    var sellService = serviceProvider.GetService<ISellTradeExecutionService>();
    result = await sellService.ExecuteSellOrderAsync(btcAmount);
}

if (result != null && result.Success)
{
    // Order Summary
    Console.WriteLine(new String('-', 80));
    Console.WriteLine($"Order Execution Summary");
    Console.WriteLine(new String('-', 80));
    Console.WriteLine($"Order Type: {result.OrderType}");
    Console.WriteLine($"BTC Ammount requested: {result.RequestedBTC:N8}");

    // Execution Information
    Console.WriteLine("\nExecution Details:");
    foreach (var exec in result.Execution)
    {
        Console.WriteLine($"- Exchange: {exec.Exchange}");
        Console.WriteLine($"  Action: {exec.Action}");
        Console.WriteLine($"  BTC: {exec.RequestedBTCAmount:N8}");
        Console.WriteLine($"  Price per BTC: {exec.PricePerBTC}");
        Console.WriteLine($"  Total Cost EUR: {exec.TotalCostEUR}\n");
    }

    // Summary
    Console.WriteLine(new String('-', 80));
    Console.WriteLine("Summary:");
    Console.WriteLine($"Total BTC Acquired: {result.Summary.BTCVolume:N8}");
    Console.WriteLine($"Average BTC Price: {result.Summary.AverageBTCPrice:N2}");
    Console.WriteLine($"Total Cost EUR: {result.Summary.TotalEUR:N2}");
    Console.WriteLine(new String('-', 80));

    // If there is a partial success where some of the order was not fulfilled
    if (!string.IsNullOrEmpty(result.ErrorMessage))
    {
        Console.WriteLine($"Error Message: {result.ErrorMessage}");
    }
}
else
{
    // if the success is false, it could still be partial success,
    // but it could also mean the order was not successful entirely
    if (result.ErrorMessage != "") 
    {
        Console.WriteLine($"Error Message: {result.ErrorMessage}");
    }
    else
    {
        Console.WriteLine("Trade could not be executed.");
    }
}
