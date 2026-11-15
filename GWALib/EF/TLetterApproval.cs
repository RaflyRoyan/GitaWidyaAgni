using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_letter_approval")]
[Index("PositionId", Name = "fk_letterappr_position")]
[Index("FlowStepId", Name = "fk_letterappr_step")]
[Index("ApproverUserId", Name = "fk_letterappr_user")]
[Index("LetterId", "ApprovalLevel", Name = "uq_letter_approval", IsUnique = true)]
public partial class TLetterApproval
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("letter_id")]
    public int LetterId { get; set; }

    [Column("approver_user_id")]
    public int ApproverUserId { get; set; }

    /// <summary>
    /// Jabatan penandatangan (Ketua, Sekretaris, ...)
    /// </summary>
    [Column("position_id")]
    public int? PositionId { get; set; }

    [Column("approval_level")]
    public int ApprovalLevel { get; set; }

    /// <summary>
    /// Langkah m_approval_flow_step yang diputuskan
    /// </summary>
    [Column("flow_step_id")]
    public int? FlowStepId { get; set; }

    /// <summary>
    /// 1=PENDING, 2=APPROVED, 3=REJECTED
    /// </summary>
    [Column("decision")]
    public int Decision { get; set; }

    [Column("note")]
    [StringLength(255)]
    public string? Note { get; set; }

    /// <summary>
    /// Gambar tanda tangan
    /// </summary>
    [Column("signature", TypeName = "mediumblob")]
    public byte[]? Signature { get; set; }

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
    [InverseProperty("TLetterApprovals")]
    public virtual MUser ApproverUser { get; set; } = null!;

    [ForeignKey("FlowStepId")]
    [InverseProperty("TLetterApprovals")]
    public virtual MApprovalFlowStep? FlowStep { get; set; }

    [ForeignKey("LetterId")]
    [InverseProperty("TLetterApprovals")]
    public virtual TLetter Letter { get; set; } = null!;

    [ForeignKey("PositionId")]
    [InverseProperty("TLetterApprovals")]
    public virtual MPosition? Position { get; set; }
}
