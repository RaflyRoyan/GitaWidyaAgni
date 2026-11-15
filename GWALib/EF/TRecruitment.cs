using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_recruitment")]
[Index("PeriodId", Name = "fk_recruitment_period")]
public partial class TRecruitment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("period_id")]
    public int? PeriodId { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("open_date", TypeName = "datetime")]
    public DateTime OpenDate { get; set; }

    [Column("close_date", TypeName = "datetime")]
    public DateTime CloseDate { get; set; }

    [Column("quota")]
    public int? Quota { get; set; }

    /// <summary>
    /// 0=Hanya mahasiswa, 1=Juga menerima pendaftar umum
    /// </summary>
    [Column("accept_public")]
    public int AcceptPublic { get; set; }

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

    [ForeignKey("PeriodId")]
    [InverseProperty("TRecruitments")]
    public virtual MManagementPeriod? Period { get; set; }

    [InverseProperty("Recruitment")]
    public virtual ICollection<TForm> TForms { get; set; } = new List<TForm>();

    [InverseProperty("Recruitment")]
    public virtual ICollection<TRecruitmentApplicant> TRecruitmentApplicants { get; set; } = new List<TRecruitmentApplicant>();
}
