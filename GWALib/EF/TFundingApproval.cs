using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_funding_approval")]
[Index("FlowStepId", Name = "fk_fundappr_step")]
[Index("ApproverUserId", Name = "fk_fundappr_user")]
[Index("RequestId", "ApprovalLevel", Name = "uq_funding_approval", IsUnique = true)]
public partial class TFundingApproval
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("request_id")]
    public int RequestId { get; set; }

    [Column("approver_user_id")]
    public int ApproverUserId { get; set; }

    [Column("approval_level")]
    public int ApprovalLevel { get; set; }

    /// <summary>
    /// Langkah m_approval_flow_step yang diputuskan
    /// </summary>
    [Column("flow_step_id")]
    public int? FlowStepId { get; set; }

    /// <summary>
    /// 1=PENDING, 2=APPROVED, 3=REJECTED, 4=REVISION
    /// </summary>
    [Column("decision")]
    public int Decision { get; set; }

    [Column("note")]
    [StringLength(255)]
    public string? Note { get; set; }

    [Column("decided_at", TypeName = "datetime")]
    public DateTime? DecidedAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ApproverUserId")]
    [InverseProperty("TFundingApprovals")]
    public virtual MUser ApproverUser { get; set; } = null!;

    [ForeignKey("FlowStepId")]
    [InverseProperty("TFundingApprovals")]
    public virtual MApprovalFlowStep? FlowStep { get; set; }

    [ForeignKey("RequestId")]
    [InverseProperty("TFundingApprovals")]
    public virtual TFundingRequest Request { get; set; } = null!;
}
