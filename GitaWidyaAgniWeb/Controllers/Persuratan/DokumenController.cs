using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Persuratan.DokumenVM;

namespace GitaWidyaAgniWeb.Controllers.Persuratan
{
    /// <summary>Persuratan › Arsip Dokumen (/Persuratan/Dokumen): dokumen dan kategori dokumen.</summary>
    [Area("Persuratan")]
    public class DokumenController : BaseController
    {
        private const string KEY_DOKUMEN = "Dokumen_KEY_EDIT";
        private const string KEY_KATEGORI = "KategoriDokumen_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        // ---------- Dokumen
        [AccessAs(Acc.Index)] public IActionResult ListDokumen() => Bagian(() => new VM.ListDokumen());
        [AccessAs(Acc.Create)] public IActionResult AddDokumen() => Bagian(() => new VM.AddDokumen());
        [AccessAs(Acc.Edit)]
        public IActionResult EditDokumen(string encKey) { TempData[KEY_DOKUMEN] = encKey; return Bagian(() => new VM.EditDokumen(encKey)); }

        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanDokumen([FromForm] VM.NewDokumen m, IFormFile? berkas)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(u => VM.Method.Simpan(m, bytes, berkas, u), "Dokumen diunggah.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> UpdateDokumen([FromForm] VM.NewDokumen m, IFormFile? berkas)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(u => VM.Method.Update(EditKey(KEY_DOKUMEN), m, bytes, berkas, u), "Dokumen diubah.", KEY_DOKUMEN);
        }

        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusDokumen([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Dokumen dihapus permanen.");

        [AccessAs(Acc.Index)]
        public IActionResult Unduh(string key, bool lihat = false)
        {
            try
            {
                var f = VM.Method.Load(key).GetFile();
                return Berkas(f?.Data, f?.FileName, f?.ContentType, lihat);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        // ---------- Kategori
        [AccessAs(Acc.Index)] public IActionResult ListKategori() => Bagian(() => new VM.ListKategori());
        [AccessAs(Acc.Create)] public IActionResult AddKategori() => Bagian(() => new VM.AddKategori());
        [AccessAs(Acc.Edit)]
        public IActionResult EditKategori(string encKey) { TempData[KEY_KATEGORI] = encKey; return Bagian(() => new VM.EditKategori(encKey)); }
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanKategori([FromBody] VM.NewKategori m) => Aksi(u => VM.Method.SimpanKategori(m, u), "Kategori disimpan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateKategori([FromBody] VM.NewKategori m) => Aksi(u => VM.Method.UpdateKategori(EditKey(KEY_KATEGORI), m, u), "Kategori diubah.", KEY_KATEGORI);
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusKategori([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadKategori(i.Key).Delete(u).Ok(); }, "Kategori dinonaktifkan.");
    }
}
