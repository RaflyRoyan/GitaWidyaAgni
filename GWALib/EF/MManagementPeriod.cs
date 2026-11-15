using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_management_period")]
[Index("Status", Name = "ix_period_org_status")]
public partial class MManagementPeriod
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// mis. Periode 2026/2027
    /// </summary>
    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// 1=DRAFT, 2=ACTIVE, 3=CLOSED
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

    [InverseProperty("Period")]
    public virtual ICollection<MDuesSetting> MDuesSettings { get; set; } = new List<MDuesSetting>();

    [InverseProperty("Period")]
    public virtual ICollection<MUserRole> MUserRoles { get; set; } = new List<MUserRole>();

    [InverseProperty("Period")]
    public virtual ICollection<TBudget> TBudgets { get; set; } = new List<TBudget>();

    [InverseProperty("Period")]
    public virtual ICollection<TDebt> TDebts { get; set; } = new List<TDebt>();

    [InverseProperty("Period")]
    public virtual ICollection<TDivisionMember> TDivisionMembers { get; set; } = new List<TDivisionMember>();

    [InverseProperty("Period")]
    public virtual ICollection<TElection> TElections { get; set; } = new List<TElection>();

    [InverseProperty("Period")]
    public virtual ICollection<TFinancialTransaction> TFinancialTransactions { get; set; } = new List<TFinancialTransaction>();

    [InverseProperty("Period")]
    public virtual ICollection<TFundingRequest> TFundingRequests { get; set; } = new List<TFundingRequest>();

    [InverseProperty("Period")]
    public virtual ICollection<TLetter> TLetters { get; set; } = new List<TLetter>();

    [InverseProperty("Period")]
    public virtual ICollection<TManagementAssignment> TManagementAssignments { get; set; } = new List<TManagementAssignment>();

    [InverseProperty("Period")]
    public virtual ICollection<TMemberPoint> TMemberPoints { get; set; } = new List<TMemberPoint>();

    [InverseProperty("Period")]
    public virtual ICollection<TPeriodBalance> TPeriodBalances { get; set; } = new List<TPeriodBalance>();

    [InverseProperty("FromPeriod")]
    public virtual TPeriodHandover? TPeriodHandoverFromPeriod { get; set; }

    [InverseProperty("ToPeriod")]
    public virtual ICollection<TPeriodHandover> TPeriodHandoverToPeriods { get; set; } = new List<TPeriodHandover>();

    [InverseProperty("Period")]
    public virtual ICollection<TProcurementRequest> TProcurementRequests { get; set; } = new List<TProcurementRequest>();

    [InverseProperty("Period")]
    public virtual ICollection<TProgramTask> TProgramTasks { get; set; } = new List<TProgramTask>();

    [InverseProperty("Period")]
    public virtual ICollection<TProgram> TPrograms { get; set; } = new List<TProgram>();

    [InverseProperty("Period")]
    public virtual ICollection<TRecruitment> TRecruitments { get; set; } = new List<TRecruitment>();

    [InverseProperty("Period")]
    public virtual ICollection<TSalesOrder> TSalesOrders { get; set; } = new List<TSalesOrder>();

    [InverseProperty("Period")]
    public virtual ICollection<TSponsorship> TSponsorships { get; set; } = new List<TSponsorship>();

    [InverseProperty("Period")]
    public virtual ICollection<TTimelinePlan> TTimelinePlans { get; set; } = new List<TTimelinePlan>();
}
