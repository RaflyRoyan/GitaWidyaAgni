using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_approval_flow")]
[Index("DocumentType", "ActiveKey", Name = "uq_approval_flow_active", IsUnique = true)]
public partial class MApprovalFlow
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// FUNDING_REQUEST, PROCUREMENT, LETTER, ACTIVITY_REPORT, BUDGET, BORROW_REQUEST, PERIOD_HANDOVER
    /// </summary>
    [Column("document_type")]
    [StringLength(50)]
    public string DocumentType { get; set; } = null!;

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya
    /// </summary>
    [Column("is_active")]
    public int IsActive { get; set; }

    /// <summary>
    /// Kolom bantu: satu alur aktif per jenis dokumen
    /// </summary>
    [Column("active_key")]
    public int? ActiveKey { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [InverseProperty("Flow")]
    public virtual ICollection<MApprovalFlowStep> MApprovalFlowSteps { get; set; } = new List<MApprovalFlowStep>();
}
