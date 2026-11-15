using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_program")]
[Index("PeriodId", Name = "fk_program_period")]
[Index("PicMemberId", Name = "fk_program_pic")]
[Index("PreviousProgramId", Name = "fk_program_previous")]
public partial class TProgram
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("period_id")]
    public int PeriodId { get; set; }

    /// <summary>
    /// Program periode sebelumnya yang dilanjutkan
    /// </summary>
    [Column("previous_program_id")]
    public int? PreviousProgramId { get; set; }

    /// <summary>
    /// Penanggung jawab
    /// </summary>
    [Column("pic_member_id")]
    public int? PicMemberId { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("start_date")]
    public DateOnly? StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("budget_amount")]
    [Precision(15, 2)]
    public decimal BudgetAmount { get; set; }

    /// <summary>
    /// 1=PLANNED, 2=ONGOING, 3=DONE, 4=CANCELLED
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [InverseProperty("PreviousProgram")]
    public virtual ICollection<TProgram> InversePreviousProgram { get; set; } = new List<TProgram>();

    [ForeignKey("PeriodId")]
    [InverseProperty("TPrograms")]
    public virtual MManagementPeriod Period { get; set; } = null!;

    [ForeignKey("PicMemberId")]
    [InverseProperty("TPrograms")]
    public virtual MMember? PicMember { get; set; }

    [ForeignKey("PreviousProgramId")]
    [InverseProperty("InversePreviousProgram")]
    public virtual TProgram? PreviousProgram { get; set; }

    [InverseProperty("Program")]
    public virtual ICollection<TActivity> TActivities { get; set; } = new List<TActivity>();

    [InverseProperty("Program")]
    public virtual ICollection<TActivityReport> TActivityReports { get; set; } = new List<TActivityReport>();

    [InverseProperty("Program")]
    public virtual ICollection<TBudgetItem> TBudgetItems { get; set; } = new List<TBudgetItem>();

    [InverseProperty("Program")]
    public virtual ICollection<TFinancialTransaction> TFinancialTransactions { get; set; } = new List<TFinancialTransaction>();

    [InverseProperty("Program")]
    public virtual ICollection<TFundingRequest> TFundingRequests { get; set; } = new List<TFundingRequest>();

    [InverseProperty("Program")]
    public virtual ICollection<TProcurementRequest> TProcurementRequests { get; set; } = new List<TProcurementRequest>();

    [InverseProperty("Program")]
    public virtual ICollection<TProgramTask> TProgramTasks { get; set; } = new List<TProgramTask>();

    [InverseProperty("Program")]
    public virtual ICollection<TTimelinePlan> TTimelinePlans { get; set; } = new List<TTimelinePlan>();
}
