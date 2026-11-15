using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_period_balance")]
[Index("AccountId", Name = "fk_balance_account")]
[Index("ClosedBy", Name = "fk_balance_closer")]
[Index("PeriodId", "AccountId", Name = "uq_balance_period_account", IsUnique = true)]
public partial class TPeriodBalance
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("period_id")]
    public int PeriodId { get; set; }

    [Column("account_id")]
    public int AccountId { get; set; }

    /// <summary>
    /// Saldo awal = closing_balance periode sebelumnya
    /// </summary>
    [Column("opening_balance")]
    [Precision(15, 2)]
    public decimal OpeningBalance { get; set; }

    [Column("total_income")]
    [Precision(15, 2)]
    public decimal TotalIncome { get; set; }

    [Column("total_expense")]
    [Precision(15, 2)]
    public decimal TotalExpense { get; set; }

    /// <summary>
    /// Diisi saat periode ditutup
    /// </summary>
    [Column("closing_balance")]
    [Precision(15, 2)]
    public decimal? ClosingBalance { get; set; }

    /// <summary>
    /// 1=OPEN, 2=CLOSED
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("closed_at", TypeName = "datetime")]
    public DateTime? ClosedAt { get; set; }

    [Column("closed_by")]
    public int? ClosedBy { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("TPeriodBalances")]
    public virtual MFinancialAccount Account { get; set; } = null!;

    [ForeignKey("ClosedBy")]
    [InverseProperty("TPeriodBalances")]
    public virtual MUser? ClosedByNavigation { get; set; }

    [ForeignKey("PeriodId")]
    [InverseProperty("TPeriodBalances")]
    public virtual MManagementPeriod Period { get; set; } = null!;
}
