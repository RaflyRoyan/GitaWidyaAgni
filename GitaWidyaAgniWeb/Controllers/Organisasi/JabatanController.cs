using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Organisasi;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Organisasi
{
    /// <summary>Organisasi › Jabatan (/Organisasi/Jabatan): master jabatan pengurus.</summary>
    [Area("Organisasi")]
    public class JabatanController : BaseController
    {
        private const string KEY_EDIT = "Jabatan_KEY_EDIT";

        public IActionResult Index() => HalamanIndex(new JabatanVM.Index());

        [AccessAs(Acc.Index)]
        public IActionResult List() => Bagian(() => new JabatanVM.List());

        [AccessAs(Acc.Create)]
        public IActionResult Add() => Bagian(() => new JabatanVM.Add());

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new JabatanVM.Edit(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanBaru([FromBody] JabatanVM.NewRow model)
            => Aksi(user => JabatanVM.Method.Simpan(model, user), "Jabatan berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult Update([FromBody] JabatanVM.NewRow model)
            => Aksi(user => JabatanVM.Method.Update(EditKey(KEY_EDIT), model, user), "Jabatan berhasil diubah.", KEY_EDIT);

        [HttpPost]
        public IActionResult Delete([FromBody] GwaVM.KeyInput input)
            => Aksi(user => JabatanVM.Method.Nonaktifkan(input.Key, user), "Jabatan dinonaktifkan.");
    }
}
