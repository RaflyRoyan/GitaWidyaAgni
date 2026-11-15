using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_asset")]
[Index("CategoryId", Name = "fk_asset_category")]
[Index("ProcurementItemId", Name = "fk_asset_pgditem")]
[Index("SponsorshipId", Name = "fk_asset_sponsorship")]
[Index("AssetCode", Name = "uq_asset_code", IsUnique = true)]
public partial class MAsset
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("asset_code")]
    [StringLength(50)]
    public string AssetCode { get; set; } = null!;

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Column("brand")]
    [StringLength(100)]
    public string? Brand { get; set; }

    [Column("specification")]
    [StringLength(2000)]
    public string? Specification { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("unit")]
    [StringLength(30)]
    public string Unit { get; set; } = null!;

    /// <summary>
    /// 1=dicatat per unit (punya kode/QR di m_asset_unit), 0=cukup jumlah
    /// </summary>
    [Column("track_unit")]
    public int TrackUnit { get; set; }

    /// <summary>
    /// Awalan kode unit, mis. HT, KAM
    /// </summary>
    [Column("unit_prefix")]
    [StringLength(20)]
    public string? UnitPrefix { get; set; }

    [Column("acquisition_date")]
    public DateOnly? AcquisitionDate { get; set; }

    [Column("acquisition_cost")]
    [Precision(15, 2)]
    public decimal? AcquisitionCost { get; set; }

    /// <summary>
    /// 1=BELI, 2=HIBAH, 3=SPONSOR, 4=LAINNYA
    /// </summary>
    [Column("acquisition_source")]
    public int AcquisitionSource { get; set; }

    /// <summary>
    /// Asal dari item pengadaan
    /// </summary>
    [Column("procurement_item_id")]
    public int? ProcurementItemId { get; set; }

    /// <summary>
    /// Asal dari sponsor barang
    /// </summary>
    [Column("sponsorship_id")]
    public int? SponsorshipId { get; set; }

    /// <summary>
    /// Tarif sewa per hari per unit (0 = gratis)
    /// </summary>
    [Column("rental_fee_per_day")]
    [Precision(15, 2)]
    public decimal RentalFeePerDay { get; set; }

    /// <summary>
    /// Denda telat per hari per unit (0 = tanpa denda)
    /// </summary>
    [Column("late_fee_per_day")]
    [Precision(15, 2)]
    public decimal LateFeePerDay { get; set; }

    [Column("location")]
    [StringLength(150)]
    public string? Location { get; set; }

    /// <summary>
    /// 1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT, 4=HILANG
    /// </summary>
    [Column("asset_condition")]
    public int AssetCondition { get; set; }

    /// <summary>
    /// 1=AVAILABLE, 2=BORROWED, 3=MAINTENANCE, 4=DISPOSED
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("MAssets")]
    public virtual MAssetCategory Category { get; set; } = null!;

    [InverseProperty("Asset")]
    public virtual ICollection<MAssetPhoto> MAssetPhotos { get; set; } = new List<MAssetPhoto>();

    [InverseProperty("Asset")]
    public virtual ICollection<MAssetUnit> MAssetUnits { get; set; } = new List<MAssetUnit>();

    [ForeignKey("ProcurementItemId")]
    [InverseProperty("MAssets")]
    public virtual TProcurementItem? ProcurementItem { get; set; }

    [ForeignKey("SponsorshipId")]
    [InverseProperty("MAssets")]
    public virtual TSponsorship? Sponsorship { get; set; }

    [InverseProperty("Asset")]
    public virtual ICollection<TAssetInspection> TAssetInspections { get; set; } = new List<TAssetInspection>();

    [InverseProperty("Asset")]
    public virtual ICollection<TAssetMovement> TAssetMovements { get; set; } = new List<TAssetMovement>();

    [InverseProperty("Asset")]
    public virtual ICollection<TBorrowItem> TBorrowItems { get; set; } = new List<TBorrowItem>();

    [InverseProperty("Asset")]
    public virtual ICollection<TMaintenanceLog> TMaintenanceLogs { get; set; } = new List<TMaintenanceLog>();
}
