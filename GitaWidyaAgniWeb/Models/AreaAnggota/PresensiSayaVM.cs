using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Presensi;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.AreaAnggota
{
    /// <summary>
    /// Tampilan Area Anggota › Presensi: status kartu sesi (seperti timeline absensi aplikasi lapangan) dan teks jam.
    /// </summary>
    public static class PresensiSayaVM
    {
        // Kunci (dipakai kelas CSS kartu), label, kelas badge, ikon.
        public record Tampil(string Key, string Label, string Css, string Icon);

        public static Tampil StatusOf(AttendanceSession.MySession s)
        {
            var now = DateTime.Now;
            if (s.CheckOutAt != null) return new("selesai", "Selesai", "ps-badge--ok", "ri-checkbox-circle-line");
            if (s.CheckInAt != null && s.CanCheckOut) return new("berjalan", "Sedang berlangsung", "ps-badge--live", "ri-radio-button-line");
            if (s.CheckInAt != null) return new("tidakco", "Tidak check-out", "ps-badge--warn", "ri-error-warning-line");
            if (s.RecordStatus == AttendanceStatus.Izin) return new("izin", "Izin", "ps-badge--info", "ri-mail-check-line");
            if (s.RecordStatus == AttendanceStatus.Sakit) return new("sakit", "Sakit", "ps-badge--info", "ri-first-aid-kit-line");
            if (s.RecordStatus == AttendanceStatus.Alpa) return new("alpa", "Alpa", "ps-badge--danger", "ri-close-circle-line");
            if (s.RecordStatus != null) return new("hadir", EnumHelper.Text(s.RecordStatus.Value), "ps-badge--ok", "ri-checkbox-circle-line");
            if (s.CanCheckIn) return new("siap", now < s.OpenAt ? "Check-in dibuka" : "Belum check-in", "ps-badge--accent", "ri-login-circle-line");
            if (s.SessionStatus == AttendanceSessionStatus.Closed || now > s.CloseAt) return new("terlewat", "Terlewat", "ps-badge--danger", "ri-close-circle-line");
            return new("terjadwal", "Terjadwal", "ps-badge--muted", "ri-calendar-check-line");
        }

        public static string Jam(AttendanceSession.MySession s)
            => s.OpenAt.ToString("HH:mm") + " – " + s.CloseAt.ToString(s.CloseAt.Date == s.OpenAt.Date ? "HH:mm" : "dd MMM HH:mm");
    }
}
