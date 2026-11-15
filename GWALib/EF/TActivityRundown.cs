using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_activity_rundown")]
[Index("PicMemberId", Name = "fk_rundown_pic")]
[Index("SectionId", Name = "fk_rundown_section")]
[Index("ActivityId", "RundownDate", "StartTime", Name = "ix_rundown_activity")]
public partial class TActivityRundown
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("activity_id")]
    public int ActivityId { get; set; }

    [Column("rundown_date")]
    public DateOnly RundownDate { get; set; }

    [Column("start_time", TypeName = "time")]
    public TimeOnly StartTime { get; set; }

    [Column("end_time", TypeName = "time")]
    public TimeOnly? EndTime { get; set; }

    /// <summary>
    /// mis. Berangkat dari kampus, Technical meeting, Tampil
    /// </summary>
    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("location_name")]
    [StringLength(200)]
    public string? LocationName { get; set; }

    /// <summary>
    /// Sie penanggung jawab
    /// </summary>
    [Column("section_id")]
    public int? SectionId { get; set; }

    [Column("pic_member_id")]
    public int? PicMemberId { get; set; }

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

    [ForeignKey("ActivityId")]
    [InverseProperty("TActivityRundowns")]
    public virtual TActivity Activity { get; set; } = null!;

    [ForeignKey("PicMemberId")]
    [InverseProperty("TActivityRundowns")]
    public virtual MMember? PicMember { get; set; }

    [ForeignKey("SectionId")]
    [InverseProperty("TActivityRundowns")]
    public virtual TCommitteeSection? Section { get; set; }
}
