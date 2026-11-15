using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_checklist_template")]
public partial class MChecklistTemplate
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// mis. Seminar, Lomba luar kota, Konser
    /// </summary>
    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Opsional: cocok untuk t_activity.activity_type tertentu
    /// </summary>
    [Column("activity_type")]
    public int? ActivityType { get; set; }

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya
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

    [InverseProperty("Template")]
    public virtual ICollection<MChecklistTemplateItem> MChecklistTemplateItems { get; set; } = new List<MChecklistTemplateItem>();
}
