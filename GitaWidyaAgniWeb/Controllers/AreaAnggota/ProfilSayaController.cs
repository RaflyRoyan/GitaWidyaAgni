using GWALib.Lib.Master.Akses;
using GWALib.Lib.Master.Anggota;
using Microsoft.AspNetCore.Mvc;
using GitaWidyaAgniWeb.Infrastructure;
using VM = GitaWidyaAgniWeb.Models.AreaAnggota.ProfilSayaVM;

namespace GitaWidyaAgniWeb.Controllers.AreaAnggota
{
    /// <summary>Area Anggota › Profil Saya (/AreaAnggota/ProfilSaya).</summary>
    public class ProfilSayaController : AreaAnggotaController
    {
        public IActionResult Index() => HalamanIndex(new VM.Index(User.Identity?.Name ?? "", SayaAtauNull()));

        [HttpPost]
        public IActionResult SimpanKontak([FromBody] VM.NewKontak m) => Aksi(u =>
        {
            var saya = Saya();
            saya.Phone = m.Phone;
            saya.Email = m.Email;
            saya.Address = m.Address;
            saya.Update(u).Ok();
        }, "Kontak disimpan.");

        [HttpPost]
        public IActionResult GantiPassword([FromBody] VM.PasswordInput m)
            => Aksi(u => { UserAccount.ChangeOwnPassword(m.Lama ?? "", m.Baru ?? "", u).Ok(); }, "Password diganti. Gunakan password baru saat login berikutnya.");

        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
        public IActionResult Foto()
        {
            var saya = SayaAtauNull();
            var foto = saya == null ? null : MemberPhoto.GetDataList(saya.Id).OrderByDescending(x => x.InsDate).FirstOrDefault();
            return foto == null ? NotFound() : File(foto.Photo, foto.ContentType);
        }

        // KTA milik sendiri (DevExpress Reports, seukuran kartu; ?pdf=1 = unduh PDF). Token QR dibuat saat pertama kali dibuka.
        public IActionResult Kta() => Laporan(() => Models.Anggota.KtaCetak.Satu(Saya(), LibUser,
            HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath,
            AsetLabel.BaseUrl(Request, HttpContext.RequestServices.GetRequiredService<IConfiguration>())));
    }
}
