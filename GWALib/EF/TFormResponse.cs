using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_form_response")]
[Index("ApplicantId", Name = "fk_response_applicant")]
[Index("MemberId", Name = "fk_response_member")]
[Index("FormId", Name = "ix_response_form")]
public partial class TFormResponse
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("form_id")]
    public int FormId { get; set; }

    /// <summary>
    /// NULL jika form anonim atau responden bukan anggota
    /// </summary>
    [Column("member_id")]
    public int? MemberId { get; set; }

    /// <summary>
    /// Diisi untuk jawaban formulir rekrutmen
    /// </summary>
    [Column("applicant_id")]
    public int? ApplicantId { get; set; }

    [Column("submitted_at", TypeName = "datetime")]
    public DateTime SubmittedAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ApplicantId")]
    [InverseProperty("TFormResponses")]
    public virtual TRecruitmentApplicant? Applicant { get; set; }

    [ForeignKey("FormId")]
    [InverseProperty("TFormResponses")]
    public virtual TForm Form { get; set; } = null!;

    [ForeignKey("MemberId")]
    [InverseProperty("TFormResponses")]
    public virtual MMember? Member { get; set; }

    [InverseProperty("FormResponse")]
    public virtual ICollection<TCommitteeApplication> TCommitteeApplications { get; set; } = new List<TCommitteeApplication>();

    [InverseProperty("Response")]
    public virtual ICollection<TFormAnswer> TFormAnswers { get; set; } = new List<TFormAnswer>();
}
