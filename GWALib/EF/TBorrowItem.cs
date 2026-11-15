using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_borrow_item")]
[Index("AssetId", Name = "fk_borrowitem_asset")]
[Index("BorrowRequestId", "AssetId", Name = "uq_borrow_item", IsUnique = true)]
public partial class TBorrowItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("borrow_request_id")]
    public int BorrowRequestId { get; set; }

    [Column("asset_id")]
    public int AssetId { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("returned_quantity")]
    public int ReturnedQuantity { get; set; }

    /// <summary>
    /// 1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG
    /// </summary>
    [Column("condition_on_borrow")]
    public int? ConditionOnBorrow { get; set; }

    /// <summary>
    /// 1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG
    /// </summary>
    [Column("condition_on_return")]
    public int? ConditionOnReturn { get; set; }

    [Column("returned_at", TypeName = "datetime")]
    public DateTime? ReturnedAt { get; set; }

    /// <summary>
    /// Salinan tarif sewa saat pinjam
    /// </summary>
    [Column("rental_fee_per_day")]
    [Precision(15, 2)]
    public decimal RentalFeePerDay { get; set; }

    /// <summary>
    /// Lama sewa (hari)
    /// </summary>
    [Column("rental_days")]
    public int RentalDays { get; set; }

    /// <summary>
    /// rental_days x rental_fee_per_day x quantity
    /// </summary>
    [Column("rental_fee")]
    [Precision(15, 2)]
    public decimal RentalFee { get; set; }

    /// <summary>
    /// Salinan tarif denda saat pinjam
    /// </summary>
    [Column("late_fee_per_day")]
    [Precision(15, 2)]
    public decimal LateFeePerDay { get; set; }

    /// <summary>
    /// Hari terlambat (track_unit=0); untuk track_unit=1 dari unit terakhir
    /// </summary>
    [Column("late_days")]
    public int LateDays { get; set; }

    /// <summary>
    /// track_unit=0: late_days x late_fee_per_day x quantity; track_unit=1: SUM(t_borrow_item_unit.late_fee)
    /// </summary>
    [Column("late_fee")]
    [Precision(15, 2)]
    public decimal LateFee { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("AssetId")]
    [InverseProperty("TBorrowItems")]
    public virtual MAsset Asset { get; set; } = null!;

    [ForeignKey("BorrowRequestId")]
    [InverseProperty("TBorrowItems")]
    public virtual TBorrowRequest BorrowRequest { get; set; } = null!;

    [InverseProperty("BorrowItem")]
    public virtual ICollection<TAssetInspection> TAssetInspections { get; set; } = new List<TAssetInspection>();

    [InverseProperty("BorrowItem")]
    public virtual ICollection<TBorrowItemUnit> TBorrowItemUnits { get; set; } = new List<TBorrowItemUnit>();
}
