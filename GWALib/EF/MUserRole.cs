using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_user_role")]
[Index("PeriodId", Name = "fk_userrole_period")]
[Index("RoleId", Name = "fk_userrole_role")]
[Index("UserId", "RoleId", "PeriodId", Name = "uq_user_role", IsUnique = true)]
public partial class MUserRole
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    /// <summary>
    /// NULL = tidak terikat periode (mis. admin aplikasi). Jika periode CLOSED, role hanya boleh baca
    /// </summary>
    [Column("period_id")]
    public int? PeriodId { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("PeriodId")]
    [InverseProperty("MUserRoles")]
    public virtual MManagementPeriod? Period { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("MUserRoles")]
    public virtual MRole Role { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("MUserRoles")]
    public virtual MUser User { get; set; } = null!;
}
