using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Pengadaan.PengadaanVM;

namespace GitaWidyaAgniWeb.Controllers.Pengadaan
{
    /// <summary>
    /// Pengadaan (/Pengadaan/Pengadaan). Ajukan = Submit; setuju / revisi / tolak = Approve (penyetuju per langkah dari alur PROCUREMENT);
    /// beli, harga aktual, penerimaan, batal = Process; bayar = Payment.
    /// </summary>
    [Area("Pengadaan")]
    public class PengadaanController : BaseController
    {
        private const string KEY_EDIT = "Pengadaan_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        [AccessAs(Acc.Index)] public IActionResult List() => Bagian(() => new VM.List());
        [AccessAs(Acc.Create)] public IActionResult Add() => Bagian(() => new VM.Add());

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new VM.Edit(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanBaru([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Simpan(m, u), "Pengadaan dibuat. Tambahkan barang dari Detail lalu ajukan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult Update([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Update(EditKey(KEY_EDIT), m, u), "Pengadaan diubah.", KEY_EDIT);
        [HttpPost] public IActionResult Delete([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Pengadaan dihapus.");

        [HttpPost, AccessAs(Acc.Submit)] public IActionResult Ajukan([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Submit(u).Ok(); }, "Pengadaan diajukan.");
        [HttpPost, AccessAs(Acc.Approve)] public IActionResult Setujui([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.Load(i.Key).Approve(i.Keterangan, null, null, null, u).Ok(); }, "Keputusan Setuju dicatat.");
        [HttpPost, AccessAs(Acc.Approve)] public IActionResult Revisi([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.Load(i.Key).RequestRevision(i.Keterangan, u).Ok(); }, "Pengadaan dikembalikan untuk revisi.");
        [HttpPost, AccessAs(Acc.Approve)] public IActionResult Tolak([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.Load(i.Key).Reject(i.Keterangan, u).Ok(); }, "Pengadaan ditolak.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Batal([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.Load(i.Key).Cancel(i.Keterangan, u).Ok(); }, "Pengadaan dibatalkan.");

        [AccessAs(Acc.Process)] public IActionResult OpsiPengajuanDana() => Json(VM.Method.OpsiPengajuanDana());
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Beli([FromBody] VM.BeliInput i) => Aksi(u => VM.Method.Beli(i, u), "Status pengadaan: Dibeli.");

        // ---------- Detail
        [AccessAs(Acc.Index)] public IActionResult Detail(string encKey) => Bagian(() => new VM.Detail(encKey));
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult TambahItem([FromBody] VM.ItemInput m) => Aksi(u => VM.Method.TambahItem(m, u), "Barang ditambahkan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult HapusItem([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadItem(i.Key).Delete(u).Ok(); }, "Barang dihapus.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult HargaAktual([FromBody] VM.AktualInput m) => Aksi(u => VM.Method.HargaAktual(m, u), "Harga aktual disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> TambahPenawaran([FromForm] VM.PenawaranInput m, IFormFile? berkas)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(u => VM.Method.TambahPenawaran(m, bytes, berkas, u), "Penawaran ditambahkan.");
        }

        [HttpPost, AccessAs(Acc.Edit)] public IActionResult PilihPenawaran([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadPenawaran(i.Key).Select(u).Ok(); }, "Penawaran dipilih.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult HapusPenawaran([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadPenawaran(i.Key).Delete(u).Ok(); }, "Penawaran dihapus.");

        [AccessAs(Acc.Index)]
        public IActionResult BerkasPenawaran(string key)
        {
            try
            {
                var file = VM.Method.LoadPenawaran(key).GetFile();
                return Berkas(file?.Data, file?.FileName, file?.ContentType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        // ---------- Pembayaran & penerimaan
        [AccessAs(Acc.Payment)] public IActionResult Bayar(string encKey) => FormBayar(() => VM.Method.FormBayar(encKey));
        [HttpPost, AccessAs(Acc.Payment)] public IActionResult SimpanBayar([FromBody] GwaVM.BayarInput m) => Aksi(u => VM.Method.Bayar(m, u), "Pembayaran dicatat.");
        [HttpPost, AccessAs(Acc.Payment)] public IActionResult BatalBayar([FromBody] VM.BatalBayarInput m) => Aksi(u => VM.Method.BatalBayar(m, u), "Pembayaran dibatalkan.");

        [AccessAs(Acc.Process)] public IActionResult Terima(string encKey) => Bagian(() => new VM.Terima(encKey));
        [HttpPost, AccessAs(Acc.Process)] public IActionResult SimpanTerima([FromBody] VM.TerimaInput m) => Aksi(u => VM.Method.SimpanTerima(m, u), "Penerimaan barang dicatat.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult HapusTerima([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadTerima(i.Key).Delete(u).Ok(); }, "Penerimaan dihapus.");
    }
}
