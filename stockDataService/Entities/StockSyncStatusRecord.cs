using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockDataService.Entities;

[Table("StockSyncStatus", Schema = "dbo")]
public class StockSyncStatusRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public long Stock_Id { get; set; }

    [Column(TypeName = "nvarchar(50)")]
    public string Symbol { get; set; } = string.Empty;

    public DateTime LastCandleTimestamp { get; set; }

    public DateTime? LastFundamentalSync { get; set; }

    [ForeignKey("Stock_Id")]
    public virtual StockRecord? Stock { get; set; }
}