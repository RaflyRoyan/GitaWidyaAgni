using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_scheduled_job_log")]
[Index("JobName", "StartedAt", Name = "ix_joblog")]
public partial class TScheduledJobLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// mis. SEND_OUTBOX, REMIND_CHECKLIST, EXPIRE_MOU, CLOSE_ATTENDANCE
    /// </summary>
    [Column("job_name")]
    [StringLength(100)]
    public string JobName { get; set; } = null!;

    [Column("started_at", TypeName = "datetime")]
    public DateTime StartedAt { get; set; }

    [Column("finished_at", TypeName = "datetime")]
    public DateTime? FinishedAt { get; set; }

    /// <summary>
    /// 1=BERJALAN, 2=SUKSES, 3=GAGAL
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("processed_count")]
    public int ProcessedCount { get; set; }

    [Column("message")]
    [StringLength(1000)]
    public string? Message { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }
}
