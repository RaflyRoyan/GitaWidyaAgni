using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_asset_inspection")]
[Index("AssetId", Name = "fk_inspection_asset")]
[Index("BorrowItemId", Name = "fk_inspection_borrow")]
[Index("InspectedBy", Name = "fk_inspection_inspector")]
[Index("AssetUnitId", Name = "fk_t_asset_inspection_asset_unit_id")]
public partial class TAssetInspection
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("asset_id")]
    public int AssetId { get; set; }

    /// <summary>
    /// Unit yang diinspeksi (aset track_unit=1)
    /// </summary>
    [Column("asset_unit_id")]
    public int? AssetUnitId { get; set; }

    /// <summary>
    /// Terisi jika inspeksi saat pengembalian
    /// </summary>
    [Column("borrow_item_id")]
    public int? BorrowItemId { get; set; }

    [Column("inspected_by")]
    public int InspectedBy { get; set; }

    [Column("inspected_at", TypeName = "datetime")]
    public DateTime InspectedAt { get; set; }

    /// <summary>
    /// 1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG
    /// </summary>
    [Column("item_condition")]
    public int ItemCondition { get; set; }

    [Column("notes")]
    [StringLength(255)]
    public string? Notes { get; set; }

    [Column("photo", TypeName = "mediumblob")]
    public byte[]? Photo { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("AssetId")]
    [InverseProperty("TAssetInspections")]
    public virtual MAsset Asset { get; set; } = null!;

    [ForeignKey("AssetUnitId")]
    [InverseProperty("TAssetInspections")]
    public virtual MAssetUnit? AssetUnit { get; set; }

    [ForeignKey("BorrowItemId")]
    [InverseProperty("TAssetInspections")]
    public virtual TBorrowItem? BorrowItem { get; set; }

    [ForeignKey("InspectedBy")]
    [InverseProperty("TAssetInspections")]
    public virtual MUser InspectedByNavigation { get; set; } = null!;
}
