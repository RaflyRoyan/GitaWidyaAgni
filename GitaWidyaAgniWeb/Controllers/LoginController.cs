using System.Security.Claims;
using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models.Login;
using GWALib.Lib.Master.Akses;
using GWALib.Lib.Transaksi.Information;
using GWAShared.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly AccessService _access;

        public LoginController(AccessService access)
        {
            _access = access;
        }

        [HttpGet]
        public IActionResult Index(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginVM { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginVM model)
        {
            var result = await UserAccount.LoginAsync(model.Username ?? string.Empty, model.Password ?? string.Empty);
            if (result.Status != ResponseStatus.Success || result.Data == null)
            {
                await LogAsync(null, "LOGIN_FAILED", new { model.Username });
                model.Password = null;
                model.PesanError = result.Message;
                return View(model);
            }

            var account = result.Data;
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new(ClaimTypes.Name, account.Username)
            };
            claims.AddRange(UserAccess.GetRoleCodes(account.Id).Select(code => new Claim(ClaimTypes.Role, code)));
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            _access.Clear(account.Id);
            await LogAsync(account.Id, "LOGIN", null);

            return Url.IsLocalUrl(model.ReturnUrl) ? Redirect(model.ReturnUrl!) : RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var userId = User.GetUserId();
            if (userId > 0)
            {
                _access.Clear(userId);
                await LogAsync(userId, "LOGOUT", null);
            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Landing");
        }

        [HttpGet]
        public IActionResult Denied()
        {
            return View();
        }

        private Task LogAsync(int? userId, string action, object? newValue)
        {
            return ActivityLog.LogAsync(new ActivityLog.NewRow
            {
                UserId = userId,
                Module = "AKSES",
                Action = action,
                EntityName = "m_user",
                EntityId = userId,
                NewValue = newValue,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            });
        }
    }
}
