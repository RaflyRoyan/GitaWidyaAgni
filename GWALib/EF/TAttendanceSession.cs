using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_attendance_session")]
[Index("ActivityId", Name = "fk_session_activity")]
[Index("QrToken", Name = "uq_session_qr", IsUnique = true)]
[Index("ScheduleId", "OpenAt", Name = "uq_session_schedule_date", IsUnique = true)]
public partial class TAttendanceSession
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("activity_id")]
    public int ActivityId { get; set; }

    /// <summary>
    /// Terisi jika sesi dibuat otomatis dari jadwal rutin
    /// </summary>
    [Column("schedule_id")]
    public int? ScheduleId { get; set; }

    /// <summary>
    /// mis. Hari 1 - Sesi Pagi
    /// </summary>
    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Column("open_at", TypeName = "datetime")]
    public DateTime OpenAt { get; set; }

    [Column("close_at", TypeName = "datetime")]
    public DateTime CloseAt { get; set; }

    /// <summary>
    /// Check-in lewat open_at + menit ini = TERLAMBAT
    /// </summary>
    [Column("late_tolerance_minute")]
    public int LateToleranceMinute { get; set; }

    /// <summary>
    /// 1=TANPA_QR, 2=QR
    /// </summary>
    [Column("attendance_mode")]
    public int AttendanceMode { get; set; }

    /// <summary>
    /// Nama tempat
    /// </summary>
    [Column("location")]
    [StringLength(255)]
    public string? Location { get; set; }

    [Column("qr_token")]
    [StringLength(100)]
    public string? QrToken { get; set; }

    [Column("latitude")]
    [Precision(10, 7)]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    [Precision(10, 7)]
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Radius geofence yang diizinkan
    /// </summary>
    [Column("radius_meter")]
    public int? RadiusMeter { get; set; }

    /// <summary>
    /// 1=SCHEDULED, 2=OPEN, 3=CLOSED, 4=CANCELLED
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    /// <summary>
    /// Diisi jika status 4, mis. libur UTS
    /// </summary>
    [Column("cancel_reason")]
    [StringLength(255)]
    public string? CancelReason { get; set; }

    /// <summary>
    /// Pengingat check-in terkirim
    /// </summary>
    [Column("reminder_sent_at", TypeName = "datetime")]
    public DateTime? ReminderSentAt { get; set; }

    /// <summary>
    /// Pengingat check-out terkirim
    /// </summary>
    [Column("checkout_reminder_sent_at", TypeName = "datetime")]
    public DateTime? CheckoutReminderSentAt { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("TAttendanceSessions")]
    public virtual TActivity Activity { get; set; } = null!;

    [ForeignKey("ScheduleId")]
    [InverseProperty("TAttendanceSessions")]
    public virtual TActivitySchedule? Schedule { get; set; }

    [InverseProperty("Session")]
    public virtual ICollection<TAttendanceRecord> TAttendanceRecords { get; set; } = new List<TAttendanceRecord>();

    [InverseProperty("Session")]
    public virtual TMeetingMinute? TMeetingMinute { get; set; }
}
