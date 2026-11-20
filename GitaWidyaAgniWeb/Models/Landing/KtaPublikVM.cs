using GWALib.Lib.Master.Anggota;

namespace GitaWidyaAgniWeb.Models.Landing
{
    /// <summary>Halaman publik verifikasi KTA (/kta/{token}); Data null = QR tidak dikenal / sudah diganti.</summary>
    public class KtaPublikVM
    {
        public string Token { get; set; } = "";
        public Member.CardCheck? Data { get; set; }
        public string OrgName { get; set; } = "Gita Widya Agni";
        public string? WhatsApp { get; set; }
    }
}
