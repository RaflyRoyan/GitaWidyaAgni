using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Information;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Informasi
{
    /// <summary>
    /// Informasi › Berita: berita, pengumuman, dan agenda. Draft → Terbit → Diarsipkan (Terbit → Draft lewat Tarik, Diarsipkan → Terbit).
    /// Publik tampil di landing page; Private hanya untuk anggota yang login. Disorot wajib publik.
    /// </summary>
    public class BeritaVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = Announcement.GetDataList().Select(x => new Row
            {
                EncDecrKey = x.EncDecrKey,
                Title = x.Title,
                Slug = x.Slug,
                Kategori = EnumHelper.Text(x.Category),
                AuthorName = x.AuthorName,
                Publik = x.IsPublic == PublicVisibility.Public,
                Disorot = x.IsFeatured == FeaturedStatus.Disorot,
                PublishedAt = x.PublishedAt,
                InsDate = x.InsDate,
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status)
            }).OrderByDescending(x => x.PublishedAt ?? x.InsDate).ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string Slug { get; set; } = "";
            public string Kategori { get; set; } = "";
            public string AuthorName { get; set; } = "";
            public bool Publik { get; set; }
            public bool Disorot { get; set; }
            public DateTime? PublishedAt { get; set; }
            public DateTime InsDate { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> KategoriOpsi { get; set; } = GwaVM.Opsi<AnnouncementCategory>();
            public bool HasCover { get; set; }
            public string? Key { get; set; }
        }

        public class Edit : Add
        {
            public string StatusText { get; set; } = "";
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<Announcement>(encKey, "Berita");
                Key = obj.EncDecrKey;
                StatusText = EnumHelper.Text(obj.Status) + (obj.PublishedAt.HasValue ? " · terbit " + obj.PublishedAt.Value.ToString("dd MMM yyyy HH:mm") : "");
                HasCover = obj.GetCover() != null;
                Row = new NewRow
                {
                    Category = (int)obj.Category,
                    Title = obj.Title,
                    Slug = obj.Slug,
                    Excerpt = obj.Excerpt,
                    Content = obj.Content,
                    Publik = obj.IsPublic == PublicVisibility.Public,
                    Disorot = obj.IsFeatured == FeaturedStatus.Disorot
                };
            }
        }

        public class NewRow
        {
            [Display(Name = "Kategori")]
            public int Category { get; set; } = (int)AnnouncementCategory.Berita;

            [Required(ErrorMessage = "Judul harus diisi")]
            [StringLength(255, ErrorMessage = "Judul maksimal 255 karakter")]
            [Display(Name = "Judul")]
            public string? Title { get; set; }

            [StringLength(255, ErrorMessage = "Slug maksimal 255 karakter")]
            [Display(Name = "Slug URL (kosong = dari judul)")]
            public string? Slug { get; set; }

            [StringLength(500, ErrorMessage = "Ringkasan maksimal 500 karakter")]
            [Display(Name = "Ringkasan")]
            public string? Excerpt { get; set; }

            [Required(ErrorMessage = "Isi berita harus diisi")]
            [Display(Name = "Isi")]
            public string? Content { get; set; }

            [Display(Name = "Publik (tampil di landing page)")]
            public bool Publik { get; set; } = true;

            [Display(Name = "Disorot")]
            public bool Disorot { get; set; }
        }

        public class Method
        {
            public static Announcement Load(string? key) => LibExtensions.Load<Announcement>(key, "Berita");

            public static void Simpan(NewRow input, byte[]? cover, string user)
            {
                Announcement.Save(new Announcement.NewRow
                {
                    Category = (AnnouncementCategory)input.Category,
                    Title = input.Title ?? "",
                    Slug = input.Slug,
                    Excerpt = input.Excerpt,
                    Content = input.Content ?? "",
                    CoverPhoto = cover,
                    IsPublic = input.Publik ? PublicVisibility.Public : PublicVisibility.Private,
                    IsFeatured = input.Disorot ? FeaturedStatus.Disorot : FeaturedStatus.Biasa
                }, user).Ok();
            }

            public static void Update(string key, NewRow input, byte[]? cover, bool hapusCover, string user)
            {
                var obj = Load(key);
                obj.Category = (AnnouncementCategory)input.Category;
                obj.Title = input.Title ?? "";
                obj.Slug = input.Slug ?? "";
                obj.Excerpt = input.Excerpt;
                obj.Content = input.Content;
                obj.IsPublic = input.Publik ? PublicVisibility.Public : PublicVisibility.Private;
                obj.IsFeatured = input.Disorot ? FeaturedStatus.Disorot : FeaturedStatus.Biasa;
                obj.Update(user).Ok();
                if (cover != null || hapusCover)
                {
                    obj.UpdateCover(cover, user).Ok();
                }
            }
        }
    }
}
