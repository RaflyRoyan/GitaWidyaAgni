using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_role_menu")]
[Index("HeaderMenuId", Name = "fk_rolemenu_header")]
[Index("SubMenuId", Name = "fk_rolemenu_sub")]
[Index("RoleId", "HeaderMenuId", "SubMenuId", Name = "uq_role_menu", IsUnique = true)]
public partial class MRoleMenu
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("header_menu_id")]
    public int HeaderMenuId { get; set; }

    /// <summary>
    /// NULL = akses ke header menu saja
    /// </summary>
    [Column("sub_menu_id")]
    public int? SubMenuId { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("HeaderMenuId")]
    [InverseProperty("MRoleMenus")]
    public virtual MHeaderMenu HeaderMenu { get; set; } = null!;

    [ForeignKey("RoleId")]
    [InverseProperty("MRoleMenus")]
    public virtual MRole Role { get; set; } = null!;

    [ForeignKey("SubMenuId")]
    [InverseProperty("MRoleMenus")]
    public virtual MSubMenu? SubMenu { get; set; }
}
