using ConsoleApp.PoC.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Diagnostics;
using TradingBot.Api.Services;
using TradingBot.Domain;
using TradingBot.Domain.Entities;
using Microsoft.Extensions.Logging;
using TradingBot.Domain.Models;
using Microsoft.Extensions.Configuration;
using TradingBot.Domain.Settings;

namespace ConsoleApp.PoC;

internal partial class Program
{
    private static IPolygonService _polygonService;
    private static decimal position;
    private static decimal entryPrice;
    public static decimal balance { get; private set; } = 10000;
    public static List<Trade> trades { get; private set; } = new List<Trade>();

    public static async Task Main(string[] args)
    {
        // Build the configuration
        var configuration = BuildConfiguration();

        // Set up dependency injection
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection, configuration);

        // Build the service provider
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Example: Access configuration values
        var appName = configuration["AppSettings:ApplicationName"];
        var logger = serviceProvider.GetService<ILogger<Program>>();
        logger.LogInformation($"Starting {appName}...");

        _polygonService = serviceProvider.GetService<PolygonService>(); 


        var now = DateTime.Now;
        string from = now.AddMonths(-1).ToString("yyyy-MM-dd");
        string to  = now.AddDays(-1).ToString("yyyy-MM-dd");

        TimeFrame timespan = TimeFrame.Minute;
        int multiplier = 1;

        //var tickerTypes = await _polygonService.GetTickerTypes(AssetClass.Stocks, Locale.US);

        //string[] tickers = { "AAPL", "MSFT", "GOOGL" };
        string[] tickers = { "AAPL" };
        foreach (var ticker in tickers)
        {
            var historicalData = await _polygonService.GetAggregateBars(ticker, multiplier, timespan, from, to);
            var signals = GenerateSignals(historicalData);
            //trades = SimulateTrades(historicalData, signals, ticker);
            //DisplayPerformance(trades);
            //DisplayTradeLog(trades);

        }
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add logging
        services.AddLogging(configure =>
        {
            configure.AddConsole();
            configure.SetMinimumLevel(LogLevel.Information);
        });

        // Bind AppSettings section to AppSettings class
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
        services.AddSingleton(appSettings);


