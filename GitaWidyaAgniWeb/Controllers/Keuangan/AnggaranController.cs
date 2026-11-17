using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Keuangan.AnggaranVM;

namespace GitaWidyaAgniWeb.Controllers.Keuangan
{
    /// <summary>Keuangan › Anggaran (/Keuangan/Anggaran): anggaran per periode, item, dokumen, ajukan / setujui / tolak.</summary>
    [Area("Keuangan")]
    public class AnggaranController : BaseController
    {
        private const string KEY_EDIT = "Anggaran_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        [AccessAs(Acc.Index)] public IActionResult List() => Bagian(() => new VM.List());
        [AccessAs(Acc.Create)] public IActionResult Add() => Bagian(() => new VM.Add());

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new VM.Edit(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanBaru([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Simpan(m, u), "Anggaran dibuat. Tambahkan item lalu ajukan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult Update([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Update(EditKey(KEY_EDIT), m, u), "Anggaran diubah.", KEY_EDIT);
        [HttpPost, AccessAs(Acc.Submit)] public IActionResult Ajukan([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Submit(u).Ok(); }, "Anggaran diajukan.");
        [HttpPost, AccessAs(Acc.Approve)] public IActionResult Setujui([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Approve(u).Ok(); }, "Anggaran disetujui.");
        [HttpPost, AccessAs(Acc.Approve)] public IActionResult Tolak([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Reject(u).Ok(); }, "Anggaran ditolak; bisa diubah dan diajukan lagi.");
        [HttpPost] public IActionResult Delete([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Anggaran dihapus.");

        // ---------- Rincian
        [AccessAs(Acc.Index)] public IActionResult Rincian(string encKey) => Bagian(() => new VM.Rincian(encKey));
        [AccessAs(Acc.Index)] public IActionResult ItemList(string encKey) => Bagian(() => new VM.ItemList(encKey));
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult TambahItem([FromBody] VM.ItemInput m) => Aksi(u => VM.Method.TambahItem(m, u), "Item ditambahkan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult HapusItem([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusItem(i.Key, u), "Item dihapus.");

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> TambahDokumen(string key, IFormFile? berkas)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(u => VM.Method.TambahDokumen(key, bytes, berkas, u), "Dokumen ditambahkan.");
        }

        [HttpPost, AccessAs(Acc.Edit)] public IActionResult HapusDokumen([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadDokumen(i.Key).Delete(u).Ok(); }, "Dokumen dihapus.");

        [AccessAs(Acc.Index)]
        public IActionResult BerkasDokumen(string key)
        {
            try
            {
                var file = VM.Method.LoadDokumen(key).GetFile();
                return Berkas(file?.Data, file?.FileName, file?.FileType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
