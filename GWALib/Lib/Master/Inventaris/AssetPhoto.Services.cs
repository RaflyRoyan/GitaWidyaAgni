using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Inventaris
{
    public partial class AssetPhoto
    {
        // Data ringan foto (tanpa blob) untuk daftar: ada/tidaknya foto, ukuran dan cap waktu untuk cache-buster (?v=).
        public record Info(int AssetId, int SeqPhoto, int FileSize, DateTime Stamp);

        #region SYNC
        public static List<Info> GetInfoList(int? assetId = null)
        {
            return InfoQuery(assetId).ToList();
        }
        public static List<AssetPhoto> GetDataList(int assetId)
        {
            return FilterQuery(assetId).ToList().MappingData<AssetPhoto, EF.MAssetPhoto>();
        }
        public static Response<AssetPhoto> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MAssetPhotos.Add(row);
                context.SaveChanges();
                return new AssetPhoto(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AssetPhoto>(msg));
        }
        public Response<AssetPhoto> Update(byte[] photo, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, AssetId, SeqPhoto);
                SetRow(row, photo, userId);
                context.SaveChanges();
                return new AssetPhoto(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AssetPhoto>(msg));
        }
        public Response<AssetPhoto> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, AssetId, SeqPhoto);
                context.MAssetPhotos.Remove(row);
                context.SaveChanges();
                return new AssetPhoto(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<AssetPhoto>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<Info>> GetInfoListAsync(int? assetId = null)
        {
            return await InfoQuery(assetId).ToListAsync();
        }
        public static async Task<List<AssetPhoto>> GetDataListAsync(int assetId)
        {
            return await (await FilterQuery(assetId).ToListAsync()).MappingDataAsync<AssetPhoto, EF.MAssetPhoto>();
        }
        public static async Task<Response<AssetPhoto>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MAssetPhotos.Add(row);
                await context.SaveChangesAsync();
                return new AssetPhoto(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AssetPhoto>(msg));
        }
        public async Task<Response<AssetPhoto>> UpdateAsync(byte[] photo, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, AssetId, SeqPhoto);
                SetRow(row, photo, userId);
                await context.SaveChangesAsync();
                return new AssetPhoto(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AssetPhoto>(msg));
        }
        public async Task<Response<AssetPhoto>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, AssetId, SeqPhoto);
                context.MAssetPhotos.Remove(row);
                await context.SaveChangesAsync();
                return new AssetPhoto(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<AssetPhoto>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<Info> InfoQuery(int? assetId)
        {
            return _Querys.Where(x => assetId == null || x.AssetId == assetId)
                .OrderBy(x => x.AssetId).ThenBy(x => x.SeqPhoto)
                .Select(x => new Info(x.AssetId, x.SeqPhoto, x.Photo.Length, x.UpdDate ?? x.InsDate));
        }
        private static IQueryable<EF.MAssetPhoto> FilterQuery(int assetId)
        {
            return _Querys.Where(x => x.AssetId == assetId).OrderBy(x => x.SeqPhoto);
        }
        private static EF.MAssetPhoto GetRow(EF.ModelContext context, int assetId, int seqPhoto)
        {
            return context.MAssetPhotos.SingleOrDefault(x => x.AssetId == assetId && x.SeqPhoto == seqPhoto)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Foto Aset"));
        }
        private static async Task<EF.MAssetPhoto> GetRowAsync(EF.ModelContext context, int assetId, int seqPhoto)
        {
            return await context.MAssetPhotos.SingleOrDefaultAsync(x => x.AssetId == assetId && x.SeqPhoto == seqPhoto)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Foto Aset"));
        }
        internal static EF.MAssetPhoto CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            Asset.GetAsset(context, newrow.AssetId);
            var image = ImageUtility.Normalize(newrow.Data);

            return new EF.MAssetPhoto
            {
                AssetId = newrow.AssetId,
                SeqPhoto = ValueUtility.GetNextId<EF.MAssetPhoto>(context, x => x.SeqPhoto, x => x.AssetId == newrow.AssetId),
                Photo = image.Data,
                InsDate = DateTime.Now,
                InsBy = userId
            };
        }
        private static void SetRow(EF.MAssetPhoto row, byte[]? data, int userId)
        {
            var image = ImageUtility.Normalize(data);

            row.Photo = image.Data;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
