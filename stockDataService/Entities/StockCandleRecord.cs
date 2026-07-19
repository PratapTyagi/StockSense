using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockDataService.Entities;

[Table("StockCandles", Schema = "dbo")]
public class StockCandleRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public long StockId { get; set; }

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

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(StockId))]
    public virtual StockRecord? Stock { get; set; }
}