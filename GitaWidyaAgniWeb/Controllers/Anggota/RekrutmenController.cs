using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Anggota;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Anggota
{
    /// <summary>Anggota › Rekrutmen (/Anggota/Rekrutmen): open recruitment, buka / tutup, review pendaftar.</summary>
    [Area("Anggota")]
    public class RekrutmenController : BaseController
    {
        private const string KEY_EDIT = "Rekrutmen_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new RekrutmenVM.Index());

        [AccessAs(Acc.Index)]
        public IActionResult List() => Bagian(() => new RekrutmenVM.List());

        [AccessAs(Acc.Create)]
        public IActionResult Add() => Bagian(() => new RekrutmenVM.Add());

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new RekrutmenVM.Edit(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanBaru([FromBody] RekrutmenVM.NewRow model)
            => Aksi(user => RekrutmenVM.Method.Simpan(model, user), "Rekrutmen berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult Update([FromBody] RekrutmenVM.NewRow model)
            => Aksi(user => RekrutmenVM.Method.Update(EditKey(KEY_EDIT), model, user), "Rekrutmen berhasil diubah.", KEY_EDIT);

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult Buka([FromBody] GwaVM.KeyInput input)
            => Aksi(user => RekrutmenVM.Method.Load(input.Key).Open(user).Ok(), "Rekrutmen dibuka.");

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult Tutup([FromBody] GwaVM.KeyInput input)
            => Aksi(user => RekrutmenVM.Method.Load(input.Key).Close(user).Ok(), "Rekrutmen ditutup.");

        [HttpPost]
        public IActionResult Delete([FromBody] GwaVM.KeyInput input)
            => Aksi(user => RekrutmenVM.Method.Load(input.Key).Delete(user).Ok(), "Rekrutmen dihapus.");

        // ---------- Pendaftar
        [AccessAs(Acc.Index)]
        public IActionResult Pendaftar(string encKey) => Bagian(() => new RekrutmenVM.Pendaftar(encKey));

        [AccessAs(Acc.Index)]
        public IActionResult PendaftarList(string encKey) => Bagian(() => new RekrutmenVM.PendaftarList(encKey));

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult Review([FromBody] RekrutmenVM.ReviewInput input)
            => Aksi(user => RekrutmenVM.Method.Review(input, user), "Status pendaftar diperbarui.");
    }
}
