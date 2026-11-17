using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Keuangan.PengajuanDanaVM;

namespace GitaWidyaAgniWeb.Controllers.Keuangan
{
    /// <summary>
    /// Keuangan › Pengajuan Dana (/Keuangan/PengajuanDana). Ajukan = Submit; setujui / tolak / revisi = Approve
    /// (siapa yang boleh memutuskan tiap langkah ditentukan alur persetujuan); cair, realisasi, LPJ = Process.
    /// </summary>
    [Area("Keuangan")]
    public class PengajuanDanaController : BaseController
    {
        private const string KEY_EDIT = "PengajuanDana_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        [AccessAs(Acc.Index)] public IActionResult List() => Bagian(() => new VM.List());
        [AccessAs(Acc.Create)] public IActionResult Add() => Bagian(() => new VM.Add());

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new VM.Edit(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanBaru([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Simpan(m, u), "Pengajuan dana disimpan sebagai Draft.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult Update([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Update(EditKey(KEY_EDIT), m, u), "Pengajuan dana diubah.", KEY_EDIT);
        [HttpPost] public IActionResult Delete([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Pengajuan dana dihapus.");

        [HttpPost, AccessAs(Acc.Submit)] public IActionResult Ajukan([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Submit(u).Ok(); }, "Pengajuan dana diajukan.");
        [HttpPost, AccessAs(Acc.Approve)] public IActionResult Setujui([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.Load(i.Key).Approve(i.Keterangan, u).Ok(); }, "Keputusan Setuju dicatat.");
        [HttpPost, AccessAs(Acc.Approve)] public IActionResult Tolak([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.Load(i.Key).Reject(i.Keterangan, u).Ok(); }, "Pengajuan dana ditolak.");
        [HttpPost, AccessAs(Acc.Approve)] public IActionResult Revisi([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.Load(i.Key).RequestRevision(i.Keterangan, u).Ok(); }, "Pengajuan dikembalikan ke Draft untuk revisi.");

        [AccessAs(Acc.Process)] public IActionResult Cair(string encKey) => FormBayar(() => VM.Method.FormCair(encKey));
        [HttpPost, AccessAs(Acc.Process)] public IActionResult SimpanCair([FromBody] GwaVM.BayarInput m) => Aksi(u => VM.Method.Cair(m, u), "Dana dicairkan.");

        // ---------- Detail + realisasi
        [AccessAs(Acc.Index)] public IActionResult Detail(string encKey) => Bagian(() => new VM.Detail(encKey));

        [HttpPost, AccessAs(Acc.Process)]
        public async Task<IActionResult> TambahRealisasi([FromForm] VM.RealisasiInput m, IFormFile? bukti)
        {
            var bytes = await BacaBerkasAsync(bukti);
            return Aksi(u => VM.Method.TambahRealisasi(m, bytes, bukti, u), "Realisasi ditambahkan.");
        }

        [HttpPost, AccessAs(Acc.Process)] public IActionResult HapusRealisasi([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadRealisasi(i.Key).Delete(u).Ok(); }, "Realisasi dihapus.");

        [AccessAs(Acc.Index)]
        public IActionResult Bukti(string key)
        {
            try
            {
                var file = VM.Method.LoadRealisasi(key).GetReceipt();
                return Berkas(file?.Data, file?.FileName, file?.ContentType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        // ---------- LPJ
        [HttpPost, AccessAs(Acc.Process)] public IActionResult AjukanLpj([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).SubmitSettlement(u).Ok(); }, "LPJ diajukan.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult KembalikanLpj([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).ReturnSettlement(u).Ok(); }, "LPJ dikembalikan ke pengaju.");
        [AccessAs(Acc.Process)] public IActionResult Selesai(string encKey) => FormBayar(() => VM.Method.FormSelesai(encKey));
        [HttpPost, AccessAs(Acc.Process)] public IActionResult SimpanSelesai([FromBody] GwaVM.BayarInput m) => Aksi(u => VM.Method.Selesai(m, u), "LPJ selesai.");
    }
}
