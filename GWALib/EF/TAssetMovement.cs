using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_asset_movement")]
[Index("AssetId", Name = "fk_movement_asset")]
[Index("CreatedBy", Name = "fk_movement_creator")]
[Index("AssetUnitId", Name = "fk_t_asset_movement_asset_unit_id")]
[Index("ReferenceType", "ReferenceId", Name = "ix_movement_ref")]
public partial class TAssetMovement
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("asset_id")]
    public int AssetId { get; set; }

    /// <summary>
    /// Unit yang bergerak (aset track_unit=1)
    /// </summary>
    [Column("asset_unit_id")]
    public int? AssetUnitId { get; set; }

    /// <summary>
    /// 1=IN, 2=OUT, 3=BORROW, 4=RETURN, 5=ADJUSTMENT, 6=DISPOSAL
    /// </summary>
    [Column("movement_type")]
    public int MovementType { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("movement_date", TypeName = "datetime")]
    public DateTime MovementDate { get; set; }

    [Column("reference_type")]
    [StringLength(50)]
    public string? ReferenceType { get; set; }

    [Column("reference_id")]
    public int? ReferenceId { get; set; }

    [Column("notes")]
    [StringLength(255)]
    public string? Notes { get; set; }

    [Column("created_by")]
    public int CreatedBy { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("AssetId")]
    [InverseProperty("TAssetMovements")]
    public virtual MAsset Asset { get; set; } = null!;

    [ForeignKey("AssetUnitId")]
    [InverseProperty("TAssetMovements")]
    public virtual MAssetUnit? AssetUnit { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("TAssetMovements")]
    public virtual MUser CreatedByNavigation { get; set; } = null!;
}
