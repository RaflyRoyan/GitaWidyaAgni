using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Informasi.BannerVM;

namespace GitaWidyaAgniWeb.Controllers.Informasi
{
    /// <summary>Informasi › Banner (/Informasi/Banner): slide hero landing page.</summary>
    [Area("Informasi")]
    public class BannerController : BaseController
    {
        private const string KEY_EDIT = "Banner_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());
        [AccessAs(Acc.Index)] public IActionResult List() => Bagian(() => new VM.List());
        [AccessAs(Acc.Create)] public IActionResult Add() => Bagian(() => new VM.Add());
        [AccessAs(Acc.Edit)]
        public IActionResult Edit(string encKey) { TempData[KEY_EDIT] = encKey; return Bagian(() => new VM.Edit(encKey)); }

        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanBaru([FromForm] VM.NewRow m, IFormFile? gambar)
        {
            var bytes = await BacaBerkasAsync(gambar);
            return Aksi(u => VM.Method.Simpan(m, bytes, u), "Banner disimpan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> Update([FromForm] VM.NewRow m, IFormFile? gambar)
        {
            var bytes = await BacaBerkasAsync(gambar);
            return Aksi(u => VM.Method.Update(EditKey(KEY_EDIT), m, bytes, u), "Banner diubah.", KEY_EDIT);
        }

        [HttpPost, AccessAs(Acc.Delete)] public IActionResult Delete([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Banner dihapus.");

        [AccessAs(Acc.Index), ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
        public IActionResult Gambar(string key)
        {
            try
            {
                var f = VM.Method.Load(key).GetImage();
                return Berkas(f?.Data, null, f?.ContentType, true);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
