using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

/// <summary>
/// Unit fisik aset berkode QR
/// </summary>
[Table("m_asset_unit")]
[Index("AssetId", Name = "fk_m_asset_unit_asset_id")]
[Index("UnitCode", Name = "uq_asset_unit_code", IsUnique = true)]
[Index("QrToken", Name = "uq_asset_unit_qr_token", IsUnique = true)]
public partial class MAssetUnit
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("asset_id")]
    public int AssetId { get; set; }

    /// <summary>
    /// Kode inventaris unit: unit_prefix + nomor urut (dicetak di label; QR berisi token)
    /// </summary>
    [Column("unit_code")]
    [StringLength(50)]
    public string UnitCode { get; set; } = null!;

    /// <summary>
    /// Token acak 128-bit (hex huruf besar) untuk URL QR /aset/{token} (migrasi 2026-11-18_asset_qr_token, ditulis tangan)
    /// </summary>
    [Column("qr_token")]
    [StringLength(32)]
    public string QrToken { get; set; } = null!;

    /// <summary>
    /// Nomor seri pabrik (opsional)
    /// </summary>
    [Column("serial_number")]
    [StringLength(100)]
    public string? SerialNumber { get; set; }

    /// <summary>
    /// 1=BAIK, 2=RUSAK_RINGAN, 3=RUSAK_BERAT
    /// </summary>
    [Column("unit_condition")]
    public int UnitCondition { get; set; }

    /// <summary>
    /// 1=TERSEDIA, 2=DIPINJAM, 3=PERAWATAN, 4=HILANG, 5=DIHAPUS
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    /// <summary>
    /// Kapan label QR dicetak (NULL = belum)
    /// </summary>
    [Column("label_printed_at", TypeName = "datetime")]
    public DateTime? LabelPrintedAt { get; set; }

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

    [ForeignKey("AssetId")]
    [InverseProperty("MAssetUnits")]
    public virtual MAsset Asset { get; set; } = null!;

    [InverseProperty("AssetUnit")]
    public virtual ICollection<TAssetInspection> TAssetInspections { get; set; } = new List<TAssetInspection>();

    [InverseProperty("AssetUnit")]
    public virtual ICollection<TAssetMovement> TAssetMovements { get; set; } = new List<TAssetMovement>();

    [InverseProperty("AssetUnit")]
    public virtual ICollection<TBorrowItemUnit> TBorrowItemUnits { get; set; } = new List<TBorrowItemUnit>();

    [InverseProperty("AssetUnit")]
    public virtual ICollection<TMaintenanceLog> TMaintenanceLogs { get; set; } = new List<TMaintenanceLog>();
}
