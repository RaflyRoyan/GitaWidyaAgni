using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Prestasi;
using GWAShared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.AreaAnggota
{
    /// <summary>Area Anggota › Prestasi Saya (/AreaAnggota/PrestasiSaya): prestasi, poin keaktifan, lomba yang diikuti, dan sertifikat milik anggota login.</summary>
    public class PrestasiSayaController : AreaAnggotaController
    {
        public class Model
        {
            public List<Achievement> Prestasi { get; set; } = new();
            public List<MemberPoint> Poin { get; set; } = new();
            public List<CompetitionParticipant> Lomba { get; set; } = new();
            public List<Certificate> Sertifikat { get; set; } = new();
            public int Peringkat { get; set; }
        }

        public IActionResult Index()
        {
            var saya = SayaAtauNull();
            if (saya == null)
            {
                return HalamanIndex(null);
            }
            var board = MemberPoint.GetLeaderboard(null, 1000);
            var rank = board.FindIndex(x => x.MemberId == saya.Id);
            return HalamanIndex(new Model
            {
                Prestasi = Achievement.GetDataList(memberId: saya.Id).OrderByDescending(x => x.AchievedDate).ToList(),
                Poin = MemberPoint.GetDataList(memberId: saya.Id).OrderByDescending(x => x.EarnedDate).ToList(),
                Lomba = CompetitionParticipant.GetDataList(memberId: saya.Id).OrderByDescending(x => x.InsDate).ToList(),
                Sertifikat = Certificate.GetDataList(memberId: saya.Id, status: CertificateStatus.Terbit).OrderByDescending(x => x.IssuedDate).ToList(),
                Peringkat = rank < 0 ? 0 : rank + 1
            });
        }

        // Cetak sertifikat milik sendiri (DevExpress Reports; ?pdf=1 = unduh PDF).
        public IActionResult CetakSertifikat(string key) => Laporan(() =>
        {
            var c = LibExtensions.Load<Certificate>(key, "Sertifikat");
            if (c.MemberId != Saya().Id || c.Status != CertificateStatus.Terbit)
            {
                throw new ArgumentException("Sertifikat tidak ditemukan.");
            }
            return Models.Prestasi.SertifikatVM.Method.Cetak(c,
                HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath,
                AsetLabel.BaseUrl(Request, HttpContext.RequestServices.GetRequiredService<IConfiguration>()));
        });

        // Unduh berkas sertifikat milik sendiri.
        public IActionResult Sertifikat(string key)
        {
            try
            {
                var c = LibExtensions.Load<Certificate>(key, "Sertifikat");
                if (c.MemberId != Saya().Id)
                {
                    return NotFound();
                }
                var f = c.GetFile();
                return Berkas(f?.Data, f?.FileName);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
