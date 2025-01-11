using Newtonsoft.Json;

namespace TradingBot.Domain.Entities;
public class TickerType
{
    [JsonProperty("asset_class")]
    public AssetClass AssetClass { get; set; }

    [JsonProperty("code")]
    public string? Code { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("locale")]
    public Locale Locale { get; set; }
}
