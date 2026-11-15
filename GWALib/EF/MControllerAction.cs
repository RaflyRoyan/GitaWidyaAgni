using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_controller_action")]
[Index("ControllerId", "Action", Name = "uq_controller_action", IsUnique = true)]
public partial class MControllerAction
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("controller_id")]
    public int ControllerId { get; set; }

    [Column("action")]
    [StringLength(100)]
    public string Action { get; set; } = null!;

    /// <summary>
    /// 0=Non Auth, 1=Auth
    /// </summary>
    [Column("need_auth")]
    public int NeedAuth { get; set; }

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

    [ForeignKey("ControllerId")]
    [InverseProperty("MControllerActions")]
    public virtual MController Controller { get; set; } = null!;

    [InverseProperty("ControllerAction")]
    public virtual ICollection<MRoleAction> MRoleActions { get; set; } = new List<MRoleAction>();
}
