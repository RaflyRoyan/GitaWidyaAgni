using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_recruitment_applicant")]
[Index("MemberId", Name = "fk_applicant_member")]
[Index("ReviewedBy", Name = "fk_applicant_reviewer")]
[Index("RecruitmentId", "Nim", Name = "uq_applicant", IsUnique = true)]
[Index("RecruitmentId", "Email", Name = "uq_applicant_email", IsUnique = true)]
public partial class TRecruitmentApplicant
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("recruitment_id")]
    public int RecruitmentId { get; set; }

    /// <summary>
    /// Terisi jika sudah terdaftar sebagai m_member
    /// </summary>
    [Column("member_id")]
    public int? MemberId { get; set; }

    /// <summary>
    /// 1=MAHASISWA, 2=UMUM
    /// </summary>
    [Column("applicant_category")]
    public int ApplicantCategory { get; set; }

    /// <summary>
    /// Wajib untuk MAHASISWA
    /// </summary>
    [Column("nim")]
    [StringLength(30)]
    public string? Nim { get; set; }

    [Column("fullname")]
    [StringLength(150)]
    public string Fullname { get; set; } = null!;

    [Column("email")]
    [StringLength(150)]
    public string Email { get; set; } = null!;

    [Column("phone")]
    [StringLength(30)]
    public string? Phone { get; set; }

    [Column("program_studi")]
    [StringLength(100)]
    public string? ProgramStudi { get; set; }

    [Column("angkatan")]
    public short? Angkatan { get; set; }

    /// <summary>
    /// Asal instansi / kampus lain / pekerjaan (untuk UMUM)
    /// </summary>
    [Column("institution")]
    [StringLength(150)]
    public string? Institution { get; set; }

    [Column("motivation", TypeName = "text")]
    public string? Motivation { get; set; }

    /// <summary>
    /// 1=SUBMITTED, 2=SCREENING, 3=INTERVIEW, 4=ACCEPTED, 5=REJECTED
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("reviewed_by")]
    public int? ReviewedBy { get; set; }

    [Column("reviewed_at", TypeName = "datetime")]
    public DateTime? ReviewedAt { get; set; }

    [Column("notes")]
    [StringLength(255)]
    public string? Notes { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("TRecruitmentApplicants")]
    public virtual MMember? Member { get; set; }

    [ForeignKey("RecruitmentId")]
    [InverseProperty("TRecruitmentApplicants")]
    public virtual TRecruitment Recruitment { get; set; } = null!;

    [ForeignKey("ReviewedBy")]
    [InverseProperty("TRecruitmentApplicants")]
    public virtual MUser? ReviewedByNavigation { get; set; }

    [InverseProperty("Applicant")]
    public virtual ICollection<TFormResponse> TFormResponses { get; set; } = new List<TFormResponse>();
}
