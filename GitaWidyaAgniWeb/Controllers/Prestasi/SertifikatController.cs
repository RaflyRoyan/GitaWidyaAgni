using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Prestasi.SertifikatVM;

namespace GitaWidyaAgniWeb.Controllers.Prestasi
{
    /// <summary>Prestasi › Sertifikat (/Prestasi/Sertifikat). Cabut / pulihkan = Process. Verifikasi publik ada di LandingController.</summary>
    [Area("Prestasi")]
    public class SertifikatController : BaseController
    {
        private const string KEY_EDIT = "Sertifikat_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());
        [AccessAs(Acc.Index)] public IActionResult List() => Bagian(() => new VM.List());
        [AccessAs(Acc.Create)] public IActionResult Add() => Bagian(() => new VM.Add());
        [AccessAs(Acc.Edit)]
        public IActionResult Edit(string encKey) { TempData[KEY_EDIT] = encKey; return Bagian(() => new VM.Edit(encKey)); }
        [AccessAs(Acc.Index)] public IActionResult OpsiReferensi(int memberId) => Json(VM.Method.OpsiReferensi(memberId));

        [HttpPost, AccessAs(Acc.Create)]
        public async Task<IActionResult> SimpanBaru([FromForm] VM.NewRow m, IFormFile? berkas)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(u => VM.Method.Simpan(m, bytes, berkas?.FileName, u), "Sertifikat diterbitkan.");
        }

        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> Update([FromForm] VM.NewRow m, IFormFile? berkas, bool hapusBerkas = false)
        {
            var bytes = await BacaBerkasAsync(berkas);
            return Aksi(u => VM.Method.Update(EditKey(KEY_EDIT), m, bytes, berkas?.FileName, hapusBerkas, u), "Sertifikat diubah.", KEY_EDIT);
        }

        [HttpPost, AccessAs(Acc.Process)] public IActionResult Cabut([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Revoke(u).Ok(); }, "Sertifikat dicabut.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Pulihkan([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Restore(u).Ok(); }, "Sertifikat berlaku kembali.");

        [AccessAs(Acc.Index)]
        public IActionResult Unduh(string key)
        {
            try
            {
                var f = VM.Method.Load(key).GetFile();
                return Berkas(f?.Data, f?.FileName);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
