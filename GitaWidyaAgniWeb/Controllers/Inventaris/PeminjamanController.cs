using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Inventaris;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Inventaris.PeminjamanVM;

namespace GitaWidyaAgniWeb.Controllers.Inventaris
{
    /// <summary>Inventaris › Peminjaman (/Inventaris/Peminjaman): setujui / tolak = Approve; checkout, kembali, batal = Process; bayar = Payment.</summary>
    [Area("Inventaris")]
    public class PeminjamanController : BaseController
    {
        private const string KEY_EDIT = "Peminjaman_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        [AccessAs(Acc.Index)] public IActionResult List() => Bagian(() => new VM.List());
        [AccessAs(Acc.Create)] public IActionResult Add() => Bagian(() => new VM.Add());

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new VM.Edit(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanBaru([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Simpan(m, u), "Peminjaman dicatat (Menunggu).");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult Update([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Update(EditKey(KEY_EDIT), m, u), "Peminjaman diubah.", KEY_EDIT);
        [HttpPost] public IActionResult Delete([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Peminjaman dihapus.");

        [HttpPost, AccessAs(Acc.Approve)] public IActionResult Setujui([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Approve(u).Ok(); }, "Peminjaman disetujui.");
        [HttpPost, AccessAs(Acc.Approve)] public IActionResult Tolak([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.Load(i.Key).Reject(i.Keterangan, u).Ok(); }, "Peminjaman ditolak.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Batal([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.Load(i.Key).Cancel(i.Keterangan, u).Ok(); }, "Peminjaman dibatalkan.");

        // ---------- Detail
        [AccessAs(Acc.Index)] public IActionResult Detail(string encKey) => Bagian(() => new VM.Detail(encKey));
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult TambahItem([FromBody] VM.ItemInput m) => Aksi(u => VM.Method.TambahItem(m, u), "Item ditambahkan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult HapusItem([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusItem(i.Key, u), "Item dihapus.");

        // ---------- Checkout & pengembalian
        [AccessAs(Acc.Process)] public IActionResult Checkout(string encKey) => Bagian(() => new VM.Checkout(encKey));
        [HttpPost, AccessAs(Acc.Process)] public IActionResult SimpanCheckout([FromBody] VM.CheckoutInput m) => Aksi(u => VM.Method.SimpanCheckout(m, u), "Barang diserahkan; status Dipinjam.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult KembaliJumlah([FromBody] VM.KembaliJumlahInput m) => Aksi(u => VM.Method.KembaliJumlah(m, u), "Pengembalian dicatat.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult KembaliUnit([FromBody] VM.KembaliUnitInput m) => Aksi(u => VM.Method.KembaliUnit(m, u), "Unit dikembalikan.");

        // ---------- Pembayaran
        [AccessAs(Acc.Payment)] public IActionResult Bayar(string encKey) => FormBayar(() => VM.Method.FormBayar(encKey));
        [HttpPost, AccessAs(Acc.Payment)] public IActionResult SimpanBayar([FromBody] GwaVM.BayarInput m) => Aksi(u => VM.Method.Bayar(m, u), "Pembayaran dicatat.");
        [HttpPost, AccessAs(Acc.Payment)] public IActionResult BatalBayar([FromBody] VM.BatalBayarInput m) => Aksi(u => VM.Method.BatalBayar(m, u), "Pembayaran dibatalkan.");
        [HttpPost, AccessAs(Acc.Payment)] public IActionResult BebasDenda([FromBody] VM.BebasDendaInput m) => Aksi(u => VM.Method.BebasDenda(m, u), "Denda dibebaskan.");

        // ---------- Cetak (DevExpress Reports; viewer di iframe, ?pdf=1 = unduh PDF)
        private string WebRoot => HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath;

        [AccessAs(Acc.Index)] public IActionResult Invoice(string key) => Laporan(() => PeminjamanCetak.Invoice(key, WebRoot));
        /// <summary>trx = id transaksi pembayaran; kosong = kuitansi gabungan semua pembayaran.</summary>
        [AccessAs(Acc.Index)] public IActionResult Kuitansi(string key, int? trx) => Laporan(() => PeminjamanCetak.Kuitansi(key, trx, WebRoot));
    }
}
