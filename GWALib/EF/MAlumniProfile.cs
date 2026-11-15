using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("m_alumni_profile")]
[Index("MemberId", Name = "uq_alumni_member", IsUnique = true)]
public partial class MAlumniProfile
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    [Column("graduation_year")]
    public short? GraduationYear { get; set; }

    [Column("current_job")]
    [StringLength(150)]
    public string? CurrentJob { get; set; }

    [Column("company")]
    [StringLength(150)]
    public string? Company { get; set; }

    [Column("industry")]
    [StringLength(100)]
    public string? Industry { get; set; }

    [Column("city")]
    [StringLength(100)]
    public string? City { get; set; }

    [Column("linkedin_url")]
    [StringLength(255)]
    public string? LinkedinUrl { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya | alumni setuju dihubungi pengurus / adik tingkat
    /// </summary>
    [Column("is_contactable")]
    public int IsContactable { get; set; }

    [Column("notes")]
    [StringLength(500)]
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
    [InverseProperty("MAlumniProfile")]
    public virtual MMember Member { get; set; } = null!;
}
