using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VM = GitaWidyaAgniWeb.Models.Inventaris.AsetVM;

namespace GitaWidyaAgniWeb.Controllers.Inventaris
{
    /// <summary>Inventaris › Aset (/Inventaris/Aset): aset + unit QR + foto + mutasi, kategori, perawatan, inspeksi, riwayat mutasi.</summary>
    [Area("Inventaris")]
    public class AsetController : BaseController
    {
        private const string KEY = "Aset_KEY_EDIT_";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        private IActionResult EditForm(string tab, string encKey, Func<object> buat)
        {
            TempData[KEY + tab] = encKey;
            return Bagian(buat, "Edit" + tab);
        }

        // ---------- Aset
        [AccessAs(Acc.Index)] public IActionResult ListAset() => Bagian(() => new VM.ListAset());
        [AccessAs(Acc.Create)] public IActionResult AddAset() => Bagian(() => new VM.AddAset());
        [AccessAs(Acc.Edit)] public IActionResult EditAset(string encKey) => EditForm("Aset", encKey, () => new VM.EditAset(encKey));
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanAset([FromBody] VM.NewAset m) => Aksi(u => VM.Method.SimpanAset(m, u), "Aset disimpan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateAset([FromBody] VM.NewAset m) => Aksi(u => VM.Method.UpdateAset(EditKey(KEY + "Aset"), m, u), "Aset diubah.", KEY + "Aset");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusAset([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Aset dihapuskan.");

