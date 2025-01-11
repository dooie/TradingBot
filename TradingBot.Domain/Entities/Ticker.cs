using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingBot.Domain.Entities;
public class Ticker : BaseEntity
{
    public string? ShortName { get; set; }
    public string? LongName { get; set; }
    public List<Candle> DataCandles { get; set; } = new List<Candle>();
}
