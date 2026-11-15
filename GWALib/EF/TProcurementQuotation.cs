using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_procurement_quotation")]
[Index("ItemId", Name = "fk_quote_item")]
[Index("ExternalPartyId", Name = "fk_quote_party")]
[Index("RequestId", Name = "fk_quote_request")]
public partial class TProcurementQuotation
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("request_id")]
    public int RequestId { get; set; }

    /// <summary>
    /// NULL = penawaran untuk seluruh usulan
    /// </summary>
    [Column("item_id")]
    public int? ItemId { get; set; }

    [Column("external_party_id")]
    public int? ExternalPartyId { get; set; }

    /// <summary>
    /// Jika vendor belum terdaftar di m_partner
    /// </summary>
    [Column("vendor_name")]
    [StringLength(200)]
    public string? VendorName { get; set; }

    [Column("quoted_amount")]
    [Precision(15, 2)]
    public decimal QuotedAmount { get; set; }

    [Column("file", TypeName = "mediumblob")]
    public byte[]? File { get; set; }

    [Column("file_name")]
    [StringLength(255)]
    public string? FileName { get; set; }

    [Column("content_type")]
    [StringLength(100)]
    public string? ContentType { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya
    /// </summary>
    [Column("is_selected")]
    public int IsSelected { get; set; }

    [Column("notes")]
    [StringLength(500)]
    public string? Notes { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ExternalPartyId")]
    [InverseProperty("TProcurementQuotations")]
    public virtual MExternalParty? ExternalParty { get; set; }

    [ForeignKey("ItemId")]
    [InverseProperty("TProcurementQuotations")]
    public virtual TProcurementItem? Item { get; set; }

    [ForeignKey("RequestId")]
    [InverseProperty("TProcurementQuotations")]
    public virtual TProcurementRequest Request { get; set; } = null!;
}
