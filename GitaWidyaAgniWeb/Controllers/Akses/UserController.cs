using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Akses;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Akses
{
    /// <summary>Akses › User (/Akses/User). Logika di Models/Akses/UserVM.Method.</summary>
    [Area("Akses")]
    public class UserController : BaseController
    {
        private const string KEY_EDIT = "User_KEY_EDIT";
        private readonly AccessService _access;

        public UserController(AccessService access)
        {
            _access = access;
        }

        public IActionResult Index() => HalamanIndex(new UserVM.Index());

        // Grid User (dimuat via AJAX, dimuat ulang setelah simpan).
        [AccessAs(Acc.Index)]
        public IActionResult List() => Bagian(() => new UserVM.List());

        // Modal Add User.
        [AccessAs(Acc.Create)]
        public IActionResult Add() => Bagian(() => new UserVM.Add());

        // Modal Edit User.
        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Bagian(() => new UserVM.Edit(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanBaru([FromBody] UserVM.NewRow model)
            => Aksi(user => UserVM.Method.Simpan(model, user), "User berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult Update([FromBody] UserVM.NewRow model)
            => Aksi(user => _access.Clear(UserVM.Method.Update(EditKey(KEY_EDIT), model, user, UserId)), "User berhasil diubah.", KEY_EDIT);

        [HttpPost]
        public IActionResult Delete([FromBody] GwaVM.KeyInput input)
            => Aksi(user => _access.Clear(UserVM.Method.Nonaktifkan(input.Key, user, UserId)), "User dinonaktifkan.");

        // ---------- Reset password (aksi Process)
        [AccessAs(Acc.Process)]
        public IActionResult ResetPassword(string encKey)
            => Bagian(() => new UserVM.ResetPassword { Key = encKey, Username = LibExtensions.Load<GWALib.Lib.Master.Akses.UserAccount>(encKey, "User").Username });

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult SimpanPassword([FromBody] UserVM.ResetPassword model)
            => Aksi(user => UserVM.Method.GantiPassword(model, user), "Password berhasil diganti.");

        // ---------- Role user (modal _Role + grid _RoleList)
        [AccessAs(Acc.Index)]
        public IActionResult Role(string encKey) => Bagian(() => new UserVM.RoleIndex(encKey));

        [AccessAs(Acc.Index)]
        public IActionResult RoleList(string encKey) => Bagian(() => new UserVM.RoleList(encKey));

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult TambahRole([FromBody] UserVM.TambahRoleInput model)
            => Aksi(user => _access.Clear(UserVM.Method.TambahRole(model, user)), "Role ditambahkan.");

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult HapusRole([FromBody] GwaVM.KeyInput input)
            => Aksi(user => _access.Clear(UserVM.Method.HapusRole(input.Key, user, UserId)), "Role dicabut.");
    }
}
