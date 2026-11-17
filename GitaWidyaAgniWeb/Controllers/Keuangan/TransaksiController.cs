using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Keuangan.TransaksiVM;

namespace GitaWidyaAgniWeb.Controllers.Keuangan
{
    /// <summary>Keuangan › Transaksi (/Keuangan/Transaksi): transaksi kas, saldo periode, akun kas, kategori.</summary>
    [Area("Keuangan")]
    public class TransaksiController : BaseController
    {
        private const string KEY = "Transaksi_KEY_EDIT_";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        private IActionResult EditForm(string tab, string encKey, Func<object> buat)
        {
            TempData[KEY + tab] = encKey;
            return Bagian(buat, "Edit" + tab);
        }

        // ---------- Transaksi
        [AccessAs(Acc.Index)] public IActionResult ListTransaksi() => Bagian(() => new VM.ListTransaksi());
        [AccessAs(Acc.Create)] public IActionResult AddTransaksi() => Bagian(() => new VM.AddTransaksi());
        [AccessAs(Acc.Edit)] public IActionResult EditTransaksi(string encKey) => EditForm("Transaksi", encKey, () => new VM.EditTransaksi(encKey));
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanTransaksi([FromBody] VM.NewTransaksi m) => Aksi(u => VM.Method.SimpanTransaksi(m, u), "Transaksi disimpan sebagai Draft.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateTransaksi([FromBody] VM.NewTransaksi m) => Aksi(u => VM.Method.UpdateTransaksi(EditKey(KEY + "Transaksi"), m, u), "Transaksi diubah.", KEY + "Transaksi");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult PostingTransaksi([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Post(u).Ok(); }, "Transaksi diposting.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult BatalTransaksi([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.Load(i.Key).Void(i.Keterangan, u).Ok(); }, "Transaksi dibatalkan.");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusTransaksi([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Transaksi dihapus.");

        // ---------- Lampiran
        [AccessAs(Acc.Index)] public IActionResult Lampiran(string encKey) => Bagian(() => new VM.Lampiran(encKey));

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

        // ---------- Saldo periode
        [AccessAs(Acc.Index)] public IActionResult ListSaldo() => Bagian(() => new VM.ListSaldo());
        [HttpPost, AccessAs(Acc.Process)] public IActionResult BukaSaldo([FromBody] VM.BukaSaldoInput i) => Aksi(u => (object)VM.Method.BukaSaldo(i, u), "Saldo periode dibuka.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult SegarkanSaldo([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadSaldo(i.Key).Refresh(u).Ok(); }, "Saldo dihitung ulang.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult TutupSaldo([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadSaldo(i.Key).Close(u).Ok(); }, "Saldo periode ditutup.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult BukaLagiSaldo([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadSaldo(i.Key).Reopen(u).Ok(); }, "Saldo periode dibuka lagi.");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusSaldo([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadSaldo(i.Key).Delete(u).Ok(); }, "Saldo periode dihapus.");

        // ---------- Akun
        [AccessAs(Acc.Index)] public IActionResult ListAkun() => Bagian(() => new VM.ListAkun());
        [AccessAs(Acc.Create)] public IActionResult AddAkun() => Bagian(() => new VM.AddAkun());
        [AccessAs(Acc.Edit)] public IActionResult EditAkun(string encKey) => EditForm("Akun", encKey, () => new VM.EditAkun(encKey));
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanAkun([FromBody] VM.NewAkun m) => Aksi(u => VM.Method.SimpanAkun(m, u), "Akun kas disimpan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateAkun([FromBody] VM.NewAkun m) => Aksi(u => VM.Method.UpdateAkun(EditKey(KEY + "Akun"), m, u), "Akun kas diubah.", KEY + "Akun");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusAkun([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusAkun(i.Key, u), "Akun kas dinonaktifkan.");

        // ---------- Kategori
        [AccessAs(Acc.Index)] public IActionResult ListKategori() => Bagian(() => new VM.ListKategori());
        [AccessAs(Acc.Create)] public IActionResult AddKategori() => Bagian(() => new VM.AddKategori());
        [AccessAs(Acc.Edit)] public IActionResult EditKategori(string encKey) => EditForm("Kategori", encKey, () => new VM.EditKategori(encKey));
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanKategori([FromBody] VM.NewKategori m) => Aksi(u => VM.Method.SimpanKategori(m, u), "Kategori disimpan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateKategori([FromBody] VM.NewKategori m) => Aksi(u => VM.Method.UpdateKategori(EditKey(KEY + "Kategori"), m, u), "Kategori diubah.", KEY + "Kategori");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusKategori([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusKategori(i.Key, u), "Kategori dinonaktifkan.");
    }
}
