using DevExtreme.AspNet.Mvc;
using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Akses;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Akses
{
    /// <summary>
    /// Akses › Menu: tab Controller &amp; Aksi, Header Menu, Sub Menu. Logika di Models/Akses/MenuVM.Method.
    /// Setiap perubahan membersihkan cache akses web supaya sidebar dan cek hak akses langsung memakai data baru.
    /// </summary>
    public class MenuController : BaseController
    {
        const string KEY_EDIT_CONTROLLER = "Menu_EditController_EncryptKey";
        const string KEY_EDIT_HEADER = "Menu_EditHeader_EncryptKey";
        const string KEY_EDIT_SUB = "Menu_EditSub_EncryptKey";
        private readonly AccessService _access;

        public MenuController(AccessService access)
        {
            _access = access;
        }

        public IActionResult Index() => HalamanIndex();

        // Aksi lalu bersihkan cache akses.
        private IActionResult AksiMenu(Action<string> aksi, string pesan, string? keep = null)
            => Aksi(user => { aksi(user); _access.ClearAll(); }, pesan, keep);

        // ================================================================ Controller
        [HttpGet, AccessAs(Acc.Index)]
        public IActionResult GetController(DataSourceLoadOptions loadOptions) => Grid(MenuVM.Method.GetControllers, loadOptions);

        [AccessAs(Acc.Create)]
        public IActionResult AddController() => Modal(() => new MenuVM.AddController(), "FormController");

        [AccessAs(Acc.Edit)]
        public IActionResult EditController(string encKey)
        {
            TempData[KEY_EDIT_CONTROLLER] = encKey;
            return Modal(() => new MenuVM.EditController(encKey), "FormController");
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanController([FromBody] MenuVM.FormController input)
            => AksiMenu(user => MenuVM.Method.SimpanController(input, user), "Controller tersimpan");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateController([FromBody] MenuVM.FormController input)
            => AksiMenu(user => MenuVM.Method.UpdateController(TempData[KEY_EDIT_CONTROLLER]?.ToString(), input, user), "Controller berhasil diubah", KEY_EDIT_CONTROLLER);

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult DeleteController([FromBody] GwaVM.KeyInput input)
            => AksiMenu(user => MenuVM.Method.HapusController(input.Key, user), "Controller dinonaktifkan");

        [AccessAs(Acc.Index)]
        public IActionResult DaftarAksi(string encKey) => Modal(() => new MenuVM.Aksi(encKey), "Aksi");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult SimpanAksi([FromBody] MenuVM.AksiInput input)
            => AksiMenu(user => MenuVM.Method.SimpanAksi(input, user), "Aksi tersimpan");

        // ================================================================ Header menu
        [HttpGet, AccessAs(Acc.Index)]
        public IActionResult GetHeader(DataSourceLoadOptions loadOptions) => Grid(MenuVM.Method.GetHeaders, loadOptions);

        [AccessAs(Acc.Create)]
        public IActionResult AddHeader() => Modal(() => new MenuVM.AddHeader(), "FormHeader");

        [AccessAs(Acc.Edit)]
        public IActionResult EditHeader(string encKey)
        {
            TempData[KEY_EDIT_HEADER] = encKey;
            return Modal(() => new MenuVM.EditHeader(encKey), "FormHeader");
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanHeader([FromBody] MenuVM.FormHeader input)
            => AksiMenu(user => MenuVM.Method.SimpanHeader(input, user), "Header menu tersimpan");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateHeader([FromBody] MenuVM.FormHeader input)
            => AksiMenu(user => MenuVM.Method.UpdateHeader(TempData[KEY_EDIT_HEADER]?.ToString(), input, user), "Header menu berhasil diubah", KEY_EDIT_HEADER);

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult DeleteHeader([FromBody] GwaVM.KeyInput input)
            => AksiMenu(user => MenuVM.Method.HapusHeader(input.Key, user), "Header menu dinonaktifkan");

        [AccessAs(Acc.Index)]
        public IActionResult Detail(string encKey) => Modal(() => new MenuVM.Detail(encKey));

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult SimpanDetail([FromBody] MenuVM.DetailInput input)
            => AksiMenu(user => MenuVM.Method.SimpanDetail(input, user), "Sub menu grup tersimpan");

        // ================================================================ Sub menu
        [HttpGet, AccessAs(Acc.Index)]
        public IActionResult GetSub(DataSourceLoadOptions loadOptions) => Grid(MenuVM.Method.GetSubs, loadOptions);

        [AccessAs(Acc.Create)]
        public IActionResult AddSub() => Modal(() => new MenuVM.AddSub(), "FormSub");

        [AccessAs(Acc.Edit)]
        public IActionResult EditSub(string encKey)
        {
            TempData[KEY_EDIT_SUB] = encKey;
            return Modal(() => new MenuVM.EditSub(encKey), "FormSub");
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanSub([FromBody] MenuVM.FormSub input)
            => AksiMenu(user => MenuVM.Method.SimpanSub(input, user), "Sub menu tersimpan");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateSub([FromBody] MenuVM.FormSub input)
            => AksiMenu(user => MenuVM.Method.UpdateSub(TempData[KEY_EDIT_SUB]?.ToString(), input, user), "Sub menu berhasil diubah", KEY_EDIT_SUB);

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult DeleteSub([FromBody] GwaVM.KeyInput input)
            => AksiMenu(user => MenuVM.Method.HapusSub(input.Key, user), "Sub menu dinonaktifkan");
    }
}
