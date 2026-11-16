using GWALib.Lib.Master.Organization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers
{
    // Logo organisasi untuk layout (publik). Belum ada logo di profil → logo MB GWA bawaan (assets/images/gwa-logo.png).
    [AllowAnonymous]
    public class BrandController : Controller
    {
        [HttpGet]
        [ResponseCache(Duration = 300)]
        public IActionResult Logo()
        {
            var logo = OrganizationProfile.GetLogo();
            return logo == null ? File("~/assets/images/gwa-logo.png", "image/png") : File(logo.Data, logo.ContentType);
        }
    }
}
