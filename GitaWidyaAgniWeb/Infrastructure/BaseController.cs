using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using GWAShared.Enums;
using GWAShared.Results;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Infrastructure
{
    /// <summary>
    /// Dasar controller halaman login (pola SurabayaTaxWeb area Kantor):
    /// - Index → HalamanIndex(model); grid DevExtreme memuat data lewat GetData (Grid + DataSourceLoader).
    /// - Add / Edit (GET) → Modal(...) mengembalikan partial _Form.cshtml untuk modal; error → Json(Response) status Error.
    /// - SimpanBaru / Update / aksi lain (POST, body JSON) → Aksi(...) selalu Json(Response) { Status, Message }, tidak pernah redirect.
    ///   ArgumentException (dari GWALib atau VM) menjadi pesan untuk user.
    /// Logika bisnis ada di GWALib; pemanggilannya di Models/&lt;Area&gt;/&lt;Nama&gt;VM.Method.
    /// </summary>
    public abstract class BaseController : Controller
    {
        protected int UserId => User.GetUserId();
        protected string LibUser => User.GetLibUser();
        protected string ActionName => ControllerContext.RouteData.Values["action"]?.ToString() ?? string.Empty;

        protected ILogger Logger => HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());

        protected IActionResult HalamanIndex(object? model = null)
        {
            return model == null ? View("Index") : View("Index", model);
        }

        // Partial untuk modal: _<namaView ?? Action>.cshtml.
        protected IActionResult Modal(Func<object> buatModel, string? namaView = null)
        {
            try
            {
                return PartialView("_" + (namaView ?? ActionName), buatModel());
            }
            catch (ArgumentException e)
            {
                return Json(Gagal(e.InnerException?.Message ?? e.Message));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{Controller}.{Action}", GetType().Name, ActionName);
                return Json(Gagal("⚠ Server Error: Internal Server Error"));
            }
        }

        // Aksi simpan / ubah / proses. aksi(user) boleh mengembalikan data untuk dikirim balik (Data).
        // keepTempData: kunci TempData (encKey edit) yang dipertahankan bila gagal agar form bisa disimpan ulang.
        protected IActionResult Aksi(Func<string, object?> aksi, string pesanSukses, string? keepTempData = null)
        {
            if (!ModelState.IsValid)
            {
                if (keepTempData != null) TempData.Keep(keepTempData);
                var err = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).FirstOrDefault(x => !string.IsNullOrEmpty(x));
                return Json(Gagal("Format data tidak valid." + (err == null ? "" : " " + err)));
            }
            try
            {
                var data = aksi(LibUser);
                return Json(new Response<object> { Status = ResponseStatus.Success, Message = pesanSukses, Data = data });
            }
            catch (ArgumentException e)
            {
                if (keepTempData != null) TempData.Keep(keepTempData);
                return Json(Gagal(e.InnerException?.Message ?? e.Message));
            }
            catch (Exception ex)
            {
                if (keepTempData != null) TempData.Keep(keepTempData);
                Logger.LogError(ex, "{Controller}.{Action}", GetType().Name, ActionName);
                return Json(Gagal(ex.InnerException?.Message ?? ex.Message));
            }
        }
        protected IActionResult Aksi(Action<string> aksi, string pesanSukses, string? keepTempData = null)
            => Aksi(user => { aksi(user); return null; }, pesanSukses, keepTempData);

        // Sumber data grid / lookup DevExtreme (paging, filter, sort oleh DataSourceLoader).
        protected IActionResult Grid<T>(Func<IEnumerable<T>> ambil, DataSourceLoadOptions loadOptions)
        {
            try
            {
                return Json(DataSourceLoader.Load(ambil(), loadOptions));
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.InnerException?.Message ?? e.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{Controller}.{Action}", GetType().Name, ActionName);
                return BadRequest("⚠ Server Error: Internal Server Error");
            }
        }

        // Isi berkas upload (null bila tidak ada).
        protected static async Task<byte[]?> BacaBerkasAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }

        // Berkas dari library; null → 404.
        protected IActionResult FileOrNotFound(byte[]? data, string? contentType, string? fileName = null)
        {
            if (data == null || data.Length == 0)
            {
                return NotFound();
            }
            contentType ??= "application/octet-stream";
            return string.IsNullOrWhiteSpace(fileName) ? File(data, contentType) : File(data, contentType, fileName);
        }

        protected static Response Gagal(string pesan) => new() { Status = ResponseStatus.Error, Message = pesan };
    }
}
