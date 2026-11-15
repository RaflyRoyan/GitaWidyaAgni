using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_attendance_session.status
    /// </summary>
    public enum AttendanceSessionStatus
    {
        [Description("Terjadwal")] Scheduled = 1,
        [Description("Dibuka")] Open = 2,
        [Description("Ditutup")] Closed = 3,
        [Description("Dibatalkan")] Cancelled = 4
    }
}
