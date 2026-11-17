using GitaWidyaAgniWeb.Infrastructure;
using GWALib.General;
using GWALib.Lib.Master.Akses;
using GWALib.Lib.Transaksi.EVoting;
using DevExpress.AspNetCore;
using DevExpress.AspNetCore.Reporting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

var builder = WebApplication.CreateBuilder(args);

// Library: connection string dari appsettings.json (ConnectionStrings:Default).
// Kunci e-voting dari user-secrets / environment: dotnet user-secrets set "EVoting:Key" "<base64 32 byte>"
DBClass.Configure(builder.Configuration.GetConnectionString("Default"));
var evotingKey = builder.Configuration["EVoting:Key"];
if (!string.IsNullOrWhiteSpace(evotingKey))
{
    ElectionVote.ConfigureKey(evotingKey);
}

// Admin pertama: hanya dibuat bila m_user masih kosong. Isi lewat user-secrets lalu hapus lagi setelah login pertama.
// dotnet user-secrets set "Bootstrap:AdminUsername" "admin"
// dotnet user-secrets set "Bootstrap:AdminPassword" "<password kuat>"
var bootstrapUser = builder.Configuration["Bootstrap:AdminUsername"];
var bootstrapPassword = builder.Configuration["Bootstrap:AdminPassword"];
if (!string.IsNullOrWhiteSpace(bootstrapUser) && !string.IsNullOrWhiteSpace(bootstrapPassword))
{
    UserAccount.BootstrapAdmin(bootstrapUser, bootstrapPassword);
}

builder.Services.AddMemoryCache();
// DevExpress Reporting: Web Document Viewer (endpoint DXXRDV, Controllers/ReportViewerController.cs). Client viewer
// 23.2.5 di wwwroot/lib/dxreporting-23.2.5, dimuat halaman viewer mandiri (mis. Views/Inventaris/Aset/LabelQr.cshtml).
builder.Services.AddDevExpressControls();
builder.Services.ConfigureReportingServices(cfg => cfg.ConfigureWebDocumentViewer(viewer => viewer.UseCachedReportSourceBuilder()));
builder.Services.AddSingleton<AccessService>();
builder.Services.AddScoped<AccessFilter>();
// Job terjadwal (README "Job terjadwal") dan pengirim t_message_outbox (Smtp:* / WhatsApp:* dari user-secrets).
builder.Services.AddHttpClient();
builder.Services.AddSingleton<GitaWidyaAgniWeb.Infrastructure.Jobs.MessageSender>();
builder.Services.AddHostedService<GitaWidyaAgniWeb.Infrastructure.Jobs.ScheduledJobService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Login/Logout";
        options.AccessDeniedPath = "/Login/Denied";
        options.Cookie.Name = "gwa.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        // Request AJAX (jQuery / DevExtreme) mendapat 401 / 403, bukan HTML halaman login; Gwa.js yang mengarahkan ke login.
        options.Events.OnRedirectToLogin = ctx => AjaxOrRedirect(ctx, StatusCodes.Status401Unauthorized);
        options.Events.OnRedirectToAccessDenied = ctx => AjaxOrRedirect(ctx, StatusCodes.Status403Forbidden);
    });

static Task AjaxOrRedirect(Microsoft.AspNetCore.Authentication.RedirectContext<CookieAuthenticationOptions> ctx, int status)
{
    if (ctx.Request.Headers.XRequestedWith == "XMLHttpRequest")
    {
        ctx.Response.StatusCode = status;
    }
    else
    {
        ctx.Response.Redirect(ctx.RedirectUri);
    }
    return Task.CompletedTask;
}

builder.Services.AddControllersWithViews(options =>
    {
        // Semua halaman wajib login kecuali [AllowAnonymous]; hak akses per aksi dicek AccessFilter.
        options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
        options.Filters.Add<AccessFilter>();
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        // Aksi tulis yang berhasil dicatat ke t_activity_log.
        options.Filters.Add<ActivityLogFilter>();
        // Validasi data ada di GWALib; string non-nullable jangan dianggap [Required] oleh model binding.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        // Form dibaca dengan InvariantCulture (desimal titik, tanggal ISO).
        var formFactory = options.ValueProviderFactories.OfType<FormValueProviderFactory>().FirstOrDefault();
        if (formFactory != null)
        {
            options.ValueProviderFactories[options.ValueProviderFactories.IndexOf(formFactory)] = new InvariantFormValueProviderFactory();
        }
    })
    // JSON: PascalCase, Key (encKey) untuk objek GWALib, <Enum>Text, tanpa byte[] (lihat LibJson).
    .AddJsonOptions(options => LibJson.Configure(options.JsonSerializerOptions));
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");
// View area tidak di /Areas: controller [Area("Akses")] UserController → Views/Akses/User/*.cshtml (pola BEGateSecuritySystemWeb).
// BaseController memakai path absolut (URLView); format ini untuk partial bernama pendek (<partial name="_Form" />).
builder.Services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
{
    options.AreaViewLocationFormats.Clear();
    options.AreaViewLocationFormats.Add("/Views/{2}/{1}/{0}.cshtml");
    options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseDevExpressControls();

app.UseRouting();
app.UseMiddleware<RequestInfoMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landing}/{action=Index}/{id?}");

app.Run();
