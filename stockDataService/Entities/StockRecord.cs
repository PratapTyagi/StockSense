using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Interfaces;

namespace StockDataService.Entities;

[Table("Stocks", Schema = "dbo")]
public class StockRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "nvarchar(255)")]
    public string InstrumentToken { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    public string Exchange { get; set; } = string.Empty;

    [Required]
    [Column("COMPANY_NAME", TypeName = "nvarchar(255)")]
    public string CompanyName { get; set; } = string.Empty;

    [Column(TypeName = "bit")]
    public bool IsActive { get; set; }
}