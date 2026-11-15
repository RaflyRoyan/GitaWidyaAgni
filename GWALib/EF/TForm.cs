using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_form")]
[Index("ActivityId", Name = "fk_form_activity")]
[Index("RecruitmentId", Name = "fk_form_recruitment")]
[Index("FormType", "Status", Name = "ix_form_org")]
public partial class TForm
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 1=EVALUASI_KEGIATAN, 2=PENDAFTARAN_REKRUTMEN, 3=SURVEI_UMUM
    /// </summary>
    [Column("form_type")]
    public int FormType { get; set; }

    /// <summary>
    /// Untuk evaluasi kegiatan
    /// </summary>
    [Column("activity_id")]
    public int? ActivityId { get; set; }

    /// <summary>
    /// Untuk pertanyaan tambahan rekrutmen
    /// </summary>
    [Column("recruitment_id")]
    public int? RecruitmentId { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("description")]
    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya | jawaban tidak menyimpan identitas
    /// </summary>
    [Column("is_anonymous")]
    public int IsAnonymous { get; set; }

    [Column("open_at", TypeName = "datetime")]
    public DateTime? OpenAt { get; set; }

    [Column("close_at", TypeName = "datetime")]
    public DateTime? CloseAt { get; set; }

    /// <summary>
    /// 1=DRAFT, 2=OPEN, 3=CLOSED
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
    [InverseProperty("TForms")]
    public virtual TActivity? Activity { get; set; }

    [ForeignKey("RecruitmentId")]
    [InverseProperty("TForms")]
    public virtual TRecruitment? Recruitment { get; set; }

    [InverseProperty("Form")]
    public virtual ICollection<TCommitteeRecruitment> TCommitteeRecruitments { get; set; } = new List<TCommitteeRecruitment>();

    [InverseProperty("Form")]
    public virtual ICollection<TFormQuestion> TFormQuestions { get; set; } = new List<TFormQuestion>();

    [InverseProperty("Form")]
    public virtual ICollection<TFormResponse> TFormResponses { get; set; } = new List<TFormResponse>();
}
