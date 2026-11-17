using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Kegiatan.PresensiVM;

namespace GitaWidyaAgniWeb.Controllers.Kegiatan
{
    /// <summary>Kegiatan › Presensi (/Kegiatan/Presensi): sesi (buka QR / tutup / batal / manual), jadwal rutin, notulen.</summary>
    [Area("Kegiatan")]
    public class PresensiController : BaseController
    {
        private const string KEY = "Presensi_KEY_EDIT_";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        private IActionResult EditForm(string tab, string encKey, Func<object> buat)
        {
            TempData[KEY + tab] = encKey;
            return Bagian(buat, "Edit" + tab);
        }

        // ---------- Sesi
        [AccessAs(Acc.Index)] public IActionResult ListSesi() => Bagian(() => new VM.ListSesi());
        [AccessAs(Acc.Create)] public IActionResult AddSesi() => Bagian(() => new VM.AddSesi());
        [AccessAs(Acc.Edit)] public IActionResult EditSesi(string encKey) => EditForm("Sesi", encKey, () => new VM.EditSesi(encKey));
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanSesi([FromBody] VM.NewSesi m) => Aksi(u => VM.Method.SimpanSesi(m, u), "Sesi presensi dibuat.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateSesi([FromBody] VM.NewSesi m) => Aksi(u => VM.Method.UpdateSesi(EditKey(KEY + "Sesi"), m, u), "Sesi presensi diubah.", KEY + "Sesi");

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult BukaSesi([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadSesi(i.Key).Open(u).Ok(); }, "Sesi dibuka. QR baru dibuat.");
        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult TutupSesi([FromBody] GwaVM.KeyInput i) => Aksi(u => (object)VM.Method.LoadSesi(i.Key).Close(u).Ok(), "Sesi ditutup. Anggota yang tidak presensi dicatat Alpa.");
        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult BatalSesi([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.LoadSesi(i.Key).Cancel(i.Keterangan, u).Ok(); }, "Sesi dibatalkan.");

        [AccessAs(Acc.Index)] public IActionResult Qr(string encKey) => Bagian(() => new VM.Qr(encKey));
        [AccessAs(Acc.Index)] public IActionResult Rekap(string encKey) => Bagian(() => new VM.Rekap(encKey));
        [AccessAs(Acc.Index)] public IActionResult RekapList(string encKey) => Bagian(() => new VM.RekapList(encKey));
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Manual([FromBody] VM.ManualInput m) => Aksi(u => VM.Method.Manual(m, u), "Presensi dicatat.");

        [AccessAs(Acc.Index)]
        public IActionResult Foto(string key, bool keluar = false)
        {
            try
            {
                var foto = VM.Method.Foto(key, keluar);
                return Berkas(foto?.Data, null, foto?.ContentType, true);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        // ---------- Jadwal
        [AccessAs(Acc.Index)] public IActionResult ListJadwal() => Bagian(() => new VM.ListJadwal());
        [AccessAs(Acc.Create)] public IActionResult AddJadwal() => Bagian(() => new VM.AddJadwal());
        [AccessAs(Acc.Edit)] public IActionResult EditJadwal(string encKey) => EditForm("Jadwal", encKey, () => new VM.EditJadwal(encKey));
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanJadwal([FromBody] VM.NewJadwal m) => Aksi(u => VM.Method.SimpanJadwal(m, u), "Jadwal disimpan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateJadwal([FromBody] VM.NewJadwal m) => Aksi(u => VM.Method.UpdateJadwal(EditKey(KEY + "Jadwal"), m, u), "Jadwal diubah.", KEY + "Jadwal");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusJadwal([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusJadwal(i.Key, u), "Jadwal dinonaktifkan.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Generate([FromBody] VM.GenerateInput i) => Aksi(u => (object)VM.Method.Generate(i, u), "Sesi dibuat dari jadwal.");

        // ---------- Notulen (FormData: lampiran)
        [AccessAs(Acc.Index)] public IActionResult ListNotulen() => Bagian(() => new VM.ListNotulen());
        [AccessAs(Acc.Create)] public IActionResult AddNotulen() => Bagian(() => new VM.AddNotulen());
        [AccessAs(Acc.Edit)] public IActionResult EditNotulen(string encKey) => EditForm("Notulen", encKey, () => new VM.EditNotulen(encKey));

        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanNotulen([FromForm] VM.NewNotulen m, IFormFile? berkas)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(u => VM.Method.SimpanNotulen(m, bytes, berkas, u), "Notulen disimpan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> UpdateNotulen([FromForm] VM.NewNotulen m, IFormFile? berkas, bool hapusBerkas = false)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(u => VM.Method.UpdateNotulen(EditKey(KEY + "Notulen"), m, bytes, berkas, hapusBerkas, u), "Notulen diubah.", KEY + "Notulen");
        }

        [HttpPost, AccessAs(Acc.Process)] public IActionResult FinalNotulen([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadNotulen(i.Key).Finalize(u).Ok(); }, "Notulen difinalkan.");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusNotulen([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadNotulen(i.Key).Delete(u).Ok(); }, "Notulen dihapus.");

        [AccessAs(Acc.Index)]
        public IActionResult BerkasNotulen(string key)
        {
            try
            {
                var file = VM.Method.LoadNotulen(key).GetFile();
                return Berkas(file?.Data, file?.FileName, file?.ContentType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
