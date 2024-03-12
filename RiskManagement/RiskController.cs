using System;

namespace TradingBot.RiskManagement;

public class RiskController
{
    private readonly decimal _accountBalance;
    private readonly decimal _riskPerTrade;

    public RiskController(decimal accountBalance, decimal riskPerTrade = 0.02m)
    {
        _accountBalance = accountBalance;
        _riskPerTrade = riskPerTrade;
    }

    public decimal CalculatePositionSize(decimal entryPrice, decimal stopLossPrice)
    {
        if (entryPrice <= 0 || stopLossPrice <= 0)
            throw new ArgumentException("Prices must be greater than 0.");

        decimal riskAmountPerShare = entryPrice - stopLossPrice;
        if (riskAmountPerShare <= 0)
            throw new ArgumentException("Stop loss must be less than entry price.");

        decimal totalRiskAmount = _accountBalance * _riskPerTrade;
        return Math.Floor(totalRiskAmount / riskAmountPerShare);
    }

    public decimal CalculateStopLossPrice(decimal entryPrice, decimal volatility, decimal riskMultiplier = 2)
    {
        if (entryPrice <= 0 || volatility <= 0)
            throw new ArgumentException("Entry price and volatility must be greater than 0.");

        // This is a simple volatility-based stop-loss calculation.
        // It assumes that a higher volatility requires a wider stop-loss to avoid being stopped out prematurely.
        return entryPrice - (volatility * riskMultiplier);
    }
}