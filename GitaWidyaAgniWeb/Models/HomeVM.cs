using GWALib.Lib.Transaksi.Information;

namespace GitaWidyaAgniWeb.Models
{
    /// <summary>
    /// Dashboard: ringkasan GWALib Dashboard + nama tampilan user. Setiap bagian hanya tampil bila user boleh membuka
    /// halaman sumbernya (Can("Transaksi") dst.; SUPERADMIN / allowed null = semua).
    /// </summary>
    public class HomeVM
    {
        public Dashboard Data { get; set; } = new();
        public string DisplayName { get; set; } = "";
        public string? Error { get; set; }
        public HashSet<string>? Allowed { get; set; }
        /// <summary>Pintasan dari menu user (judul, ikon, URL), maksimal 8.</summary>
        public List<(string Title, string Icon, string Url)> Shortcuts { get; set; } = new();
        /// <summary>Anggota aktif yang berulang tahun dalam BIRTHDAY_DAYS hari ke depan (hari ini termasuk).</summary>
        public List<GWALib.Lib.Master.Anggota.Member.BirthdayRow> Birthdays { get; set; } = new();
        public const int BIRTHDAY_DAYS = 7;
        /// <summary>Ringkasan pribadi bila user punya data anggota (Dashboard.GetMine), else null.</summary>
        public Dashboard.MySummary? Saya { get; set; }
        /// <summary>Sesi presensi anggota hari ini (belum selesai) + mendatang, maksimal 3 (AttendanceSession.GetMyAttendance).</summary>
        public List<GWALib.Lib.Transaksi.Presensi.AttendanceSession.MySession> SesiSaya { get; set; } = new();

        public bool Can(string controller) => Allowed == null || Allowed.Contains(controller.ToLowerInvariant() + "/index");
    }
}
