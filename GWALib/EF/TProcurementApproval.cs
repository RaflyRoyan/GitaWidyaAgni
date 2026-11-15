using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_procurement_approval")]
[Index("FlowStepId", Name = "fk_pgdappr_step")]
[Index("ApproverUserId", Name = "fk_pgdappr_user")]
[Index("RequestId", "ApprovalLevel", Name = "ix_pgdappr_request")]
public partial class TProcurementApproval
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("request_id")]
    public int RequestId { get; set; }

    /// <summary>
    /// Urutan langkah m_approval_flow (PROCUREMENT), mis. 1=Bendahara, 2=Ketua Umum
    /// </summary>
    [Column("approval_level")]
    public int ApprovalLevel { get; set; }

    /// <summary>
    /// Langkah m_approval_flow_step yang diputuskan
    /// </summary>
    [Column("flow_step_id")]
    public int? FlowStepId { get; set; }

    [Column("approver_user_id")]
    public int ApproverUserId { get; set; }

    /// <summary>
    /// 1=SETUJU, 2=REVISI, 3=TOLAK
    /// </summary>
    [Column("decision")]
    public int Decision { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [Column("signature", TypeName = "mediumblob")]
    public byte[]? Signature { get; set; }

    [Column("signature_file_name")]
    [StringLength(255)]
    public string? SignatureFileName { get; set; }

    [Column("signature_content_type")]
    [StringLength(100)]
    public string? SignatureContentType { get; set; }

    [Column("decided_at", TypeName = "datetime")]
    public DateTime DecidedAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ApproverUserId")]
    [InverseProperty("TProcurementApprovals")]
    public virtual MUser ApproverUser { get; set; } = null!;

    [ForeignKey("FlowStepId")]
    [InverseProperty("TProcurementApprovals")]
    public virtual MApprovalFlowStep? FlowStep { get; set; }

    [ForeignKey("RequestId")]
    [InverseProperty("TProcurementApprovals")]
    public virtual TProcurementRequest Request { get; set; } = null!;
}
