using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingBot.Domain.Entities;

namespace ConsoleApp.PoC;
public static class IndicatorHelper
{
    public static decimal CalculateRSI(List<Candle> candles, int period)
    {
        if (candles.Count < period + 1)
        {
            Console.WriteLine("Not enough data points for RSI calculation.");
            return 50; // Neutral RSI
        }

        decimal gain = 0;
        decimal loss = 0;

        // Calculate initial average gain and loss
        for (int i = 1; i <= period; i++)
        {
            decimal change = candles[i].Close - candles[i - 1].Close;
            if (change > 0)
                gain += change;
            else
                loss -= change;
        }

        decimal avgGain = gain / period;
        decimal avgLoss = loss / period;

        // Compute RSI for subsequent periods
        for (int i = period + 1; i < candles.Count; i++)
        {
            decimal change = candles[i].Close - candles[i - 1].Close;
            if (change > 0)
            {
                avgGain = (avgGain * (period - 1) + change) / period;
                avgLoss = (avgLoss * (period - 1)) / period; // Ensure avgLoss is decayed properly
            }
            else
            {
                avgGain = (avgGain * (period - 1)) / period; // Ensure avgGain is decayed properly
                avgLoss = (avgLoss * (period - 1) - change) / period;
            }
        }

        if (avgLoss == 0) // Prevent division by zero
        {
            return 100;
        }

        decimal rs = avgGain / avgLoss;
        var rsi = 100 - (100 / (1 + rs));

        //Console.WriteLine($"RSI calculated: {rsi}");
        return rsi;
    }
    public static (List<decimal> MacdValues, List<decimal> SignalValues) CalculateMACD(List<Candle> candles, int shortPeriod, int longPeriod, int signalPeriod)
    {
        var shortEmaValues = CalculateEMAValues(candles.Select(c => c.Close).ToList(), shortPeriod);
        var longEmaValues = CalculateEMAValues(candles.Select(c => c.Close).ToList(), longPeriod);

        // Calculate MACD values
        var macdValues = shortEmaValues.Zip(longEmaValues, (shortEma, longEma) => shortEma - longEma).ToList();

        // Calculate Signal line as EMA of MACD values
        var signalValues = CalculateEMAValues(macdValues, signalPeriod);

        Console.WriteLine($"MACD Values: {string.Join(", ", macdValues.Take(5))}...");
        Console.WriteLine($"Signal Line Values: {string.Join(", ", signalValues.Take(5))}...");

        return (macdValues, signalValues);
    }
    private static List<decimal> CalculateEMAValues(List<decimal> prices, int period)
    {
        if (prices.Count < period)
        {
            throw new ArgumentException($"Not enough data points for EMA calculation. Required: {period}, Available: {prices.Count}");
        }

        var emaValues = new List<decimal>();
        decimal multiplier = 2m / (period + 1);

        // Initialize EMA with the first price
        emaValues.Add(prices[0]);

        for (int i = 1; i < prices.Count; i++)
        {
            decimal ema = ((prices[i] - emaValues[i - 1]) * multiplier) + emaValues[i - 1];
            emaValues.Add(ema);
        }

        return emaValues;
    }
    public static decimal CalculateEMA(List<Candle> candles, int period)
    {
        decimal multiplier = 2m / (period + 1);
        decimal ema = candles[0].Close;

        for (int i = 1; i < candles.Count; i++)
        {
            ema = (candles[i].Close - ema) * multiplier + ema;
        }

        return ema;
    }
    public static decimal CalculateSMA(List<Candle> candles, int period)
    {
        return candles.Take(period).Average(c => c.Close);
    }
    public static decimal CalculateATR(List<Candle> candles, int period)
    {
        if (candles.Count < period)
        {
            throw new ArgumentException($"Not enough data points for ATR calculation. Required: {period}, Available: {candles.Count}");
        }

        var trueRanges = new List<decimal>();

        for (int i = 1; i < candles.Count; i++)
        {
            decimal highLow = candles[i].High - candles[i].Low;
            decimal highClose = Math.Abs(candles[i].High - candles[i - 1].Close);
            decimal lowClose = Math.Abs(candles[i].Low - candles[i - 1].Close);

            trueRanges.Add(Math.Max(highLow, Math.Max(highClose, lowClose)));
        }

        return trueRanges.Take(period).Average();
    }

}
