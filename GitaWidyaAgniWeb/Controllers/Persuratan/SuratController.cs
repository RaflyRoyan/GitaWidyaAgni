using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Persuratan.SuratVM;

namespace GitaWidyaAgniWeb.Controllers.Persuratan
{
    /// <summary>
    /// Persuratan › Surat (/Persuratan/Surat). Ajukan = Submit; setujui / tolak = Approve (alur LETTER);
    /// kirim, arsip, disposisi = Process.
    /// </summary>
    [Area("Persuratan")]
    public class SuratController : BaseController
    {
        private const string KEY_SURAT = "Surat_KEY_EDIT";
        private const string KEY_TEMPLATE = "TemplateSurat_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        // ---------- Surat
        [AccessAs(Acc.Index)] public IActionResult ListSurat(int arah) => Bagian(() => new VM.ListSurat(arah));
        [AccessAs(Acc.Create)] public IActionResult AddSurat(int arah) => Bagian(() => new VM.AddSurat(arah));
        [AccessAs(Acc.Edit)]
        public IActionResult EditSurat(string encKey) { TempData[KEY_SURAT] = encKey; return Bagian(() => new VM.EditSurat(encKey)); }

        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanSurat([FromForm] VM.NewSurat m, IFormFile? berkas)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(u => VM.Method.SimpanSurat(m, bytes, berkas, u), "Surat disimpan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> UpdateSurat([FromForm] VM.NewSurat m, IFormFile? berkas, bool hapusBerkas = false)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(u => VM.Method.UpdateSurat(EditKey(KEY_SURAT), m, bytes, berkas, hapusBerkas, u), "Surat diubah.", KEY_SURAT);
        }

        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusSurat([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Surat dihapus.");
        [HttpPost, AccessAs(Acc.Submit)] public IActionResult Ajukan([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Submit(u).Ok(); }, "Surat diajukan.");
        [HttpPost, AccessAs(Acc.Approve)] public IActionResult Setujui([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.Load(i.Key).Approve(i.Keterangan, null, null, u).Ok(); }, "Keputusan Setuju dicatat.");
        [HttpPost, AccessAs(Acc.Approve)] public IActionResult Tolak([FromBody] GwaVM.AlasanInput i) => Aksi(u => { VM.Method.Load(i.Key).Reject(i.Keterangan, u).Ok(); }, "Surat ditolak.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Kirim([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).MarkSent(u).Ok(); }, "Surat ditandai terkirim.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Arsip([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Archive(u).Ok(); }, "Surat diarsipkan.");

        [AccessAs(Acc.Index)]
        public IActionResult BerkasSurat(string key)
        {
            try
            {
                var f = VM.Method.Load(key).GetFile();
                return Berkas(f?.Data, f?.FileName, f?.ContentType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        // ---------- Detail & disposisi
        [AccessAs(Acc.Index)] public IActionResult Detail(string encKey) => Bagian(() => new VM.Detail(encKey));
        [HttpPost, AccessAs(Acc.Process)] public IActionResult TambahDisposisi([FromBody] VM.DisposisiInput m) => Aksi(u => VM.Method.TambahDisposisi(m, u), "Disposisi dikirim.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult SelesaiDisposisi([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadDisposisi(i.Key).MarkDone(u).Ok(); }, "Disposisi selesai.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult BukaDisposisi([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadDisposisi(i.Key).Reopen(u).Ok(); }, "Disposisi dibuka lagi.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult HapusDisposisi([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadDisposisi(i.Key).Delete(u).Ok(); }, "Disposisi dihapus.");

        // ---------- Template
        [AccessAs(Acc.Index)] public IActionResult ListTemplate() => Bagian(() => new VM.ListTemplate());
        [AccessAs(Acc.Create)] public IActionResult AddTemplate() => Bagian(() => new VM.AddTemplate());
        [AccessAs(Acc.Edit)]
        public IActionResult EditTemplate(string encKey) { TempData[KEY_TEMPLATE] = encKey; return Bagian(() => new VM.EditTemplate(encKey)); }

        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanTemplate([FromForm] VM.NewTemplate m, IFormFile? berkas)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(u => VM.Method.SimpanTemplate(m, bytes, berkas?.FileName, u), "Template disimpan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> UpdateTemplate([FromForm] VM.NewTemplate m, IFormFile? berkas, bool hapusBerkas = false)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(u => VM.Method.UpdateTemplate(EditKey(KEY_TEMPLATE), m, bytes, berkas?.FileName, hapusBerkas, u), "Template diubah.", KEY_TEMPLATE);
        }

        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusTemplate([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadTemplate(i.Key).Delete(u).Ok(); }, "Template dinonaktifkan.");

        [AccessAs(Acc.Index)]
        public IActionResult BerkasTemplate(string key)
        {
            try
            {
                var f = VM.Method.LoadTemplate(key).GetFile();
                return Berkas(f?.Data, f?.FileName, f?.ContentType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
