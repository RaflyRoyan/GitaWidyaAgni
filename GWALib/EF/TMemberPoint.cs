using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_member_point")]
[Index("PeriodId", Name = "fk_point_period")]
[Index("MemberId", "EarnedDate", Name = "ix_point_member")]
[Index("MemberId", "SourceType", "RefId", Name = "uq_member_point_source", IsUnique = true)]
public partial class TMemberPoint
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    [Column("period_id")]
    public int? PeriodId { get; set; }

    /// <summary>
    /// 1=HADIR_KEGIATAN, 2=PANITIA / TIM_PELAKSANA (t_committee_member), 3=PEMATERI, 4=PENGURUS, 5=PRESTASI, 6=LAINNYA
    /// </summary>
    [Column("source_type")]
    public int SourceType { get; set; }

    /// <summary>
    /// id sumber: t_attendance_record, t_activity_participant, t_management_assignment, t_achievement
    /// </summary>
    [Column("ref_id")]
    public int? RefId { get; set; }

    [Column("points")]
    public int Points { get; set; }

    [Column("earned_date")]
    public DateOnly EarnedDate { get; set; }

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
    [InverseProperty("TMemberPoints")]
    public virtual MMember Member { get; set; } = null!;

    [ForeignKey("PeriodId")]
    [InverseProperty("TMemberPoints")]
    public virtual MManagementPeriod? Period { get; set; }
}
