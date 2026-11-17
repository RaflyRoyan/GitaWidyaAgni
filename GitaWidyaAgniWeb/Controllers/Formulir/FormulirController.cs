using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GWALib.Lib.Transaksi.Formulir;
using Microsoft.AspNetCore.Mvc;
using VM = GitaWidyaAgniWeb.Models.Formulir.FormulirVM;

namespace GitaWidyaAgniWeb.Controllers.Formulir
{
    /// <summary>Formulir › Formulir (/Formulir/Formulir). Buka / tutup = Process; pertanyaan ikut Edit.</summary>
    [Area("Formulir")]
    public class FormulirController : BaseController
    {
        private const string KEY_EDIT = "Formulir_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new VM.Index());
        [AccessAs(Acc.Index)] public IActionResult List() => Bagian(() => new VM.List());
        [AccessAs(Acc.Create)] public IActionResult Add() => Bagian(() => new VM.Add());
        [AccessAs(Acc.Edit)]
        public IActionResult Edit(string encKey) { TempData[KEY_EDIT] = encKey; return Bagian(() => new VM.Edit(encKey)); }
        [HttpPost, AccessAs(Acc.Create)] public IActionResult SimpanBaru([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Simpan(m, u), "Formulir dibuat. Tambahkan pertanyaan lalu buka formulir.");
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult Update([FromBody] VM.NewRow m) => Aksi(u => VM.Method.Update(EditKey(KEY_EDIT), m, u), "Formulir diubah.", KEY_EDIT);
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult Delete([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Delete(u).Ok(); }, "Formulir dihapus.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Buka([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Open(u).Ok(); }, "Formulir dibuka.");
        [HttpPost, AccessAs(Acc.Process)] public IActionResult Tutup([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.Load(i.Key).Close(u).Ok(); }, "Formulir ditutup.");

        // ---------- Pertanyaan
        [AccessAs(Acc.Index)] public IActionResult Pertanyaan(string encKey) => Bagian(() => new VM.Pertanyaan(encKey));
        [HttpPost, AccessAs(Acc.Edit)] public IActionResult SimpanPertanyaan([FromBody] VM.PertanyaanInput m) => Aksi(u => VM.Method.SimpanPertanyaan(m, u), "Pertanyaan disimpan.");
        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult HapusPertanyaan([FromBody] GwaVM.KeyInput i) => Aksi(u => { LibExtensions.Load<FormQuestion>(i.Key, "Pertanyaan").Delete(u).Ok(); }, "Pertanyaan dihapus.");

        // ---------- Jawaban
        [AccessAs(Acc.Index)] public IActionResult Jawaban(string encKey) => Bagian(() => new VM.Jawaban(encKey));
        [AccessAs(Acc.Index)] public IActionResult JawabanDetail(string encKey) => Bagian(() => new VM.JawabanDetail(encKey));
        [HttpPost, AccessAs(Acc.Delete)] public IActionResult HapusJawaban([FromBody] GwaVM.KeyInput i) => Aksi(u => { VM.Method.LoadRespon(i.Key).Delete(u).Ok(); }, "Jawaban dihapus.");

        [AccessAs(Acc.Index)]
        public IActionResult BerkasJawaban(string key)
        {
            try
            {
                var f = VM.Method.LoadJawaban(key).GetFile();
                return Berkas(f?.Data, f?.FileName);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
