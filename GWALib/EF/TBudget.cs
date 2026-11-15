using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_budget")]
[Index("ApprovedBy", Name = "fk_budget_approver")]
[Index("PeriodId", Name = "fk_budget_period")]
public partial class TBudget
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("period_id")]
    public int PeriodId { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Column("total_amount")]
    [Precision(15, 2)]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 1=DRAFT, 2=SUBMITTED, 3=APPROVED, 4=REJECTED
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

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

    [ForeignKey("ApprovedBy")]
    [InverseProperty("TBudgets")]
    public virtual MUser? ApprovedByNavigation { get; set; }

    [ForeignKey("PeriodId")]
    [InverseProperty("TBudgets")]
    public virtual MManagementPeriod Period { get; set; } = null!;

    [InverseProperty("Budget")]
    public virtual ICollection<TBudgetDocument> TBudgetDocuments { get; set; } = new List<TBudgetDocument>();

    [InverseProperty("Budget")]
    public virtual ICollection<TBudgetItem> TBudgetItems { get; set; } = new List<TBudgetItem>();
}
