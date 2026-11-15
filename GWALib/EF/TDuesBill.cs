using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_dues_bill")]
[Index("TransactionId", Name = "fk_duesbill_trx")]
[Index("MemberId", "Status", Name = "ix_dues_status")]
[Index("DuesSettingId", "MemberId", "BillPeriod", Name = "uq_dues_bill", IsUnique = true)]
public partial class TDuesBill
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("dues_setting_id")]
    public int DuesSettingId { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    /// <summary>
    /// mis. 2026-10, 2026-GANJIL, 2026
    /// </summary>
    [Column("bill_period")]
    [StringLength(20)]
    public string BillPeriod { get; set; } = null!;

    [Column("amount")]
    [Precision(15, 2)]
    public decimal Amount { get; set; }

    [Column("due_date")]
    public DateOnly? DueDate { get; set; }

    /// <summary>
    /// 1=UNPAID, 2=PAID, 3=WAIVED
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("paid_at", TypeName = "datetime")]
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Transaksi kas masuk saat dibayar
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

    [ForeignKey("DuesSettingId")]
    [InverseProperty("TDuesBills")]
    public virtual MDuesSetting DuesSetting { get; set; } = null!;

    [ForeignKey("MemberId")]
    [InverseProperty("TDuesBills")]
    public virtual MMember Member { get; set; } = null!;

    [ForeignKey("TransactionId")]
    [InverseProperty("TDuesBills")]
    public virtual TFinancialTransaction? Transaction { get; set; }
}
