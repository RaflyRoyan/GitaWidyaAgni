using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Anggota;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Anggota
{
    /// <summary>Anggota › Data Anggota (/Anggota/Anggota): data anggota, akun login, dan foto.</summary>
    [Area("Anggota")]
    public class AnggotaController : BaseController
    {
        private const string KEY_EDIT = "Anggota_KEY_EDIT";
        private readonly AccessService _access;

        public AnggotaController(AccessService access)
        {
            _access = access;
        }

        public IActionResult Index() => HalamanIndex(new AnggotaVM.Index());

        [AccessAs(Acc.Index)]
        public IActionResult List() => Bagian(() => new AnggotaVM.List());

        [AccessAs(Acc.Create)]
        public IActionResult Add() => Bagian(() => new AnggotaVM.Add());

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new AnggotaVM.Edit(encKey));
        }

        // Nama anggota tampil di header (profil user) → cache akses dibersihkan.
        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanBaru([FromBody] AnggotaVM.NewRow model)
            => Aksi(user => { AnggotaVM.Method.Simpan(model, user); _access.ClearAll(); }, "Anggota berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult Update([FromBody] AnggotaVM.NewRow model)
            => Aksi(user => { AnggotaVM.Method.Update(EditKey(KEY_EDIT), model, user); _access.ClearAll(); }, "Anggota berhasil diubah.", KEY_EDIT);

        [HttpPost]
        public IActionResult Delete([FromBody] GwaVM.KeyInput input)
            => Aksi(user => AnggotaVM.Method.Nonaktifkan(input.Key, user), "Anggota dinonaktifkan.");

        // ---------- Foto
        [AccessAs(Acc.Index)]
        public IActionResult Foto(string encKey) => Bagian(() => new AnggotaVM.Foto(encKey));

        [AccessAs(Acc.Index), ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
        public IActionResult Gambar(string key, int id)
        {
            try
            {
                var photo = AnggotaVM.Method.GetFoto(key, id);
                return File(photo.Photo, photo.ContentType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> TambahFoto(string key, IFormFile? foto)
        {
            var bytes = await BacaBerkasAsync(foto);
            return Aksi(user => AnggotaVM.Method.TambahFoto(key, bytes, user), "Foto ditambahkan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult HapusFoto([FromBody] AnggotaVM.FotoInput input)
            => Aksi(user => AnggotaVM.Method.HapusFoto(input, user), "Foto dihapus.");

        // ---------- KTA (Kartu Tanda Anggota). Mencetak menerbitkan token QR bila belum ada, jadi memakai hak Edit.
        private string WebRoot => HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath;
        private string PublicBase => AsetLabel.BaseUrl(Request, HttpContext.RequestServices.GetRequiredService<IConfiguration>());

        [AccessAs(Acc.Edit)]
        public IActionResult CetakKta(string key)
            => Laporan(() => KtaCetak.Satu(LibExtensions.Load<GWALib.Lib.Master.Anggota.Member>(key, "Anggota"), LibUser, WebRoot, PublicBase));

        [AccessAs(Acc.Edit)]
        public IActionResult CetakKtaMassal(int? section)
            => Laporan(() => KtaCetak.Massal(section > 0 ? section : null, LibUser, WebRoot, PublicBase));

        // Kartu hilang: token diganti, QR kartu lama tidak berlaku lagi.
        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult GantiQrKta([FromBody] GwaVM.KeyInput input)
            => Aksi(user => { LibExtensions.Load<GWALib.Lib.Master.Anggota.Member>(input.Key, "Anggota").RenewCard(user).Ok(); },
                "QR KTA diganti. Kartu lama tidak berlaku lagi; cetak ulang KTA.");
    }
}
