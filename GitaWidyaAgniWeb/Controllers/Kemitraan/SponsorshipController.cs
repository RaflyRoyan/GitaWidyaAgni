using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Kemitraan.SponsorshipVM;

namespace GitaWidyaAgniWeb.Controllers.Kemitraan
{
    /// <summary>Kemitraan › Sponsorship (/Kemitraan/Sponsorship): sponsorship, benefit, MoU, dana masuk (Payment), selesai / batal (Process), paket.</summary>
    [Area("Kemitraan")]
    public class SponsorshipController : BaseController
    {
        private const string KEY_SPONSOR = "Sponsor_KEY_EDIT";
        private const string KEY_PAKET = "Paket_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        // ---------- Sponsorship
        [AccessAs(Acc.Index)] public IActionResult ListSponsor() => Bagian(() => new VM.ListSponsor());
        [AccessAs(Acc.Create)] public IActionResult AddSponsor() => Bagian(() => new VM.AddSponsor());
        [AccessAs(Acc.Edit)]
        public IActionResult EditSponsor(string encKey) { TempData[KEY_SPONSOR] = encKey; return Bagian(() => new VM.EditSponsor(encKey)); }
        [AccessAs(Acc.Index)] public IActionResult OpsiKontak(int partyId) => Json(VM.Method.OpsiKontak(partyId));
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanSponsor([FromBody] VM.NewSponsor m) => Aksi(u => VM.Method.Simpan(m, u), "Sponsorship dicatat.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdateSponsor([FromBody] VM.NewSponsor m) => Aksi(u => VM.Method.Update(EditKey(KEY_SPONSOR), m, u), "Sponsorship diubah.", KEY_SPONSOR);
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusSponsor([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Sponsorship dihapus.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Selesai([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Finish(u).Ok(); }, "Sponsorship selesai.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Batal([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.Load(i.Key).Cancel(i.Keterangan, u).Ok(); }, "Sponsorship dibatalkan.");

        [AccessAs(Acc.Payment)] public IActionResult Terima(string encKey) => FormBayar(() => VM.Method.FormTerima(encKey));
        [HttpPost, AccessAs(Acc.Payment)] public IActionResult SimpanTerima([FromBody] GwaVM.BayarInput m) => Aksi(u => VM.Method.Terima(m, u), "Dana sponsor dicatat.");

        // ---------- Detail: benefit & MoU
        [AccessAs(Acc.Index)] public IActionResult Detail(string encKey) => Bagian(() => new VM.Detail(encKey));
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult TambahBenefit([FromBody] VM.BenefitInput m) => Aksi(u => VM.Method.TambahBenefit(m, u), "Benefit ditambahkan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult HapusBenefit([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadBenefit(i.Key).Delete(u).Ok(); }, "Benefit dihapus.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult BelumBenefit([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadBenefit(i.Key).MarkUndone(u).Ok(); }, "Benefit ditandai belum.");

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> SelesaiBenefit(string key, string? notes, IFormFile? bukti)
        {
            var bytes = await BacaBerkasAsync(bukti);
            return Aksi(u => VM.Method.Selesai(key, bytes, bukti, notes, u), "Benefit ditandai sudah diberikan.");
        }

        [AccessAs(Acc.Index)]
        public IActionResult BuktiBenefit(string key)
        {
            try
            {
                var f = VM.Method.LoadBenefit(key).GetProof();
                return Berkas(f?.Data, f?.FileName, f?.ContentType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> TambahMou([FromForm] VM.MouInput m, IFormFile? dokumen)
        {
            var bytes = await BacaBerkasAsync(dokumen);
            return Aksi(u => VM.Method.TambahMou(m, bytes, dokumen, u), "MoU ditambahkan.");
        }

        [HttpPost, AccessAs(Acc.Edit)] public IActionResult HapusMou([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadMou(i.Key).Delete(u).Ok(); }, "MoU dihapus.");

        [AccessAs(Acc.Index)]
        public IActionResult DokumenMou(string key)
        {
            try
            {
                var f = VM.Method.LoadMou(key).GetDocument();
                return Berkas(f?.Data, f?.FileName, f?.ContentType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        // ---------- Paket
        [AccessAs(Acc.Index)] public IActionResult ListPaket() => Bagian(() => new VM.ListPaket());
        [AccessAs(Acc.Create)] public IActionResult AddPaket() => Bagian(() => new VM.AddPaket());
        [AccessAs(Acc.Edit)]
        public IActionResult EditPaket(string encKey) { TempData[KEY_PAKET] = encKey; return Bagian(() => new VM.EditPaket(encKey)); }
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanPaket([FromBody] VM.NewPaket m) => Aksi(u => VM.Method.SimpanPaket(m, u), "Paket disimpan. Buka Ubah untuk menambah benefit.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult UpdatePaket([FromBody] VM.NewPaket m) => Aksi(u => VM.Method.UpdatePaket(EditKey(KEY_PAKET), m, u), "Paket diubah.", KEY_PAKET);
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusPaket([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusPaket(i.Key, u), "Paket dinonaktifkan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult TambahBenefitPaket([FromBody] VM.BenefitInput m) => Aksi(u => VM.Method.TambahBenefitPaket(m, u), "Benefit paket ditambahkan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult HapusBenefitPaket([FromBody] GwaVM.KeyInput i) => Aksi(u => VM.Method.HapusBenefitPaket(i.Key, u), "Benefit paket dihapus.");
    }
}
