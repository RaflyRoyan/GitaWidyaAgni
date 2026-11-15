using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_debt_payment")]
[Index("DebtId", Name = "fk_debtpay_debt")]
[Index("TransactionId", Name = "uq_debtpay_trx", IsUnique = true)]
public partial class TDebtPayment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("debt_id")]
    public int DebtId { get; set; }

    /// <summary>
    /// Transaksi pengeluaran untuk pembayaran
    /// </summary>
    [Column("transaction_id")]
    public int TransactionId { get; set; }

    [Column("amount")]
    [Precision(15, 2)]
    public decimal Amount { get; set; }

    [Column("paid_date")]
    public DateOnly PaidDate { get; set; }

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

    [ForeignKey("DebtId")]
    [InverseProperty("TDebtPayments")]
    public virtual TDebt Debt { get; set; } = null!;

    [ForeignKey("TransactionId")]
    [InverseProperty("TDebtPayment")]
    public virtual TFinancialTransaction Transaction { get; set; } = null!;
}
