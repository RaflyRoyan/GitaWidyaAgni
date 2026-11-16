using GWALib.Lib.Master.Organization;
using GWALib.Lib.Transaksi.Anggota;
using GWALib.Lib.Transaksi.Information;
using GWALib.Lib.Transaksi.Prestasi;
using GWALib.Lib.Transaksi.Program;

namespace GitaWidyaAgniWeb.Models.Landing
{
    public class LandingVM
    {
        public string AppName { get; set; } = null!;
        public OrganizationProfile? Profile { get; set; }
        public SiteInfo Site { get; set; } = new();
        public List<Banner> Banners { get; set; } = new();
        public List<Announcement> News { get; set; } = new();
        public List<Activity> Activities { get; set; } = new();
        public List<Achievement> Achievements { get; set; } = new();
        public List<Gallery> Galleries { get; set; } = new();
        public List<int> OrganizationPhotos { get; set; } = new();
        public Recruitment? OpenRecruitment { get; set; }
    }

    // Isi m_site_setting yang dipakai halaman publik (kosong = pakai bawaan di view).
    public class SiteInfo
    {
        public string? HeroTitle { get; set; }
        public string? HeroSubtitle { get; set; }
        public string? Address { get; set; }
        public string? WhatsApp { get; set; }
        public string? Instagram { get; set; }
        public string? YouTube { get; set; }
        public string? TikTok { get; set; }
    }

    public class NewsListVM
    {
        public List<Announcement> Items { get; set; } = new();
        public int Page { get; set; }
        public bool HasNext { get; set; }
    }
}
