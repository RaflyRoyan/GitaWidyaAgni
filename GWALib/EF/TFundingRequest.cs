using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_funding_request")]
[Index("ActivityId", Name = "fk_funding_activity")]
[Index("BudgetItemId", Name = "fk_funding_budgetitem")]
[Index("CompetitionId", Name = "fk_funding_competition")]
[Index("PeriodId", Name = "fk_funding_period")]
[Index("ProgramId", Name = "fk_funding_program")]
[Index("RequestedBy", Name = "fk_funding_requester")]
[Index("SettledBy", Name = "fk_funding_settler")]
[Index("SettlementStatus", Name = "ix_funding_settlement")]
[Index("RequestNumber", Name = "uq_funding_number", IsUnique = true)]
public partial class TFundingRequest
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("period_id")]
    public int? PeriodId { get; set; }

    [Column("budget_item_id")]
    public int? BudgetItemId { get; set; }

    [Column("program_id")]
    public int? ProgramId { get; set; }

    [Column("activity_id")]
    public int? ActivityId { get; set; }

    /// <summary>
    /// Lomba
    /// </summary>
    [Column("competition_id")]
    public int? CompetitionId { get; set; }

    [Column("requested_by")]
    public int RequestedBy { get; set; }

    [Column("request_number")]
    [StringLength(50)]
    public string RequestNumber { get; set; } = null!;

    [Column("purpose")]
    [StringLength(255)]
    public string Purpose { get; set; } = null!;

    [Column("amount")]
    [Precision(15, 2)]
    public decimal Amount { get; set; }

    /// <summary>
    /// Total terpakai (jumlah t_funding_realization)
    /// </summary>
    [Column("realized_amount")]
    [Precision(15, 2)]
    public decimal RealizedAmount { get; set; }

    /// <summary>
    /// Sisa dana yang dikembalikan ke kas
    /// </summary>
    [Column("returned_amount")]
    [Precision(15, 2)]
    public decimal ReturnedAmount { get; set; }

    /// <summary>
    /// 1=DRAFT, 2=SUBMITTED, 3=APPROVED, 4=REJECTED, 5=DISBURSED
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    /// <summary>
    /// 1=BELUM, 2=DIAJUKAN, 3=SELESAI
    /// </summary>
    [Column("settlement_status")]
    public int SettlementStatus { get; set; }

    /// <summary>
    /// Bendahara yang memeriksa
    /// </summary>
    [Column("settled_by")]
    public int? SettledBy { get; set; }

    [Column("settled_at", TypeName = "datetime")]
    public DateTime? SettledAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TFundingRequests")]
    public virtual TActivity? Activity { get; set; }

    [ForeignKey("BudgetItemId")]
    [InverseProperty("TFundingRequests")]
    public virtual TBudgetItem? BudgetItem { get; set; }

    [ForeignKey("CompetitionId")]
    [InverseProperty("TFundingRequests")]
    public virtual TCompetition? Competition { get; set; }

    [ForeignKey("PeriodId")]
    [InverseProperty("TFundingRequests")]
    public virtual MManagementPeriod? Period { get; set; }

    [ForeignKey("ProgramId")]
    [InverseProperty("TFundingRequests")]
    public virtual TProgram? Program { get; set; }

    [ForeignKey("RequestedBy")]
    [InverseProperty("TFundingRequestRequestedByNavigations")]
    public virtual MUser RequestedByNavigation { get; set; } = null!;

    [ForeignKey("SettledBy")]
    [InverseProperty("TFundingRequestSettledByNavigations")]
    public virtual MUser? SettledByNavigation { get; set; }

    [InverseProperty("FundingRequest")]
    public virtual ICollection<TFinancialTransaction> TFinancialTransactions { get; set; } = new List<TFinancialTransaction>();

    [InverseProperty("Request")]
    public virtual ICollection<TFundingApproval> TFundingApprovals { get; set; } = new List<TFundingApproval>();

    [InverseProperty("FundingRequest")]
    public virtual ICollection<TFundingRealization> TFundingRealizations { get; set; } = new List<TFundingRealization>();

    [InverseProperty("FundingRequest")]
    public virtual ICollection<TProcurementRequest> TProcurementRequests { get; set; } = new List<TProcurementRequest>();
}
