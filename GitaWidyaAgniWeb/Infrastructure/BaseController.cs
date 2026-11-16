using GWAShared.Enums;
using GWAShared.Results;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Infrastructure
{
    /// <summary>
    /// Dasar controller halaman pengelolaan (pola BEGateSecuritySystemWeb):
    /// - Controller memakai [Area("&lt;Folder&gt;")]; view dicari di ~/Views/&lt;Area&gt;/&lt;Controller&gt;/ (URLView).
    /// - Index → HalamanIndex(): view lengkap. List → Bagian(...): partial _List (grid ListGrid, data lokal),
    ///   dimuat lewat AppUI.loadPartial dan dimuat ulang setelah simpan.
    /// - Add / Edit / modal lain → Bagian(...): partial _Add / _Edit / _X di modal bersama (_Modal.cshtml).
    ///   Edit menyimpan encKey di TempData[KEY_EDIT].
    /// - SimpanBaru / Update / aksi lain (POST, body JSON) → Aksi(...): selalu Json(Response) { Status, Message, Data }.
    ///   ArgumentException (dari GWALib atau VM) → JsonError (pesan untuk user); lainnya → JsonServerError (dicatat di log).
    /// Logika bisnis ada di GWALib; pemanggilannya di Models/&lt;Area&gt;/&lt;Nama&gt;VM.Method.
    /// </summary>
    public abstract class BaseController : Controller
    {
        public const string SESSION_EXPIRED_MESSAGE = "Session habis, silakan login kembali.";
        public const string ACCESS_DENIED_MESSAGE = "Anda tidak memiliki akses ke fitur ini.";
        public const string EDIT_EXPIRED_MESSAGE = "Form edit sudah kedaluwarsa. Tutup lalu buka kembali form-nya.";
        public const string SERVER_ERROR_MESSAGE = "⚠ Server Error: Internal Server Error";

        protected int UserId => User.GetUserId();
        protected string LibUser => User.GetLibUser();
        protected string controllerName => ControllerContext.RouteData.Values["controller"]?.ToString() ?? string.Empty;
        protected string actionName => ControllerContext.RouteData.Values["action"]?.ToString() ?? string.Empty;
        protected string areaName => ControllerContext.RouteData.Values["area"]?.ToString() ?? string.Empty;

        /// <summary>Folder view controller ini: ~/Views/&lt;Area&gt;/&lt;Controller&gt;/.</summary>
        protected string URLView => string.IsNullOrEmpty(areaName)
            ? $"~/Views/{controllerName}/"
            : $"~/Views/{areaName}/{controllerName}/";

        protected ILogger Logger => HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());

        protected IActionResult HalamanIndex(object? model = null)
        {
            return View($"{URLView}Index.cshtml", model);
        }

        /// <summary>Partial _&lt;namaView ?? Action&gt;.cshtml (list grid atau isi modal). Error → Json(Response) status Error.</summary>
        protected IActionResult Bagian(Func<object?> buatModel, string? namaView = null)
        {
            try
            {
                return PartialView($"{URLView}_{namaView ?? actionName}.cshtml", buatModel());
            }
            catch (ArgumentException e)
            {
                return JsonError(e);
            }
            catch (Exception ex)
            {
                return JsonServerError(ex);
            }
        }

        /// <summary>
        /// Aksi simpan / ubah / proses. aksi(user) boleh mengembalikan data untuk dikirim balik (Data).
        /// keepTempData: kunci TempData (encKey edit) yang dipertahankan bila gagal agar form bisa disimpan ulang.
        /// </summary>
        protected IActionResult Aksi(Func<string, object?> aksi, string pesanSukses, string? keepTempData = null)
        {
            try
            {
                ValidateModel();
                var data = aksi(LibUser);
                return Json(new Response<object> { Status = ResponseStatus.Success, Message = pesanSukses, Data = data });
            }
            catch (ArgumentException e)
            {
                if (keepTempData != null) TempData.Keep(keepTempData);
                return JsonError(e);
            }
            catch (Exception ex)
            {
                if (keepTempData != null) TempData.Keep(keepTempData);
                return JsonServerError(ex);
            }
        }
        protected IActionResult Aksi(Action<string> aksi, string pesanSukses, string? keepTempData = null)
            => Aksi(user => { aksi(user); return null; }, pesanSukses, keepTempData);

        /// <summary>encKey form Edit dari TempData; hilang (form kedaluwarsa) → ArgumentException.</summary>
        protected string EditKey(string key)
        {
            return TempData[key]?.ToString() ?? throw new ArgumentException(EDIT_EXPIRED_MESSAGE);
        }

        /// <summary>Pesan DataAnnotations / model binding pertama sebagai ArgumentException.</summary>
        protected void ValidateModel()
        {
            if (ModelState.IsValid)
            {
                return;
            }
            var err = ModelState.Values.SelectMany(x => x.Errors)
                .Select(x => string.IsNullOrEmpty(x.ErrorMessage) ? x.Exception?.Message : x.ErrorMessage)
                .FirstOrDefault(x => !string.IsNullOrEmpty(x));
            throw new ArgumentException(err ?? "Format data tidak valid.");
        }

        protected IActionResult JsonError(ArgumentException e)
        {
            return Json(Gagal(e.InnerException?.Message ?? e.Message));
        }

        protected IActionResult JsonServerError(Exception ex)
        {
            Logger.LogError(ex, "{Controller}.{Action}", GetType().Name, actionName);
            return Json(Gagal(SERVER_ERROR_MESSAGE));
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
