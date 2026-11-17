using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GWAShared.Enums;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.EVoting.EVotingVM;

namespace GitaWidyaAgniWeb.Controllers.EVoting
{
    /// <summary>
    /// E-Voting (/EVoting/EVoting). Pendaftaran / buka / tutup / finalisasi dan verifikasi kandidat = Process.
    /// Anggota memilih dari Area Anggota › Pemilihan.
    /// </summary>
    [Area("EVoting")]
    public class EVotingController : BaseController
    {
        private const string KEY_EDIT = "EVoting_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());
        [AccessAs(Acc.Index)] public IActionResult List() => Bagian(() => new VM.List());
        [AccessAs(Acc.Create)] public IActionResult Add() => Bagian(() => new VM.Add());
        [AccessAs(Acc.Edit)]
        public IActionResult Edit(string encKey) { TempData[KEY_EDIT] = encKey; return Bagian(() => new VM.Edit(encKey)); }
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanBaru([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Simpan(m, u), "Pemilihan dibuat.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult Update([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Update(EditKey(KEY_EDIT), m, u), "Pemilihan diubah.", KEY_EDIT);
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult Delete([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Pemilihan dihapus.");

        [HttpPost, AccessAs(Acc.Process)] public IActionResult Pendaftaran([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).StartRegistration(u).Ok(); }, "Pendaftaran kandidat dibuka.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Buka([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).OpenVoting(u).Ok(); }, "Pemungutan suara dibuka.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Tutup([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Close(u).Ok(); }, "Pemungutan suara ditutup.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Finalisasi([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Finalize(u).Ok(); }, "Hasil pemilihan difinalkan.");

        // ---------- Kelola: kandidat, pemilih, hasil, audit
        [AccessAs(Acc.Index)] public IActionResult Kelola(string encKey) => Bagian(() => new VM.Kelola(encKey));

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> SimpanKandidat([FromForm] VM.KandidatInput m, IFormFile? foto, bool hapusFoto = false)
        {
            var bytes = await BacaBerkasAsync(foto);
            return Aksi(u => VM.Method.SimpanKandidat(m, bytes, hapusFoto, u), "Kandidat disimpan.");
        }

        [HttpPost, AccessAs(Acc.Process)] public IActionResult VerifikasiKandidat([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadKandidat(i.Key).Verify(u).Ok(); }, "Kandidat terverifikasi.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult TolakKandidat([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadKandidat(i.Key).Reject(u).Ok(); }, "Kandidat ditolak.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult MundurKandidat([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadKandidat(i.Key).Withdraw(u).Ok(); }, "Kandidat mengundurkan diri.");
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusKandidat([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadKandidat(i.Key).Delete(u).Ok(); }, "Kandidat dihapus.");

        [AccessAs(Acc.Index), ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
        public IActionResult FotoKandidat(string key)
        {
            try
            {
                var f = VM.Method.LoadKandidat(key).GetPhoto();
                return Berkas(f?.Data, null, f?.ContentType, true);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult TambahPemilih([FromBody] VM.PemilihInput m)
        {
            var jumlah = 0;
            var res = Aksi(u => { jumlah = VM.Method.TambahPemilih(m, u); }, "Tidak ada pemilih baru.");
            return jumlah > 0 ? Json(new { Status = 1, Message = $"{jumlah} pemilih ditambahkan." }) : res;
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult HakPilih([FromBody] GwaVM.StatusInput i)
            => Aksi(u => { VM.Method.LoadPemilih(i.Key).SetEligibility((VoterEligibility)i.Status, u).Ok(); }, "Hak pilih diubah.");

        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusPemilih([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadPemilih(i.Key).Delete(u).Ok(); }, "Pemilih dihapus.");
    }
}
