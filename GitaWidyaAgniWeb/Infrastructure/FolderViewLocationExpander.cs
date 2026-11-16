using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;

namespace GitaWidyaAgniWeb.Infrastructure
{
    /// <summary>
    /// View mengikuti folder controller: controller di namespace GitaWidyaAgniWeb.Controllers.Akses
    /// mencari view di /Views/Akses/{Controller}/{View}.cshtml (lalu lokasi bawaan /Views/{Controller}/ dan /Views/Shared/).
    /// Struktur tiga arah sama dengan SurabayaTaxWeb: Controllers/&lt;Area&gt;, Models/&lt;Area&gt;, Views/&lt;Area&gt;.
    /// </summary>
    public class FolderViewLocationExpander : IViewLocationExpander
    {
        private const string KEY = "gwa-folder";
        private const string ROOT = "GitaWidyaAgniWeb.Controllers.";

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            if (context.ActionContext.ActionDescriptor is ControllerActionDescriptor action)
            {
                var ns = action.ControllerTypeInfo.Namespace ?? string.Empty;
                context.Values[KEY] = ns.StartsWith(ROOT) ? ns[ROOT.Length..].Replace('.', '/') : null;
            }
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.Values.TryGetValue(KEY, out var folder) && !string.IsNullOrEmpty(folder))
            {
                return new[] { $"/Views/{folder}/{{1}}/{{0}}.cshtml" }.Concat(viewLocations);
            }
            return viewLocations;
        }
    }
}
