using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_procurement_request")]
[Index("ActivityId", Name = "fk_pgd_activity")]
[Index("BudgetItemId", Name = "fk_pgd_budgetitem")]
[Index("FundingRequestId", Name = "fk_pgd_funding")]
[Index("PeriodId", Name = "fk_pgd_period")]
[Index("ProgramId", Name = "fk_pgd_program")]
[Index("RequestedByMemberId", Name = "fk_pgd_requester")]
[Index("Status", Name = "ix_procurement_status")]
[Index("RequestNumber", Name = "uq_procurement_number", IsUnique = true)]
public partial class TProcurementRequest
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// PGD-{org}-{yyyy}-{seq}
    /// </summary>
    [Column("request_number")]
    [StringLength(50)]
    public string RequestNumber { get; set; } = null!;

    [Column("period_id")]
    public int? PeriodId { get; set; }

    [Column("requested_by_member_id")]
    public int RequestedByMemberId { get; set; }

    [Column("request_date")]
    public DateOnly RequestDate { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Alasan kebutuhan
    /// </summary>
    [Column("reason", TypeName = "text")]
    public string Reason { get; set; } = null!;

    [Column("activity_id")]
    public int? ActivityId { get; set; }

    /// <summary>
    /// Tujuan: maksimal satu dari activity/program
    /// </summary>
    [Column("program_id")]
    public int? ProgramId { get; set; }

    /// <summary>
    /// Item anggaran sumber dana
    /// </summary>
    [Column("budget_item_id")]
    public int? BudgetItemId { get; set; }

    /// <summary>
    /// 1=DIBAYAR_LANGSUNG (transaksi pengeluaran oleh bendahara), 2=PENGAJUAN_DANA (uang muka lalu dipertanggungjawabkan)
    /// </summary>
    [Column("payment_method")]
    public int PaymentMethod { get; set; }

    /// <summary>
    /// Wajib jika payment_method = 2 mulai status DIBELI
    /// </summary>
    [Column("funding_request_id")]
    public int? FundingRequestId { get; set; }

    [Column("needed_by_date")]
    public DateOnly? NeededByDate { get; set; }

    /// <summary>
    /// Dihitung dari item
    /// </summary>
    [Column("total_estimate")]
    [Precision(15, 2)]
    public decimal TotalEstimate { get; set; }

    /// <summary>
    /// Jumlah transaksi POSTED dengan procurement_id ini
    /// </summary>
    [Column("total_actual")]
    [Precision(15, 2)]
    public decimal TotalActual { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya | total melebihi sisa item anggaran saat diajukan; hanya peringatan, tidak menolak
    /// </summary>
    [Column("is_over_budget")]
    public int IsOverBudget { get; set; }

    /// <summary>
    /// Sisa item anggaran saat diajukan (untuk ditampilkan ke approver)
    /// </summary>
    [Column("budget_remaining")]
    [Precision(15, 2)]
    public decimal? BudgetRemaining { get; set; }

    /// <summary>
    /// 1=DRAFT, 2=DIAJUKAN, 3=REVISI, 4=DISETUJUI, 5=DITOLAK, 6=DIBELI, 7=DITERIMA_SEBAGIAN, 8=SELESAI, 9=BATAL
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("reject_reason")]
    [StringLength(500)]
    public string? RejectReason { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TProcurementRequests")]
    public virtual TActivity? Activity { get; set; }

    [ForeignKey("BudgetItemId")]
    [InverseProperty("TProcurementRequests")]
    public virtual TBudgetItem? BudgetItem { get; set; }

    [ForeignKey("FundingRequestId")]
    [InverseProperty("TProcurementRequests")]
    public virtual TFundingRequest? FundingRequest { get; set; }

    [ForeignKey("PeriodId")]
    [InverseProperty("TProcurementRequests")]
    public virtual MManagementPeriod? Period { get; set; }

    [ForeignKey("ProgramId")]
    [InverseProperty("TProcurementRequests")]
    public virtual TProgram? Program { get; set; }

    [ForeignKey("RequestedByMemberId")]
    [InverseProperty("TProcurementRequests")]
    public virtual MMember RequestedByMember { get; set; } = null!;

    [InverseProperty("Procurement")]
    public virtual ICollection<TFinancialTransaction> TFinancialTransactions { get; set; } = new List<TFinancialTransaction>();

    [InverseProperty("Request")]
    public virtual ICollection<TProcurementApproval> TProcurementApprovals { get; set; } = new List<TProcurementApproval>();

    [InverseProperty("Request")]
    public virtual ICollection<TProcurementItem> TProcurementItems { get; set; } = new List<TProcurementItem>();

    [InverseProperty("Request")]
    public virtual ICollection<TProcurementQuotation> TProcurementQuotations { get; set; } = new List<TProcurementQuotation>();

    [InverseProperty("Request")]
    public virtual ICollection<TProcurementReceipt> TProcurementReceipts { get; set; } = new List<TProcurementReceipt>();

    [InverseProperty("Procurement")]
    public virtual ICollection<TProductStockMovement> TProductStockMovements { get; set; } = new List<TProductStockMovement>();
}
