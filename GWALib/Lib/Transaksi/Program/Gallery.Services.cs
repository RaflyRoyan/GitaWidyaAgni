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

namespace GWALib.Lib.Transaksi.Program
{
    public partial class Gallery
    {
        #region SYNC
        public List<GalleryPhoto.Info> GetPhotos(bool refresh)
        {
            if (refresh || PhotoList == null)
            {
                PhotoList = GalleryPhoto.GetInfoList(Id);
            }
            return PhotoList;
        }
        public static List<Gallery> GetDataList(PublicationStatus? status = null, int? activityId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, activityId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Gallery, EF.TGallery>();
        }
        // Untuk landing page: galeri publik yang sudah terbit, yang disorot lebih dulu.
        public static List<Gallery> GetPublicList(int jmldata = 20)
        {
            return PublicQuery().Take(jmldata).ToList().MappingData<Gallery, EF.TGallery>();
        }
        public static Response<Gallery> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TGalleries.Add(row);
                context.SaveChanges();
                return new Gallery(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Gallery>(msg));
        }
        public Response<Gallery> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new Gallery(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Gallery>(msg));
        }
        public Response<Gallery> Publish(string user)
        {
            return SetStatus(PublicationStatus.Published, user);
        }
        public Response<Gallery> Archive(string user)
        {
            return SetStatus(PublicationStatus.Archived, user);
        }
        public Response<Gallery> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((PublicationStatus)row.Status, "menghapus galeri", PublicationStatus.Draft);
                RemovePhotos(context, row.Id);
                context.TGalleries.Remove(row);
                context.SaveChanges();
                return new Gallery(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Gallery>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<GalleryPhoto.Info>> GetPhotosAsync(bool refresh)
        {
            if (refresh || PhotoList == null)
            {
                PhotoList = await GalleryPhoto.GetInfoListAsync(Id);
            }
            return PhotoList;
        }
        public static async Task<List<Gallery>> GetDataListAsync(PublicationStatus? status = null, int? activityId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, activityId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Gallery, EF.TGallery>();
        }
        public static async Task<List<Gallery>> GetPublicListAsync(int jmldata = 20)
        {
            return await (await PublicQuery().Take(jmldata).ToListAsync()).MappingDataAsync<Gallery, EF.TGallery>();
        }
        public static async Task<Response<Gallery>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TGalleries.Add(row);
                await context.SaveChangesAsync();
                return new Gallery(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Gallery>(msg));
        }
        public async Task<Response<Gallery>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new Gallery(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Gallery>(msg));
        }
        public async Task<Response<Gallery>> PublishAsync(string user)
        {
            return await SetStatusAsync(PublicationStatus.Published, user);
        }
        public async Task<Response<Gallery>> ArchiveAsync(string user)
        {
            return await SetStatusAsync(PublicationStatus.Archived, user);
        }
        public async Task<Response<Gallery>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                TransaksiHelper.ValidateStatus((PublicationStatus)row.Status, "menghapus galeri", PublicationStatus.Draft);
                RemovePhotos(context, row.Id);
                context.TGalleries.Remove(row);
                await context.SaveChangesAsync();
                return new Gallery(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Gallery>(msg));
        }
        #endregion

        #region INTERNAL
        internal static EF.TGallery GetGallery(EF.ModelContext context, int galleryId)
        {
            return context.TGalleries.SingleOrDefault(x => x.Id == galleryId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Galeri"));
        }
        #endregion

        #region HELPER
        // Foto dihapus bersama galeri dalam SaveChanges yang sama; entity stub supaya blob tidak ikut dimuat.
        private static void RemovePhotos(EF.ModelContext context, int galleryId)
        {
            var ids = context.TGalleryPhotos.Where(x => x.GalleryId == galleryId).Select(x => x.Id).ToList();
            context.TGalleryPhotos.RemoveRange(ids.Select(id => new EF.TGalleryPhoto { Id = id, GalleryId = galleryId }));
        }
        private Response<Gallery> SetStatus(PublicationStatus target, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ApplyStatus(context, row, target, userId);
                context.SaveChanges();
                return new Gallery(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Gallery>(msg));
        }
        private async Task<Response<Gallery>> SetStatusAsync(PublicationStatus target, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ApplyStatus(context, row, target, userId);
                await context.SaveChangesAsync();
                return new Gallery(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Gallery>(msg));
        }
        private static IQueryable<EF.TGallery> FilterQuery(PublicationStatus? status, int? activityId)
        {
            return _Querys.Where(x => (status == null || x.Status == (int)status)
                    && (activityId == null || x.ActivityId == activityId))
                .OrderByDescending(x => x.EventDate).ThenByDescending(x => x.Id);
        }
        private static IQueryable<EF.TGallery> PublicQuery()
        {
            return _Querys.Where(x => x.IsPublic == (int)PublicVisibility.Public && x.Status == (int)PublicationStatus.Published)
                .OrderByDescending(x => x.IsFeatured).ThenByDescending(x => x.EventDate).ThenByDescending(x => x.PublishedAt);
        }
        private static EF.TGallery GetRow(EF.ModelContext context, int id)
        {
            return GetGallery(context, id);
        }
        private static async Task<EF.TGallery> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TGalleries.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Galeri"));
        }
        private static void ValidateData(string title, PublicVisibility isPublic, FeaturedStatus isFeatured)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Galeri"));
            }
            if (!Enum.IsDefined(isPublic) || !Enum.IsDefined(isFeatured))
            {
                throw new ArgumentException(ExceptionMessage.DATA_INVALID);
            }
            if (isFeatured == FeaturedStatus.Disorot && isPublic != PublicVisibility.Public)
            {
                throw new ArgumentException(ExceptionMessage.FEATURED_NOT_PUBLIC);
            }
        }
        private static EF.TGallery CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var title = newrow.Title?.Trim() ?? string.Empty;
            var activityId = newrow.ActivityId > 0 ? newrow.ActivityId : null;
            var eventDate = newrow.EventDate;
            if (activityId != null)
            {
                var activity = Activity.GetActivity(context, activityId.Value);
                eventDate ??= DateOnly.FromDateTime(activity.StartAt);
            }
            ValidateData(title, newrow.IsPublic, newrow.IsFeatured);

            var row = new EF.TGallery
            {
                Id = SequenceUtility.GetNextId<EF.TGallery>(context),
                ActivityId = activityId,
                Title = title,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                EventDate = eventDate,
                IsPublic = (int)newrow.IsPublic,
                IsFeatured = (int)newrow.IsFeatured,
                Status = (int)PublicationStatus.Draft,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.TGallery row, int userId)
        {
            var title = Title?.Trim() ?? string.Empty;
            var activityId = ActivityId > 0 ? ActivityId : null;
            if (activityId != null && activityId != row.ActivityId)
            {
                Activity.GetActivity(context, activityId.Value);
            }
            ValidateData(title, IsPublic, IsFeatured);

            row.ActivityId = activityId;
            row.Title = title;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.EventDate = EventDate;
            row.IsPublic = (int)IsPublic;
            row.IsFeatured = (int)IsFeatured;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void ApplyStatus(EF.ModelContext context, EF.TGallery row, PublicationStatus target, int userId)
        {
            var current = (PublicationStatus)row.Status;
            if (target == PublicationStatus.Published)
            {
                TransaksiHelper.ValidateStatus(current, "menerbitkan galeri", PublicationStatus.Draft, PublicationStatus.Archived);
                if (!context.TGalleryPhotos.Any(x => x.GalleryId == row.Id))
                {
                    throw new ArgumentException(ExceptionMessage.GALLERY_EMPTY);
                }
                row.PublishedAt ??= DateTime.Now;
            }
            else if (target == PublicationStatus.Archived)
            {
                TransaksiHelper.ValidateStatus(current, "mengarsipkan galeri", PublicationStatus.Published);
            }
            else
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Galeri"));
            }
            row.Status = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
