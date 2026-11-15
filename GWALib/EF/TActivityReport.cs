using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_activity_report")]
[Index("ActivityId", Name = "fk_report_activity")]
[Index("ApprovedBy", Name = "fk_report_approver")]
[Index("ProgramId", Name = "fk_report_program")]
[Index("SubmittedBy", Name = "fk_report_submitter")]
public partial class TActivityReport
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("activity_id")]
    public int? ActivityId { get; set; }

    [Column("program_id")]
    public int? ProgramId { get; set; }

    [Column("submitted_by")]
    public int SubmittedBy { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("summary", TypeName = "text")]
    public string? Summary { get; set; }

    /// <summary>
    /// File laporan kegiatan
    /// </summary>
    [Column("file_data", TypeName = "mediumblob")]
    public byte[]? FileData { get; set; }

    [Column("file_name")]
    [StringLength(255)]
    public string? FileName { get; set; }

    [Column("content_type")]
    [StringLength(100)]
    public string? ContentType { get; set; }

    /// <summary>
    /// 1=DRAFT, 2=SUBMITTED, 3=REVISION, 4=APPROVED
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("submitted_at", TypeName = "datetime")]
    public DateTime? SubmittedAt { get; set; }

    [Column("approved_by")]
    public int? ApprovedBy { get; set; }

    [Column("approved_at", TypeName = "datetime")]
    public DateTime? ApprovedAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TActivityReports")]
    public virtual TActivity? Activity { get; set; }

    [ForeignKey("ApprovedBy")]
    [InverseProperty("TActivityReportApprovedByNavigations")]
    public virtual MUser? ApprovedByNavigation { get; set; }

    [ForeignKey("ProgramId")]
    [InverseProperty("TActivityReports")]
    public virtual TProgram? Program { get; set; }

    [ForeignKey("SubmittedBy")]
    [InverseProperty("TActivityReportSubmittedByNavigations")]
    public virtual MUser SubmittedByNavigation { get; set; } = null!;
}
