using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

/// <summary>
/// Unit yang diambil dan dikembalikan per peminjaman
/// </summary>
[Table("t_borrow_item_unit")]
[Index("AssetUnitId", Name = "fk_t_borrow_item_unit_asset_unit_id")]
[Index("BorrowItemId", "AssetUnitId", Name = "uq_borrow_item_unit", IsUnique = true)]
public partial class TBorrowItemUnit
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("borrow_item_id")]
    public int BorrowItemId { get; set; }

    [Column("asset_unit_id")]
    public int AssetUnitId { get; set; }

    /// <summary>
    /// Waktu unit di-scan saat diambil
    /// </summary>
    [Column("taken_at", TypeName = "datetime")]
    public DateTime TakenAt { get; set; }

    /// <summary>
    /// 1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT
    /// </summary>
    [Column("condition_out")]
    public int ConditionOut { get; set; }

    /// <summary>
    /// Waktu unit di-scan saat dikembalikan
    /// </summary>
    [Column("returned_at", TypeName = "datetime")]
    public DateTime? ReturnedAt { get; set; }

    /// <summary>
    /// 1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT
    /// </summary>
    [Column("condition_in")]
    public int? ConditionIn { get; set; }

    /// <summary>
    /// 1=BELUM_KEMBALI, 2=KEMBALI, 3=KEMBALI_RUSAK, 4=HILANG
    /// </summary>
    [Column("return_status")]
    public int ReturnStatus { get; set; }

    /// <summary>
    /// MAX(0, CEIL(hari returned_at - expected_return_date))
    /// </summary>
    [Column("late_days")]
    public int LateDays { get; set; }

    /// <summary>
    /// late_days x t_borrow_item.late_fee_per_day
    /// </summary>
    [Column("late_fee")]
    [Precision(15, 2)]
    public decimal LateFee { get; set; }

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

    [ForeignKey("AssetUnitId")]
    [InverseProperty("TBorrowItemUnits")]
    public virtual MAssetUnit AssetUnit { get; set; } = null!;

    [ForeignKey("BorrowItemId")]
    [InverseProperty("TBorrowItemUnits")]
    public virtual TBorrowItem BorrowItem { get; set; } = null!;
}
