using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Informasi.BeritaVM;

namespace GitaWidyaAgniWeb.Controllers.Informasi
{
    /// <summary>Informasi › Berita (/Informasi/Berita). Terbitkan / tarik / arsipkan = Process.</summary>
    [Area("Informasi")]
    public class BeritaController : BaseController
    {
        private const string KEY_EDIT = "Berita_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());
        [AccessAs(Acc.Index)] public IActionResult List() => Bagian(() => new VM.List());
        [AccessAs(Acc.Create)] public IActionResult Add() => Bagian(() => new VM.Add());
        [AccessAs(Acc.Edit)]
        public IActionResult Edit(string encKey) { TempData[KEY_EDIT] = encKey; return Bagian(() => new VM.Edit(encKey)); }

        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanBaru([FromForm] VM.NewRow m, IFormFile? sampul)
        {
            var bytes = await BacaBerkasAsync(sampul);
            return Aksi(u => VM.Method.Simpan(m, bytes, u), "Berita disimpan sebagai Draft.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> Update([FromForm] VM.NewRow m, IFormFile? sampul, bool hapusSampul = false)
        {
            var bytes = await BacaBerkasAsync(sampul);
            return Aksi(u => VM.Method.Update(EditKey(KEY_EDIT), m, bytes, hapusSampul, u), "Berita diubah.", KEY_EDIT);
        }

        [HttpPost, AccessAs(Acc.Delete)] public IActionResult Delete([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Berita dihapus.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Terbitkan([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Publish(u).Ok(); }, "Berita diterbitkan.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Tarik([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Unpublish(u).Ok(); }, "Berita ditarik ke Draft.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Arsipkan([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Archive(u).Ok(); }, "Berita diarsipkan.");

        [AccessAs(Acc.Index), ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
        public IActionResult Sampul(string key)
        {
            try
            {
                var f = VM.Method.Load(key).GetCover();
                return Berkas(f?.Data, null, f?.ContentType, true);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
