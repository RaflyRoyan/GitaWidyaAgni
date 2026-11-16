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

namespace GWALib.Lib.Transaksi.Inventaris
{
    public partial class BorrowItemUnit
    {
        #region SYNC
        public static List<BorrowItemUnit> GetDataList(int borrowItemId)
        {
            return FilterQuery(borrowItemId).ToList().MappingData<BorrowItemUnit, EF.TBorrowItemUnit>();
        }
        // lost = unit hilang; selain itu condition Baik → Kembali, Rusak → KembaliRusak (kondisi unit hanya Baik / Rusak Ringan / Rusak Berat).
        public static Response<BorrowItemUnit> ReturnUnit(int borrowRequestId, string unitCode, ItemCondition condition, bool lost, string? notes, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = SetReturn(context, borrowRequestId, unitCode, condition, lost, notes, userId);
                context.SaveChanges();
                transaction.Commit();
                return new BorrowItemUnit(_Querys.Single(x => x.Id == row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<BorrowItemUnit>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<BorrowItemUnit>> GetDataListAsync(int borrowItemId)
        {
            return await (await FilterQuery(borrowItemId).ToListAsync()).MappingDataAsync<BorrowItemUnit, EF.TBorrowItemUnit>();
        }
        public static async Task<Response<BorrowItemUnit>> ReturnUnitAsync(int borrowRequestId, string unitCode, ItemCondition condition, bool lost, string? notes, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = SetReturn(context, borrowRequestId, unitCode, condition, lost, notes, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new BorrowItemUnit(_Querys.Single(x => x.Id == row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<BorrowItemUnit>(msg));
        }
        #endregion

        #region INTERNAL
        internal static EF.TBorrowItemUnit CreateRow(EF.ModelContext context, EF.TBorrowRequest request, EF.TBorrowItem item, EF.MAsset asset, EF.MAssetUnit unit,
            DateTime takenAt, int userId)
        {
            if (unit.Status != (int)AssetUnitStatus.Tersedia)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.ASSET_UNIT_NOT_AVAILABLE, unit.UnitCode));
            }
            var row = new EF.TBorrowItemUnit
            {
                Id = SequenceUtility.GetNextId<EF.TBorrowItemUnit>(context),
                BorrowItemId = item.Id,
                AssetUnitId = unit.Id,
                TakenAt = takenAt,
                ConditionOut = unit.UnitCondition,
                ReturnStatus = (int)UnitReturnStatus.BelumKembali,
                LateDays = 0,
                LateFee = 0,
                InsDate = takenAt,
                InsBy = userId
            };
            context.TBorrowItemUnits.Add(row);
            unit.Status = (int)AssetUnitStatus.Dipinjam;
            unit.UpdDate = takenAt;
            unit.UpdBy = userId;
            InventarisHelper.AddMovement(context, asset.Id, unit.Id, AssetMovementType.Borrow, 1, InventarisHelper.REF_BORROW, request.Id,
                "Dipinjam " + request.RequestNumber, userId);
            return row;
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TBorrowItemUnit> FilterQuery(int borrowItemId)
        {
            return _Querys.Where(x => x.BorrowItemId == borrowItemId).OrderBy(x => x.AssetUnit.UnitCode);
        }
        private static EF.TBorrowItemUnit SetReturn(EF.ModelContext context, int borrowRequestId, string unitCode, ItemCondition condition, bool lost, string? notes, int userId)
        {
            var request = BorrowRequest.GetOutRequest(context, borrowRequestId);
            var code = unitCode?.Trim().ToUpperInvariant() ?? string.Empty;
            var row = context.TBorrowItemUnits.Include(x => x.BorrowItem).Include(x => x.AssetUnit)
                .SingleOrDefault(x => x.AssetUnit.UnitCode == code && x.BorrowItem.BorrowRequestId == request.Id && x.ReturnStatus == (int)UnitReturnStatus.BelumKembali)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.BORROW_UNIT_NOT_BORROWED, code));
            if (!lost && (condition == ItemCondition.Hilang || !Enum.IsDefined(condition)))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kondisi Kembali"));
            }

            var now = DateTime.Now;
            var item = row.BorrowItem;
            var unit = row.AssetUnit;
            row.ReturnedAt = now;
            row.ConditionIn = lost ? null : (int)condition;
            row.ReturnStatus = (int)(lost ? UnitReturnStatus.Hilang : condition == ItemCondition.Baik ? UnitReturnStatus.Kembali : UnitReturnStatus.KembaliRusak);
            row.LateDays = InventarisHelper.GetLateDays(now, request.ExpectedReturnDate);
            row.LateFee = row.LateDays * item.LateFeePerDay;
            row.Notes = TransaksiHelper.Truncate(notes, 255);
            row.UpdDate = now;
            row.UpdBy = userId;

            unit.Status = (int)(lost ? AssetUnitStatus.Hilang : AssetUnitStatus.Tersedia);
            if (!lost)
            {
                unit.UnitCondition = (int)condition;
                InventarisHelper.AddMovement(context, unit.AssetId, unit.Id, AssetMovementType.Return, 1, InventarisHelper.REF_BORROW, request.Id,
                    "Kembali " + request.RequestNumber, userId);
            }
            unit.UpdDate = now;
            unit.UpdBy = userId;

            var asset = context.MAssets.Single(x => x.Id == unit.AssetId);
            AssetInspection.CreateRow(context, asset, unit, item.Id, lost ? ItemCondition.Hilang : condition, notes, null, false, userId);
            BorrowItem.RecalculateFromUnits(context, item, userId);
            InventarisHelper.RefreshAssetStatus(context, asset, userId);
            BorrowRequest.RecalculateTotals(context, request, userId);
            return row;
        }
        #endregion
    }
}
