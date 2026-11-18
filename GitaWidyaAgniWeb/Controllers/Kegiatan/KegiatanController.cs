using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Kegiatan.KegiatanVM;

namespace GitaWidyaAgniWeb.Controllers.Kegiatan
{
    /// <summary>Kegiatan › Daftar Kegiatan (/Kegiatan/Kegiatan): tab Kegiatan (+ peserta, kolaborator) dan tab Laporan.</summary>
    [Area("Kegiatan")]
    public class KegiatanController : BaseController
    {
        private const string KEY_KEGIATAN = "Kegiatan_KEY_EDIT";
        private const string KEY_LAPORAN = "Laporan_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        // ---------- Kegiatan
        [AccessAs(Acc.Index)] public IActionResult ListKegiatan() => Bagian(() => new VM.ListKegiatan());
        [AccessAs(Acc.Create)] public IActionResult AddKegiatan() => Bagian(() => new VM.AddKegiatan());
        [AccessAs(Acc.Edit)]
        public IActionResult EditKegiatan(string encKey) { TempData[KEY_KEGIATAN] = encKey; return Bagian(() => new VM.EditKegiatan(encKey)); }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanKegiatan([FromBody] VM.NewKegiatan model) => Aksi(user => VM.Method.SimpanKegiatan(model, user), "Kegiatan berhasil disimpan.");
        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateKegiatan([FromBody] VM.NewKegiatan model) => Aksi(user => VM.Method.UpdateKegiatan(EditKey(KEY_KEGIATAN), model, user), "Kegiatan berhasil diubah.", KEY_KEGIATAN);
        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult StatusKegiatan([FromBody] GwaVM.StatusInput input) => Aksi(user => VM.Method.StatusKegiatan(input, user), "Status kegiatan diperbarui.");
        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult HapusKegiatan([FromBody] GwaVM.KeyInput input) => Aksi(user => VM.Method.HapusKegiatan(input.Key, user), "Kegiatan dihapus.");

        // ---------- Peserta
        [AccessAs(Acc.Index)] public IActionResult Peserta(string encKey) => Bagian(() => new VM.Peserta(encKey));
        [AccessAs(Acc.Index)] public IActionResult PesertaList(string encKey) => Bagian(() => new VM.PesertaList(encKey));
        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult TambahPeserta([FromBody] VM.PesertaInput input) => Aksi(user => VM.Method.TambahPeserta(input, user), "Peserta ditambahkan.");
        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult KonfirmasiPeserta([FromBody] GwaVM.KeyInput input) => Aksi(user => VM.Method.KonfirmasiPeserta(input.Key, user), "Peserta dikonfirmasi.");
        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult BatalPeserta([FromBody] GwaVM.KeyInput input) => Aksi(user => VM.Method.BatalPeserta(input.Key, user), "Pendaftaran peserta dibatalkan.");

        // ---------- Kolaborator
        // ---------- Pengajuan tidak ikut (anggota audiens otomatis)
        [AccessAs(Acc.Index)] public IActionResult Izin(string encKey) => Bagian(() => new VM.Izin(encKey));
        [AccessAs(Acc.Index)] public IActionResult IzinList(string encKey) => Bagian(() => new VM.IzinList(encKey));
        [HttpPost, AccessAs(Acc.Approve)]
        public IActionResult SetujuiIzin([FromBody] VM.IzinInput input) => Aksi(user => { LibExtensions.Load<GWALib.Lib.Transaksi.Program.ActivityAbsenceRequest>(input.Key, "Pengajuan").Approve(input.Catatan, user).Ok(); }, "Pengajuan disetujui.");
        [HttpPost, AccessAs(Acc.Approve)]
        public IActionResult TolakIzin([FromBody] VM.IzinInput input) => Aksi(user => { LibExtensions.Load<GWALib.Lib.Transaksi.Program.ActivityAbsenceRequest>(input.Key, "Pengajuan").Reject(input.Catatan, user).Ok(); }, "Pengajuan ditolak.");

        [AccessAs(Acc.Index)] public IActionResult Kolaborator(string encKey) => Bagian(() => new VM.Kolaborator(encKey));
        [AccessAs(Acc.Index)] public IActionResult KolaboratorList(string encKey) => Bagian(() => new VM.KolaboratorList(encKey));
        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult TambahKolaborator([FromBody] VM.KolaboratorInput input) => Aksi(user => VM.Method.TambahKolaborator(input, user), "Mitra diundang.");
        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult ResponKolaborator([FromBody] GwaVM.StatusInput input) => Aksi(user => VM.Method.ResponKolaborator(input, user), "Respon mitra dicatat.");
        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult HapusKolaborator([FromBody] GwaVM.KeyInput input) => Aksi(user => VM.Method.HapusKolaborator(input.Key, user), "Kolaborator dihapus.");

        // ---------- Laporan
        [AccessAs(Acc.Index)] public IActionResult ListLaporan() => Bagian(() => new VM.ListLaporan());
        [AccessAs(Acc.Create)] public IActionResult AddLaporan() => Bagian(() => new VM.AddLaporan());
        [AccessAs(Acc.Edit)]
        public IActionResult EditLaporan(string encKey) { TempData[KEY_LAPORAN] = encKey; return Bagian(() => new VM.EditLaporan(encKey)); }

        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanLaporan([FromForm] VM.NewLaporan model, IFormFile? berkas)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(user => VM.Method.SimpanLaporan(model, bytes, berkas, user), "Laporan berhasil disimpan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> UpdateLaporan([FromForm] VM.NewLaporan model, IFormFile? berkas, bool hapusBerkas = false)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(user => VM.Method.UpdateLaporan(EditKey(KEY_LAPORAN), model, bytes, berkas, hapusBerkas, user), "Laporan berhasil diubah.", KEY_LAPORAN);
        }

        [AccessAs(Acc.Index)]
        public IActionResult BerkasLaporan(string key)
        {
            try
            {
                var file = VM.Method.LoadLaporan(key).GetFile();
                return Berkas(file?.Data, file?.FileName, file?.ContentType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpPost, AccessAs(Acc.Submit)]
        public IActionResult AjukanLaporan([FromBody] GwaVM.KeyInput input) => Aksi(user => { VM.Method.LoadLaporan(input.Key).Submit(user).Ok(); }, "Laporan diajukan.");
        [HttpPost, AccessAs(Acc.Approve)]
        public IActionResult SetujuiLaporan([FromBody] GwaVM.KeyInput input) => Aksi(user => { VM.Method.LoadLaporan(input.Key).Approve(user).Ok(); }, "Laporan disetujui.");
        [HttpPost, AccessAs(Acc.Approve)]
        public IActionResult RevisiLaporan([FromBody] GwaVM.KeyInput input) => Aksi(user => { VM.Method.LoadLaporan(input.Key).RequestRevision(user).Ok(); }, "Laporan dikembalikan untuk revisi.");
        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult HapusLaporan([FromBody] GwaVM.KeyInput input) => Aksi(user => { VM.Method.LoadLaporan(input.Key).Delete(user).Ok(); }, "Laporan dihapus.");
    }
}
