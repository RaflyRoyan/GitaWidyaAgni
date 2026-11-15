using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_dues_setting")]
[Index("CategoryId", Name = "fk_dues_category")]
[Index("PeriodId", Name = "fk_dues_period")]
public partial class MDuesSetting
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("period_id")]
    public int? PeriodId { get; set; }

    /// <summary>
    /// mis. Iuran Bulanan 2026
    /// </summary>
    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("amount")]
    [Precision(15, 2)]
    public decimal Amount { get; set; }

    /// <summary>
    /// 1=MONTHLY, 2=SEMESTER, 3=YEARLY, 4=ONCE
    /// </summary>
    [Column("frequency")]
    public int Frequency { get; set; }

    /// <summary>
    /// Kategori pemasukan saat iuran dibayar
    /// </summary>
    [Column("category_id")]
    public int? CategoryId { get; set; }

    /// <summary>
    /// 0=Tidak Aktif, 1=Aktif
    /// </summary>
    [Column("is_active")]
    public int IsActive { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("MDuesSettings")]
    public virtual MFinancialCategory? Category { get; set; }

    [ForeignKey("PeriodId")]
    [InverseProperty("MDuesSettings")]
    public virtual MManagementPeriod? Period { get; set; }

    [InverseProperty("DuesSetting")]
    public virtual ICollection<TDuesBill> TDuesBills { get; set; } = new List<TDuesBill>();
}
