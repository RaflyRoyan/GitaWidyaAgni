using System.Diagnostics;
using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GWALib.Lib.Transaksi.Information;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers
{
    // Dashboard: cukup login. Ringkasan (GWALib Dashboard) dipilah per hak akses di view (HomeVM.Can).
    [LoginOnly]
    public class HomeController : BaseController
    {
        private readonly AccessService _access;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AccessService access, ILogger<HomeController> logger)
        {
            _access = access;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new HomeVM
            {
                DisplayName = _access.GetProfile(UserId)?.DisplayName ?? User.GetUsername(),
                Allowed = _access.GetAllowedActions(UserId)
            };
            // Pintasan: menu yang boleh dibuka user (tanpa Area Anggota), maksimal 8.
            foreach (var item in _access.GetMenu(UserId))
            {
                foreach (var m in item.Controller != null ? new[] { item } : item.Children.ToArray())
                {
                    if (m.Controller == null || model.Shortcuts.Count >= 8 || _access.AreaOf(m.Controller) == "AreaAnggota")
                    {
                        continue;
                    }
                    model.Shortcuts.Add((m.Name, m.Icon ?? item.Icon ?? "ri-apps-2-line",
                        Url.Action(m.Action ?? "Index", m.Controller, new { area = _access.AreaOf(m.Controller) }) ?? "#"));
                }
            }
            try
            {
                model.Data = Dashboard.GetSummary(LibUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ringkasan dashboard gagal dimuat");
                model.Error = "Ringkasan belum bisa dimuat. Coba muat ulang halaman.";
            }
            return View(model);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
