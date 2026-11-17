using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Kegiatan.GaleriVM;

namespace GitaWidyaAgniWeb.Controllers.Kegiatan
{
    /// <summary>Kegiatan › Galeri (/Kegiatan/Galeri): galeri foto kegiatan, terbit / arsip, sampul.</summary>
    [Area("Kegiatan")]
    public class GaleriController : BaseController
    {
        private const string KEY_EDIT = "Galeri_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());

        [AccessAs(Acc.Index)] public IActionResult List() => Bagian(() => new VM.List());
        [AccessAs(Acc.Create)] public IActionResult Add() => Bagian(() => new VM.Add());

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new VM.Edit(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanBaru([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Simpan(m, u), "Galeri dibuat. Tambahkan foto lalu terbitkan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult Update([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Update(EditKey(KEY_EDIT), m, u), "Galeri diubah.", KEY_EDIT);
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Terbitkan([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Publish(u).Ok(); }, "Galeri diterbitkan.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Arsipkan([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Archive(u).Ok(); }, "Galeri diarsipkan.");
        [HttpPost] public IActionResult Delete([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Galeri dihapus.");

        // ---------- Foto
        [AccessAs(Acc.Index)] public IActionResult Foto(string encKey) => Bagian(() => new VM.Foto(encKey));

        [AccessAs(Acc.Index), ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
        public IActionResult Gambar(string key, int id)
        {
            try
            {
                var foto = VM.Method.GetFoto(key, id);
                return File(foto.Photo, foto.ContentType);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        // FormData: banyak foto sekaligus (name="foto").
        [HttpPost, AccessAs(Acc.Edit)]
        public async Task<IActionResult> TambahFoto(string key, List<IFormFile> foto)
        {
            var files = new List<(byte[] Data, string Caption)>();
            foreach (var f in foto ?? new List<IFormFile>())
            {
                var bytes = await BacaBerkasAsync(f);
                if (bytes != null)
                {
                    files.Add((bytes, Path.GetFileNameWithoutExtension(f.FileName)));
                }
            }
            return Aksi(u => VM.Method.TambahFoto(key, files, u), files.Count + " foto ditambahkan.");
        }

        [HttpPost, AccessAs(Acc.Edit)] public IActionResult Sampul([FromBody] VM.FotoInput i) => Aksi(u => VM.Method.Sampul(i, u), "Sampul galeri diganti.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult Keterangan([FromBody] VM.FotoInput i) => Aksi(u => VM.Method.Keterangan(i, u), "Keterangan foto disimpan.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult HapusFoto([FromBody] VM.FotoInput i) => Aksi(u => VM.Method.HapusFoto(i, u), "Foto dihapus.");
    }
}
