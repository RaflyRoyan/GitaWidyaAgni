using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_activity_checklist")]
[Index("DoneBy", Name = "fk_checklist_doneby")]
[Index("PicMemberId", Name = "fk_checklist_pic")]
[Index("SectionId", Name = "fk_checklist_section")]
[Index("TemplateItemId", Name = "fk_checklist_titem")]
[Index("ActivityId", "Status", "DueDate", Name = "ix_checklist_due")]
[Index("ActivityId", "IsMilestone", "DueDate", Name = "ix_checklist_milestone")]
[Index("ActivityId", "TemplateItemId", Name = "uq_checklist_template_item", IsUnique = true)]
public partial class TActivityChecklist
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("activity_id")]
    public int ActivityId { get; set; }

    [Column("section_id")]
    public int? SectionId { get; set; }

    /// <summary>
    /// Asal template
    /// </summary>
    [Column("template_item_id")]
    public int? TemplateItemId { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("due_date")]
    public DateOnly? DueDate { get; set; }

    [Column("pic_member_id")]
    public int? PicMemberId { get; set; }

    /// <summary>
    /// 1=RENDAH, 2=SEDANG, 3=TINGGI
    /// </summary>
    [Column("priority")]
    public int Priority { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya | tampil di timeline kegiatan
    /// </summary>
    [Column("is_milestone")]
    public int IsMilestone { get; set; }

    /// <summary>
    /// 1=BELUM, 2=PROSES, 3=SELESAI, 4=BATAL
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("done_at", TypeName = "datetime")]
    public DateTime? DoneAt { get; set; }

    [Column("done_by")]
    public int? DoneBy { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TActivityChecklists")]
    public virtual TActivity Activity { get; set; } = null!;

    [ForeignKey("DoneBy")]
    [InverseProperty("TActivityChecklists")]
    public virtual MUser? DoneByNavigation { get; set; }

    [ForeignKey("PicMemberId")]
    [InverseProperty("TActivityChecklists")]
    public virtual MMember? PicMember { get; set; }

    [ForeignKey("SectionId")]
    [InverseProperty("TActivityChecklists")]
    public virtual TCommitteeSection? Section { get; set; }

    [ForeignKey("TemplateItemId")]
    [InverseProperty("TActivityChecklists")]
    public virtual MChecklistTemplateItem? TemplateItem { get; set; }
}
