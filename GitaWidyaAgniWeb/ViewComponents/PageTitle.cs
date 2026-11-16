using GitaWidyaAgniWeb.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.ViewComponents
{
    /// <summary>
    /// Judul halaman (kiri) + breadcrumb (kanan), dirender otomatis oleh _Layout (pola BEGateSecuritySystemWeb).
    /// Default dari route: {Area} › [{Controller} bila action bukan Index] › {Title}.
    /// Override lewat ViewData di view:
    ///   ViewData["Title"]           judul halaman / item terakhir breadcrumb
    ///   ViewData["BreadcrumbArea"]  teks area (default: nama area dipisah per kata)
    ///   ViewData["HidePageTitle"]   true = jangan tampilkan (mis. dashboard)
    /// </summary>
    public class PageTitle : ViewComponent
    {
        const string URLView = "~/Views/Shared/Components/PageTitle/Default.cshtml";

        public IViewComponentResult Invoke()
        {
            if (ViewData["HidePageTitle"] is true)
            {
                return Content(string.Empty);
            }

            var route = ViewContext.RouteData.Values;
            var area = route["area"]?.ToString() ?? "";
            var controller = route["controller"]?.ToString() ?? "";
            var action = route["action"]?.ToString() ?? "";
            var isIndex = action.Equals("Index", StringComparison.OrdinalIgnoreCase);

            var title = ViewData["Title"] as string;
            if (string.IsNullOrWhiteSpace(title))
            {
                title = isIndex ? controller.ToTitleWords() : action.ToTitleWords();
            }

            var model = new PageTitleVMC { Title = title };
            model.Breadcrumbs.Add(new PageTitleVMC.BreadcrumbItem { Text = "Dashboard", Url = Url.Action("Index", "Home", new { area = "" }) });

            if (!string.IsNullOrEmpty(area))
            {
                model.Breadcrumbs.Add(new PageTitleVMC.BreadcrumbItem
                {
                    Text = ViewData["BreadcrumbArea"] as string ?? area.ToTitleWords()
                });
            }

            // Halaman selain Index (mis. Detail) -> controller sebagai link ke Index-nya.
            if (!isIndex)
            {
                model.Breadcrumbs.Add(new PageTitleVMC.BreadcrumbItem
                {
                    Text = controller.ToTitleWords(),
                    Url = Url.Action("Index", controller, new { area })
                });
            }

            model.Breadcrumbs.Add(new PageTitleVMC.BreadcrumbItem { Text = title });

            return View(URLView, model);
        }
    }

    public class PageTitleVMC
    {
        public class BreadcrumbItem
        {
            public string Text { get; set; } = string.Empty;
            /// <summary>null = tidak bisa diklik (item terakhir / grup area).</summary>
            public string? Url { get; set; }
        }

        public string Title { get; set; } = string.Empty;
        public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
    }
}
