using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Keuangan.IuranVM;

namespace GitaWidyaAgniWeb.Controllers.Keuangan
{
    /// <summary>Keuangan › Iuran (/Keuangan/Iuran): tagihan iuran anggota dan pengaturan iuran.</summary>
    [Area("Keuangan")]
    public class IuranController : BaseController
    {
        private const string KEY_TAGIHAN = "Tagihan_KEY_EDIT";
        private const string KEY_PENGATURAN = "PengaturanIuran_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        // ---------- Tagihan
        [AccessAs(Acc.Index)] public IActionResult ListTagihan() => Bagian(() => new VM.ListTagihan());
        [AccessAs(Acc.Create)] public IActionResult Generate() => Bagian(() => new VM.Generate());
        [HttpPost, AccessAs(Acc.Create)] public IActionResult Buat([FromBody] VM.GenerateInput m) => Aksi(u => (object)VM.Method.Buat(m, u), "Tagihan dibuat.");

        [AccessAs(Acc.Edit)]
        public IActionResult EditTagihan(string encKey) { TempData[KEY_TAGIHAN] = encKey; return Bagian(() => new VM.EditTagihan(encKey)); }
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateTagihan([FromBody] VM.TagihanInput m) => Aksi(u => VM.Method.UpdateTagihan(EditKey(KEY_TAGIHAN), m, u), "Tagihan diubah.", KEY_TAGIHAN);
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusTagihan([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadTagihan(i.Key).Delete(u).Ok(); }, "Tagihan dihapus.");

        [AccessAs(Acc.Payment)] public IActionResult Bayar(string encKey) => FormBayar(() => VM.Method.FormBayar(encKey));
        [HttpPost, AccessAs(Acc.Payment)] public IActionResult SimpanBayar([FromBody] GwaVM.BayarInput m) => Aksi(u => VM.Method.Bayar(m, u), "Pembayaran iuran dicatat.");
        [HttpPost, AccessAs(Acc.Payment)] public IActionResult BatalBayar([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.LoadTagihan(i.Key).CancelPayment(i.Keterangan, u).Ok(); }, "Pembayaran dibatalkan; transaksinya di-void.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Bebaskan([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadTagihan(i.Key).Waive(u).Ok(); }, "Tagihan dibebaskan.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult BatalBebas([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadTagihan(i.Key).Unwaive(u).Ok(); }, "Pembebasan dibatalkan.");

        // ---------- Pengaturan iuran
        [AccessAs(Acc.Index)] public IActionResult ListPengaturan() => Bagian(() => new VM.ListPengaturan());
        [AccessAs(Acc.Create)] public IActionResult AddPengaturan() => Bagian(() => new VM.AddPengaturan());
        [AccessAs(Acc.Edit)]
        public IActionResult EditPengaturan(string encKey) { TempData[KEY_PENGATURAN] = encKey; return Bagian(() => new VM.EditPengaturan(encKey)); }
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanPengaturan([FromBody] VM.NewPengaturan m) => Aksi(u => VM.Method.SimpanPengaturan(m, u), "Pengaturan iuran disimpan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdatePengaturan([FromBody] VM.NewPengaturan m) => Aksi(u => VM.Method.UpdatePengaturan(EditKey(KEY_PENGATURAN), m, u), "Pengaturan iuran diubah.", KEY_PENGATURAN);
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusPengaturan([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusPengaturan(i.Key, u), "Pengaturan iuran dinonaktifkan.");
    }
}
