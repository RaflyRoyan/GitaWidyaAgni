using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models.Landing;
using GWALib.Lib.Master.Information;
using GWALib.Lib.Master.Organization;
using GWALib.Lib.Transaksi.Anggota;
using GWALib.Lib.Transaksi.Information;
using GWALib.Lib.Transaksi.Prestasi;
using GWALib.Lib.Transaksi.Program;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers
{
    // Halaman publik (tanpa login): landing page dan berita. Hanya data Public / Published yang tampil.
    [AllowAnonymous]
    public class LandingController : Controller
    {
        private const int NEWS_PAGE_SIZE = 9;
        private readonly AccessService _access;
        private readonly ILogger<LandingController> _logger;

        public LandingController(AccessService access, ILogger<LandingController> logger)
        {
            _access = access;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new LandingVM
            {
                AppName = _access.GetAppName(),
                Profile = Load(OrganizationProfile.GetData, null),
                Site = Load(LoadSite, new SiteInfo()),
                Banners = Load(Banner.GetActiveList, new List<Banner>()),
                News = Load(() => Announcement.GetPublishedList(true, startdata: 0, jmldata: 3), new List<Announcement>()),
                Activities = Load(() => Activity.GetPublicList(6), new List<Activity>()),
                Achievements = Load(() => Achievement.GetPublicList(6), new List<Achievement>()),
                Galleries = Load(() => Gallery.GetPublicList(6), new List<Gallery>()),
                OrganizationPhotos = Load(() => OrganizationPhoto.GetInfoList().Select(x => x.SeqPhoto).Take(4).ToList(), new List<int>()),
                OpenRecruitment = Load(() => Recruitment.GetOpenList().FirstOrDefault(), null)
            };
            return View(model);
        }

        public IActionResult Berita(int page = 1)
        {
            page = Math.Max(1, page);
            // Ambil satu lebih banyak untuk tahu ada halaman berikutnya.
            var items = Load(() => Announcement.GetPublishedList(true, startdata: (page - 1) * NEWS_PAGE_SIZE, jmldata: NEWS_PAGE_SIZE + 1), new List<Announcement>());
            return View(new NewsListVM
            {
                Items = items.Take(NEWS_PAGE_SIZE).ToList(),
                Page = page,
                HasNext = items.Count > NEWS_PAGE_SIZE
            });
        }

        [Route("berita/{slug}")]
        public IActionResult BeritaDetail(string slug)
        {
            var item = Announcement.GetBySlug(slug, true);
            return item == null ? NotFound() : View(item);
        }

        [ResponseCache(Duration = 300)]
        public IActionResult BannerImage(string key)
        {
            var image = SafeKey(() => new Banner(key).GetImage());
            return image == null ? NotFound() : File(image.Data, image.ContentType);
        }

        [ResponseCache(Duration = 300)]
        public IActionResult NewsCover(string key)
        {
            var cover = SafeKey(() => new Announcement(key).GetCover());
            return cover == null ? NotFound() : File(cover.Data, cover.ContentType);
        }

        // Sampul galeri, hanya untuk galeri yang Published dan Public.
        [ResponseCache(Duration = 300)]
        public IActionResult GalleryCover(int id)
        {
            if (!Gallery.GetPublicList(100).Any(x => x.Id == id))
            {
                return NotFound();
            }
            var info = GalleryPhoto.GetInfoList(id).OrderByDescending(x => x.IsCover == GalleryPhotoRole.Sampul).ThenBy(x => x.Seq).FirstOrDefault();
            if (info == null)
            {
                return NotFound();
            }
            var photo = new GalleryPhoto(info.Id.ToString().EncryptString<GalleryPhoto>());
            return File(photo.Photo, photo.ContentType);
        }

        [ResponseCache(Duration = 300)]
        public IActionResult OrganizationImage(int id)
        {
            var photo = OrganizationPhoto.GetDataList().FirstOrDefault(x => x.SeqPhoto == id);
            return photo == null ? NotFound() : File(photo.Photo, photo.ContentType);
        }

        private static SiteInfo LoadSite()
        {
            string? Get(string key) => SiteSetting.GetByKey(key)?.SettingValue;
            return new SiteInfo
            {
                HeroTitle = Get("hero_title"),
                HeroSubtitle = Get("hero_subtitle"),
                Address = Get("alamat"),
                WhatsApp = Get("whatsapp"),
                Instagram = Get("instagram"),
                YouTube = Get("youtube"),
                TikTok = Get("tiktok")
            };
        }

        // Satu bagian gagal dimuat (mis. tabel masih kosong / bermasalah) tidak boleh menjatuhkan landing page.
        private T Load<T>(Func<T> load, T fallback)
        {
            try
            {
                return load();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memuat data landing page");
                return fallback;
            }
        }
        // encKey tidak valid / kedaluwarsa → null.
        private static T? SafeKey<T>(Func<T?> load) where T : class
        {
            try
            {
                return load();
            }
            catch (FormatException)
            {
                return null;
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                return null;
            }
        }
    }
}
