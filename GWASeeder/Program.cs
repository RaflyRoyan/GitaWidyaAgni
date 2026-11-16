using System.Security.Cryptography;
using System.Text.Json;
using GWALib.General;
using GWALib.Lib.Master.Akses;
using GWALib.Lib.Master.Organization;
using GWALib.Lib.Transaksi.EVoting;
using GWAShared.Enums;
using GWASeeder;

// Pemakaian (dari folder repo):
//   dotnet run --project GWASeeder                      isi database kosong dengan data contoh
//   dotnet run --project GWASeeder -- --reset           KOSONGKAN semua tabel lalu isi ulang (minta ketik RESET; --yes melewati)
//   dotnet run --project GWASeeder -- --sync-akses      hanya sinkron controller/aksi, menu, dan role (database yang sudah berisi)
// Opsi lain: --evoting-key <base64 32 byte>, --admin-password <password>.
// Connection string: env GWA_DB, lalu GitaWidyaAgniWeb/appsettings.Development.json, lalu appsettings.json (ConnectionStrings:Default).
// Kunci e-voting: argumen --evoting-key, env GWA_EVOTING_KEY, atau EVoting:Key di appsettings; bila kosong dibuat acak dan dicetak.
// User "admin" (SUPERADMIN) dibuat otomatis bila m_user masih kosong; password dari --admin-password, env GWA_ADMIN_PASSWORD,
// atau DEFAULT_ADMIN_PASSWORD.

var repoRoot = FindRepoRoot();
var webDir = Path.Combine(repoRoot, "GitaWidyaAgniWeb");
Seed.WebRoot = Path.Combine(webDir, "wwwroot");

using var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(webDir, "appsettings.json")),
    new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
// appsettings.Development.json (lokal, di-ignore git) menimpa placeholder di appsettings.json.
string? localConnection = null;
var devSettingsPath = Path.Combine(webDir, "appsettings.Development.json");
if (File.Exists(devSettingsPath))
{
    using var dev = JsonDocument.Parse(File.ReadAllText(devSettingsPath),
        new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
    if (dev.RootElement.TryGetProperty("ConnectionStrings", out var cs) && cs.TryGetProperty("Default", out var def))
    {
        localConnection = def.GetString();
    }
}
var connectionString = Environment.GetEnvironmentVariable("GWA_DB")
    ?? localConnection
    ?? settings.RootElement.GetProperty("ConnectionStrings").GetProperty("Default").GetString();
DBClass.Configure(connectionString);

var keyArg = Array.IndexOf(args, "--evoting-key");
var evotingKey = keyArg >= 0 && keyArg + 1 < args.Length ? args[keyArg + 1] : Environment.GetEnvironmentVariable("GWA_EVOTING_KEY");
if (string.IsNullOrWhiteSpace(evotingKey) && settings.RootElement.TryGetProperty("EVoting", out var ev) && ev.TryGetProperty("Key", out var k))
{
    evotingKey = k.GetString();
}
var generatedKey = false;
if (string.IsNullOrWhiteSpace(evotingKey))
{
    evotingKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    generatedKey = true;
}
ElectionVote.ConfigureKey(evotingKey);

string? Arg(string name)
{
    var i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}
var reset = args.Contains("--reset");
var syncOnly = args.Contains("--sync-akses");

if (reset)
{
    Console.WriteLine("PERINGATAN: --reset menghapus SELURUH isi database (semua tabel, termasuk user) lalu mengisi ulang data contoh.");
    if (!args.Contains("--yes"))
    {
        Console.Write("Ketik RESET untuk melanjutkan: ");
        if (Console.ReadLine()?.Trim() != "RESET")
        {
            Console.WriteLine("Dibatalkan.");
            return 1;
        }
    }
    Console.WriteLine($"Mengosongkan {Seed.ResetDatabase()} tabel...");
}

var admin = UserAccount.GetDataList(FilterAktif.Semua).SingleOrDefault(x => x.Username == "admin");
var adminPassword = Arg("--admin-password") ?? Environment.GetEnvironmentVariable("GWA_ADMIN_PASSWORD") ?? Seed.DEFAULT_ADMIN_PASSWORD;
var adminCreated = false;
if (admin == null)
{
    admin = UserAccount.BootstrapAdmin("admin", adminPassword);
    if (admin == null)
    {
        Console.Error.WriteLine("User 'admin' tidak ada, padahal m_user sudah berisi. Buat user admin dengan role SUPERADMIN, atau jalankan dengan --reset.");
        return 1;
    }
    adminCreated = true;
}
Seed.Admin = admin.Id.ToString();

if (syncOnly)
{
    try
    {
        Seed.RunSyncAccess();
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine("GAGAL: " + ex.Message);
        return 2;
    }
    Console.WriteLine("Selesai. Hak akses tersinkron. Login ulang (atau tunggu 5 menit) agar cache menu web diperbarui.");
    return 0;
}
if (OrganizationProfile.GetData() != null)
{
    Console.Error.WriteLine("Database sudah berisi data (m_organization terisi). Pakai --reset untuk mengisi ulang, atau --sync-akses untuk hak akses saja.");
    return 1;
}

try
{
    Seed.RunAll();
}
catch (Exception ex)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine("GAGAL: " + ex.Message);
    Console.Error.WriteLine(ex.StackTrace);
    return 2;
}

Console.WriteLine();
Console.WriteLine("Selesai. Akun yang bisa dipakai login:");
Console.WriteLine($"  {"Username",-16} {"Nama",-28} {"Jabatan",-30} Role");
Console.WriteLine($"  {"admin",-16} {"Administrator",-28} {"-",-30} SUPERADMIN" + (adminCreated ? $"  (password: {adminPassword})" : ""));
foreach (var a in Seed.AccountSummary)
{
    Console.WriteLine($"  {a.Username,-16} {a.Name,-28} {a.Jabatan,-30} {a.Roles}");
}
Console.WriteLine("Password semua akun contoh: " + Seed.DEFAULT_PASSWORD);
if (generatedKey)
{
    Console.WriteLine("Kunci e-voting dibuat acak. Simpan ke web supaya pemilihan bisa dikelola:");
    Console.WriteLine($"  dotnet user-secrets set \"EVoting:Key\" \"{evotingKey}\" --project GitaWidyaAgniWeb");
}
return 0;

static string FindRepoRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null && !File.Exists(Path.Combine(dir.FullName, "GitaWidyaAgni.sln")))
    {
        dir = dir.Parent;
    }
    return dir?.FullName ?? throw new InvalidOperationException("Folder repo (GitaWidyaAgni.sln) tidak ditemukan");
}
