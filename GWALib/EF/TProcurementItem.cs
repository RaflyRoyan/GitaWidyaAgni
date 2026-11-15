using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_procurement_item")]
[Index("AssetCategoryId", Name = "fk_pgditem_category")]
[Index("ExternalPartyId", Name = "fk_pgditem_party")]
[Index("RequestId", Name = "fk_pgditem_request")]
public partial class TProcurementItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("request_id")]
    public int RequestId { get; set; }

    [Column("item_name")]
    [StringLength(200)]
    public string ItemName { get; set; } = null!;

    /// <summary>
    /// Merek, ukuran, spesifikasi
    /// </summary>
    [Column("specification", TypeName = "text")]
    public string? Specification { get; set; }

    /// <summary>
    /// Wajib jika is_asset = 1
    /// </summary>
    [Column("asset_category_id")]
    public int? AssetCategoryId { get; set; }

    /// <summary>
    /// 0=Habis pakai, 1=Masuk inventaris sebagai aset
    /// </summary>
    [Column("is_asset")]
    public int IsAsset { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// pcs, set, lusin, dll.
    /// </summary>
    [Column("unit")]
    [StringLength(30)]
    public string Unit { get; set; } = null!;

    [Column("estimated_unit_price")]
    [Precision(15, 2)]
    public decimal EstimatedUnitPrice { get; set; }

    [Column("actual_unit_price")]
    [Precision(15, 2)]
    public decimal? ActualUnitPrice { get; set; }

    /// <summary>
    /// Vendor (m_partner)
    /// </summary>
    [Column("external_party_id")]
    public int? ExternalPartyId { get; set; }

    /// <summary>
    /// Dihitung dari penerimaan, tidak boleh melebihi quantity
    /// </summary>
    [Column("received_quantity")]
    public int ReceivedQuantity { get; set; }

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

    [ForeignKey("AssetCategoryId")]
    [InverseProperty("TProcurementItems")]
    public virtual MAssetCategory? AssetCategory { get; set; }

    [ForeignKey("ExternalPartyId")]
    [InverseProperty("TProcurementItems")]
    public virtual MExternalParty? ExternalParty { get; set; }

    [InverseProperty("ProcurementItem")]
    public virtual ICollection<MAsset> MAssets { get; set; } = new List<MAsset>();

    [ForeignKey("RequestId")]
    [InverseProperty("TProcurementItems")]
    public virtual TProcurementRequest Request { get; set; } = null!;

    [InverseProperty("Item")]
    public virtual ICollection<TProcurementQuotation> TProcurementQuotations { get; set; } = new List<TProcurementQuotation>();

    [InverseProperty("Item")]
    public virtual ICollection<TProcurementReceiptItem> TProcurementReceiptItems { get; set; } = new List<TProcurementReceiptItem>();
}
