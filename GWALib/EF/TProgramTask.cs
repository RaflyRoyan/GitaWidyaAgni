using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_program_task")]
[Index("DivisionId", Name = "fk_task_division")]
[Index("DoneBy", Name = "fk_task_doneby")]
[Index("ResponsibleMemberId", Name = "fk_task_member")]
[Index("PeriodId", Name = "fk_task_period")]
[Index("ProgramId", Name = "fk_task_program")]
[Index("PeriodId", "IsMilestone", "DueDate", Name = "ix_task_milestone")]
[Index("PeriodId", "Status", "DueDate", Name = "ix_task_period")]
public partial class TProgramTask
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("period_id")]
    public int PeriodId { get; set; }

    /// <summary>
    /// NULL = tugas umum pengurus (bukan bagian program kerja)
    /// </summary>
    [Column("program_id")]
    public int? ProgramId { get; set; }

    /// <summary>
    /// Opsional: tugas untuk divisi tertentu
    /// </summary>
    [Column("division_id")]
    public int? DivisionId { get; set; }

    [Column("responsible_member_id")]
    public int? ResponsibleMemberId { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    /// <summary>
    /// 1=RENDAH, 2=SEDANG, 3=TINGGI
    /// </summary>
    [Column("priority")]
    public int Priority { get; set; }

    [Column("start_date")]
    public DateOnly? StartDate { get; set; }

    [Column("due_date")]
    public DateOnly? DueDate { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya | tampil di timeline periode
    /// </summary>
    [Column("is_milestone")]
    public int IsMilestone { get; set; }

    /// <summary>
    /// 1=BELUM, 2=PROSES, 3=SELESAI, 4=BATAL (sama dengan t_activity_checklist)
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("completed_at", TypeName = "datetime")]
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// User yang menandai selesai
    /// </summary>
    [Column("done_by")]
    public int? DoneBy { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("DivisionId")]
    [InverseProperty("TProgramTasks")]
    public virtual MDivision? Division { get; set; }

    [ForeignKey("DoneBy")]
    [InverseProperty("TProgramTasks")]
    public virtual MUser? DoneByNavigation { get; set; }

    [ForeignKey("PeriodId")]
    [InverseProperty("TProgramTasks")]
    public virtual MManagementPeriod Period { get; set; } = null!;

    [ForeignKey("ProgramId")]
    [InverseProperty("TProgramTasks")]
    public virtual TProgram? Program { get; set; }

    [ForeignKey("ResponsibleMemberId")]
    [InverseProperty("TProgramTasks")]
    public virtual MMember? ResponsibleMember { get; set; }
}
