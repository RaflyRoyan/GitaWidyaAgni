using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_attendance_record")]
[Index("MemberId", Name = "fk_attendance_member")]
[Index("SessionId", "MemberId", Name = "uq_attendance_session_member", IsUnique = true)]
public partial class TAttendanceRecord
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("session_id")]
    public int SessionId { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    [Column("check_in_at", TypeName = "datetime")]
    public DateTime? CheckInAt { get; set; }

    [Column("check_out_at", TypeName = "datetime")]
    public DateTime? CheckOutAt { get; set; }

    /// <summary>
    /// 1=HADIR, 2=TERLAMBAT, 3=IZIN, 4=SAKIT, 5=ALPA
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

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
    [InverseProperty("TAttendanceRecords")]
    public virtual MMember Member { get; set; } = null!;

    [ForeignKey("SessionId")]
    [InverseProperty("TAttendanceRecords")]
    public virtual TAttendanceSession Session { get; set; } = null!;

    [InverseProperty("Attendance")]
    public virtual TAttendanceRecordIn? TAttendanceRecordIn { get; set; }

    [InverseProperty("Attendance")]
    public virtual TAttendanceRecordOut? TAttendanceRecordOut { get; set; }
}
