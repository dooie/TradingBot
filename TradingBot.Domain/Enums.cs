namespace TradingBot.Domain;
public enum TimeFrame
{
    Second,
    Minute,
    Hour,
    Day,
    Week,
    Month,
    Quarter,
    Year
}

public enum AssetClass
{
    Stocks,
    Options,
    Crypto,
    Fx,
    Indices
}

public enum Locale
{
    US,
    Global
}