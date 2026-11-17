using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Kemitraan.MitraVM;

namespace GitaWidyaAgniWeb.Controllers.Kemitraan
{
    /// <summary>Kemitraan › Mitra (/Kemitraan/Mitra): mitra / pihak luar, logo, kontak.</summary>
    [Area("Kemitraan")]
    public class MitraController : BaseController
    {
        private const string KEY_EDIT = "Mitra_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        [AccessAs(Acc.Index)] public IActionResult List() => Bagian(() => new VM.List());
        [AccessAs(Acc.Create)] public IActionResult Add() => Bagian(() => new VM.Add());

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new VM.Edit(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanBaru([FromForm] VM.NewRow m, IFormFile? logo)
        {
            var bytes = await BacaBerkasAsync(logo);
            return Aksi(u => VM.Method.Simpan(m, bytes, logo?.FileName, u), "Mitra disimpan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> Update([FromForm] VM.NewRow m, IFormFile? logo, bool hapusLogo = false)
        {
            var bytes = await BacaBerkasAsync(logo);
            return Aksi(u => VM.Method.Update(EditKey(KEY_EDIT), m, bytes, logo?.FileName, hapusLogo, u), "Mitra diubah.", KEY_EDIT);
        }

        [HttpPost] public IActionResult Delete([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Mitra dinonaktifkan.");

        [AccessAs(Acc.Index), ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
        public IActionResult Logo(string key)
        {
            try
            {
                var logo = VM.Method.Load(key).GetLogo();
                return Berkas(logo?.Data, null, logo?.ContentType, true);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        // ---------- Kontak
        [AccessAs(Acc.Index)] public IActionResult Kontak(string encKey) => Bagian(() => new VM.Kontak(encKey));
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult TambahKontak([FromBody] VM.KontakInput m) => Aksi(u => VM.Method.TambahKontak(m, u), "Kontak ditambahkan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UtamaKontak([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.JadikanUtama(i.Key, u), "Kontak utama diganti.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult HapusKontak([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadKontak(i.Key).Delete(u).Ok(); }, "Kontak dinonaktifkan.");
    }
}
