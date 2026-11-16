using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Akses;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.Akses
{
    /// <summary>
    /// Akses › Menu (/Akses/Menu): tab Controller &amp; Aksi, Header Menu, Sub Menu. Logika di Models/Akses/MenuVM.Method.
    /// Setiap perubahan membersihkan cache akses web supaya sidebar dan cek hak akses langsung memakai data baru.
    /// </summary>
    [Area("Akses")]
    public class MenuController : BaseController
    {
        private const string KEY_EDIT_CONTROLLER = "Menu_KEY_EDIT_CONTROLLER";
        private const string KEY_EDIT_HEADER = "Menu_KEY_EDIT_HEADER";
        private const string KEY_EDIT_SUB = "Menu_KEY_EDIT_SUB";
        private readonly AccessService _access;

        public MenuController(AccessService access)
        {
            _access = access;
        }

        public IActionResult Index() => HalamanIndex(new MenuVM.Index());

        // Aksi lalu bersihkan cache akses.
        private IActionResult AksiMenu(Action<string> aksi, string pesan, string? keep = null)
            => Aksi(user => { aksi(user); _access.ClearAll(); }, pesan, keep);

        // ================================================================ Controller
        [AccessAs(Acc.Index)]
        public IActionResult ListController() => Bagian(() => new MenuVM.ListController());

        [AccessAs(Acc.Create)]
        public IActionResult AddController() => Bagian(() => new MenuVM.AddController());

        [AccessAs(Acc.Edit)]
        public IActionResult EditController(string encKey)
        {
            TempData[KEY_EDIT_CONTROLLER] = encKey;
            return Bagian(() => new MenuVM.EditController(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanController([FromBody] MenuVM.NewController model)
            => AksiMenu(user => MenuVM.Method.SimpanController(model, user), "Controller berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateController([FromBody] MenuVM.NewController model)
            => AksiMenu(user => MenuVM.Method.UpdateController(EditKey(KEY_EDIT_CONTROLLER), model, user), "Controller berhasil diubah.", KEY_EDIT_CONTROLLER);

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult DeleteController([FromBody] GwaVM.KeyInput input)
            => AksiMenu(user => MenuVM.Method.HapusController(input.Key, user), "Controller dinonaktifkan.");

        [AccessAs(Acc.Index)]
        public IActionResult DaftarAksi(string encKey) => Bagian(() => new MenuVM.Aksi(encKey), "Aksi");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult SimpanAksi([FromBody] MenuVM.AksiInput model)
            => AksiMenu(user => MenuVM.Method.SimpanAksi(model, user), "Aksi tersimpan.");

        // ================================================================ Header menu
        [AccessAs(Acc.Index)]
        public IActionResult ListHeader() => Bagian(() => new MenuVM.ListHeader());

        [AccessAs(Acc.Create)]
        public IActionResult AddHeader() => Bagian(() => new MenuVM.AddHeader());

        [AccessAs(Acc.Edit)]
        public IActionResult EditHeader(string encKey)
        {
            TempData[KEY_EDIT_HEADER] = encKey;
            return Bagian(() => new MenuVM.EditHeader(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanHeader([FromBody] MenuVM.NewHeader model)
            => AksiMenu(user => MenuVM.Method.SimpanHeader(model, user), "Header menu berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateHeader([FromBody] MenuVM.NewHeader model)
            => AksiMenu(user => MenuVM.Method.UpdateHeader(EditKey(KEY_EDIT_HEADER), model, user), "Header menu berhasil diubah.", KEY_EDIT_HEADER);

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult DeleteHeader([FromBody] GwaVM.KeyInput input)
            => AksiMenu(user => MenuVM.Method.HapusHeader(input.Key, user), "Header menu dinonaktifkan.");

        [AccessAs(Acc.Index)]
        public IActionResult Detail(string encKey) => Bagian(() => new MenuVM.Detail(encKey));

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult SimpanDetail([FromBody] MenuVM.DetailInput model)
            => AksiMenu(user => MenuVM.Method.SimpanDetail(model, user), "Sub menu grup tersimpan.");

        // ================================================================ Sub menu
        [AccessAs(Acc.Index)]
        public IActionResult ListSub() => Bagian(() => new MenuVM.ListSub());

        [AccessAs(Acc.Create)]
        public IActionResult AddSub() => Bagian(() => new MenuVM.AddSub());

        [AccessAs(Acc.Edit)]
        public IActionResult EditSub(string encKey)
        {
            TempData[KEY_EDIT_SUB] = encKey;
            return Bagian(() => new MenuVM.EditSub(encKey));
        }

        [HttpPost, AccessAs(Acc.Create)]
        public IActionResult SimpanSub([FromBody] MenuVM.NewSub model)
            => AksiMenu(user => MenuVM.Method.SimpanSub(model, user), "Sub menu berhasil disimpan.");

        [HttpPost, AccessAs(Acc.Edit)]
        public IActionResult UpdateSub([FromBody] MenuVM.NewSub model)
            => AksiMenu(user => MenuVM.Method.UpdateSub(EditKey(KEY_EDIT_SUB), model, user), "Sub menu berhasil diubah.", KEY_EDIT_SUB);

        [HttpPost, AccessAs(Acc.Delete)]
        public IActionResult DeleteSub([FromBody] GwaVM.KeyInput input)
            => AksiMenu(user => MenuVM.Method.HapusSub(input.Key, user), "Sub menu dinonaktifkan.");
    }
}
