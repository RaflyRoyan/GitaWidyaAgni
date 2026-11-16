using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Inventaris;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Inventaris
{
    public partial class AssetInspection
    {
        public const int MAX_PHOTO_BYTES = 5 * 1024 * 1024;

        #region SYNC
        public byte[]? GetPhoto()
        {
            using var context = DBClass.GetContext();
            return context.TAssetInspections.Where(x => x.Id == Id).Select(x => x.Photo).SingleOrDefault();
        }
        public static List<AssetInspection> GetDataList(int? assetId = null, int? assetUnitId = null, int? borrowItemId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(assetId, assetUnitId, borrowItemId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<AssetInspection, EF.TAssetInspection>();
        }
        public static Response<AssetInspection> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateManualRow(context, newrow, userId);
                context.SaveChanges();
                return new AssetInspection(_Querys.Single(x => x.Id == row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AssetInspection>(msg));
        }
        // Hanya inspeksi manual; kondisi aset / unit tidak dikembalikan.
        public Response<AssetInspection> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetManualRow(context, Id);
                var data = new AssetInspection(_Querys.Single(x => x.Id == row.Id));
                context.TAssetInspections.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<AssetInspection>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<byte[]?> GetPhotoAsync()
        {
            using var context = DBClass.GetContext();
            return await context.TAssetInspections.Where(x => x.Id == Id).Select(x => x.Photo).SingleOrDefaultAsync();
        }
        public static async Task<List<AssetInspection>> GetDataListAsync(int? assetId = null, int? assetUnitId = null, int? borrowItemId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(assetId, assetUnitId, borrowItemId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<AssetInspection, EF.TAssetInspection>();
        }
        public static async Task<Response<AssetInspection>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateManualRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new AssetInspection(_Querys.Single(x => x.Id == row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AssetInspection>(msg));
        }
        public async Task<Response<AssetInspection>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetManualRow(context, Id);
                var data = new AssetInspection(_Querys.Single(x => x.Id == row.Id));
                context.TAssetInspections.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<AssetInspection>(msg));
        }
        #endregion

        #region INTERNAL
        // applyCondition = perbarui kondisi aset / unit (inspeksi manual); pengembalian sudah mengatur unit sendiri.
        internal static EF.TAssetInspection CreateRow(EF.ModelContext context, EF.MAsset asset, EF.MAssetUnit? unit, int? borrowItemId, ItemCondition condition,
            string? notes, byte[]? photo, bool applyCondition, int userId)
        {
            if (!Enum.IsDefined(condition))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kondisi"));
            }
            if (photo != null && photo.Length > MAX_PHOTO_BYTES)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FILE_TOO_LARGE, "Foto Inspeksi", MAX_PHOTO_BYTES / 1024 / 1024));
            }
            var now = DateTime.Now;
            var row = new EF.TAssetInspection
            {
                Id = SequenceUtility.GetNextId<EF.TAssetInspection>(context),
                AssetId = asset.Id,
                AssetUnitId = unit?.Id,
                BorrowItemId = borrowItemId,
                InspectedBy = userId,
                InspectedAt = now,
                ItemCondition = (int)condition,
                Notes = TransaksiHelper.Truncate(notes, 255),
                Photo = photo == null || photo.Length == 0 ? null : photo,
                InsDate = now,
                InsBy = userId
            };
            context.TAssetInspections.Add(row);
            if (applyCondition)
            {
                if (unit != null)
                {
                    if (condition == ItemCondition.Hilang)
                    {
                        if (unit.Status == (int)AssetUnitStatus.Dipinjam)
                        {
                            throw new ArgumentException(ExceptionMessage.ASSET_UNIT_IN_USE);
                        }
                        unit.Status = (int)AssetUnitStatus.Hilang;
                    }
                    else
                    {
                        unit.UnitCondition = (int)condition;
                    }
                    unit.UpdDate = now;
                    unit.UpdBy = userId;
                }
                else
                {
                    asset.AssetCondition = (int)condition;
                    asset.UpdDate = now;
                    asset.UpdBy = userId;
                }
            }
            return row;
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TAssetInspection> FilterQuery(int? assetId, int? assetUnitId, int? borrowItemId)
        {
            return _Querys.Where(x => (assetId == null || x.AssetId == assetId)
                    && (assetUnitId == null || x.AssetUnitId == assetUnitId)
                    && (borrowItemId == null || x.BorrowItemId == borrowItemId))
                .OrderByDescending(x => x.InspectedAt).ThenByDescending(x => x.Id);
        }
        private static EF.TAssetInspection GetManualRow(EF.ModelContext context, int id)
        {
            var row = context.TAssetInspections.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Inspeksi Aset"));
            if (row.BorrowItemId != null)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.STATUS_ACTION_INVALID, "menghapus inspeksi", "inspeksi pengembalian"));
            }
            return row;
        }
        private static EF.TAssetInspection CreateManualRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var asset = Asset.GetAsset(context, newrow.AssetId);
            EF.MAssetUnit? unit = null;
            if (asset.TrackUnit == (int)UnitTracking.PerUnit)
            {
                if (!(newrow.AssetUnitId > 0))
                {
                    throw new ArgumentException(ExceptionMessage.ASSET_PER_UNIT_ONLY);
                }
                unit = context.MAssetUnits.SingleOrDefault(x => x.Id == newrow.AssetUnitId && x.AssetId == asset.Id && x.Status != (int)AssetUnitStatus.Dihapus)
                    ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Unit Aset"));
            }
            var row = CreateRow(context, asset, unit, null, newrow.ItemCondition, newrow.Notes, newrow.Photo, true, userId);
            row.InspectedAt = newrow.InspectedAt ?? row.InspectedAt;
            return row;
        }
        #endregion
    }
}
