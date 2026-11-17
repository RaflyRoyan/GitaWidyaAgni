using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Keuangan.HutangVM;

namespace GitaWidyaAgniWeb.Controllers.Keuangan
{
    /// <summary>Keuangan › Hutang (/Keuangan/Hutang): hutang organisasi, pembayaran, lampiran, penghapusan.</summary>
    [Area("Keuangan")]
    public class HutangController : BaseController
    {
        private const string KEY_EDIT = "Hutang_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        [AccessAs(Acc.Index)] public IActionResult List() => Bagian(() => new VM.List());
        [AccessAs(Acc.Create)] public IActionResult Add() => Bagian(() => new VM.Add());

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new VM.Edit(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanBaru([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Simpan(m, u), "Hutang dicatat.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult Update([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Update(EditKey(KEY_EDIT), m, u), "Hutang diubah.", KEY_EDIT);
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Hapuskan([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.Load(i.Key).WriteOff(i.Keterangan, u).Ok(); }, "Hutang dihapuskan.");
        [HttpPost] public IActionResult Delete([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Hutang dihapus.");

        // ---------- Pembayaran
        [AccessAs(Acc.Payment)] public IActionResult Bayar(string encKey) => FormBayar(() => VM.Method.FormBayar(encKey));
        [HttpPost, AccessAs(Acc.Payment)] public IActionResult SimpanBayar([FromBody] GwaVM.BayarInput m) => Aksi(u => VM.Method.Bayar(m, u), "Pembayaran hutang dicatat.");
        [HttpPost, AccessAs(Acc.Payment)] public IActionResult HapusBayar([FromBody] GwaVM.AlasanInput i) => Aksi(u => VM.Method.HapusBayar(i, u), "Pembayaran dihapus; transaksinya di-void.");

        // ---------- Riwayat + lampiran
        [AccessAs(Acc.Index)] public IActionResult Riwayat(string encKey) => Bagian(() => new VM.Riwayat(encKey));

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> TambahLampiran(string key, IFormFile? berkas)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(u => VM.Method.TambahLampiran(key, bytes, berkas, u), "Lampiran ditambahkan.");
        }

        [HttpPost, AccessAs(Acc.Edit)] public IActionResult HapusLampiran([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadLampiran(i.Key).Delete(u).Ok(); }, "Lampiran dihapus.");

        [AccessAs(Acc.Index)]
        public IActionResult BerkasLampiran(string key)
        {
            try
            {
                var file = VM.Method.LoadLampiran(key).GetFile();
                return Berkas(file?.Data, file?.FileName, file?.FileType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
