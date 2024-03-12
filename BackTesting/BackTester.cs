using System;
using System.Collections.Generic;
using System.Linq;
using TradingBot.Strategy;

namespace TradingBot.BackTesting;

public class BackTester
{
    private List<decimal> _historicalPrices;
    private decimal _initialCash;
    private decimal _cash;
    private int _position;
    private decimal _transactionCost;
    private decimal _slippageFactor;
    private List<decimal> _equityCurve;
    private List<decimal> _returns;

    public BackTester(List<decimal> historicalPrices, decimal initialCash, decimal transactionCost = 0.0005m, decimal slippageFactor = 0.0001m)
    {
        _historicalPrices = historicalPrices ?? throw new ArgumentNullException(nameof(historicalPrices));
        _initialCash = initialCash;
        _cash = initialCash;
        _transactionCost = transactionCost;
        _slippageFactor = slippageFactor;
        _position = 0;
        _equityCurve = new List<decimal>();
        _returns = new List<decimal>();
    }

    public void Run(IStrategy strategy)
    {
        for (int i = 0; i < _historicalPrices.Count; i++)
        {
            decimal price = _historicalPrices[i];
            var adjustedPrice = AdjustForSlippageAndCosts(price);
            var signal = strategy.Evaluate(price);

            ExecuteTrade(signal, adjustedPrice);
            decimal portfolioValue = _cash + (_position * adjustedPrice);
            _equityCurve.Add(portfolioValue);

            if (i > 0)
            {
                decimal dailyReturn = (portfolioValue / _equityCurve[i - 1]) - 1;
                _returns.Add(dailyReturn);
            }
        }

        DisplayResults();
    }

    private decimal AdjustForSlippageAndCosts(decimal price)
    {
        decimal slippage = price * _slippageFactor;
        return price + slippage + _transactionCost;
    }

    private void ExecuteTrade(TradeSignal signal, decimal price)
    {
        switch (signal)
        {
            case TradeSignal.Buy:
                // Assuming fixed quantity for simplicity
                if (_cash >= price)
                {
                    _position++;
                    _cash -= price;
                }
                break;
            case TradeSignal.Sell:
                if (_position > 0)
                {
                    _position--;
                    _cash += price;
                }
                break;
            case TradeSignal.Hold:
            default:
                // No action
                break;
        }
    }

    private void DisplayResults()
    {
        var totalReturn = _cash + (_position * _historicalPrices.Last()) - _initialCash;
        var annualReturn = Math.Pow((double)(_cash + (_position * _historicalPrices.Last()) / _initialCash), (1.0 / (_historicalPrices.Count / 252.0))) - 1;
        var averageDailyReturn = _returns.Average();
        var stdDevDailyReturns = Math.Sqrt(_returns.Select(r => Math.Pow((double)r - (double)averageDailyReturn, 2)).Average());
        var sharpeRatio = ((double)averageDailyReturn / stdDevDailyReturns) * Math.Sqrt(252);

        Console.WriteLine($"Total Return: ${totalReturn}");
        Console.WriteLine($"Annual Return: {annualReturn:P2}");
        Console.WriteLine($"Average Daily Return: {averageDailyReturn:P2}");
        Console.WriteLine($"Standard Deviation of Daily Returns: {stdDevDailyReturns:P2}");
        Console.WriteLine($"Sharpe Ratio: {sharpeRatio:F2}");
    }
}

public interface IStrategy
{
    TradeSignal Evaluate(decimal price);
}

public enum TradeSignal
{
    Buy,
    Sell,
    Hold
}
