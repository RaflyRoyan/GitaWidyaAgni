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
    public partial class BorrowItem
    {
        #region SYNC
        public List<BorrowItemUnit> GetUnits(bool refresh)
        {
            if (refresh || UnitList == null)
            {
                UnitList = BorrowItemUnit.GetDataList(Id);
            }
            return UnitList;
        }
        public static List<BorrowItem> GetDataList(int borrowRequestId)
        {
            return FilterQuery(borrowRequestId).ToList().MappingData<BorrowItem, EF.TBorrowItem>();
        }
        public static Response<BorrowItem> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                BorrowRequest.RecalculateTotals(context, BorrowRequest.GetPendingRequest(context, row.BorrowRequestId), userId);
                context.SaveChanges();
                return new BorrowItem(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<BorrowItem>(msg));
        }
        // Hanya jumlah yang diubah.
        public Response<BorrowItem> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetQuantity(context, row, Quantity, userId);
                context.SaveChanges();
                return new BorrowItem(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<BorrowItem>(msg));
        }
        // Aset cukup jumlah: returned kembali + lost hilang (boleh bertahap). Kondisi dicatat sebagai inspeksi pengembalian.
        public Response<BorrowItem> ReturnQuantity(int returned, int lost, ItemCondition condition, string? notes, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = GetRow(context, Id);
                SetReturn(context, row, returned, lost, condition, notes, userId);
                context.SaveChanges();
                transaction.Commit();
                return new BorrowItem(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<BorrowItem>(msg));
        }
        public Response<BorrowItem> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var request = BorrowRequest.GetPendingRequest(context, row.BorrowRequestId);
                var data = new BorrowItem(Reload(row.Id));
                context.TBorrowItems.Remove(row);
                BorrowRequest.RecalculateTotals(context, request, userId);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<BorrowItem>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<BorrowItemUnit>> GetUnitsAsync(bool refresh)
        {
            if (refresh || UnitList == null)
            {
                UnitList = await BorrowItemUnit.GetDataListAsync(Id);
            }
            return UnitList;
        }
        public static async Task<List<BorrowItem>> GetDataListAsync(int borrowRequestId)
        {
            return await (await FilterQuery(borrowRequestId).ToListAsync()).MappingDataAsync<BorrowItem, EF.TBorrowItem>();
        }
        public static async Task<Response<BorrowItem>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                BorrowRequest.RecalculateTotals(context, BorrowRequest.GetPendingRequest(context, row.BorrowRequestId), userId);
                await context.SaveChangesAsync();
                return new BorrowItem(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<BorrowItem>(msg));
        }
        public async Task<Response<BorrowItem>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetQuantity(context, row, Quantity, userId);
                await context.SaveChangesAsync();
                return new BorrowItem(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<BorrowItem>(msg));
        }
        public async Task<Response<BorrowItem>> ReturnQuantityAsync(int returned, int lost, ItemCondition condition, string? notes, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = await GetRowAsync(context, Id);
                SetReturn(context, row, returned, lost, condition, notes, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new BorrowItem(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<BorrowItem>(msg));
        }
        public async Task<Response<BorrowItem>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var request = BorrowRequest.GetPendingRequest(context, row.BorrowRequestId);
                var data = new BorrowItem(Reload(row.Id));
                context.TBorrowItems.Remove(row);
                BorrowRequest.RecalculateTotals(context, request, userId);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<BorrowItem>(msg));
        }
        #endregion

        #region INTERNAL
        // Tidak menghitung ulang total peminjaman; pemanggil (Save / BorrowRequest.CreateRow) yang menghitung.
        internal static EF.TBorrowItem CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var request = BorrowRequest.GetPendingRequest(context, newrow.BorrowRequestId);
            var asset = Asset.GetAsset(context, newrow.AssetId);
            if (context.TBorrowItems.Local.Any(x => x.BorrowRequestId == request.Id && x.AssetId == asset.Id)
                || context.TBorrowItems.Any(x => x.BorrowRequestId == request.Id && x.AssetId == asset.Id))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_DUPLICATE, "aset yang sama dalam satu peminjaman"));
            }
            ValidateQuantity(context, asset, newrow.Quantity);
            var row = new EF.TBorrowItem
            {
                Id = SequenceUtility.GetNextId<EF.TBorrowItem>(context),
                BorrowRequestId = request.Id,
                AssetId = asset.Id,
                Quantity = newrow.Quantity,
                ReturnedQuantity = 0,
                RentalFeePerDay = asset.RentalFeePerDay,
                RentalDays = BorrowRequest.GetRentalDays(request),
                RentalFee = BorrowRequest.GetRentalFee(request, asset.RentalFeePerDay, newrow.Quantity),
                LateFeePerDay = asset.LateFeePerDay,
                LateDays = 0,
                LateFee = 0,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TBorrowItems.Add(row);
            return row;
        }
        // Aset per unit: jumlah kembali, hari dan denda dihitung dari unit-unitnya.
        internal static void RecalculateFromUnits(EF.ModelContext context, EF.TBorrowItem item, int userId)
        {
            context.TBorrowItemUnits.Where(x => x.BorrowItemId == item.Id).Load();
            var units = context.TBorrowItemUnits.Local.Where(x => x.BorrowItemId == item.Id).ToList();
            var done = units.Where(x => x.ReturnStatus != (int)UnitReturnStatus.BelumKembali).ToList();
            item.ReturnedQuantity = done.Count;
            item.LateDays = units.Select(x => x.LateDays).DefaultIfEmpty(0).Max();
            item.LateFee = units.Sum(x => x.LateFee);
            item.ReturnedAt = done.Count == units.Count && units.Count > 0 ? done.Max(x => x.ReturnedAt) : null;
            item.UpdDate = DateTime.Now;
            item.UpdBy = userId;
        }
        #endregion

        #region HELPER
        private static EF.TBorrowItem Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TBorrowItem> FilterQuery(int borrowRequestId)
        {
            return _Querys.Where(x => x.BorrowRequestId == borrowRequestId).OrderBy(x => x.Id);
        }
        private static EF.TBorrowItem GetRow(EF.ModelContext context, int id)
        {
            return context.TBorrowItems.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Item Peminjaman"));
        }
        private static async Task<EF.TBorrowItem> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TBorrowItems.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Item Peminjaman"));
        }
        // Jumlah tidak boleh melebihi stok aset (per unit: unit yang belum dihapus / hilang). Ketersediaan saat itu dicek waktu Checkout.
        private static void ValidateQuantity(EF.ModelContext context, EF.MAsset asset, int quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.AMOUNT_MUST_POSITIVE, "Jumlah Pinjam"));
            }
            var max = asset.TrackUnit == (int)UnitTracking.PerUnit
                ? context.MAssetUnits.Count(x => x.AssetId == asset.Id && x.Status != (int)AssetUnitStatus.Dihapus && x.Status != (int)AssetUnitStatus.Hilang)
                : asset.Quantity;
            if (quantity > max)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.ASSET_QUANTITY_INSUFFICIENT, asset.Name, max));
            }
        }
        private static void SetQuantity(EF.ModelContext context, EF.TBorrowItem row, int quantity, int userId)
        {
            var request = BorrowRequest.GetPendingRequest(context, row.BorrowRequestId);
            ValidateQuantity(context, Asset.GetAsset(context, row.AssetId), quantity);
            row.Quantity = quantity;
            row.RentalDays = BorrowRequest.GetRentalDays(request);
            row.RentalFee = BorrowRequest.GetRentalFee(request, row.RentalFeePerDay, quantity);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            BorrowRequest.RecalculateTotals(context, request, userId);
        }
        private static void SetReturn(EF.ModelContext context, EF.TBorrowItem row, int returned, int lost, ItemCondition condition, string? notes, int userId)
        {
            var request = BorrowRequest.GetOutRequest(context, row.BorrowRequestId);
            var asset = context.MAssets.Single(x => x.Id == row.AssetId);
            if (asset.TrackUnit == (int)UnitTracking.PerUnit)
            {
                throw new ArgumentException(ExceptionMessage.ASSET_PER_UNIT_ONLY);
            }
            if (!Enum.IsDefined(condition))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kondisi Kembali"));
            }
            var count = returned + lost;
            var remaining = row.Quantity - row.ReturnedQuantity;
            if (returned < 0 || lost < 0 || count <= 0 || count > remaining)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.BORROW_RETURN_EXCEEDS, asset.Name, remaining));
            }

            var now = DateTime.Now;
            var lateDays = InventarisHelper.GetLateDays(now, request.ExpectedReturnDate);
            row.ReturnedQuantity += count;
            row.LateDays = Math.Max(row.LateDays, lateDays);
            row.LateFee += lateDays * row.LateFeePerDay * count;
            row.ConditionOnReturn = (int)(returned == 0 ? ItemCondition.Hilang : condition);
            row.ReturnedAt = now;
            row.UpdDate = now;
            row.UpdBy = userId;
            if (returned > 0)
            {
                InventarisHelper.AddMovement(context, asset.Id, null, AssetMovementType.Return, returned, InventarisHelper.REF_BORROW, request.Id,
                    "Kembali " + request.RequestNumber, userId);
            }
            if (lost > 0)
            {
                asset.Quantity -= lost;
                InventarisHelper.AddMovement(context, asset.Id, null, AssetMovementType.Out, lost, InventarisHelper.REF_BORROW, request.Id,
                    "Hilang saat dipinjam " + request.RequestNumber, userId);
            }
            AssetInspection.CreateRow(context, asset, null, row.Id, (ItemCondition)row.ConditionOnReturn, notes, null, false, userId);
            InventarisHelper.RefreshAssetStatus(context, asset, userId);
            BorrowRequest.RecalculateTotals(context, request, userId);
        }
        #endregion
    }
}
