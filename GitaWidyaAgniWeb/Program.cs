using GitaWidyaAgniWeb.Infrastructure;
using GWALib.General;
using GWALib.Lib.Master.Akses;
using GWALib.Lib.Transaksi.EVoting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;

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
builder.Services.AddSingleton<AccessService>();
builder.Services.AddScoped<AccessFilter>();

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
    });

builder.Services.AddControllersWithViews(options =>
    {
        // Semua halaman wajib login kecuali [AllowAnonymous]; hak akses per aksi dicek AccessFilter.
        options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
        options.Filters.Add<AccessFilter>();
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    })
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landing}/{action=Index}/{id?}");

app.Run();
