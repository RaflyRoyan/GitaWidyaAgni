using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Information
{
    public partial class Announcement
    {
        private const int SLUG_LENGTH = 220;

        #region SYNC
        public static List<Announcement> GetDataList(PublicationStatus? status = null, AnnouncementCategory? category = null, int startdata = -1, int jmldata = 50)
        {
            return Page(FilterQuery(status, category), startdata, jmldata).ToList().MappingData<Announcement, EF.TAnnouncement>();
        }
        // Landing page (publicOnly) atau anggota login: hanya Published, terbaru dulu.
        public static List<Announcement> GetPublishedList(bool publicOnly, AnnouncementCategory? category = null, bool featuredOnly = false, int startdata = -1, int jmldata = 20)
        {
            return Page(PublishedQuery(publicOnly, category, featuredOnly), startdata, jmldata).ToList().MappingData<Announcement, EF.TAnnouncement>();
        }
        // Detail Published berdasarkan slug; publicOnly = tamu (Private disembunyikan).
        public static Announcement? GetBySlug(string slug, bool publicOnly)
        {
            var row = SlugQuery(slug, publicOnly).SingleOrDefault();
            return row == null ? null : new Announcement(row);
        }
        public static Response<Announcement> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TAnnouncements.Add(row);
                context.SaveChanges();
                return new Announcement(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Announcement>(msg));
        }
        public Response<Announcement> Update(string user)
        {
            return Run(SetRow, user);
        }
        // null = hapus foto sampul.
        public Response<Announcement> UpdateCover(byte[]? photo, string user)
        {
            return Run((context, row, userId) => SetCover(row, photo, userId), user);
        }
        public Response<Announcement> Publish(string user)
        {
            return Run((context, row, userId) => SetStatus(row, PublicationStatus.Published, userId), user);
        }
        public Response<Announcement> Unpublish(string user)
        {
            return Run((context, row, userId) => SetStatus(row, PublicationStatus.Draft, userId), user);
        }
        public Response<Announcement> Archive(string user)
        {
            return Run((context, row, userId) => SetStatus(row, PublicationStatus.Archived, userId), user);
        }
        public Response<Announcement> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateDelete(row);
                context.TAnnouncements.Remove(row);
                context.SaveChanges();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Announcement>(msg));
        }
        public PhotoContent? GetCover()
        {
            using var context = DBClass.GetContext();
            var photo = context.TAnnouncements.Where(x => x.Id == Id).Select(x => x.CoverPhoto).SingleOrDefault();
            return photo == null ? null : new PhotoContent(photo, ImageUtility.GetContentType(photo));
        }
        #endregion

        #region ASYNC
        public static async Task<List<Announcement>> GetDataListAsync(PublicationStatus? status = null, AnnouncementCategory? category = null, int startdata = -1, int jmldata = 50)
        {
            return await (await Page(FilterQuery(status, category), startdata, jmldata).ToListAsync()).MappingDataAsync<Announcement, EF.TAnnouncement>();
        }
        public static async Task<List<Announcement>> GetPublishedListAsync(bool publicOnly, AnnouncementCategory? category = null, bool featuredOnly = false, int startdata = -1, int jmldata = 20)
        {
            return await (await Page(PublishedQuery(publicOnly, category, featuredOnly), startdata, jmldata).ToListAsync())
                .MappingDataAsync<Announcement, EF.TAnnouncement>();
        }
        public static async Task<Announcement?> GetBySlugAsync(string slug, bool publicOnly)
        {
            var row = await SlugQuery(slug, publicOnly).SingleOrDefaultAsync();
            return row == null ? null : new Announcement(row);
        }
        public static async Task<Response<Announcement>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TAnnouncements.Add(row);
                await context.SaveChangesAsync();
                return new Announcement(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Announcement>(msg));
        }
        public Task<Response<Announcement>> UpdateAsync(string user)
        {
            return RunAsync(SetRow, user);
        }
        public Task<Response<Announcement>> UpdateCoverAsync(byte[]? photo, string user)
        {
            return RunAsync((context, row, userId) => SetCover(row, photo, userId), user);
        }
        public Task<Response<Announcement>> PublishAsync(string user)
        {
            return RunAsync((context, row, userId) => SetStatus(row, PublicationStatus.Published, userId), user);
        }
        public Task<Response<Announcement>> UnpublishAsync(string user)
        {
            return RunAsync((context, row, userId) => SetStatus(row, PublicationStatus.Draft, userId), user);
        }
        public Task<Response<Announcement>> ArchiveAsync(string user)
        {
            return RunAsync((context, row, userId) => SetStatus(row, PublicationStatus.Archived, userId), user);
        }
        public async Task<Response<Announcement>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateDelete(row);
                context.TAnnouncements.Remove(row);
                await context.SaveChangesAsync();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Announcement>(msg));
        }
        public async Task<PhotoContent?> GetCoverAsync()
        {
            using var context = DBClass.GetContext();
            var photo = await context.TAnnouncements.Where(x => x.Id == Id).Select(x => x.CoverPhoto).SingleOrDefaultAsync();
            return photo == null ? null : new PhotoContent(photo, ImageUtility.GetContentType(photo));
        }
        #endregion

        #region INTERNAL
        // Huruf kecil a-z 0-9 dipisah tanda hubung; diakritik dibuang.
        internal static string ToSlug(string? value)
        {
            var normalized = (value ?? string.Empty).Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }
            var slug = Regex.Replace(builder.ToString().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
            return slug.Length <= SLUG_LENGTH - 4 ? slug : slug[..(SLUG_LENGTH - 4)].TrimEnd('-');
        }
        #endregion

        #region HELPER
        private delegate void RowAction(EF.ModelContext context, EF.TAnnouncement row, int userId);

        private Response<Announcement> Run(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                return new Announcement(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Announcement>(msg));
        }
        private async Task<Response<Announcement>> RunAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                return new Announcement(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Announcement>(msg));
        }
        private static EF.TAnnouncement Reload(int id)
        {
            return _DetailQuerys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TAnnouncement> Page(IQueryable<EF.TAnnouncement> query, int startdata, int jmldata)
        {
            return startdata > -1 ? query.Skip(startdata).Take(jmldata) : query;
        }
        private static IQueryable<EF.TAnnouncement> FilterQuery(PublicationStatus? status, AnnouncementCategory? category)
        {
            return _Querys.Where(x => (status == null || x.Status == (int)status) && (category == null || x.Category == (int)category))
                .OrderByDescending(x => x.PublishedAt ?? x.InsDate).ThenByDescending(x => x.Id);
        }
        private static IQueryable<EF.TAnnouncement> PublishedQuery(bool publicOnly, AnnouncementCategory? category, bool featuredOnly)
        {
            return _Querys.Where(x => x.Status == (int)PublicationStatus.Published
                    && (!publicOnly || x.IsPublic == (int)PublicVisibility.Public)
                    && (!featuredOnly || x.IsFeatured == (int)FeaturedStatus.Disorot)
                    && (category == null || x.Category == (int)category))
                .OrderByDescending(x => x.PublishedAt).ThenByDescending(x => x.Id);
        }
        private static IQueryable<EF.TAnnouncement> SlugQuery(string slug, bool publicOnly)
        {
            var key = slug?.Trim().ToLowerInvariant() ?? string.Empty;
            return _DetailQuerys.Where(x => x.Slug == key && x.Status == (int)PublicationStatus.Published
                && (!publicOnly || x.IsPublic == (int)PublicVisibility.Public));
        }
        private static EF.TAnnouncement GetRow(EF.ModelContext context, int id)
        {
            return context.TAnnouncements.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Berita / Pengumuman"));
        }
        private static async Task<EF.TAnnouncement> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TAnnouncements.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Berita / Pengumuman"));
        }
        // Slug manual harus unik; slug otomatis dari judul diberi akhiran angka bila bentrok.
        private static string ResolveSlug(EF.ModelContext context, int id, string? slug, string title)
        {
            if (!string.IsNullOrWhiteSpace(slug))
            {
                var manual = ToSlug(slug);
                if (manual.Length == 0)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Slug"));
                }
                if (context.TAnnouncements.Any(x => x.Slug == manual && x.Id != id))
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.ANNOUNCEMENT_SLUG_EXISTS, manual));
                }
                return manual;
            }
            var baseSlug = ToSlug(title);
            if (baseSlug.Length == 0)
            {
                baseSlug = "berita";
            }
            var used = context.TAnnouncements.Where(x => x.Id != id && (x.Slug == baseSlug || x.Slug.StartsWith(baseSlug + "-")))
                .Select(x => x.Slug).ToHashSet();
            var candidate = baseSlug;
            for (var n = 2; used.Contains(candidate); n++)
            {
                candidate = $"{baseSlug}-{n}";
            }
            return candidate;
        }
        private static void SetValues(EF.ModelContext context, EF.TAnnouncement row, AnnouncementCategory category, string? title, string? slug,
            string? excerpt, string? content, PublicVisibility isPublic, FeaturedStatus isFeatured)
        {
            if (!Enum.IsDefined(category))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kategori"));
            }
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul"));
            }
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Konten"));
            }
            if (!Enum.IsDefined(isPublic) || !Enum.IsDefined(isFeatured))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Visibilitas"));
            }
            if (isFeatured == FeaturedStatus.Disorot && isPublic != PublicVisibility.Public)
            {
                throw new ArgumentException(ExceptionMessage.FEATURED_NOT_PUBLIC);
            }
            row.Category = (int)category;
            row.Title = title.Trim();
            row.Slug = ResolveSlug(context, row.Id, slug, row.Title);
            row.Excerpt = TransaksiHelper.ToNullable(excerpt);
            row.Content = content;
            row.IsPublic = (int)isPublic;
            row.IsFeatured = (int)isFeatured;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static EF.TAnnouncement CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var row = new EF.TAnnouncement
            {
                AuthorUserId = userId,
                Status = (int)PublicationStatus.Draft,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            SetValues(context, row, newrow.Category, newrow.Title, newrow.Slug, newrow.Excerpt, newrow.Content, newrow.IsPublic, newrow.IsFeatured);
            if (newrow.CoverPhoto != null && newrow.CoverPhoto.Length > 0)
            {
                row.CoverPhoto = ImageUtility.Normalize(newrow.CoverPhoto).Data;
            }
            row.Id = SequenceUtility.GetNextId<EF.TAnnouncement>(context);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.TAnnouncement row, int userId)
        {
            // Slug kosong saat ubah = pertahankan slug lama.
            var slug = string.IsNullOrWhiteSpace(Slug) ? row.Slug : Slug;
            SetValues(context, row, Category, Title, slug, Excerpt, Content ?? row.Content, IsPublic, IsFeatured);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetCover(EF.TAnnouncement row, byte[]? photo, int userId)
        {
            row.CoverPhoto = photo == null || photo.Length == 0 ? null : ImageUtility.Normalize(photo).Data;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetStatus(EF.TAnnouncement row, PublicationStatus target, int userId)
        {
            var current = (PublicationStatus)row.Status;
            switch (target)
            {
                case PublicationStatus.Published:
                    TransaksiHelper.ValidateStatus(current, "menerbitkan berita", PublicationStatus.Draft, PublicationStatus.Archived);
                    row.PublishedAt ??= DateTime.Now;
                    break;
                case PublicationStatus.Draft:
                    TransaksiHelper.ValidateStatus(current, "membatalkan terbit berita", PublicationStatus.Published);
                    break;
                default:
                    TransaksiHelper.ValidateStatus(current, "mengarsipkan berita", PublicationStatus.Published);
                    break;
            }
            row.Status = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void ValidateDelete(EF.TAnnouncement row)
        {
            TransaksiHelper.ValidateStatus((PublicationStatus)row.Status, "menghapus berita", PublicationStatus.Draft, PublicationStatus.Archived);
        }
        #endregion
    }
}
