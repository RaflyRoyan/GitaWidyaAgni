using System.Security.Cryptography;
using System.Text.Json;
using GWALib.General;
using GWALib.Lib.Master.Akses;
using GWALib.Lib.Master.Organization;
using GWALib.Lib.Transaksi.EVoting;
using GWAShared.Enums;
using GWASeeder;

// Pemakaian (dari folder repo):
//   dotnet run --project GWASeeder [-- --evoting-key <base64 32 byte>]
// Connection string: env GWA_DB, lalu GitaWidyaAgniWeb/appsettings.Development.json, lalu appsettings.json (ConnectionStrings:Default).
// Kunci e-voting: argumen --evoting-key, env GWA_EVOTING_KEY, atau EVoting:Key di appsettings; bila kosong dibuat acak dan dicetak.
// Butuh user "admin" (dibuat web lewat Bootstrap) dan database yang belum di-seed (m_organization kosong).

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

var admin = UserAccount.GetDataList(FilterAktif.Semua).SingleOrDefault(x => x.Username == "admin");
if (admin == null)
{
    Console.Error.WriteLine("User 'admin' belum ada. Jalankan web sekali dengan Bootstrap:AdminUsername / AdminPassword.");
    return 1;
}
if (OrganizationProfile.GetData() != null)
{
    Console.Error.WriteLine("Database sudah berisi data (m_organization terisi). Seeder hanya untuk database kosong.");
    return 1;
}
Seed.Admin = admin.Id.ToString();

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
Console.WriteLine("Selesai. Password semua user contoh: " + Seed.DEFAULT_PASSWORD);
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
