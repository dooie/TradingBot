namespace TradingBot.Strategy;
public static class TechnicalIndicators
{
    public static List<decimal> CalculateSMA(List<decimal> prices, int period)
    {
        if (prices == null || prices.Count < period)
            throw new ArgumentException("Prices list is too short for the specified period.");

        var sma = new List<decimal>();
        for (int i = 0; i <= prices.Count - period; i++)
        {
            decimal periodSum = 0;
            for (int j = i; j < i + period; j++)
            {
                periodSum += prices[j];
            }
            sma.Add(periodSum / period);
        }

        return sma;
    }

    public static List<decimal> CalculateRSI(List<decimal> prices, int period = 14)
    {
        if (prices == null || prices.Count < period)
            throw new ArgumentException("Prices list is too short for the specified period.");

        var rsi = new List<decimal>();
        var gains = new List<decimal>();
        var losses = new List<decimal>();

        for (int i = 1; i < prices.Count; i++)
        {
            var difference = prices[i] - prices[i - 1];
            if (difference > 0) gains.Add(difference);
            else losses.Add(Math.Abs(difference));
        }

        decimal averageGain = gains.Take(period).Average();
        decimal averageLoss = losses.Take(period).Average();

        for (int i = period; i < prices.Count; i++)
        {
            decimal rs = averageGain / averageLoss;
            rsi.Add(100 - (100 / (1 + rs)));

            // Update average gain and loss for the next period
            averageGain = ((averageGain * (period - 1)) + (gains.ElementAtOrDefault(i) > 0 ? gains[i] : 0)) / period;
            averageLoss = ((averageLoss * (period - 1)) + (losses.ElementAtOrDefault(i) > 0 ? losses[i] : 0)) / period;
        }

        return rsi;
    }
}
