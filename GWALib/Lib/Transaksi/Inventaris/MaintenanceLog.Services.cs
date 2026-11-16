using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Inventaris;
using GWALib.Lib.Transaksi.Keuangan;
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
    public partial class MaintenanceLog
    {
        #region SYNC
        public static List<MaintenanceLog> GetDataList(int? assetId = null, int? assetUnitId = null, MaintenanceStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(assetId, assetUnitId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<MaintenanceLog, EF.TMaintenanceLog>();
        }
        public static Response<MaintenanceLog> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new MaintenanceLog(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<MaintenanceLog>(msg));
        }
        public Response<MaintenanceLog> Update(string user)
        {
            return Run(SetRow, user);
        }
        public Response<MaintenanceLog> Start(string user)
        {
            return Run((context, row, userId) => SetStart(context, row, userId), user);
        }
        // condition opsional (kondisi setelah dirawat); akun + kategori pengeluaran mencatat biaya.
        public Response<MaintenanceLog> Finish(ItemCondition? condition, int? accountId, int? categoryId, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = GetRow(context, Id);
                SetFinish(context, row, condition, accountId, categoryId, userId);
                context.SaveChanges();
                transaction.Commit();
                return new MaintenanceLog(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MaintenanceLog>(msg));
        }
        public Response<MaintenanceLog> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((MaintenanceStatus)row.Status, "menghapus perawatan", MaintenanceStatus.Scheduled);
                var data = new MaintenanceLog(Reload(row.Id));
                context.TMaintenanceLogs.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<MaintenanceLog>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<MaintenanceLog>> GetDataListAsync(int? assetId = null, int? assetUnitId = null, MaintenanceStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(assetId, assetUnitId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<MaintenanceLog, EF.TMaintenanceLog>();
        }
        public static async Task<Response<MaintenanceLog>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new MaintenanceLog(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<MaintenanceLog>(msg));
        }
        public Task<Response<MaintenanceLog>> UpdateAsync(string user)
        {
            return RunAsync(SetRow, user);
        }
        public Task<Response<MaintenanceLog>> StartAsync(string user)
        {
            return RunAsync((context, row, userId) => SetStart(context, row, userId), user);
        }
        public async Task<Response<MaintenanceLog>> FinishAsync(ItemCondition? condition, int? accountId, int? categoryId, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = await GetRowAsync(context, Id);
                SetFinish(context, row, condition, accountId, categoryId, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new MaintenanceLog(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MaintenanceLog>(msg));
        }
        public async Task<Response<MaintenanceLog>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                TransaksiHelper.ValidateStatus((MaintenanceStatus)row.Status, "menghapus perawatan", MaintenanceStatus.Scheduled);
                var data = new MaintenanceLog(Reload(row.Id));
                context.TMaintenanceLogs.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<MaintenanceLog>(msg));
        }
        #endregion

        #region HELPER
        private delegate void RowAction(EF.ModelContext context, EF.TMaintenanceLog row, int userId);

        private Response<MaintenanceLog> Run(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                return new MaintenanceLog(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MaintenanceLog>(msg));
        }
        private async Task<Response<MaintenanceLog>> RunAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                return new MaintenanceLog(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MaintenanceLog>(msg));
        }
        private static EF.TMaintenanceLog Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TMaintenanceLog> FilterQuery(int? assetId, int? assetUnitId, MaintenanceStatus? status)
        {
            return _Querys.Where(x => (assetId == null || x.AssetId == assetId)
                    && (assetUnitId == null || x.AssetUnitId == assetUnitId)
                    && (status == null || x.Status == (int)status))
                .OrderByDescending(x => x.MaintenanceDate).ThenByDescending(x => x.Id);
        }
        private static EF.TMaintenanceLog GetRow(EF.ModelContext context, int id)
        {
            return context.TMaintenanceLogs.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Perawatan Aset"));
        }
        private static async Task<EF.TMaintenanceLog> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TMaintenanceLogs.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Perawatan Aset"));
        }
        private static string ValidateData(DateOnly maintenanceDate, string? description, decimal cost)
        {
            if (maintenanceDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Perawatan"));
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Uraian Perawatan"));
            }
            if (cost < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Biaya"));
            }
            return description.Trim();
        }
        private static EF.TMaintenanceLog CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var asset = Asset.GetAsset(context, newrow.AssetId);
            int? unitId = null;
            if (asset.TrackUnit == (int)UnitTracking.PerUnit)
            {
                if (!(newrow.AssetUnitId > 0))
                {
                    throw new ArgumentException(ExceptionMessage.ASSET_PER_UNIT_ONLY);
                }
                unitId = context.MAssetUnits.Where(x => x.Id == newrow.AssetUnitId && x.AssetId == asset.Id && x.Status != (int)AssetUnitStatus.Dihapus)
                    .Select(x => (int?)x.Id).SingleOrDefault()
                    ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Unit Aset"));
            }
            var row = new EF.TMaintenanceLog
            {
                Id = SequenceUtility.GetNextId<EF.TMaintenanceLog>(context),
                AssetId = asset.Id,
                AssetUnitId = unitId,
                MaintenanceDate = newrow.MaintenanceDate,
                Description = ValidateData(newrow.MaintenanceDate, newrow.Description, newrow.Cost),
                Vendor = TransaksiHelper.Truncate(newrow.Vendor, 150),
                Cost = newrow.Cost,
                Status = (int)MaintenanceStatus.Scheduled,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TMaintenanceLogs.Add(row);
            return row;
        }
        // Aset / unit tetap.
        private void SetRow(EF.ModelContext context, EF.TMaintenanceLog row, int userId)
        {
            TransaksiHelper.ValidateStatus((MaintenanceStatus)row.Status, "mengubah perawatan", MaintenanceStatus.Scheduled, MaintenanceStatus.InProgress);
            row.Description = ValidateData(MaintenanceDate, Description, Cost);
            row.MaintenanceDate = MaintenanceDate;
            row.Vendor = TransaksiHelper.Truncate(Vendor, 150);
            row.Cost = Cost;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetStart(EF.ModelContext context, EF.TMaintenanceLog row, int userId)
        {
            TransaksiHelper.ValidateStatus((MaintenanceStatus)row.Status, "memulai perawatan", MaintenanceStatus.Scheduled);
            var asset = Asset.GetAsset(context, row.AssetId);
            var now = DateTime.Now;
            if (row.AssetUnitId != null)
            {
                var unit = context.MAssetUnits.Single(x => x.Id == row.AssetUnitId);
                if (unit.Status != (int)AssetUnitStatus.Tersedia)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.ASSET_UNIT_NOT_AVAILABLE, unit.UnitCode));
                }
                unit.Status = (int)AssetUnitStatus.Perawatan;
                unit.UpdDate = now;
                unit.UpdBy = userId;
                InventarisHelper.RefreshAssetStatus(context, asset, userId);
            }
            else
            {
                if (InventarisHelper.GetOutstanding(context, asset.Id) > 0)
                {
                    throw new ArgumentException(ExceptionMessage.MAINTENANCE_ASSET_BORROWED);
                }
                asset.Status = (int)AssetStatus.Maintenance;
                asset.UpdDate = now;
                asset.UpdBy = userId;
            }
            row.Status = (int)MaintenanceStatus.InProgress;
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        private static void SetFinish(EF.ModelContext context, EF.TMaintenanceLog row, ItemCondition? condition, int? accountId, int? categoryId, int userId)
        {
            TransaksiHelper.ValidateStatus((MaintenanceStatus)row.Status, "menyelesaikan perawatan", MaintenanceStatus.Scheduled, MaintenanceStatus.InProgress);
            if (condition != null && (!Enum.IsDefined(condition.Value) || condition == ItemCondition.Hilang))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kondisi Setelah Perawatan"));
            }
            var hasAccount = accountId > 0;
            if (hasAccount != (categoryId > 0))
            {
                throw new ArgumentException(ExceptionMessage.MAINTENANCE_COST_ACCOUNT_REQUIRED);
            }
            var asset = context.MAssets.Single(x => x.Id == row.AssetId);
            var now = DateTime.Now;
            var wasInProgress = row.Status == (int)MaintenanceStatus.InProgress;
            if (row.AssetUnitId != null)
            {
                var unit = context.MAssetUnits.Single(x => x.Id == row.AssetUnitId);
                if (wasInProgress && unit.Status == (int)AssetUnitStatus.Perawatan)
                {
                    unit.Status = (int)AssetUnitStatus.Tersedia;
                }
                if (condition != null)
                {
                    unit.UnitCondition = (int)condition;
                }
                unit.UpdDate = now;
                unit.UpdBy = userId;
            }
            else
            {
                if (wasInProgress && asset.Status == (int)AssetStatus.Maintenance)
                {
                    asset.Status = (int)AssetStatus.Available;
                }
                if (condition != null)
                {
                    asset.AssetCondition = (int)condition;
                }
                asset.UpdDate = now;
                asset.UpdBy = userId;
            }
            InventarisHelper.RefreshAssetStatus(context, asset, userId);
            if (hasAccount && row.Cost > 0)
            {
                row.TransactionId = FinancialTransaction.CreateRow(context, new FinancialTransaction.NewRow
                {
                    AccountId = accountId!.Value,
                    CategoryId = categoryId!.Value,
                    TransactionType = CashFlowType.Expense,
                    Amount = row.Cost,
                    TransactionDate = DateOnly.FromDateTime(now),
                    Description = $"Perawatan {asset.Name}: {TransaksiHelper.Truncate(row.Description, 150)}"
                }, null, true, userId).Id;
            }
            row.Status = (int)MaintenanceStatus.Done;
            row.FinishedAt = now;
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
