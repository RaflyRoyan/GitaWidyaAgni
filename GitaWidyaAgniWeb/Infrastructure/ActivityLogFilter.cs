using GWALib.Lib.Transaksi.Information;
using GWAShared.Enums;
using GWAShared.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitaWidyaAgniWeb.Infrastructure
{
    /// <summary>
    /// Mencatat setiap aksi tulis yang berhasil ke t_activity_log: request selain GET oleh user login yang menghasilkan
    /// Json(Response) dengan Status Success (pola Aksi di BaseController).
    /// Module = area (atau controller), Action = nama action, EntityName = controller, EntityId = Data.Id bila ada,
    /// NewValue = argumen action (file diganti nama + ukuran). Kunci berbau rahasia (password, token, ...) disamarkan GWALib.
    /// IP dan user agent diisi dari RequestInfo (RequestInfoMiddleware). Gagal mencatat tidak boleh menggagalkan request.
    /// </summary>
    public class ActivityLogFilter : IAsyncActionFilter
    {
        private const int MAX_JSON_LENGTH = 20000;
        // Aksi berfrekuensi tinggi tanpa nilai audit.
        private static readonly HashSet<string> SKIP_CONTROLLERS = new(StringComparer.OrdinalIgnoreCase) { "Notifikasi", "Login" };
        private static readonly JsonSerializerOptions JSON = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            MaxDepth = 8,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly ILogger<ActivityLogFilter> _logger;

        public ActivityLogFilter(ILogger<ActivityLogFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var track = descriptor != null && !HttpMethods.IsGet(http.Request.Method) && !HttpMethods.IsHead(http.Request.Method)
                && http.User.Identity?.IsAuthenticated == true && !SKIP_CONTROLLERS.Contains(descriptor.ControllerName);
            var arguments = track ? Snapshot(context.ActionArguments) : null;

            var executed = await next();
            if (!track || executed.Exception != null || executed.Result is not JsonResult { Value: Response response }
                || response.Status != ResponseStatus.Success)
            {
                return;
            }
            try
            {
                var area = descriptor!.RouteValues.TryGetValue("area", out var a) && !string.IsNullOrEmpty(a) ? a : descriptor.ControllerName;
                await ActivityLog.LogAsync(new ActivityLog.NewRow
                {
                    UserId = int.TryParse(http.User.GetLibUser(), out var userId) ? userId : null,
                    Module = area,
                    Action = descriptor.ActionName,
                    EntityName = descriptor.ControllerName,
                    EntityId = GetId(response),
                    NewValue = arguments
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gagal mencatat activity log {Controller}/{Action}", descriptor!.ControllerName, descriptor.ActionName);
            }
        }

        // Argumen action sebagai JSON; file hanya nama dan ukurannya.
        private static string? Snapshot(IDictionary<string, object?> arguments)
        {
            if (arguments.Count == 0)
            {
                return null;
            }
            try
            {
                var values = arguments.ToDictionary(x => x.Key, x => x.Value switch
                {
                    IFormFile f => new { f.FileName, f.Length },
                    IEnumerable<IFormFile> files => files.Select(f => new { f.FileName, f.Length }).ToList(),
                    byte[] bytes => new { Bytes = bytes.Length },
                    _ => x.Value
                });
                var json = JsonSerializer.Serialize(values, JSON);
                return json.Length <= MAX_JSON_LENGTH ? json : JsonSerializer.Serialize(new { Truncated = true, Keys = arguments.Keys });
            }
            catch (Exception)
            {
                return JsonSerializer.Serialize(new { Keys = arguments.Keys });
            }
        }

        private static int? GetId(Response response)
        {
            var data = response.GetType().GetProperty("Data")?.GetValue(response);
            return data?.GetType().GetProperty("Id")?.GetValue(data) is int id && id > 0 ? id : null;
        }
    }

    /// <summary>Mengisi GWALib RequestInfo (IP, user agent) untuk jejak audit di GWALib.</summary>
    public class RequestInfoMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestInfoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            GWALib.General.RequestInfo.IpAddress = context.Connection.RemoteIpAddress?.ToString();
            GWALib.General.RequestInfo.UserAgent = context.Request.Headers.UserAgent.ToString();
            return _next(context);
        }
    }
}
