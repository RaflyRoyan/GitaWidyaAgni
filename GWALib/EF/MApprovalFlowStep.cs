using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_approval_flow_step")]
[Index("RoleId", Name = "fk_apstep_role")]
[Index("UserId", Name = "fk_apstep_user")]
[Index("PositionId", Name = "fk_m_approval_flow_step_position_id")]
[Index("FlowId", "StepLevel", Name = "uq_flow_step", IsUnique = true)]
public partial class MApprovalFlowStep
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("flow_id")]
    public int FlowId { get; set; }

    /// <summary>
    /// Urutan, mulai 1
    /// </summary>
    [Column("step_level")]
    public int StepLevel { get; set; }

    /// <summary>
    /// mis. Bendahara, Ketua Umum
    /// </summary>
    [Column("step_name")]
    [StringLength(100)]
    public string StepName { get; set; } = null!;

    /// <summary>
    /// 1=ROLE (role pengurus), 2=JABATAN (position_id), 3=USER_TERTENTU (user_id)
    /// </summary>
    [Column("approver_type")]
    public int ApproverType { get; set; }

    /// <summary>
    /// Jabatan pengurus yang menyetujui (approver_type=2)
    /// </summary>
    [Column("position_id")]
    public int? PositionId { get; set; }

    /// <summary>
    /// Role yang menyetujui (approver_type=1)
    /// </summary>
    [Column("role_id")]
    public int? RoleId { get; set; }

    /// <summary>
    /// User tertentu yang menyetujui (approver_type=3)
    /// </summary>
    [Column("user_id")]
    public int? UserId { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya | boleh mengembalikan untuk revisi
    /// </summary>
    [Column("allow_revision")]
    public int AllowRevision { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("FlowId")]
    [InverseProperty("MApprovalFlowSteps")]
    public virtual MApprovalFlow Flow { get; set; } = null!;

    [ForeignKey("PositionId")]
    [InverseProperty("MApprovalFlowSteps")]
    public virtual MPosition? Position { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("MApprovalFlowSteps")]
    public virtual MRole? Role { get; set; }

    [InverseProperty("FlowStep")]
    public virtual ICollection<TFundingApproval> TFundingApprovals { get; set; } = new List<TFundingApproval>();

    [InverseProperty("FlowStep")]
    public virtual ICollection<TLetterApproval> TLetterApprovals { get; set; } = new List<TLetterApproval>();

    [InverseProperty("FlowStep")]
    public virtual ICollection<TProcurementApproval> TProcurementApprovals { get; set; } = new List<TProcurementApproval>();

    [ForeignKey("UserId")]
    [InverseProperty("MApprovalFlowSteps")]
    public virtual MUser? User { get; set; }
}
