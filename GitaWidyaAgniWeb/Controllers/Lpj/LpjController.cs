using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Lpj.LpjVM;

namespace GitaWidyaAgniWeb.Controllers.Lpj
{
    /// <summary>
    /// LPJ (/Lpj/Lpj): laporan pertanggungjawaban kegiatan dan kepengurusan. Dokumen dibuat DevExpress Reports dari data
    /// terkini (Pratinjau = Web Document Viewer di iframe); Finalisasi menyimpan PDF-nya (LPJ kepengurusan juga dipasang ke
    /// serah terima periode yang masih Draft / Ditolak).
    /// </summary>
    [Area("Lpj")]
    public class LpjController : BaseController
    {
        private const string KEY_EDIT = "Lpj_KEY_EDIT";

        private string WebRoot => HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath;

        public IActionResult Index() => HalamanIndex(new VM.Index());
        [AccessAs(Acc.Index)] public IActionResult List() => Bagian(() => new VM.List());
        [AccessAs(Acc.Create)] public IActionResult Add(int jenis) => Bagian(() => new VM.Add(jenis));
        [AccessAs(Acc.Edit)]
        public IActionResult Edit(string encKey) { TempData[KEY_EDIT] = encKey; return Bagian(() => new VM.Edit(encKey)); }

        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanBaru([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Simpan(m, u), "LPJ dibuat (Draft).");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult Update([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Update(EditKey(KEY_EDIT), m, u), "LPJ diubah.", KEY_EDIT);
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult Delete([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "LPJ dihapus.");

        // Halaman viewer mandiri (Layout = null) untuk iframe modal layar penuh.
        [AccessAs(Acc.Index)]
        public IActionResult Pratinjau(string key)
        {
            try
            {
                return View("~/Views/Shared/_ReportViewer.cshtml", VM.Method.BuildReport(VM.Method.Load(key), WebRoot));
            }
            catch (ArgumentException ex)
            {
                return Content(ex.Message, "text/plain; charset=utf-8");
            }
        }

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult Finalisasi([FromBody] GwaVM.KeyInput i) => Aksi(u =>
        {
            var lpj = VM.Method.Load(i.Key);
            var pdf = VM.Method.ToPdf(VM.Method.BuildReport(lpj, WebRoot));
            var data = lpj.Finalize(pdf, u).Ok();
            return new { data.HandoverAttached };
        }, "LPJ difinalkan; PDF tersimpan.");

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult BukaLagi([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Reopen(u).Ok(); }, "LPJ dibuka kembali (Draft).");

        [AccessAs(Acc.Index)]
        public IActionResult Unduh(string key)
        {
            try
            {
                var f = VM.Method.Load(key).GetDocument();
                return Berkas(f?.Data, f?.FileName, "application/pdf");
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
