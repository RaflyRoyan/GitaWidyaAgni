using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_certificate")]
[Index("AchievementId", Name = "fk_cert_achievement")]
[Index("ActivityId", Name = "fk_cert_activity")]
[Index("AssignmentId", Name = "fk_cert_assignment")]
[Index("MemberId", Name = "fk_cert_member")]
[Index("CertificateNumber", Name = "uq_certificate_number", IsUnique = true)]
[Index("VerificationCode", Name = "uq_certificate_verify", IsUnique = true)]
public partial class TCertificate
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    /// <summary>
    /// 1=PESERTA, 2=PANITIA / TIM_PELAKSANA (t_committee_member), 3=PEMATERI, 4=PENGURUS, 5=PRESTASI, 6=LAINNYA
    /// </summary>
    [Column("certificate_type")]
    public int CertificateType { get; set; }

    [Column("activity_id")]
    public int? ActivityId { get; set; }

    /// <summary>
    /// Untuk sertifikat kepengurusan
    /// </summary>
    [Column("assignment_id")]
    public int? AssignmentId { get; set; }

    /// <summary>
    /// Untuk sertifikat prestasi
    /// </summary>
    [Column("achievement_id")]
    public int? AchievementId { get; set; }

    [Column("certificate_number")]
    [StringLength(100)]
    public string CertificateNumber { get; set; } = null!;

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("issued_date")]
    public DateOnly IssuedDate { get; set; }

    /// <summary>
    /// Kode acak untuk cek keaslian sertifikat secara publik
    /// </summary>
    [Column("verification_code")]
    [StringLength(32)]
    public string VerificationCode { get; set; } = null!;

    [Column("file_data", TypeName = "mediumblob")]
    public byte[]? FileData { get; set; }

    [Column("file_name")]
    [StringLength(255)]
    public string? FileName { get; set; }

    /// <summary>
    /// 1=TERBIT, 2=DICABUT
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

    [ForeignKey("AchievementId")]
    [InverseProperty("TCertificates")]
    public virtual TAchievement? Achievement { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TCertificates")]
    public virtual TActivity? Activity { get; set; }

    [ForeignKey("AssignmentId")]
    [InverseProperty("TCertificates")]
    public virtual TManagementAssignment? Assignment { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("TCertificates")]
    public virtual MMember Member { get; set; } = null!;
}
