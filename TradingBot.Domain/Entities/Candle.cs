using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace TradingBot.Domain.Entities;

public class Candle : BaseEntity
{
    [Required]
    [JsonProperty("t")]
    public long Timestamp { get; set; }
    [Required]
    [JsonProperty("o")]
    public decimal Open { get; set; }
    [Required]
    [JsonProperty("h")]
    public decimal High { get; set; }
    [Required]
    [JsonProperty("l")]
    public decimal Low { get; set; }
    [Required]
    [JsonProperty("c")]
    public decimal Close { get; set; }
    [Required]
    [JsonProperty("v")]
    public long Volume { get; set; }
    [Required]
    [JsonProperty("vw")]
    public long VolumeWeight { get; set; }
    [Required]
    [JsonProperty("n")]
    public int Number { get; set; }
}
