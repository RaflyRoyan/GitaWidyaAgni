using GWAShared.Enums;
using GWAShared.Results;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Infrastructure
{
    /// <summary>
    /// Dasar controller halaman login. LibUser = parameter "user" untuk method GWALib.
    /// Untuk request AJAX, hasil Response&lt;T&gt; dikembalikan apa adanya lewat LibResult (status gagal → HTTP 400).
    /// </summary>
    public abstract class BaseController : Controller
    {
        protected int UserId => User.GetUserId();
        protected string LibUser => User.GetLibUser();

        protected IActionResult LibResult<T>(Response<T> response)
        {
            var ok = response.Status == ResponseStatus.Success;
            return ok ? Json(response) : BadRequest(response);
        }
    }
}
