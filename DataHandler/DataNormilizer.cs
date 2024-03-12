using System;
using System.Collections.Generic;

namespace TradingBot.DataHandler;
/// <summary>
/// code provides a basic structure for price normalization by dividing each 
/// price by the first price in the dataset, essentially setting the first 
/// price as a base (100%) and calculating subsequent prices relative to this base. 
/// This approach helps in comparing price movements over time on a relative scale.
/// </summary>
public class DataNormalizer
{
    public Dictionary<string, decimal> NormalizePrices(List<decimal> prices)
    {

        if (prices == null || prices.Count == 0)
            throw new ArgumentException("Prices list cannot be null or empty.");

        decimal basePrice = prices[0]; // Assuming the first price is the base for normalization.
        var normalizedPrices = new Dictionary<string, decimal>();

        for (int i = 0; i < prices.Count; i++)
        {
            decimal normalizedPrice = prices[i] / basePrice;
            normalizedPrices.Add($"Price_{i}", normalizedPrice);
        }

        return normalizedPrices;
    }

    /// <summary>
    /// This method scales the data within a specified range (typically 0 to 1), 
    /// maintaining the distribution of the original data while controlling for scale. 
    /// It's especially useful when different parameters have vastly different scales.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public List<decimal> MinMaxScaling(List<decimal> data)
    {
        if (data == null || !data.Any())
            throw new ArgumentException("Data list cannot be null or empty.");

        decimal min = data.Min();
        decimal max = data.Max();

        var scaledData = data.Select(d => (d - min) / (max - min)).ToList();
        return scaledData;
    }

    /// <summary>
    /// This method normalizes the data based on the mean and standard deviation of the dataset. 
    /// It transforms the data into a distribution with a mean of 0 and a standard deviation of 1, 
    /// useful for models that assume the distribution of input features is normal.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public List<decimal> ZScoreNormalization(List<decimal> data)
    {
        if (data == null || !data.Any())
            throw new ArgumentException("Data list cannot be null or empty.");

        double mean = (double)data.Average();
        double standardDeviation = Math.Sqrt(data.Select(d => Math.Pow((double)d - mean, 2)).Average());

        var normalizedData = data.Select(d => (decimal)((double)d - mean) / (decimal)standardDeviation).ToList();
        return normalizedData;
    }
}
