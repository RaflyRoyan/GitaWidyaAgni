using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_header_menu")]
[Index("ControllerId", Name = "fk_headermenu_ctrl")]
public partial class MHeaderMenu
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Seksi / grup menu di sidebar
    /// </summary>
    [Column("section")]
    [StringLength(100)]
    public string? Section { get; set; }

    /// <summary>
    /// Diisi jika header langsung ke halaman (tanpa submenu)
    /// </summary>
    [Column("controller_id")]
    public int? ControllerId { get; set; }

    [Column("icon")]
    [StringLength(100)]
    public string? Icon { get; set; }

    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }

    [Column("seq")]
    public int Seq { get; set; }

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
    [InverseProperty("MHeaderMenus")]
    public virtual MController? Controller { get; set; }

    [InverseProperty("HeaderMenu")]
    public virtual ICollection<MHeaderMenuDetail> MHeaderMenuDetails { get; set; } = new List<MHeaderMenuDetail>();

    [InverseProperty("HeaderMenu")]
    public virtual ICollection<MRoleMenu> MRoleMenus { get; set; } = new List<MRoleMenu>();
}
