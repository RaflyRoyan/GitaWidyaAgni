using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_header_menu_detail")]
[Index("SubMenuId", Name = "fk_hmd_sub")]
[Index("HeaderMenuId", "SubMenuId", Name = "uq_headermenu_detail", IsUnique = true)]
public partial class MHeaderMenuDetail
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("header_menu_id")]
    public int HeaderMenuId { get; set; }

    [Column("sub_menu_id")]
    public int SubMenuId { get; set; }

    [Column("seq")]
    public int Seq { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("HeaderMenuId")]
    [InverseProperty("MHeaderMenuDetails")]
    public virtual MHeaderMenu HeaderMenu { get; set; } = null!;

    [ForeignKey("SubMenuId")]
    [InverseProperty("MHeaderMenuDetails")]
    public virtual MSubMenu SubMenu { get; set; } = null!;
}