        services.AddTransient<PolygonService>();
    }

    private static List<int> GenerateSignals(List<Candle> data)
    {
        var signals = new List<int>();
        int rsiPeriod = 14;

        for (int i = 0; i < data.Count; i++)
        {
            if (i < rsiPeriod)
            {
                signals.Add(0); // Not enough data for RSI; default to Hold
                continue;
            }

            bool isAccumulation = PO3Strategy.IsAccumulation(data.Take(i).ToList());
            decimal rsi = IndicatorHelper.CalculateRSI(data.Take(i + 1).ToList(), rsiPeriod);

            int signal = 0; // Default to Hold

            // Adjusted Buy Signal Logic
            if (rsi < 30)
            {
                signal = 1; // Strong oversold Buy signal
                Console.WriteLine($"Standalone Buy signal at {data[i].Timestamp}: RSI={rsi}");
            }
            else if (isAccumulation && rsi < 35)
            {
                signal = 1; // Accumulation + RSI Buy
                Console.WriteLine($"Buy signal at {data[i].Timestamp}: RSI={rsi}, Accumulation={isAccumulation}");
            }

            // Adjusted Sell Signal Logic
            if (position > 0) // Only generate sell signals if a position is open
            {
                if (rsi > 60 || (!isAccumulation && rsi > 55))
                {
                    signal = -1; // Relaxed Sell signal
                    Console.WriteLine($"Sell signal at {data[i].Timestamp}: RSI={rsi}, Accumulation={isAccumulation}");
                }
            }

            signals.Add(signal); // Add only one signal per iteration
        }

        // Ensure signals match data count
        if (signals.Count != data.Count)
        {
            throw new InvalidOperationException($"Signals count ({signals.Count}) does not match data count ({data.Count}).");
        }

        Console.WriteLine($"Signals Generated: Buy={signals.Count(s => s == 1)}, Sell={signals.Count(s => s == -1)}, Hold={signals.Count(s => s == 0)}");

        return signals;
    }

    //private static List<Trade> SimulateTrades(List<Candle> data, List<int> signals, string ticker)
    //{
    //    if (data.Count != signals.Count)
    //    {
    //        throw new InvalidOperationException($"Mismatch between data count ({data.Count}) and signals count ({signals.Count}).");
    //    }

    //    decimal stopLossPercentage = 0.02m; // 2% stop-loss threshold
    //    decimal trailingStopPercentage = 0.03m; // 3% trailing stop-loss

    //    decimal highestPrice = 0;

    //    for (int i = 0; i < data.Count; i++)
    //    {
    //        // Track highest price for trailing stop
    //        if (position > 0 && data[i].High > highestPrice)
    //        {
    //            highestPrice = data[i].High;
    //        }

    //        // Check Trailing Stop-Loss
    //        if (position > 0 && data[i].Low <= highestPrice * (1 - trailingStopPercentage))
    //        {
    //            decimal trailingStopPrice = highestPrice * (1 - trailingStopPercentage);
    //            balance = position * trailingStopPrice;

    //            trades[^1].ExitPrice = trailingStopPrice;
    //            trades[^1].ExitTime = DateTime.fr data[i].Timestamp;
    //            trades[^1].Profit = balance - (position * entryPrice);

    //            Console.WriteLine($"Trailing stop-loss triggered at {data[i].Timestamp}: Price={trailingStopPrice}, Profit={trades[^1].Profit}");
    //            position = 0; // Close position
    //            highestPrice = 0; // Reset for next trade
    //            continue;
    //        }

    //        // Check Fixed Stop-Loss
    //        if (position > 0 && data[i].Low <= entryPrice * (1 - stopLossPercentage))
    //        {
    //            decimal stopLossPrice = entryPrice * (1 - stopLossPercentage);
    //            balance = position * stopLossPrice;

    //            trades[^1].ExitPrice = stopLossPrice;
    //            trades[^1].ExitTime = data[i].Timestamp;
    //            trades[^1].Profit = balance - (position * entryPrice);

    //            Console.WriteLine($"Fixed stop-loss triggered at {data[i].Timestamp}: Price={stopLossPrice}, Profit={trades[^1].Profit}");
    //            position = 0; // Close position
    //            highestPrice = 0; // Reset for next trade
    //            continue;
    //        }

    //        // Execute Buy
    //        if (signals[i] == 1 && position == 0) // Buy signal
    //        {
    //            position = balance / data[i].Close;
    //            entryPrice = data[i].Close;
    //            balance = 0;
    //            highestPrice = data[i].Close; // Initialize for trailing stop

    //            trades.Add(new Trade
    //            {
    //                Ticker = ticker,
    //                EntryTime = data[i].Timestamp,
    //                EntryPrice = entryPrice,
    //                TradeType = "Buy"
    //            });

    //            Console.WriteLine($"Buy trade placed at {data[i].Timestamp}: Price={entryPrice}");
    //        }
    //        // Execute Sell
    //        else if (signals[i] == -1 && position > 0) // Sell signal
    //        {
    //            balance = position * data[i].Close;

    //            trades[^1].ExitPrice = data[i].Close;
    //            trades[^1].ExitTime = data[i].Timestamp;
    //            trades[^1].Profit = balance - (position * entryPrice);

    //            Console.WriteLine($"Sell trade placed at {data[i].Timestamp}: Price={data[i].Close}, Profit={trades[^1].Profit}");
    //            position = 0;
    //            highestPrice = 0; // Reset for next trade
    //        }
    //    }

    //    // If still holding a position, close it at the last price
    //    if (position > 0)
    //    {
    //        balance = position * data[^1].Close;

    //        trades[^1].ExitPrice = data[^1].Close;
    //        trades[^1].ExitTime = data[^1].Timestamp;
    //        trades[^1].Profit = balance - (position * entryPrice);

    //        Console.WriteLine($"Final sell trade placed at {data[^1].Timestamp}: Price={data[^1].Close}, Profit={trades[^1].Profit}");
    //    }

    //    return trades;
    //}

    private static void DisplayPerformance(List<Trade> trades)
    {
        if (trades.Count == 0)
        {
            Console.WriteLine("No trades to display.");
            return;
        }

        var totalProfit = trades.Sum(t => t.Profit);
        var winningTrades = trades.Count(t => t.Profit > 0);
        var losingTrades = trades.Count(t => t.Profit < 0);
        var maxDrawdown = trades.Min(t => t.Profit); // Simplified for POC

        Console.WriteLine($"Total Profit: {totalProfit:C}");
        Console.WriteLine($"Win Rate: {winningTrades / (double)trades.Count:P}");
        Console.WriteLine($"Average Trade Profit: {totalProfit / trades.Count:C}");
        Console.WriteLine($"Max Drawdown: {maxDrawdown:C}");
    }

    private static void DisplayTradeLog(List<Trade> trades)
    {
        Console.WriteLine("Trade Log:");
        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"{"Ticker", -10} {"Trade Type",-10} {"Entry Time",-20} {"Entry Price",-15} {"Exit Time",-20} {"Exit Price",-15} {"Profit",-10}");
        Console.WriteLine(new string('-', 80));

        foreach (var trade in trades)
        {
            Console.WriteLine(
                $"{trade.Ticker, -10} {trade.TradeType,-10} {trade.EntryTime,-20} {trade.EntryPrice,-15:C} " +
                $"{(trade.ExitTime.HasValue ? trade.ExitTime.Value.ToString() : "Open"),-20} " +
                $"{(trade.ExitPrice.HasValue ? trade.ExitPrice.Value.ToString("C") : "Open"),-15} " +
                $"{trade.Profit,-10:C}"
            );
        }

        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"Total Trades: {trades.Count}");
        Console.WriteLine($"Total Profit: {trades.Sum(t => t.Profit):C}");
        Console.WriteLine($"Winning Trades: {trades.Count(t => t.Profit > 0)}");
        Console.WriteLine($"Losing Trades: {trades.Count(t => t.Profit < 0)}");
        Console.WriteLine(new string('-', 80));
    }

}
