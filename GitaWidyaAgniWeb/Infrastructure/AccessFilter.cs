using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GitaWidyaAgniWeb.Infrastructure
{
    /// <summary>
    /// Aksi cukup login saja (tanpa cek m_role_action), mis. dashboard dan profil sendiri.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class LoginOnlyAttribute : Attribute
    {
    }

    /// <summary>
    /// Filter hak akses global. Urutan:
    /// 1. [AllowAnonymous] atau belum login → dilewati (yang belum login ditangani AuthorizeFilter global).
    /// 2. [LoginOnly] → cukup login.
    /// 3. Aksi terdaftar di m_controller_action dengan need_auth = 1 → harus ada di m_role_action salah satu role user.
    /// 4. Aksi terdaftar dengan need_auth = 0 → cukup login.
    /// 5. Aksi belum terdaftar → hanya SUPERADMIN (aman secara bawaan; daftarkan aksi baru di master Controller Action).
    /// Ditolak: request AJAX mendapat 403, selain itu diarahkan ke /Login/Denied.
    /// </summary>
    public class AccessFilter : IAuthorizationFilter
    {
        private readonly AccessService _access;

        public AccessFilter(AccessService access)
        {
            _access = access;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var metadata = context.ActionDescriptor.EndpointMetadata;
            if (metadata.OfType<IAllowAnonymous>().Any() || context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                return;
            }
            if (metadata.OfType<LoginOnlyAttribute>().Any())
            {
                return;
            }
            if (context.ActionDescriptor is not ControllerActionDescriptor action)
            {
                return;
            }
            var userId = context.HttpContext.User.GetUserId();
            var allowed = _access.GetAllowedActions(userId);
            if (allowed == null)
            {
                return;
            }
            var key = $"{action.ControllerName}/{action.ActionName}".ToLowerInvariant();
            var registered = _access.GetRegisteredActions();
            if (registered.TryGetValue(key, out var needAuth) && (!needAuth || allowed.Contains(key)))
            {
                return;
            }
            context.Result = IsAjax(context.HttpContext.Request)
                ? new StatusCodeResult(StatusCodes.Status403Forbidden)
                : new RedirectToActionResult("Denied", "Login", null);
        }
        private static bool IsAjax(HttpRequest request)
        {
            return request.Headers.XRequestedWith == "XMLHttpRequest" || request.Headers.Accept.ToString().Contains("application/json");
        }
    }
}
