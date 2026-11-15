using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_debt")]
[Index("PeriodId", Name = "fk_debt_period")]
[Index("TransactionId", Name = "fk_debt_trx")]
[Index("Status", Name = "ix_debt_status")]
[Index("DebtNumber", Name = "uq_debt_number", IsUnique = true)]
public partial class TDebt
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Periode saat hutang dibuat
    /// </summary>
    [Column("period_id")]
    public int PeriodId { get; set; }

    [Column("debt_number")]
    [StringLength(50)]
    public string DebtNumber { get; set; } = null!;

    /// <summary>
    /// Pihak pemberi hutang
    /// </summary>
    [Column("creditor_name")]
    [StringLength(200)]
    public string CreditorName { get; set; } = null!;

    [Column("creditor_contact")]
    [StringLength(150)]
    public string? CreditorContact { get; set; }

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("amount")]
    [Precision(15, 2)]
    public decimal Amount { get; set; }

    [Column("paid_amount")]
    [Precision(15, 2)]
    public decimal PaidAmount { get; set; }

    [Column("debt_date")]
    public DateOnly DebtDate { get; set; }

    [Column("due_date")]
    public DateOnly? DueDate { get; set; }

    /// <summary>
    /// 1=BELUM_BAYAR, 2=SEBAGIAN, 3=LUNAS, 4=DIHAPUSKAN
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    /// <summary>
    /// Transaksi uang masuk jika hutang berupa pinjaman uang
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

    [ForeignKey("PeriodId")]
    [InverseProperty("TDebts")]
    public virtual MManagementPeriod Period { get; set; } = null!;

    [InverseProperty("Debt")]
    public virtual ICollection<TDebtAttachment> TDebtAttachments { get; set; } = new List<TDebtAttachment>();

    [InverseProperty("Debt")]
    public virtual ICollection<TDebtPayment> TDebtPayments { get; set; } = new List<TDebtPayment>();

    [ForeignKey("TransactionId")]
    [InverseProperty("TDebts")]
    public virtual TFinancialTransaction? Transaction { get; set; }
}
