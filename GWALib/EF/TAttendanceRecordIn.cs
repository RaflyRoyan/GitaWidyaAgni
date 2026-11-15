using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_attendance_record_in")]
[Index("AttendanceId", Name = "uq_record_in_attendance", IsUnique = true)]
public partial class TAttendanceRecordIn
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("attendance_id")]
    public int AttendanceId { get; set; }

    [Column("latitude")]
    [Precision(10, 7)]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    [Precision(10, 7)]
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Foto check-in (wajib)
    /// </summary>
    [Column("photo", TypeName = "mediumblob")]
    public byte[] Photo { get; set; } = null!;

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("AttendanceId")]
    [InverseProperty("TAttendanceRecordIn")]
    public virtual TAttendanceRecord Attendance { get; set; } = null!;
}