        // ---------- Unit QR
        [AccessAs(Acc.Index)] public IActionResult Unit(string encKey) => Bagian(() => new VM.Unit(encKey));
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult TambahUnit([FromBody] VM.UnitInput m) => Aksi(u => VM.Method.TambahUnit(m, u), "Unit ditambahkan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult HapusUnit([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusUnit(i.Key, u), "Unit dihapus.");
        // Label QR (DevExpress Reports): CetakLabel menyiapkan data + menandai label tercetak dan mengembalikan id;
        // LabelQr?id= adalah halaman WebDocumentViewer (dimuat di iframe modal layar penuh). Data label disimpan di cache
        // 10 menit, terikat ke user yang menyiapkannya.
        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult CetakLabel([FromBody] VM.LabelInput i) => Aksi(u =>
        {
            var rows = VM.Method.SiapkanLabel(i, AsetLabel.BaseUrl(Request, HttpContext.RequestServices.GetRequiredService<IConfiguration>()), u);
            var id = Guid.NewGuid().ToString("N");
            Cache.Set(LabelCacheKey(id, u), rows, TimeSpan.FromMinutes(10));
            return id;
        }, "Label siap dicetak; unit ditandai label tercetak.");

        [AccessAs(Acc.Index)]
        public IActionResult LabelQr(string id)
        {
            if (!Cache.TryGetValue(LabelCacheKey(id, LibUser), out List<Reports.LabelQrUnitReport.Row>? rows) || rows == null)
            {
                return Content("Data label sudah kedaluwarsa. Tutup lalu cetak ulang dari daftar unit.", "text/plain; charset=utf-8");
            }
            var org = GWALib.Lib.Master.Organization.OrganizationProfile.GetData();
            var logo = GWALib.Lib.Master.Organization.OrganizationProfile.GetLogo()?.Data;
            logo ??= System.IO.File.ReadAllBytes(Path.Combine(HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath, "assets", "images", "gwa-logo-sm.png"));
            var report = new Reports.LabelQrUnitReport(rows, org?.Name ?? "Gita Widya Agni", logo, "Label QR " + rows[0].AssetName);
            return View("~/Views/Shared/_ReportViewer.cshtml", report);
        }

        // Laporan inventaris (DevExpress; ?pdf=1 / ?xlsx=1 = unduh): kondisi aset + riwayat perawatan dari..sampai.
        [AccessAs(Acc.Index)]
        public IActionResult CetakLaporan(DateTime? dari, DateTime? sampai, int? kategori) => Laporan(() => Models.Inventaris.InventarisCetak.Cetak(dari, sampai, kategori,
            HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath));

        private Microsoft.Extensions.Caching.Memory.IMemoryCache Cache => HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
        private static string LabelCacheKey(string? id, string user) => $"label-qr:{user}:{id}";

        // ---------- Foto
        [AccessAs(Acc.Index)] public IActionResult Foto(string encKey) => Bagian(() => new VM.Foto(encKey));

        [AccessAs(Acc.Index), ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
        public IActionResult Gambar(string key, int seq)
        {
            try
            {
                var foto = VM.Method.GetFoto(key, seq);
                return File(foto.Photo, foto.ContentType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> TambahFoto(string key, IFormFile? foto)
        {
            var bytes = await BacaBerkasAsync(foto);
            return Aksi(u => VM.Method.TambahFoto(key, bytes, u), "Foto ditambahkan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult HapusFoto([FromBody] VM.FotoInput i) => Aksi(u => { VM.Method.GetFoto(i.Key, i.Seq).Delete(u).Ok(); }, "Foto dihapus.");

        // ---------- Mutasi manual
        [AccessAs(Acc.Process)] public IActionResult Mutasi(string encKey) => Bagian(() => new VM.Mutasi(encKey));
        [HttpPost, AccessAs(Acc.Process)] public IActionResult SimpanMutasi([FromBody] VM.MutasiInput m) => Aksi(u => VM.Method.SimpanMutasi(m, u), "Mutasi dicatat.");
        [AccessAs(Acc.Index)] public IActionResult ListMutasi() => Bagian(() => new VM.ListMutasi());

        // ---------- Kategori
        [AccessAs(Acc.Index)] public IActionResult ListKategori() => Bagian(() => new VM.ListKategori());
        [AccessAs(Acc.Create)] public IActionResult AddKategori() => Bagian(() => new VM.AddKategori());
        [AccessAs(Acc.Edit)] public IActionResult EditKategori(string encKey) => EditForm("Kategori", encKey, () => new VM.EditKategori(encKey));
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanKategori([FromBody] VM.NewKategori m) => Aksi(u => VM.Method.SimpanKategori(m, u), "Kategori disimpan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateKategori([FromBody] VM.NewKategori m) => Aksi(u => VM.Method.UpdateKategori(EditKey(KEY + "Kategori"), m, u), "Kategori diubah.", KEY + "Kategori");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusKategori([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusKategori(i.Key, u), "Kategori dinonaktifkan.");

        // ---------- Perawatan
        [AccessAs(Acc.Index)] public IActionResult ListPerawatan() => Bagian(() => new VM.ListPerawatan());
        [AccessAs(Acc.Create)] public IActionResult AddPerawatan() => Bagian(() => new VM.AddPerawatan());
        [AccessAs(Acc.Edit)] public IActionResult EditPerawatan(string encKey) => EditForm("Perawatan", encKey, () => new VM.EditPerawatan(encKey));
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanPerawatan([FromBody] VM.NewPerawatan m) => Aksi(u => VM.Method.SimpanPerawatan(m, u), "Perawatan dijadwalkan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdatePerawatan([FromBody] VM.NewPerawatan m) => Aksi(u => VM.Method.UpdatePerawatan(EditKey(KEY + "Perawatan"), m, u), "Perawatan diubah.", KEY + "Perawatan");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult MulaiPerawatan([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadPerawatan(i.Key).Start(u).Ok(); }, "Perawatan dimulai; aset berstatus Perawatan.");
        [AccessAs(Acc.Process)] public IActionResult Selesai(string encKey) => Bagian(() => new VM.Selesai(encKey));
        [HttpPost, AccessAs(Acc.Process)] public IActionResult SimpanSelesai([FromBody] VM.SelesaiInput m) => Aksi(u => VM.Method.SelesaiPerawatan(m, u), "Perawatan selesai.");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusPerawatan([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadPerawatan(i.Key).Delete(u).Ok(); }, "Perawatan dihapus.");

        // ---------- Inspeksi (FormData: foto opsional)
        [AccessAs(Acc.Index)] public IActionResult ListInspeksi() => Bagian(() => new VM.ListInspeksi());
        [AccessAs(Acc.Create)] public IActionResult AddInspeksi() => Bagian(() => new VM.AddInspeksi());

        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanInspeksi([FromForm] VM.NewInspeksi m, IFormFile? foto)
        {
            var bytes = await BacaBerkasAsync(foto);
            return Aksi(u => VM.Method.SimpanInspeksi(m, bytes, u), "Inspeksi dicatat; kondisi aset diperbarui.");
        }

        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusInspeksi([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadInspeksi(i.Key).Delete(u).Ok(); }, "Inspeksi dihapus.");

        [AccessAs(Acc.Index)]
        public IActionResult FotoInspeksi(string key)
        {
            try
            {
                var data = VM.Method.LoadInspeksi(key).GetPhoto();
                return Berkas(data, "inspeksi.jpg", "image/jpeg", true);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
