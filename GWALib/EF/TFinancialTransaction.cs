using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_financial_transaction")]
[Index("BorrowRequestId", Name = "fk_t_financial_transaction_borrow_request_id")]
[Index("AccountId", Name = "fk_trx_account")]
[Index("ActivityId", Name = "fk_trx_activity")]
[Index("ApprovedBy", Name = "fk_trx_approver")]
[Index("BudgetItemId", Name = "fk_trx_budgetitem")]
[Index("CategoryId", Name = "fk_trx_category")]
[Index("CompetitionId", Name = "fk_trx_competition")]
[Index("CreatedBy", Name = "fk_trx_creator")]
[Index("FundingRequestId", Name = "fk_trx_funding")]
[Index("PeriodId", Name = "fk_trx_period")]
[Index("ProcurementId", Name = "fk_trx_procurement")]
[Index("ProgramId", Name = "fk_trx_program")]
[Index("SalesOrderId", Name = "fk_trx_sales")]
[Index("SponsorshipId", Name = "fk_trx_sponsorship")]
[Index("TransactionDate", Name = "ix_trx_date")]
[Index("TransactionNumber", Name = "uq_trx_number", IsUnique = true)]
public partial class TFinancialTransaction
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("account_id")]
    public int AccountId { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("period_id")]
    public int? PeriodId { get; set; }

    /// <summary>
    /// Realisasi terhadap item anggaran
    /// </summary>
    [Column("budget_item_id")]
    public int? BudgetItemId { get; set; }

    /// <summary>
    /// Pencairan dari pengajuan dana
    /// </summary>
    [Column("funding_request_id")]
    public int? FundingRequestId { get; set; }

    /// <summary>
    /// Kegiatan (event, rapat, latihan, ...)
    /// </summary>
    [Column("activity_id")]
    public int? ActivityId { get; set; }

    /// <summary>
    /// Program kerja
    /// </summary>
    [Column("program_id")]
    public int? ProgramId { get; set; }

    /// <summary>
    /// Lomba
    /// </summary>
    [Column("competition_id")]
    public int? CompetitionId { get; set; }

    /// <summary>
    /// Pemasukan dari sponsor
    /// </summary>
    [Column("sponsorship_id")]
    public int? SponsorshipId { get; set; }

    /// <summary>
    /// Pengeluaran untuk pengadaan barang
    /// </summary>
    [Column("procurement_id")]
    public int? ProcurementId { get; set; }

    /// <summary>
    /// Pembayaran penjualan danus/merchandise
    /// </summary>
    [Column("sales_order_id")]
    public int? SalesOrderId { get; set; }

    /// <summary>
    /// Pembayaran sewa/sumbangan/denda peminjaman; jenisnya lewat category_id
    /// </summary>
    [Column("borrow_request_id")]
    public int? BorrowRequestId { get; set; }

    [Column("transaction_number")]
    [StringLength(50)]
    public string TransactionNumber { get; set; } = null!;

    /// <summary>
    /// 1=INCOME, 2=EXPENSE
    /// </summary>
    [Column("transaction_type")]
    public int TransactionType { get; set; }

    [Column("amount")]
    [Precision(15, 2)]
    public decimal Amount { get; set; }

    [Column("transaction_date")]
    public DateOnly TransactionDate { get; set; }

    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// 1=DRAFT, 2=POSTED, 3=VOID
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("created_by")]
    public int CreatedBy { get; set; }

    [Column("approved_by")]
    public int? ApprovedBy { get; set; }

    [Column("approved_at", TypeName = "datetime")]
    public DateTime? ApprovedAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("TFinancialTransactions")]
    public virtual MFinancialAccount Account { get; set; } = null!;

    [ForeignKey("ActivityId")]
    [InverseProperty("TFinancialTransactions")]
    public virtual TActivity? Activity { get; set; }

    [ForeignKey("ApprovedBy")]
    [InverseProperty("TFinancialTransactionApprovedByNavigations")]
    public virtual MUser? ApprovedByNavigation { get; set; }

    [ForeignKey("BorrowRequestId")]
    [InverseProperty("TFinancialTransactions")]
    public virtual TBorrowRequest? BorrowRequest { get; set; }

    [ForeignKey("BudgetItemId")]
    [InverseProperty("TFinancialTransactions")]
    public virtual TBudgetItem? BudgetItem { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("TFinancialTransactions")]
    public virtual MFinancialCategory Category { get; set; } = null!;

    [ForeignKey("CompetitionId")]
    [InverseProperty("TFinancialTransactions")]
    public virtual TCompetition? Competition { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("TFinancialTransactionCreatedByNavigations")]
    public virtual MUser CreatedByNavigation { get; set; } = null!;

    [ForeignKey("FundingRequestId")]
    [InverseProperty("TFinancialTransactions")]
    public virtual TFundingRequest? FundingRequest { get; set; }

    [ForeignKey("PeriodId")]
    [InverseProperty("TFinancialTransactions")]
    public virtual MManagementPeriod? Period { get; set; }

    [ForeignKey("ProcurementId")]
    [InverseProperty("TFinancialTransactions")]
    public virtual TProcurementRequest? Procurement { get; set; }

    [ForeignKey("ProgramId")]
    [InverseProperty("TFinancialTransactions")]
    public virtual TProgram? Program { get; set; }

    [ForeignKey("SalesOrderId")]
    [InverseProperty("TFinancialTransactions")]
    public virtual TSalesOrder? SalesOrder { get; set; }

    [ForeignKey("SponsorshipId")]
    [InverseProperty("TFinancialTransactions")]
    public virtual TSponsorship? Sponsorship { get; set; }

    [InverseProperty("Transaction")]
    public virtual TDebtPayment? TDebtPayment { get; set; }

    [InverseProperty("Transaction")]
    public virtual ICollection<TDebt> TDebts { get; set; } = new List<TDebt>();

    [InverseProperty("Transaction")]
    public virtual ICollection<TDuesBill> TDuesBills { get; set; } = new List<TDuesBill>();

    [InverseProperty("Transaction")]
    public virtual ICollection<TFinancialAttachment> TFinancialAttachments { get; set; } = new List<TFinancialAttachment>();

    [InverseProperty("Transaction")]
    public virtual ICollection<TMaintenanceLog> TMaintenanceLogs { get; set; } = new List<TMaintenanceLog>();
}
