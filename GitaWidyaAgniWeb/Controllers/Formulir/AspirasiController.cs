using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GWAShared.Enums;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Formulir.AspirasiVM;

namespace GitaWidyaAgniWeb.Controllers.Formulir
{
    /// <summary>Formulir › Aspirasi (/Formulir/Aspirasi). Tanggapi = Process. Anggota mengirim aspirasi dari Area Anggota.</summary>
    [Area("Formulir")]
    public class AspirasiController : BaseController
    {
        public IActionResult Index() => HalamanIndex(new VM.Index());
        [AccessAs(Acc.Index)] public IActionResult List() => Bagian(() => new VM.List());
        [AccessAs(Acc.Index)] public IActionResult Detail(string encKey) => Bagian(() => new VM.Detail(encKey));

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult Tanggapi([FromBody] GwaVM.StatusInput i)
            => Aksi(u => { VM.Method.Load(i.Key).Respond((AspirationStatus)i.Status, i.Keterangan, u).Ok(); }, "Tanggapan disimpan.");

        [HttpPost] public IActionResult Delete([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Aspirasi dihapus.");

        [AccessAs(Acc.Index)]
        public IActionResult Lampiran(string key)
        {
            try
            {
                var f = VM.Method.Load(key).GetAttachment();
                return Berkas(f?.Data, f?.FileName);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
