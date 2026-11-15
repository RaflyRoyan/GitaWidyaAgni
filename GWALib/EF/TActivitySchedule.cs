using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_activity_schedule")]
[Index("ActivityId", "IsActive", "DayOfWeek", Name = "ix_schedule_activity")]
public partial class TActivitySchedule
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("activity_id")]
    public int ActivityId { get; set; }

    /// <summary>
    /// 1=Senin, 2=Selasa, 3=Rabu, 4=Kamis, 5=Jumat, 6=Sabtu, 7=Minggu
    /// </summary>
    [Column("day_of_week")]
    public int DayOfWeek { get; set; }

    [Column("start_time", TypeName = "time")]
    public TimeOnly StartTime { get; set; }

    [Column("end_time", TypeName = "time")]
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// Nama tempat, mis. GOR Kampus
    /// </summary>
    [Column("location")]
    [StringLength(255)]
    public string? Location { get; set; }

    [Column("latitude")]
    [Precision(10, 7)]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    [Precision(10, 7)]
    public decimal? Longitude { get; set; }

    [Column("radius_meter")]
    public int? RadiusMeter { get; set; }

    [Column("late_tolerance_minute")]
    public int LateToleranceMinute { get; set; }

    [Column("valid_from")]
    public DateOnly ValidFrom { get; set; }

    /// <summary>
    /// NULL = berlaku sampai kegiatan selesai / dinonaktifkan
    /// </summary>
    [Column("valid_until")]
    public DateOnly? ValidUntil { get; set; }

    /// <summary>
    /// 0=Tidak, 1=Ya
    /// </summary>
    [Column("is_active")]
    public int IsActive { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TActivitySchedules")]
    public virtual TActivity Activity { get; set; } = null!;

    [InverseProperty("Schedule")]
    public virtual ICollection<TAttendanceSession> TAttendanceSessions { get; set; } = new List<TAttendanceSession>();
}
