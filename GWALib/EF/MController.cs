using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_controller")]
[Index("Name", Name = "uq_controller_name", IsUnique = true)]
public partial class MController
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Nama controller, mis. Member
    /// </summary>
    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("index_action")]
    [StringLength(100)]
    public string IndexAction { get; set; } = null!;

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

    [InverseProperty("Controller")]
    public virtual ICollection<MControllerAction> MControllerActions { get; set; } = new List<MControllerAction>();

    [InverseProperty("Controller")]
    public virtual ICollection<MHeaderMenu> MHeaderMenus { get; set; } = new List<MHeaderMenu>();

    [InverseProperty("Controller")]
    public virtual ICollection<MSubMenu> MSubMenus { get; set; } = new List<MSubMenu>();
}
