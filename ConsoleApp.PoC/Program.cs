using ConsoleApp.PoC.Models;
using ConsoleApp.PoC.Strategies;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace ConsoleApp.PoC;

internal partial class Program
{
    private static readonly string apiKey = "R9FFj1qmyjD84ZJ5fyUtES_Bd6SjHprK";
    private static readonly HttpClient client = new HttpClient();

    public static async Task Main(string[] args)
    {
        var now = DateTime.Now;
        string from = now.AddMonths(-24).ToString("YYYY-MM-DD");
        string to  = now.AddDays(-1).ToString("YYYY-MM-DD");

        string timespan = "minute";
        int multiplier = 1;

        //string[] tickers = { "AAPL", "MSFT", "GOOGL" };
        string[] tickers = { "AAPL" };
        foreach (var ticker in tickers)
        {
            var historicalData = await GetHistoricalData(ticker, multiplier, timespan, from, to);
            var signals = GenerateSignals(historicalData);
            var trades = SimulateTrades(historicalData, signals);
            DisplayPerformance(trades);
            DisplayTradeLog(trades);

        }
    }

    private static async Task<List<Candle>> GetHistoricalData(string ticker, int multiplier, string timespan, string from, string to)
    {
        // Define the data folder path
        string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        var historicalData = new List<Candle>();

        // Ensure the data folder exists
        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
        }

        // Define the file path for the ticker
        string filePath = Path.Combine(dataFolder, $"{ticker}.json");

        // Check if the file exists in the data folder
        if (File.Exists(filePath))
        {
            Console.WriteLine($"Loading cached data for {ticker} from {filePath}...");
            string cachedData = await File.ReadAllTextAsync(filePath);
            JObject data = JObject.Parse(cachedData);
            historicalData = ParseHistoricalData(data);

            historicalData = historicalData.Where(c => c.Timestamp >= DateTime.Parse(from) && c.Timestamp <= DateTime.Parse(to)).ToList();

            return historicalData;
        }

        // If no cache, make an API call
        Console.WriteLine($"Fetching data from API for {ticker}...");
        string url = $"https://api.polygon.io/v2/aggs/ticker/{ticker}/range/{multiplier}/{timespan}/{from}/{to}?adjusted=true&sort=asc&apiKey={apiKey}";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        await File.WriteAllTextAsync(filePath, responseBody); // Save JSON to file in the Data folder

        JObject apiData = JObject.Parse(responseBody);
        historicalData = ParseHistoricalData(apiData);
        return historicalData;
    }

    private static List<Candle> ParseHistoricalData(JObject data)
    {
        var results = (JArray)data["results"];

        var candles = new List<Candle>();
        foreach (var result in results)
        {
            candles.Add(new Candle
            {
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)result["t"]).DateTime,
                Open = (decimal)result["o"],
                High = (decimal)result["h"],
                Low = (decimal)result["l"],
                Close = (decimal)result["c"],
                Volume = (long)result["v"]
            });
        }

        foreach (var candle in candles.Take(5)) // Print the first 5 candles
        {
            Console.WriteLine($"Candle: Time={candle.Timestamp}, Open={candle.Open}, High={candle.High}, Low={candle.Low}, Close={candle.Close}");
        }


        return candles;
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

            if (rsi < 30)
            {
                signal = 1; // Buy based on RSI alone
                Console.WriteLine($"Standalone Buy signal at {data[i].Timestamp}: RSI={rsi}");
            }
            else if (isAccumulation && rsi < 35)
            {
                signal = 1; // Buy
                Console.WriteLine($"Buy signal at {data[i].Timestamp}: RSI={rsi}");
            }
            else if (!isAccumulation && rsi > 65)
            {
                signal = -1; // Sell
                Console.WriteLine($"Sell signal at {data[i].Timestamp}: RSI={rsi}");
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

    private static List<Trade> SimulateTrades(List<Candle> data, List<int> signals)
    {
        if (data.Count != signals.Count)
        {
            throw new InvalidOperationException($"Mismatch between data count ({data.Count}) and signals count ({signals.Count}).");
        }

        decimal balance = 10000; // Starting capital
        decimal position = 0;
        decimal entryPrice = 0;

        var trades = new List<Trade>();

        for (int i = 0; i < data.Count; i++)
        {
            if (signals[i] == 1 && position == 0) // Buy signal
            {
                position = balance / data[i].Close;
                entryPrice = data[i].Close;
                balance = 0;

                trades.Add(new Trade
                {
                    EntryTime = data[i].Timestamp,
                    EntryPrice = entryPrice,
                    TradeType = "Buy"
                });

                Console.WriteLine($"Buy trade placed at {data[i].Timestamp}: Price={entryPrice}");
            }
            else if (signals[i] == -1 && position > 0) // Sell signal
            {
                balance = position * data[i].Close;

                trades[^1].ExitPrice = data[i].Close;
                trades[^1].ExitTime = data[i].Timestamp;
                trades[^1].Profit = balance - (position * entryPrice);

                position = 0;

                Console.WriteLine($"Sell trade placed at {data[i].Timestamp}: Price={data[i].Close}, Profit={trades[^1].Profit}");
            }
        }

        // If still holding a position, close it at the last price
        if (position > 0)
        {
            balance = position * data[^1].Close;

            trades[^1].ExitPrice = data[^1].Close;
            trades[^1].ExitTime = data[^1].Timestamp;
            trades[^1].Profit = balance - (position * entryPrice);

            Console.WriteLine($"Final sell trade placed at {data[^1].Timestamp}: Price={data[^1].Close}, Profit={trades[^1].Profit}");
        }

        return trades;
    }

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
        Console.WriteLine($"{"Trade Type",-10} {"Entry Time",-20} {"Entry Price",-15} {"Exit Time",-20} {"Exit Price",-15} {"Profit",-10}");
        Console.WriteLine(new string('-', 80));

        foreach (var trade in trades)
        {
            Console.WriteLine(
                $"{trade.TradeType,-10} {trade.EntryTime,-20} {trade.EntryPrice,-15:C} " +
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
