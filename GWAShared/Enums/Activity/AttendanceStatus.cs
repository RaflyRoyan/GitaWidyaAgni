using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_attendance_record.status
    /// </summary>
    public enum AttendanceStatus
    {
        [Description("Hadir")] Hadir = 1,
        [Description("Terlambat")] Terlambat = 2,
        [Description("Izin")] Izin = 3,
        [Description("Sakit")] Sakit = 4,
        [Description("Alpa")] Alpa = 5
    }
}
