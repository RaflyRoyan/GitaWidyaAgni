using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_checklist_template_item")]
[Index("TemplateId", "SeqNo", Name = "ix_cltitem_template")]
public partial class MChecklistTemplateItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("template_id")]
    public int TemplateId { get; set; }

    /// <summary>
    /// Sie tujuan, dicocokkan dengan t_committee_section.name
    /// </summary>
    [Column("section_name")]
    [StringLength(100)]
    public string? SectionName { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    /// <summary>
    /// Hari relatif terhadap hari H, negatif = sebelum (mis. -21)
    /// </summary>
    [Column("offset_day")]
    public int OffsetDay { get; set; }

    /// <summary>
    /// 1=RENDAH, 2=SEDANG, 3=TINGGI
    /// </summary>
    [Column("priority")]
    public int Priority { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya | disalin ke t_activity_checklist
    /// </summary>
    [Column("is_milestone")]
    public int IsMilestone { get; set; }

    [Column("seq_no")]
    public int SeqNo { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [InverseProperty("TemplateItem")]
    public virtual ICollection<TActivityChecklist> TActivityChecklists { get; set; } = new List<TActivityChecklist>();

    [ForeignKey("TemplateId")]
    [InverseProperty("MChecklistTemplateItems")]
    public virtual MChecklistTemplate Template { get; set; } = null!;
}
