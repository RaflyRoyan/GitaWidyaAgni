using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_funding_realization")]
[Index("FundingRequestId", "RealizationDate", Name = "ix_realization_request")]
public partial class TFundingRealization
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("funding_request_id")]
    public int FundingRequestId { get; set; }

    [Column("realization_date")]
    public DateOnly RealizationDate { get; set; }

    /// <summary>
    /// mis. Konsumsi rapat 20 orang
    /// </summary>
    [Column("description")]
    [StringLength(255)]
    public string Description { get; set; } = null!;

    [Column("quantity")]
    [Precision(12, 2)]
    public decimal? Quantity { get; set; }

    [Column("unit")]
    [StringLength(30)]
    public string? Unit { get; set; }

    [Column("amount")]
    [Precision(15, 2)]
    public decimal Amount { get; set; }

    /// <summary>
    /// Foto / scan nota
    /// </summary>
    [Column("receipt", TypeName = "mediumblob")]
    public byte[]? Receipt { get; set; }

    [Column("receipt_file_name")]
    [StringLength(255)]
    public string? ReceiptFileName { get; set; }

    [Column("receipt_content_type")]
    [StringLength(100)]
    public string? ReceiptContentType { get; set; }

    [Column("notes")]
    [StringLength(255)]
    public string? Notes { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("FundingRequestId")]
    [InverseProperty("TFundingRealizations")]
    public virtual TFundingRequest FundingRequest { get; set; } = null!;
}
