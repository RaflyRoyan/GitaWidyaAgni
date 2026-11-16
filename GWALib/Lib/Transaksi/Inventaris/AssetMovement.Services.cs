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
    public partial class AssetMovement
    {
        #region SYNC
        public static List<AssetMovement> GetDataList(int? assetId = null, int? assetUnitId = null, AssetMovementType? movementType = null,
            DateTime? from = null, DateTime? to = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(assetId, assetUnitId, movementType, from, to);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<AssetMovement, EF.TAssetMovement>();
        }
        public static Response<AssetMovement> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateManualRow(context, newrow, userId);
                context.SaveChanges();
                return new AssetMovement(_Querys.Single(x => x.Id == row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AssetMovement>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<AssetMovement>> GetDataListAsync(int? assetId = null, int? assetUnitId = null, AssetMovementType? movementType = null,
            DateTime? from = null, DateTime? to = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(assetId, assetUnitId, movementType, from, to);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<AssetMovement, EF.TAssetMovement>();
        }
        public static async Task<Response<AssetMovement>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateManualRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new AssetMovement(_Querys.Single(x => x.Id == row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AssetMovement>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TAssetMovement> FilterQuery(int? assetId, int? assetUnitId, AssetMovementType? movementType, DateTime? from, DateTime? to)
        {
            return _Querys.Where(x => (assetId == null || x.AssetId == assetId)
                    && (assetUnitId == null || x.AssetUnitId == assetUnitId)
                    && (movementType == null || x.MovementType == (int)movementType)
                    && (from == null || x.MovementDate >= from)
                    && (to == null || x.MovementDate <= to))
                .OrderByDescending(x => x.MovementDate).ThenByDescending(x => x.Id);
        }
        private static EF.TAssetMovement CreateManualRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var asset = Asset.GetAsset(context, newrow.AssetId);
            if (asset.TrackUnit == (int)UnitTracking.PerUnit)
            {
                throw new ArgumentException(ExceptionMessage.ASSET_PER_UNIT_ONLY);
            }
            var delta = newrow.MovementType switch
            {
                AssetMovementType.In => newrow.Quantity,
                AssetMovementType.Out => -newrow.Quantity,
                AssetMovementType.Adjustment => newrow.Quantity,
                _ => throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Mutasi"))
            };
            if (newrow.Quantity == 0 || (newrow.MovementType != AssetMovementType.Adjustment && newrow.Quantity < 0))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jumlah Mutasi"));
            }
            var outstanding = InventarisHelper.GetOutstanding(context, asset.Id);
            if (asset.Quantity + delta < outstanding)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.ASSET_QUANTITY_INSUFFICIENT, asset.Name, asset.Quantity - outstanding));
            }
            asset.Quantity += delta;
            asset.UpdDate = DateTime.Now;
            asset.UpdBy = userId;
            var row = InventarisHelper.AddMovement(context, asset.Id, null, newrow.MovementType, newrow.Quantity, InventarisHelper.REF_MANUAL, null, newrow.Notes, userId);
            InventarisHelper.RefreshAssetStatus(context, asset, userId);
            return row;
        }
        #endregion
    }
}
