using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_role")]
[Index("Code", Name = "uq_role_code", IsUnique = true)]
public partial class MRole
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// SUPERADMIN, ADMIN, KETUA, BENDAHARA, SEKRETARIS, ANGGOTA, ...
    /// </summary>
    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }

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

    [InverseProperty("Role")]
    public virtual ICollection<MApprovalFlowStep> MApprovalFlowSteps { get; set; } = new List<MApprovalFlowStep>();

    [InverseProperty("Role")]
    public virtual ICollection<MRoleAction> MRoleActions { get; set; } = new List<MRoleAction>();

    [InverseProperty("Role")]
    public virtual ICollection<MRoleMenu> MRoleMenus { get; set; } = new List<MRoleMenu>();

    [InverseProperty("Role")]
    public virtual ICollection<MUserRole> MUserRoles { get; set; } = new List<MUserRole>();
}
