using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Information
{
    public partial class Banner
    {
        #region SYNC
        public static List<Banner> GetDataList(FilterAktif aktif)
        {
            return FilterQuery(aktif).ToList().MappingData<Banner, EF.TBanner>();
        }
        // Landing page: aktif dan sedang dalam jadwal tayang.
        public static List<Banner> GetActiveList()
        {
            return ActiveQuery().ToList().MappingData<Banner, EF.TBanner>();
        }
        public static Response<Banner> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TBanners.Add(row);
                context.SaveChanges();
                return new Banner(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Banner>(msg));
        }
        public Response<Banner> Update(string user)
        {
            return Run(SetRow, user);
        }
        public Response<Banner> UpdateImage(byte[] image, string user)
        {
            return Run((context, row, userId) => SetImage(row, image, userId), user);
        }
        public Response<Banner> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                // Stub supaya gambar tidak dimuat.
                if (!context.TBanners.Any(x => x.Id == Id))
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Banner"));
                }
                context.TBanners.Remove(new EF.TBanner { Id = Id });
                context.SaveChanges();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Banner>(msg));
        }
        public ImageContent? GetImage()
        {
            using var context = DBClass.GetContext();
            var image = context.TBanners.Where(x => x.Id == Id).Select(x => x.Image).SingleOrDefault();
            return image == null ? null : new ImageContent(image, DetectImageType(image) ?? "application/octet-stream");
        }
        #endregion

        #region ASYNC
        public static async Task<List<Banner>> GetDataListAsync(FilterAktif aktif)
        {
            return await (await FilterQuery(aktif).ToListAsync()).MappingDataAsync<Banner, EF.TBanner>();
        }
        public static async Task<List<Banner>> GetActiveListAsync()
        {
            return await (await ActiveQuery().ToListAsync()).MappingDataAsync<Banner, EF.TBanner>();
        }
        public static async Task<Response<Banner>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TBanners.Add(row);
                await context.SaveChangesAsync();
                return new Banner(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Banner>(msg));
        }
        public Task<Response<Banner>> UpdateAsync(string user)
        {
            return RunAsync(SetRow, user);
        }
        public Task<Response<Banner>> UpdateImageAsync(byte[] image, string user)
        {
            return RunAsync((context, row, userId) => SetImage(row, image, userId), user);
        }
        public async Task<Response<Banner>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                if (!await context.TBanners.AnyAsync(x => x.Id == Id))
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Banner"));
                }
                context.TBanners.Remove(new EF.TBanner { Id = Id });
                await context.SaveChangesAsync();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Banner>(msg));
        }
        public async Task<ImageContent?> GetImageAsync()
        {
            using var context = DBClass.GetContext();
            var image = await context.TBanners.Where(x => x.Id == Id).Select(x => x.Image).SingleOrDefaultAsync();
            return image == null ? null : new ImageContent(image, DetectImageType(image) ?? "application/octet-stream");
        }
        #endregion

        #region HELPER
        private delegate void RowAction(EF.ModelContext context, EF.TBanner row, int userId);

        private Response<Banner> Run(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                return new Banner(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Banner>(msg));
        }
        private async Task<Response<Banner>> RunAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                return new Banner(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Banner>(msg));
        }
        private static EF.TBanner Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TBanner> FilterQuery(FilterAktif aktif)
        {
            return _Querys.Where(x => aktif == FilterAktif.Semua
                    || (aktif == FilterAktif.Aktif && x.IsActive == (int)ActiveStatus.Aktif)
                    || (aktif == FilterAktif.TidakAktif && x.IsActive == (int)ActiveStatus.TidakAktif))
                .OrderBy(x => x.Seq).ThenBy(x => x.Id);
        }
        private static IQueryable<EF.TBanner> ActiveQuery()
        {
            var now = DateTime.Now;
            return _Querys.Where(x => x.IsActive == (int)ActiveStatus.Aktif
                    && (x.StartAt == null || x.StartAt <= now)
                    && (x.EndAt == null || x.EndAt >= now))
                .OrderBy(x => x.Seq).ThenBy(x => x.Id);
        }
        private static EF.TBanner GetRow(EF.ModelContext context, int id)
        {
            return context.TBanners.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Banner"));
        }
        private static async Task<EF.TBanner> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TBanners.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Banner"));
        }
        // Dari magic bytes; null = bukan JPEG / PNG / WebP.
        private static string? DetectImageType(byte[] data)
        {
            if (data.Length >= 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            {
                return "image/png";
            }
            if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            {
                return "image/jpeg";
            }
            if (data.Length >= 12 && Encoding.ASCII.GetString(data, 0, 4) == "RIFF" && Encoding.ASCII.GetString(data, 8, 4) == "WEBP")
            {
                return "image/webp";
            }
            return null;
        }
        private static byte[] ValidateImage(byte[]? image)
        {
            if (image == null || image.Length == 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Gambar Banner"));
            }
            if (image.Length > MAX_IMAGE_BYTES)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FILE_TOO_LARGE, "Gambar Banner", MAX_IMAGE_BYTES / 1024 / 1024));
            }
            if (DetectImageType(image) == null)
            {
                throw new ArgumentException(ExceptionMessage.IMAGE_INVALID);
            }
            return image;
        }
        private static int NextSeq(EF.ModelContext context, int seq)
        {
            return seq > 0 ? seq : (context.TBanners.Max(x => (int?)x.Seq) ?? 0) + 1;
        }
        private static void SetValues(EF.ModelContext context, EF.TBanner row, string? title, string? subtitle, string? buttonText, string? linkUrl,
            int seq, DateTime? startAt, DateTime? endAt, ActiveStatus isActive)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Banner"));
            }
            if (!Enum.IsDefined(isActive))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Aktif"));
            }
            if (startAt != null && endAt != null && endAt <= startAt)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            var link = InformationHelper.ValidateLink(linkUrl, "Link Banner");
            var button = TransaksiHelper.ToNullable(buttonText);
            if (button != null && link == null)
            {
                throw new ArgumentException(ExceptionMessage.BANNER_LINK_REQUIRED);
            }
            row.Title = title.Trim();
            row.Subtitle = TransaksiHelper.ToNullable(subtitle);
            row.ButtonText = button;
            row.LinkUrl = link;
            row.Seq = NextSeq(context, seq);
            row.StartAt = startAt;
            row.EndAt = endAt;
            row.IsActive = (int)isActive;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static EF.TBanner CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var row = new EF.TBanner
            {
                Image = ValidateImage(newrow.Image),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            SetValues(context, row, newrow.Title, newrow.Subtitle, newrow.ButtonText, newrow.LinkUrl, newrow.Seq, newrow.StartAt, newrow.EndAt, newrow.IsActive);
            row.Id = SequenceUtility.GetNextId<EF.TBanner>(context);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.TBanner row, int userId)
        {
            SetValues(context, row, Title, Subtitle, ButtonText, LinkUrl, Seq, StartAt, EndAt, IsActive);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetImage(EF.TBanner row, byte[]? image, int userId)
        {
            row.Image = ValidateImage(image);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
