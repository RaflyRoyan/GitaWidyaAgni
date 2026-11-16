using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Information
{
    /// <summary>
    /// Berita / pengumuman / agenda (t_announcement). Penulis = user login saat dibuat. Slug unik, huruf kecil a-z 0-9 dan tanda hubung;
    /// kosong = dibuat dari judul (diberi akhiran -2, -3, ... bila bentrok), slug yang diisi manual dan bentrok ditolak.
    /// Alur: Draft → Published → Archived; Published → Draft (Unpublish) dan Archived → Published. PublishedAt diisi saat pertama terbit.
    /// Featured butuh Public. Private = hanya anggota login. Hapus hanya Draft / Archived. Foto sampul dinormalisasi ImageUtility dan
    /// tidak dimuat di daftar (GetCover / UpdateCover); konten tidak dimuat di daftar (ambil lewat constructor encKey / GetBySlug).
    /// </summary>
    public partial class Announcement : BaseLib
    {
        public class NewRow
        {
            public AnnouncementCategory Category { get; set; } = AnnouncementCategory.Pengumuman;
            public string Title { get; set; } = null!;
            public string? Slug { get; set; }
            public string? Excerpt { get; set; }
            public string Content { get; set; } = null!;
            public byte[]? CoverPhoto { get; set; }
            public PublicVisibility IsPublic { get; set; } = PublicVisibility.Public;
            public FeaturedStatus IsFeatured { get; set; } = FeaturedStatus.Biasa;
        }
        public record PhotoContent(byte[] Data, string ContentType);

        public int Id { get; set; }
        public int AuthorUserId { get; private set; }
        public string AuthorName { get; private set; } = null!;
        public AnnouncementCategory Category { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Excerpt { get; set; }
        // Kosong di daftar; terisi bila dimuat per data.
        public string? Content { get; set; }
        public PublicVisibility IsPublic { get; set; }
        public FeaturedStatus IsFeatured { get; set; }
        // Hanya-baca: diubah lewat Publish / Unpublish / Archive.
        public PublicationStatus Status { get; private set; }
        public DateTime? PublishedAt { get; private set; }
        public DateTime InsDate { get; private set; }
        public DateTime? UpdDate { get; private set; }

        // Daftar: tanpa konten dan foto sampul.
        private static IQueryable<EF.TAnnouncement> _Querys
        {
            get
            {
                return DBClass.GetContext().TAnnouncements.Select(x => new EF.TAnnouncement
                {
                    Id = x.Id,
                    AuthorUserId = x.AuthorUserId,
                    AuthorUser = new EF.MUser { Id = x.AuthorUserId, Username = x.AuthorUser.Username },
                    Category = x.Category,
                    Title = x.Title,
                    Slug = x.Slug,
                    Excerpt = x.Excerpt,
                    IsPublic = x.IsPublic,
                    IsFeatured = x.IsFeatured,
                    Status = x.Status,
                    PublishedAt = x.PublishedAt,
                    InsDate = x.InsDate,
                    UpdDate = x.UpdDate
                });
            }
        }
        // Detail: dengan konten, tanpa foto sampul.
        private static IQueryable<EF.TAnnouncement> _DetailQuerys
        {
            get
            {
                return DBClass.GetContext().TAnnouncements.Select(x => new EF.TAnnouncement
                {
                    Id = x.Id,
                    AuthorUserId = x.AuthorUserId,
                    AuthorUser = new EF.MUser { Id = x.AuthorUserId, Username = x.AuthorUser.Username },
                    Category = x.Category,
                    Title = x.Title,
                    Slug = x.Slug,
                    Excerpt = x.Excerpt,
                    Content = x.Content,
                    IsPublic = x.IsPublic,
                    IsFeatured = x.IsFeatured,
                    Status = x.Status,
                    PublishedAt = x.PublishedAt,
                    InsDate = x.InsDate,
                    UpdDate = x.UpdDate
                });
            }
        }
        public Announcement()
        {
        }
        public Announcement(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Announcement>());
            var row = _DetailQuerys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Announcement(EF.TAnnouncement row)
        {
            SetData(row);
        }
        private void SetData(EF.TAnnouncement row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Announcement>();
            AuthorUserId = row.AuthorUserId;
            AuthorName = row.AuthorUser?.Username ?? string.Empty;
            Category = (AnnouncementCategory)row.Category;
            Title = row.Title;
            Slug = row.Slug;
            Excerpt = row.Excerpt;
            Content = row.Content;
            IsPublic = (PublicVisibility)row.IsPublic;
            IsFeatured = (FeaturedStatus)row.IsFeatured;
            Status = (PublicationStatus)row.Status;
            PublishedAt = row.PublishedAt;
            InsDate = row.InsDate;
            UpdDate = row.UpdDate;
        }
    }
}
