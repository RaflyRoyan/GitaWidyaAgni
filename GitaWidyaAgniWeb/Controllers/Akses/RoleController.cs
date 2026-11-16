using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Akses;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Akses
{
    /// <summary>Akses › Role (/Akses/Role): data role + hak akses (aksi &amp; menu). Setiap perubahan membersihkan cache akses web.</summary>
    [Area("Akses")]
    public class RoleController : BaseController
    {
        private const string KEY_EDIT = "Role_KEY_EDIT";
        private readonly AccessService _access;

        public RoleController(AccessService access)
        {
            _access = access;
        }

        public IActionResult Index() => HalamanIndex(new RoleVM.Index());

        // Grid Role (dimuat via AJAX, dimuat ulang setelah simpan).
        [AccessAs(Acc.Index)]
        public IActionResult List() => Bagian(() => new RoleVM.List());

        // Modal Add Role.
        [AccessAs(Acc.Create)]
        public IActionResult Add() => Bagian(() => new RoleVM.Add());

        // Modal Edit Role.
        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new RoleVM.Edit(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanBaru([FromBody] RoleVM.NewRow model)
            => Aksi(user => RoleVM.Method.Simpan(model, user), "Role berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult Update([FromBody] RoleVM.NewRow model)
            => Aksi(user => { RoleVM.Method.Update(EditKey(KEY_EDIT), model, user); _access.ClearAll(); }, "Role berhasil diubah.", KEY_EDIT);

        [HttpPost]
        public IActionResult Delete([FromBody] GwaVM.KeyInput input)
            => Aksi(user => { RoleVM.Method.Nonaktifkan(input.Key, user); _access.ClearAll(); }, "Role dinonaktifkan.");

        // ---------- Hak akses (modal _Akses: dua TreeList berseleksi rekursif)
        [AccessAs(Acc.Index)]
        public IActionResult Akses(string encKey) => Bagian(() => new RoleVM.Akses(encKey));

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult SimpanAkses([FromBody] RoleVM.AksesInput model)
            => Aksi(user => { RoleVM.Method.SimpanAkses(model, user); _access.ClearAll(); }, "Hak akses tersimpan.");
    }
}
