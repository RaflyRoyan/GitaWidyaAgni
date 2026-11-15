using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_timeline_plan")]
[Index("ActivityId", Name = "fk_plan_activity")]
[Index("PeriodId", Name = "fk_plan_period")]
[Index("PicMemberId", Name = "fk_plan_pic")]
[Index("ProgramId", Name = "fk_plan_program")]
[Index("PlanYear", "PlanMonth", Name = "ix_plan_org_year")]
public partial class TTimelinePlan
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("period_id")]
    public int? PeriodId { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("description")]
    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// 1=EVENT, 2=RAPAT, 3=LATIHAN, 4=PERTEMUAN, 5=DEADLINE, 6=LAINNYA
    /// </summary>
    [Column("plan_type")]
    public int PlanType { get; set; }

    [Column("plan_year")]
    public short PlanYear { get; set; }

    /// <summary>
    /// 1-12
    /// </summary>
    [Column("plan_month")]
    public int PlanMonth { get; set; }

    /// <summary>
    /// NULL = tanggal belum pasti
    /// </summary>
    [Column("start_date")]
    public DateOnly? StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// 1=RENCANA, 2=TERJADWAL, 3=SELESAI, 4=BATAL
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    /// <summary>
    /// Terisi saat rencana dijadikan kegiatan
    /// </summary>
    [Column("activity_id")]
    public int? ActivityId { get; set; }

    /// <summary>
    /// Program kerja terkait
    /// </summary>
    [Column("program_id")]
    public int? ProgramId { get; set; }

    /// <summary>
    /// Penanggung jawab
    /// </summary>
    [Column("pic_member_id")]
    public int? PicMemberId { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TTimelinePlans")]
    public virtual TActivity? Activity { get; set; }

    [ForeignKey("PeriodId")]
    [InverseProperty("TTimelinePlans")]
    public virtual MManagementPeriod? Period { get; set; }

    [ForeignKey("PicMemberId")]
    [InverseProperty("TTimelinePlans")]
    public virtual MMember? PicMember { get; set; }

    [ForeignKey("ProgramId")]
    [InverseProperty("TTimelinePlans")]
    public virtual TProgram? Program { get; set; }
}
