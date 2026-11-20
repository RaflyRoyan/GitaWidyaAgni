using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GWAShared.Enums;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Prestasi.PrestasiVM;

namespace GitaWidyaAgniWeb.Controllers.Prestasi
{
    /// <summary>
    /// Prestasi › Lomba &amp; Prestasi (/Prestasi/Prestasi). Status lomba, peserta mengikuti / mundur, dan generate poin = Process.
    /// </summary>
    [Area("Prestasi")]
    public class PrestasiController : BaseController
    {
        private const string KEY_LOMBA = "Lomba_KEY_EDIT";
        private const string KEY_PRESTASI = "Prestasi_KEY_EDIT";
        private const string KEY_MATERI = "Materi_KEY_EDIT";
        private const string KEY_ATURAN = "AturanPoin_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        // ---------- Lomba
        [AccessAs(Acc.Index)] public IActionResult ListLomba() => Bagian(() => new VM.ListLomba());
        [AccessAs(Acc.Create)] public IActionResult AddLomba() => Bagian(() => new VM.AddLomba());
        [AccessAs(Acc.Edit)]
        public IActionResult EditLomba(string encKey) { TempData[KEY_LOMBA] = encKey; return Bagian(() => new VM.EditLomba(encKey)); }

        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanLomba([FromForm] VM.NewLomba m, IFormFile? poster)
        {
            var bytes = await BacaBerkasAsync(poster);
            return Aksi(u => VM.Method.SimpanLomba(m, bytes, u), "Lomba disimpan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> UpdateLomba([FromForm] VM.NewLomba m, IFormFile? poster, bool hapusPoster = false)
        {
            var bytes = await BacaBerkasAsync(poster);
            return Aksi(u => VM.Method.UpdateLomba(EditKey(KEY_LOMBA), m, bytes, hapusPoster, u), "Lomba diubah.", KEY_LOMBA);
        }

        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusLomba([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadLomba(i.Key).Delete(u).Ok(); }, "Lomba dihapus.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult StatusLomba([FromBody] GwaVM.StatusInput i) => Aksi(u => { VM.Method.LoadLomba(i.Key).SetStatus((CompetitionStatus)i.Status, u).Ok(); }, "Status lomba diubah.");

        [AccessAs(Acc.Index), ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
        public IActionResult Poster(string key)
        {
            try
            {
                return Berkas(VM.Method.LoadLomba(key).GetPoster(), null, "image/jpeg", true);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        // ---------- Peserta lomba
        [AccessAs(Acc.Index)] public IActionResult Peserta(string encKey) => Bagian(() => new VM.Peserta(encKey));
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult DaftarPeserta([FromBody] VM.PesertaInput m) => Aksi(u => VM.Method.Daftar(m, u), "Peserta didaftarkan.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult IkutPeserta([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadPeserta(i.Key).SetParticipating(u).Ok(); }, "Peserta ditandai mengikuti.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult MundurPeserta([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadPeserta(i.Key).Withdraw(u).Ok(); }, "Peserta mengundurkan diri.");

        // ---------- Prestasi
        [AccessAs(Acc.Index)] public IActionResult ListPrestasi() => Bagian(() => new VM.ListPrestasi());
        [AccessAs(Acc.Create)] public IActionResult AddPrestasi() => Bagian(() => new VM.AddPrestasi());
        [AccessAs(Acc.Edit)]
        public IActionResult EditPrestasi(string encKey) { TempData[KEY_PRESTASI] = encKey; return Bagian(() => new VM.EditPrestasi(encKey)); }

        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanPrestasi([FromForm] VM.NewPrestasi m, IFormFile? sertifikat, IFormFile? foto)
        {
            var s = await BacaBerkasAsync(sertifikat);
            var f = await BacaBerkasAsync(foto);
            return Aksi(u => VM.Method.SimpanPrestasi(m, s, sertifikat?.FileName, f, u), "Prestasi dicatat.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> UpdatePrestasi([FromForm] VM.NewPrestasi m, IFormFile? sertifikat, IFormFile? foto, bool hapusSertifikat = false, bool hapusFoto = false)
        {
            var s = await BacaBerkasAsync(sertifikat);
            var f = await BacaBerkasAsync(foto);
            return Aksi(u => VM.Method.UpdatePrestasi(EditKey(KEY_PRESTASI), m, s, sertifikat?.FileName, hapusSertifikat, f, hapusFoto, u), "Prestasi diubah.", KEY_PRESTASI);
        }

        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusPrestasi([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadPrestasi(i.Key).Delete(u).Ok(); }, "Prestasi dihapus.");

        [AccessAs(Acc.Index)]
        public IActionResult SertifikatPrestasi(string key)
        {
            try
            {
                var f = VM.Method.LoadPrestasi(key).GetCertificate();
                return Berkas(f?.Data, f?.FileName);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [AccessAs(Acc.Index), ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
        public IActionResult FotoPrestasi(string key)
        {
            try
            {
                return Berkas(VM.Method.LoadPrestasi(key).GetPhoto(), null, "image/jpeg", true);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        // ---------- Materi
        [AccessAs(Acc.Index)] public IActionResult ListMateri() => Bagian(() => new VM.ListMateri());
        [AccessAs(Acc.Create)] public IActionResult AddMateri() => Bagian(() => new VM.AddMateri());
        [AccessAs(Acc.Edit)]
        public IActionResult EditMateri(string encKey) { TempData[KEY_MATERI] = encKey; return Bagian(() => new VM.EditMateri(encKey)); }

        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanMateri([FromForm] VM.NewMateri m, IFormFile? berkas)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(u => VM.Method.SimpanMateri(m, bytes, berkas, u), "Materi disimpan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> UpdateMateri([FromForm] VM.NewMateri m, IFormFile? berkas)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(u => VM.Method.UpdateMateri(EditKey(KEY_MATERI), m, bytes, berkas, u), "Materi diubah.", KEY_MATERI);
        }

        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusMateri([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadMateri(i.Key).Delete(u).Ok(); }, "Materi dihapus.");

        [AccessAs(Acc.Index)]
        public IActionResult BerkasMateri(string key)
        {
            try
            {
                var f = VM.Method.LoadMateri(key).GetFile();
                return Berkas(f?.Data, f?.FileName, f?.ContentType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        // ---------- Poin
        [AccessAs(Acc.Index)] public IActionResult ListPoin() => Bagian(() => new VM.ListPoin());
        [AccessAs(Acc.Create)] public IActionResult AddPoin() => Bagian(() => new VM.AddPoin());
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanPoin([FromBody] VM.NewPoin m) => Aksi(u => VM.Method.SimpanPoin(m, u), "Poin dicatat.");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusPoin([FromBody] GwaVM.KeyInput i) => Aksi(u => { LibExtensions.Load<GWALib.Lib.Transaksi.Prestasi.MemberPoint>(i.Key, "Poin").Delete(u).Ok(); }, "Poin dihapus.");
        [AccessAs(Acc.Index)] public IActionResult Peringkat(int? periodId) => Bagian(() => new VM.Peringkat(periodId));
        [AccessAs(Acc.Index)] public IActionResult OpsiKegiatan() => Json(Opsi.Kegiatan());
        [AccessAs(Acc.Index)] public IActionResult OpsiPeriode() => Json(Opsi.Periode());
        [AccessAs(Acc.Index)] public IActionResult OpsiSection() => Json(Opsi.Section());
        // Rekap poin per periode (DevExpress; ?pdf=1 / ?xlsx=1 = unduh). Periode kosong = periode Aktif.
        [AccessAs(Acc.Index)]
        public IActionResult CetakRekapPoin(int? periode, int? section) => Laporan(() => Models.Prestasi.PoinCetak.Cetak(periode, section,
            HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath));

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult GenerateKegiatan([FromBody] GwaVM.StatusInput i)
        {
            var jumlah = 0;
            var res = Aksi(u => { jumlah = VM.Method.GenerateKegiatan(i.Status, u); }, "Poin kegiatan dibuat.");
            return jumlah > 0 ? Json(new { Status = 1, Message = $"{jumlah} poin kegiatan dibuat / diperbarui." }) : res;
        }

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult GeneratePeriode([FromBody] GwaVM.StatusInput i)
        {
            var jumlah = 0;
            var res = Aksi(u => { jumlah = VM.Method.GeneratePeriode(i.Status, u); }, "Poin pengurus dibuat.");
            return jumlah > 0 ? Json(new { Status = 1, Message = $"{jumlah} poin pengurus dibuat / diperbarui." }) : res;
        }

        // ---------- Aturan
        [AccessAs(Acc.Index)] public IActionResult ListAturan() => Bagian(() => new VM.ListAturan());
        [AccessAs(Acc.Create)] public IActionResult AddAturan() => Bagian(() => new VM.AddAturan());
        [AccessAs(Acc.Edit)]
        public IActionResult EditAturan(string encKey) { TempData[KEY_ATURAN] = encKey; return Bagian(() => new VM.EditAturan(encKey)); }
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanAturan([FromBody] VM.NewAturan m) => Aksi(u => VM.Method.SimpanAturan(m, u), "Aturan poin disimpan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateAturan([FromBody] VM.NewAturan m) => Aksi(u => VM.Method.UpdateAturan(EditKey(KEY_ATURAN), m, u), "Aturan poin diubah.", KEY_ATURAN);
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusAturan([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadAturan(i.Key).Delete(u).Ok(); }, "Aturan poin dinonaktifkan.");
    }
}
