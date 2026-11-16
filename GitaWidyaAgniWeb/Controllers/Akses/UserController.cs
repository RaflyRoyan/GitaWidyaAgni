using DevExtreme.AspNet.Mvc;
using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Akses;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Akses
{
    /// <summary>Akses › User. Logika di Models/Akses/UserVM.Method.</summary>
    public class UserController : BaseController
    {
        const string KEY_EDIT = "User_Edit_EncryptKey";
        private readonly AccessService _access;

        public UserController(AccessService access)
        {
            _access = access;
        }

        public IActionResult Index() => HalamanIndex();

        [HttpGet, AccessAs(Acc.Index)]
        public IActionResult GetData(DataSourceLoadOptions loadOptions) => Grid(UserVM.Method.GetDataList, loadOptions);

        [AccessAs(Acc.Create)]
        public IActionResult Add() => Modal(() => new UserVM.Add(), "Form");

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Modal(() => new UserVM.Edit(encKey), "Form");
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanBaru([FromBody] UserVM.Form input)
            => Aksi(user => UserVM.Method.Simpan(input, user), "User tersimpan");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult Update([FromBody] UserVM.Form input)
            => Aksi(user => _access.Clear(UserVM.Method.Update(TempData[KEY_EDIT]?.ToString(), input, user, UserId)), "User berhasil diubah", KEY_EDIT);

        [HttpPost]
        public IActionResult Delete([FromBody] GwaVM.KeyInput input)
            => Aksi(user => _access.Clear(UserVM.Method.Nonaktifkan(input.Key, user, UserId)), "User dinonaktifkan");

        // ---------- Reset password (aksi Process)
        [AccessAs(Acc.Process)]
        public IActionResult ResetPassword(string encKey)
            => Modal(() => new UserVM.ResetPassword { Key = encKey, Username = LibExtensions.Load<GWALib.Lib.Master.Akses.UserAccount>(encKey, "User").Username });

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult SimpanPassword([FromBody] UserVM.ResetPassword input)
            => Aksi(user => UserVM.Method.GantiPassword(input, user), "Password berhasil diganti");

        // ---------- Role user
        [AccessAs(Acc.Index)]
        public IActionResult Role(string encKey) => Modal(() => new UserVM.RoleIndex(encKey));

        [HttpGet, AccessAs(Acc.Index)]
        public IActionResult GetRoles(string key, DataSourceLoadOptions loadOptions) => Grid(() => UserVM.Method.GetRoles(key), loadOptions);

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult TambahRole([FromBody] UserVM.TambahRoleInput input)
            => Aksi(user => _access.Clear(UserVM.Method.TambahRole(input, user)), "Role ditambahkan");

        [HttpPost, AccessAs(Acc.Process)]
        public IActionResult HapusRole([FromBody] GwaVM.KeyInput input)
            => Aksi(user => _access.Clear(UserVM.Method.HapusRole(input.Key, user, UserId)), "Role dicabut");
    }
}
