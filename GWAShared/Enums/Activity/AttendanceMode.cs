using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_activity_schedule.attendance_mode, t_attendance_session.attendance_mode | TanpaQr = anggota check-in dari daftar sesi
    /// (lokasi + selfie); Qr = wajib scan QR yang ditampilkan pengurus (mis. rapat)
    /// </summary>
    public enum AttendanceMode
    {
        [Description("Tanpa QR")] TanpaQr = 1,
        [Description("Wajib Scan QR")] Qr = 2
    }
}
