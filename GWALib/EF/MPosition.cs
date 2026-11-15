using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_position")]
[Index("Name", Name = "uq_position_name", IsUnique = true)]
public partial class MPosition
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Ketua, Wakil, Sekretaris, Bendahara, Kadiv, ...
    /// </summary>
    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya | 1 = pengurus inti
    /// </summary>
    [Column("is_core")]
    public int IsCore { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    /// <summary>
    /// 0=Tidak Aktif, 1=Aktif
    /// </summary>
    [Column("is_active")]
    public int IsActive { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [InverseProperty("Position")]
    public virtual ICollection<MApprovalFlowStep> MApprovalFlowSteps { get; set; } = new List<MApprovalFlowStep>();

    [InverseProperty("Position")]
    public virtual ICollection<TLetterApproval> TLetterApprovals { get; set; } = new List<TLetterApproval>();

    [InverseProperty("Position")]
    public virtual ICollection<TManagementAssignment> TManagementAssignments { get; set; } = new List<TManagementAssignment>();
}
