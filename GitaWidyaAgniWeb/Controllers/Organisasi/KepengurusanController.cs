using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Organisasi;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Organisasi
{
    /// <summary>Organisasi › Kepengurusan (/Organisasi/Kepengurusan): penugasan pengurus per periode.</summary>
    [Area("Organisasi")]
    public class KepengurusanController : BaseController
    {
        private const string KEY_EDIT = "Kepengurusan_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new KepengurusanVM.Index());

        [AccessAs(Acc.Index)]
        public IActionResult List() => Bagian(() => new KepengurusanVM.List());

        [AccessAs(Acc.Create)]
        public IActionResult Add() => Bagian(() => new KepengurusanVM.Add());

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new KepengurusanVM.Edit(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanBaru([FromBody] KepengurusanVM.NewRow model)
            => Aksi(user => KepengurusanVM.Method.Simpan(model, user), "Penugasan berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult Update([FromBody] KepengurusanVM.NewRow model)
            => Aksi(user => KepengurusanVM.Method.Update(EditKey(KEY_EDIT), model, user), "Penugasan berhasil diubah.", KEY_EDIT);

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult Akhiri([FromBody] KepengurusanVM.AkhiriInput input)
            => Aksi(user => KepengurusanVM.Method.Akhiri(input, user), "Penugasan diakhiri.");

        [HttpPost]
        public IActionResult Delete([FromBody] GwaVM.KeyInput input)
            => Aksi(user => KepengurusanVM.Method.Hapus(input.Key, user), "Penugasan dihapus.");
    }
}
