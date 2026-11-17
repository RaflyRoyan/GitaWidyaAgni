using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Organisasi;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Organisasi
{
    /// <summary>Organisasi › Divisi (/Organisasi/Divisi): divisi / tim dan anggotanya.</summary>
    [Area("Organisasi")]
    public class DivisiController : BaseController
    {
        private const string KEY_EDIT = "Divisi_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new DivisiVM.Index());

        [AccessAs(Acc.Index)]
        public IActionResult List() => Bagian(() => new DivisiVM.List());

        [AccessAs(Acc.Create)]
        public IActionResult Add() => Bagian(() => new DivisiVM.Add());

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new DivisiVM.Edit(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanBaru([FromBody] DivisiVM.NewRow model)
            => Aksi(user => DivisiVM.Method.Simpan(model, user), "Divisi berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult Update([FromBody] DivisiVM.NewRow model)
            => Aksi(user => DivisiVM.Method.Update(EditKey(KEY_EDIT), model, user), "Divisi berhasil diubah.", KEY_EDIT);

        [HttpPost]
        public IActionResult Delete([FromBody] GwaVM.KeyInput input)
            => Aksi(user => DivisiVM.Method.Nonaktifkan(input.Key, user), "Divisi dinonaktifkan.");

        // ---------- Anggota divisi
        [AccessAs(Acc.Index)]
        public IActionResult Anggota(string encKey) => Bagian(() => new DivisiVM.AnggotaIndex(encKey));

        [AccessAs(Acc.Index)]
        public IActionResult AnggotaList(string encKey) => Bagian(() => new DivisiVM.AnggotaList(encKey));

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult TambahAnggota([FromBody] DivisiVM.AnggotaInput model)
            => Aksi(user => DivisiVM.Method.TambahAnggota(model, user), "Anggota ditambahkan ke divisi.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult KeluarkanAnggota([FromBody] GwaVM.KeyInput input)
            => Aksi(user => DivisiVM.Method.KeluarkanAnggota(input.Key, user), "Anggota dikeluarkan dari divisi.");
    }
}
