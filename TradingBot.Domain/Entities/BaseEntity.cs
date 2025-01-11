using System.ComponentModel.DataAnnotations;

namespace TradingBot.Domain.Entities;
public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
    [Required]
    public DateTime CreateDate { get; set; }
}
