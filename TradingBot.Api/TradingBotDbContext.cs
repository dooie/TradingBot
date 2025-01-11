namespace TradingBot.Api;

using Microsoft.EntityFrameworkCore;
using TradingBot.Domain.Entities;

public partial class TradingBotDbContext : DbContext
{
    public TradingBotDbContext(DbContextOptions
    <TradingBotDbContext> options)
        : base(options)
    {
    }
    public virtual DbSet<Candle> Candles { get; set; }
    public virtual DbSet<Ticker> Tickers { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Candle>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(e => e.Close).HasPrecision(18, 4);
            entity.Property(e => e.High).HasPrecision(18, 4);
            entity.Property(e => e.Low).HasPrecision(18, 4);
            entity.Property(e => e.Open).HasPrecision(18, 4);
        });

        modelBuilder.Entity<Ticker>(entity =>
        {
            entity.HasKey(c => c.Id);
        });
        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

