using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Merchandise.MerchandiseVM;

namespace GitaWidyaAgniWeb.Controllers.Merchandise
{
    /// <summary>Merchandise (/Merchandise/Merchandise): produk + varian + stok, pesanan (siap / selesai / batal = Process), pembayaran (Payment).</summary>
    [Area("Merchandise")]
    public class MerchandiseController : BaseController
    {
        private const string KEY_PRODUK = "Produk_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        // ---------- Produk
        [AccessAs(Acc.Index)] public IActionResult ListProduk() => Bagian(() => new VM.ListProduk());
        [AccessAs(Acc.Create)] public IActionResult AddProduk() => Bagian(() => new VM.AddProduk());
        [AccessAs(Acc.Edit)]
        public IActionResult EditProduk(string encKey) { TempData[KEY_PRODUK] = encKey; return Bagian(() => new VM.EditProduk(encKey)); }

        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanProduk([FromForm] VM.NewProduk m, IFormFile? foto)
        {
            var bytes = await BacaBerkasAsync(foto);
            return Aksi(u => VM.Method.SimpanProduk(m, bytes, foto?.FileName, u), "Produk disimpan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> UpdateProduk([FromForm] VM.NewProduk m, IFormFile? foto, bool hapusFoto = false)
        {
            var bytes = await BacaBerkasAsync(foto);
            return Aksi(u => VM.Method.UpdateProduk(EditKey(KEY_PRODUK), m, bytes, foto?.FileName, hapusFoto, u), "Produk diubah.", KEY_PRODUK);
        }

        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusProduk([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadProduk(i.Key).Delete(u).Ok(); }, "Produk dinonaktifkan.");

        [AccessAs(Acc.Index), ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
        public IActionResult FotoProduk(string key)
        {
            try
            {
                var foto = VM.Method.LoadProduk(key).GetPhoto();
                return Berkas(foto?.Data, null, foto?.ContentType, true);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [AccessAs(Acc.Index)] public IActionResult Varian(string encKey) => Bagian(() => new VM.Varian(encKey));
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult TambahVarian([FromBody] VM.VarianInput m) => Aksi(u => VM.Method.TambahVarian(m, u), "Varian ditambahkan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult HapusVarian([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusVarian(i.Key, u), "Varian dinonaktifkan.");

        [AccessAs(Acc.Process)] public IActionResult Stok(string encKey) => Bagian(() => new VM.Stok(encKey));
        [HttpPost, AccessAs(Acc.Process)] public IActionResult SimpanStok([FromBody] VM.StokInput m) => Aksi(u => VM.Method.SimpanStok(m, u), "Mutasi stok dicatat.");
        [AccessAs(Acc.Index)] public IActionResult ListMutasi() => Bagian(() => new VM.ListMutasi());

        // ---------- Pesanan
        [AccessAs(Acc.Index)] public IActionResult ListPesanan() => Bagian(() => new VM.ListPesanan());
        [AccessAs(Acc.Create)] public IActionResult AddPesanan() => Bagian(() => new VM.AddPesanan());
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanPesanan([FromBody] VM.NewPesanan m) => Aksi(u => VM.Method.SimpanPesanan(m, u), "Pesanan dicatat.");
        [AccessAs(Acc.Index)] public IActionResult Detail(string encKey) => Bagian(() => new VM.Detail(encKey));
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Siap([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadPesanan(i.Key).SetReady(u).Ok(); }, "Pesanan siap diambil.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Selesai([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadPesanan(i.Key).Finish(u).Ok(); }, "Pesanan selesai.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Batal([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.LoadPesanan(i.Key).Cancel(i.Keterangan, u).Ok(); }, "Pesanan dibatalkan; stok dikembalikan.");

        [AccessAs(Acc.Payment)] public IActionResult Bayar(string encKey) => FormBayar(() => VM.Method.FormBayar(encKey));
        [HttpPost, AccessAs(Acc.Payment)] public IActionResult SimpanBayar([FromBody] GwaVM.BayarInput m) => Aksi(u => VM.Method.Bayar(m, u), "Pembayaran dicatat.");
        [HttpPost, AccessAs(Acc.Payment)] public IActionResult BatalBayar([FromBody] VM.BatalBayarInput m) => Aksi(u => VM.Method.BatalBayar(m, u), "Pembayaran dibatalkan.");
    }
}
