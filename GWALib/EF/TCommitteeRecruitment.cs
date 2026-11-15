using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_committee_recruitment")]
[Index("FormId", Name = "fk_comrec_form")]
[Index("ActivityId", Name = "uq_committee_recruitment", IsUnique = true)]
public partial class TCommitteeRecruitment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("activity_id")]
    public int ActivityId { get; set; }

    [Column("open_at", TypeName = "datetime")]
    public DateTime OpenAt { get; set; }

    [Column("close_at", TypeName = "datetime")]
    public DateTime CloseAt { get; set; }

    [Column("quota")]
    public int? Quota { get; set; }

    /// <summary>
    /// Pertanyaan tambahan (t_form)
    /// </summary>
    [Column("form_id")]
    public int? FormId { get; set; }

    /// <summary>
    /// 1=DRAFT, 2=BUKA, 3=TUTUP
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TCommitteeRecruitment")]
    public virtual TActivity Activity { get; set; } = null!;

    [ForeignKey("FormId")]
    [InverseProperty("TCommitteeRecruitments")]
    public virtual TForm? Form { get; set; }

    [InverseProperty("Recruitment")]
    public virtual ICollection<TCommitteeApplication> TCommitteeApplications { get; set; } = new List<TCommitteeApplication>();
}
