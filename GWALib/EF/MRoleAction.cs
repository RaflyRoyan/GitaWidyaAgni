using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_role_action")]
[Index("ControllerActionId", Name = "fk_roleaction_action")]
[Index("RoleId", "ControllerActionId", Name = "uq_role_action", IsUnique = true)]
public partial class MRoleAction
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("controller_action_id")]
    public int ControllerActionId { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ControllerActionId")]
    [InverseProperty("MRoleActions")]
    public virtual MControllerAction ControllerAction { get; set; } = null!;

    [ForeignKey("RoleId")]
    [InverseProperty("MRoleActions")]
    public virtual MRole Role { get; set; } = null!;
}
