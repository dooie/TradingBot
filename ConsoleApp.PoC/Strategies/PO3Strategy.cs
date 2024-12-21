using ConsoleApp.PoC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.PoC.Strategies;
internal static class PO3Strategy
{
    public static bool IsAccumulation(List<Candle> candles)
    {
        int period = 10; // Number of candles to consider for accumulation
        decimal rangeThreshold = 1.5m; // Adjust this threshold as needed

        if (candles.Count < period)
            return false;

        decimal range = candles.TakeLast(period).Max(c => c.High) - candles.TakeLast(period).Min(c => c.Low);
        decimal avgRange = candles.TakeLast(period).Average(c => c.High - c.Low);

        bool isAccumulating = range < rangeThreshold && avgRange < rangeThreshold / 2;
        Console.WriteLine($"Accumulation detected: {isAccumulating} (Range={range}, AvgRange={avgRange})");
        return isAccumulating;
    }
    public static bool IsManipulation(Candle candle, Candle previousCandle)
    {
        decimal breakoutFactor = 1.2m; // Manipulation defined as a move 20% larger than usual
        return (candle.High - previousCandle.High > breakoutFactor * (previousCandle.High - previousCandle.Low))
               || (previousCandle.Low - candle.Low > breakoutFactor * (previousCandle.High - previousCandle.Low));
    }
    public static bool IsDistribution(List<Candle> candles, decimal trendThreshold)
    {
        decimal avgClose = candles.Average(c => c.Close);
        decimal avgOpen = candles.Average(c => c.Open);

        return Math.Abs(avgClose - avgOpen) > trendThreshold;
    }
}
