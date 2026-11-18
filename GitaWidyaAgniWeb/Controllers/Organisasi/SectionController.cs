using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Organisasi;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Organisasi
{
    /// <summary>
    /// Organisasi › Section (/Organisasi/Section): section musik (Brass, Battery, Front Ensemble, Colour Guard) dan riwayat
    /// section anggota. Modal riwayat per anggota (RiwayatAnggota) juga dipakai halaman Data Anggota.
    /// </summary>
    [Area("Organisasi")]
    public class SectionController : BaseController
    {
        private const string KEY_EDIT = "Section_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new SectionVM.Index());

        [AccessAs(Acc.Index)]
        public IActionResult List() => Bagian(() => new SectionVM.List());

        [AccessAs(Acc.Create)]
        public IActionResult Add() => Bagian(() => new SectionVM.Add());

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new SectionVM.Edit(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanBaru([FromBody] SectionVM.NewRow model)
            => Aksi(user => SectionVM.Method.Simpan(model, user), "Section berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult Update([FromBody] SectionVM.NewRow model)
            => Aksi(user => SectionVM.Method.Update(EditKey(KEY_EDIT), model, user), "Section berhasil diubah.", KEY_EDIT);

        [HttpPost]
        public IActionResult Delete([FromBody] GwaVM.KeyInput input)
            => Aksi(user => SectionVM.Method.Nonaktifkan(input.Key, user), "Section dinonaktifkan.");

        // ---------- Anggota section
        [AccessAs(Acc.Index)]
        public IActionResult Anggota(string encKey) => Bagian(() => new SectionVM.AnggotaIndex(encKey));

        [AccessAs(Acc.Index)]
        public IActionResult AnggotaList(string encKey)
            => Bagian(() => SectionVM.RiwayatList.OfSection(LibExtensions.Load<GWALib.Lib.Master.Anggota.Section>(encKey, "Section").Id), "RiwayatList");

        // ---------- Riwayat section satu anggota (dibuka dari Data Anggota)
        [AccessAs(Acc.Index)]
        public IActionResult RiwayatAnggota(string memberKey) => Bagian(() => new SectionVM.RiwayatIndex(memberKey), "Riwayat");

        [AccessAs(Acc.Index)]
        public IActionResult RiwayatAnggotaList(string memberKey)
            => Bagian(() => SectionVM.RiwayatList.OfMember(LibExtensions.Load<GWALib.Lib.Master.Anggota.Member>(memberKey, "Anggota").Id), "RiwayatList");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult Pindah([FromBody] SectionVM.PindahInput model)
            => Aksi(user => SectionVM.Method.Pindah(model, user), "Section anggota dipindahkan.");

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult HapusRiwayat([FromBody] GwaVM.KeyInput input)
            => Aksi(user => SectionVM.Method.HapusRiwayat(input.Key, user), "Riwayat section dihapus.");
    }
}
