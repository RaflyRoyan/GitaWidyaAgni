using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models.Landing;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Master.Inventaris;
using GWALib.Lib.Master.Information;
using GWALib.Lib.Master.Merchandise;
using GWALib.Lib.Master.Organization;
using GWALib.Lib.Transaksi.Anggota;
using GWALib.Lib.Transaksi.Information;
using GWALib.Lib.Transaksi.Merchandise;
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

        // Verifikasi sertifikat publik dari kode verifikasi (tanpa login).
        [HttpGet("sertifikat/verifikasi")]
        public IActionResult VerifikasiSertifikat(string? kode)
        {
            var model = new VerifikasiSertifikatVM { Kode = kode?.Trim() };
            if (model.Dicari && model.Kode!.Length <= 32)
            {
                model.Sertifikat = Load(() => Certificate.Verify(model.Kode), null);
            }
            return View(model);
        }

        // Open recruitment publik: daftar rekrutmen yang dibuka + form pendaftaran (tanpa login).
        [HttpGet("rekrutmen/{id:int?}")]
        public IActionResult Rekrutmen(int? id)
        {
            var open = Load(Recruitment.GetOpenList, new List<Recruitment>());
            var selected = id.HasValue ? open.FirstOrDefault(x => x.Id == id) : open.Count == 1 ? open[0] : null;
            if (id.HasValue && selected == null)
            {
                return RedirectToAction(nameof(Rekrutmen), new { id = (int?)null });
            }
            return View(new RecruitmentPageVM { Open = open, Selected = selected, Input = new ApplyInput() });
        }

        [HttpPost("rekrutmen/{id:int}")]
        public IActionResult Rekrutmen(int id, ApplyInput input)
        {
            var open = Load(Recruitment.GetOpenList, new List<Recruitment>());
            var selected = open.FirstOrDefault(x => x.Id == id);
            if (selected == null)
            {
                return RedirectToAction(nameof(Rekrutmen), new { id = (int?)null });
            }
            var model = new RecruitmentPageVM { Open = open, Selected = selected, Input = input };
            var result = RecruitmentApplicant.Apply(new RecruitmentApplicant.NewRow
            {
                RecruitmentId = id,
                ApplicantCategory = input.Category,
                Nim = input.Nim,
                Fullname = input.Fullname ?? "",
                Email = input.Email ?? "",
                Phone = input.Phone,
                ProgramStudi = input.ProgramStudi,
                Angkatan = input.Angkatan,
                Institution = input.Institution,
                Motivation = input.Motivation
            });
            if (result.Status == ResponseStatus.Success)
            {
                model.Success = true;
                model.Input = new ApplyInput();
            }
            else
            {
                model.Error = result.Message;
            }
            return View(model);
        }

        // Toko merchandise publik: pesan tanpa login (nama + WhatsApp) dan lacak pesanan dengan nomor + WhatsApp.
        [HttpGet("toko")]
        public IActionResult Toko(string? nomor, string? wa)
        {
            var model = NewTokoModel();
            model.LacakNomor = nomor?.Trim();
            model.LacakWa = wa?.Trim();
            if (model.LacakDicari)
            {
                model.Lacak = Load(() => SalesOrder.FindGuestOrder(model.LacakNomor, model.LacakWa), null);
                model.LacakItem = model.Lacak == null ? new() : Load(() => model.Lacak.GetItems(true), new List<SalesOrderItem>());
            }
            return View(model);
        }

        [HttpPost("toko")]
        public IActionResult Toko(TokoInput input)
        {
            var model = NewTokoModel();
            model.Input = input;
            if (!string.IsNullOrEmpty(input.Website))
            {
                model.Error = "Pesanan gagal dikirim.";
                return View(model);
            }
            var result = SalesOrder.SaveGuest(new SalesOrder.GuestRow
            {
                BuyerName = input.BuyerName,
                BuyerPhone = input.BuyerPhone,
                Notes = input.Notes,
                Items = input.ToItems()
            });
            if (result.Status == ResponseStatus.Success)
            {
                model.Dibuat = result.Data;
                model.Input = new TokoInput();
                model.Katalog = Load(TokoProduk.LoadKatalog, new List<TokoProduk>());
            }
            else
            {
                model.Error = result.Message;
            }
            return View(model);
        }

        // Foto produk aktif untuk katalog toko publik.
        [ResponseCache(Duration = 300)]
        public IActionResult ProdukFoto(string key)
        {
            try
            {
                var product = new Product(key);
                var photo = product.Id > 0 && product.IsActive == ActiveStatus.Aktif ? product.GetPhoto() : null;
                return photo == null ? NotFound() : File(photo.Data, photo.ContentType ?? "image/jpeg");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // Hasil scan label QR unit aset (isi QR = URL ini): detail alat tanpa login. Unit yang dihapus tidak ditampilkan.
        [HttpGet("aset/{kode}")]
        public IActionResult AsetPublik(string kode)
        {
            var model = new AsetPublikVM
            {
                Kode = AsetLabel.KodeUnit(kode),
                WhatsApp = Load(LoadSite, new SiteInfo()).WhatsApp,
                OrgName = _access.GetAppName(),
                Login = User.Identity?.IsAuthenticated == true
            };
            var unit = Load(() => AssetUnit.GetByCode(model.Kode), null);
            if (unit != null && unit.Status != AssetUnitStatus.Dihapus)
            {
                model.Unit = unit;
                model.Aset = Load(() => new Asset(unit.AssetId.ToString().EncryptString<Asset>()), null);
                model.Kategori = model.Aset?.Category?.Name;
                model.AdaFoto = model.Aset != null && Load(() => AssetPhoto.GetInfoList(model.Aset.Id).Count > 0, false);
            }
            if (model.Aset == null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
            }
            return View(model);
        }

        // Foto pertama aset dari unit (halaman AsetPublik).
        [ResponseCache(Duration = 300)]
        public IActionResult AsetFoto(string kode)
        {
            var unit = Load(() => AssetUnit.GetByCode(AsetLabel.KodeUnit(kode)), null);
            if (unit == null || unit.Status == AssetUnitStatus.Dihapus)
            {
                return NotFound();
            }
            var photo = Load(() => AssetPhoto.GetDataList(unit.AssetId).OrderBy(x => x.SeqPhoto).FirstOrDefault(), null);
            return photo == null ? NotFound() : File(photo.Photo, photo.ContentType);
        }

        private TokoPageVM NewTokoModel()
        {
            var anggota = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                anggota = Load(() => Member.GetByUser(User.GetLibUser()) != null, false);
            }
            return new TokoPageVM
            {
                Katalog = Load(TokoProduk.LoadKatalog, new List<TokoProduk>()),
                WhatsAppAdmin = Load(LoadSite, new SiteInfo()).WhatsApp,
                AnggotaLogin = anggota
            };
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
