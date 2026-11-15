using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_sub_menu")]
[Index("ControllerId", Name = "fk_submenu_ctrl")]
public partial class MSubMenu
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("controller_id")]
    public int? ControllerId { get; set; }

    [Column("icon")]
    [StringLength(100)]
    public string? Icon { get; set; }

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
    [InverseProperty("MSubMenus")]
    public virtual MController? Controller { get; set; }

    [InverseProperty("SubMenu")]
    public virtual ICollection<MHeaderMenuDetail> MHeaderMenuDetails { get; set; } = new List<MHeaderMenuDetail>();

    [InverseProperty("SubMenu")]
    public virtual ICollection<MRoleMenu> MRoleMenus { get; set; } = new List<MRoleMenu>();
}
