using System.Diagnostics;
using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers
{
    // Dashboard: cukup login.
    [LoginOnly]
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
