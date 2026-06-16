using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockDataService.Entities;

[Table("stock_history")]
public class StockHistoryRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    public DateTime Timestamp { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Open { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal High { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Low { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Close { get; set; }

    public long Volume { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}