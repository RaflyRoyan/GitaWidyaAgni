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
    public partial class GalleryPhoto
    {
        private const int CAPTION_LENGTH = 255;

        // Data ringan foto (tanpa blob) untuk daftar: ukuran dan cap waktu untuk cache-buster (?v=).
        public record Info(int Id, int GalleryId, string? Caption, int Seq, GalleryPhotoRole IsCover, int FileSize, DateTime Stamp);

        #region SYNC
        public static List<Info> GetInfoList(int galleryId)
        {
            return InfoQuery(galleryId).ToList();
        }
        public static Response<GalleryPhoto> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TGalleryPhotos.Add(row);
                context.SaveChanges();
                return new GalleryPhoto(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<GalleryPhoto>(msg));
        }
        // Mengubah keterangan / urutan (foto diganti = hapus lalu unggah ulang).
        public Response<GalleryPhoto> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new GalleryPhoto(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<GalleryPhoto>(msg));
        }
        public Response<GalleryPhoto> SetCover(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ApplyCover(context, row, userId);
                context.SaveChanges();
                return new GalleryPhoto(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<GalleryPhoto>(msg));
        }
        public Response<GalleryPhoto> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                RemoveRow(context, row, userId);
                context.SaveChanges();
                return new GalleryPhoto(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<GalleryPhoto>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<Info>> GetInfoListAsync(int galleryId)
        {
            return await InfoQuery(galleryId).ToListAsync();
        }
        public static async Task<Response<GalleryPhoto>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TGalleryPhotos.Add(row);
                await context.SaveChangesAsync();
                return new GalleryPhoto(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<GalleryPhoto>(msg));
        }
        public async Task<Response<GalleryPhoto>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new GalleryPhoto(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<GalleryPhoto>(msg));
        }
        public async Task<Response<GalleryPhoto>> SetCoverAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ApplyCover(context, row, userId);
                await context.SaveChangesAsync();
                return new GalleryPhoto(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<GalleryPhoto>(msg));
        }
        public async Task<Response<GalleryPhoto>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                RemoveRow(context, row, userId);
                await context.SaveChangesAsync();
                return new GalleryPhoto(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<GalleryPhoto>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<Info> InfoQuery(int galleryId)
        {
            return _Querys.Where(x => x.GalleryId == galleryId)
                .OrderBy(x => x.Seq).ThenBy(x => x.Id)
                .Select(x => new Info(x.Id, x.GalleryId, x.Caption, x.Seq, (GalleryPhotoRole)x.IsCover, x.Photo.Length, x.UpdDate ?? x.InsDate));
        }
        private static EF.TGalleryPhoto GetRow(EF.ModelContext context, int id)
        {
            return context.TGalleryPhotos.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Foto Galeri"));
        }
        private static async Task<EF.TGalleryPhoto> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TGalleryPhotos.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Foto Galeri"));
        }
        // Gambar dinormalisasi dulu sebelum id diambil, supaya upload yang ditolak tidak memakan nomor m_sequence.
        private static EF.TGalleryPhoto CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            Gallery.GetGallery(context, newrow.GalleryId);
            var image = ImageUtility.Normalize(newrow.Data);
            var isFirst = !context.TGalleryPhotos.Any(x => x.GalleryId == newrow.GalleryId);

            return new EF.TGalleryPhoto
            {
                Id = SequenceUtility.GetNextId<EF.TGalleryPhoto>(context),
                GalleryId = newrow.GalleryId,
                Photo = image.Data,
                Caption = TransaksiHelper.Truncate(newrow.Caption, CAPTION_LENGTH),
                Seq = newrow.Seq > 0 ? newrow.Seq : ValueUtility.GetNextId<EF.TGalleryPhoto>(context, x => x.Seq, x => x.GalleryId == newrow.GalleryId),
                IsCover = (int)(isFirst ? GalleryPhotoRole.Sampul : GalleryPhotoRole.Biasa),
                InsDate = DateTime.Now,
                InsBy = userId
            };
        }
        private void SetRow(EF.ModelContext context, EF.TGalleryPhoto row, int userId)
        {
            row.Caption = TransaksiHelper.Truncate(Caption, CAPTION_LENGTH);
            row.Seq = Seq > 0 ? Seq : ValueUtility.GetNextId<EF.TGalleryPhoto>(context, x => x.Seq, x => x.GalleryId == row.GalleryId);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void ApplyCover(EF.ModelContext context, EF.TGalleryPhoto row, int userId)
        {
            foreach (var other in context.TGalleryPhotos.Where(x => x.GalleryId == row.GalleryId && x.Id != row.Id && x.IsCover == (int)GalleryPhotoRole.Sampul).ToList())
            {
                other.IsCover = (int)GalleryPhotoRole.Biasa;
                other.UpdDate = DateTime.Now;
                other.UpdBy = userId;
            }
            row.IsCover = (int)GalleryPhotoRole.Sampul;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        // Foto terakhir galeri terbit tidak boleh dihapus; sampul yang dihapus pindah ke foto berikutnya.
        private static void RemoveRow(EF.ModelContext context, EF.TGalleryPhoto row, int userId)
        {
            var gallery = Gallery.GetGallery(context, row.GalleryId);
            var next = context.TGalleryPhotos.Where(x => x.GalleryId == row.GalleryId && x.Id != row.Id).OrderBy(x => x.Seq).ThenBy(x => x.Id).FirstOrDefault();
            if (next == null && gallery.Status == (int)PublicationStatus.Published)
            {
                throw new ArgumentException(ExceptionMessage.GALLERY_EMPTY);
            }
            if (row.IsCover == (int)GalleryPhotoRole.Sampul && next != null)
            {
                next.IsCover = (int)GalleryPhotoRole.Sampul;
                next.UpdDate = DateTime.Now;
                next.UpdBy = userId;
            }
            context.TGalleryPhotos.Remove(row);
        }
        #endregion
    }
}
