using GWALib.Lib.Master.Information;
using GWALib.Lib.Master.Organization;

namespace GitaWidyaAgniWeb.Reports
{
    /// <summary>
    /// Identitas organisasi untuk kop dan tanda tangan laporan: nama + logo (profil organisasi, else logo bawaan),
    /// alamat (site setting "alamat"), kontak (email / telepon profil, WhatsApp / Instagram), kota (site setting "kota").
    /// </summary>
    public class ReportBrand
    {
        public const string INSTITUTION_LINE = "UNIVERSITAS PEMBANGUNAN NASIONAL “VETERAN” JAWA TIMUR";
        public const string DEFAULT_CITY = "Surabaya";

        public string OrgName { get; set; } = "Gita Widya Agni";
        public byte[]? Logo { get; set; }
        public string? Address { get; set; }
        public string? Contact { get; set; }
        public string City { get; set; } = DEFAULT_CITY;

        public static ReportBrand Load(string webRoot)
        {
            var profile = OrganizationProfile.GetData();
            string? Setting(string key)
            {
                var value = SiteSetting.GetByKey(key)?.SettingValue;
                return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            }
            var whatsapp = Setting("whatsapp");
            var instagram = profile?.Instagram ?? Setting("instagram");
            var contact = new[]
            {
                string.IsNullOrWhiteSpace(profile?.Phone) ? (whatsapp == null ? null : "WA " + whatsapp) : "Telp. " + profile!.Phone,
                string.IsNullOrWhiteSpace(profile?.Email) ? null : "Email: " + profile!.Email,
                string.IsNullOrWhiteSpace(instagram) ? null : "Instagram: @" + instagram.TrimStart('@')
            }.Where(x => x != null);
            return new ReportBrand
            {
                OrgName = string.IsNullOrWhiteSpace(profile?.Name) ? "Gita Widya Agni" : profile!.Name,
                Logo = OrganizationProfile.GetLogo()?.Data ?? File.ReadAllBytes(Path.Combine(webRoot, "assets", "images", "gwa-logo.png")),
                Address = Setting("alamat"),
                Contact = string.Join(" · ", contact),
                City = Setting("kota") ?? DEFAULT_CITY
            };
        }
    }
}
