using DevExtreme.AspNet.Mvc;
using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Akses;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Akses
{
    /// <summary>Akses › Role: data role + hak akses (aksi & menu). Setiap perubahan membersihkan cache akses web.</summary>
    public class RoleController : BaseController
    {
        const string KEY_EDIT = "Role_Edit_EncryptKey";
        private readonly AccessService _access;

        public RoleController(AccessService access)
        {
            _access = access;
        }

        public IActionResult Index() => HalamanIndex();

        [HttpGet, AccessAs(Acc.Index)]
        public IActionResult GetData(DataSourceLoadOptions loadOptions) => Grid(RoleVM.Method.GetDataList, loadOptions);

        [AccessAs(Acc.Create)]
        public IActionResult Add() => Modal(() => new RoleVM.Add(), "Form");

        public IActionResult Edit(string encKey)
        {
            TempData[KEY_EDIT] = encKey;
            return Modal(() => new RoleVM.Edit(encKey), "Form");
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanBaru([FromBody] RoleVM.Form input)
            => Aksi(user => RoleVM.Method.Simpan(input, user), "Role tersimpan");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult Update([FromBody] RoleVM.Form input)
            => Aksi(user => { RoleVM.Method.Update(TempData[KEY_EDIT]?.ToString(), input, user); _access.ClearAll(); }, "Role berhasil diubah", KEY_EDIT);

        [HttpPost]
        public IActionResult Delete([FromBody] GwaVM.KeyInput input)
            => Aksi(user => { RoleVM.Method.Nonaktifkan(input.Key, user); _access.ClearAll(); }, "Role dinonaktifkan");

        // ---------- Hak akses
        [AccessAs(Acc.Index)]
        public IActionResult Akses(string encKey) => Modal(() => new RoleVM.Akses(encKey));

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult SimpanAkses([FromBody] RoleVM.AksesInput input)
            => Aksi(user => { RoleVM.Method.SimpanAkses(input, user); _access.ClearAll(); }, "Hak akses tersimpan");
    }
}
