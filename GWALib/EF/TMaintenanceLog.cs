using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_maintenance_log")]
[Index("AssetId", Name = "fk_maintenance_asset")]
[Index("TransactionId", Name = "fk_maintenance_trx")]
[Index("AssetUnitId", Name = "fk_t_maintenance_log_asset_unit_id")]
public partial class TMaintenanceLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("asset_id")]
    public int AssetId { get; set; }

    /// <summary>
    /// Unit yang dirawat (aset track_unit=1)
    /// </summary>
    [Column("asset_unit_id")]
    public int? AssetUnitId { get; set; }

    [Column("maintenance_date")]
    public DateOnly MaintenanceDate { get; set; }

    [Column("description", TypeName = "text")]
    public string Description { get; set; } = null!;

    [Column("vendor")]
    [StringLength(150)]
    public string? Vendor { get; set; }

    [Column("cost")]
    [Precision(15, 2)]
    public decimal Cost { get; set; }

    /// <summary>
    /// 1=SCHEDULED, 2=IN_PROGRESS, 3=DONE
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("finished_at", TypeName = "datetime")]
    public DateTime? FinishedAt { get; set; }

    /// <summary>
    /// Link ke pengeluaran kas jika ada biaya
    /// </summary>
    [Column("transaction_id")]
    public int? TransactionId { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("AssetId")]
    [InverseProperty("TMaintenanceLogs")]
    public virtual MAsset Asset { get; set; } = null!;

    [ForeignKey("AssetUnitId")]
    [InverseProperty("TMaintenanceLogs")]
    public virtual MAssetUnit? AssetUnit { get; set; }

    [ForeignKey("TransactionId")]
    [InverseProperty("TMaintenanceLogs")]
    public virtual TFinancialTransaction? Transaction { get; set; }
}
