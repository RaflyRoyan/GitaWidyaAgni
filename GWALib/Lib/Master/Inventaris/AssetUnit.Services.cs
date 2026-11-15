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

namespace GWALib.Lib.Master.Inventaris
{
    public partial class AssetUnit
    {
        private const int CODE_DIGITS = 3;
        private const string ADD_MOVEMENT_NOTE = "Unit baru";
        private const string DISPOSAL_MOVEMENT_NOTE = "Unit dihapus";

        #region SYNC
        public Asset GetAsset(bool refresh = false)
        {
            if (refresh || AssetData == null)
            {
                AssetData = new Asset(AssetId);
            }
            return AssetData;
        }
        public static List<AssetUnit> GetDataList(int? assetId = null, AssetUnitStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(assetId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<AssetUnit, EF.MAssetUnit>();
        }
        // Untuk hasil scan QR / input kode unit.
        public static AssetUnit? GetByCode(string unitCode)
        {
            var code = unitCode?.Trim().ToUpperInvariant() ?? string.Empty;
            var row = _Querys.SingleOrDefault(x => x.UnitCode == code);
            return row != null ? new AssetUnit(row) : null;
        }
        public static Response<AssetUnit> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = AddRow(context, newrow, userId);
                context.SaveChanges();
                return new AssetUnit(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AssetUnit>(msg));
        }
        public Response<AssetUnit> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(row, userId);
                context.SaveChanges();
                return new AssetUnit(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AssetUnit>(msg));
        }
        public Response<AssetUnit> MarkLabelPrinted(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowLabelPrinted(row, userId);
                context.SaveChanges();
                return new AssetUnit(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AssetUnit>(msg));
        }
        public Response<AssetUnit> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(context, row, userId);
                context.SaveChanges();
                return new AssetUnit(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<AssetUnit>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<Asset> GetAssetAsync(bool refresh = false)
        {
            if (refresh || AssetData == null)
            {
                using var context = DBClass.GetContext();
                var row = await context.MAssets.Include(x => x.Category).SingleAsync(x => x.Id == AssetId);
                AssetData = new Asset(row);
            }
            return AssetData;
        }
        public static async Task<List<AssetUnit>> GetDataListAsync(int? assetId = null, AssetUnitStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(assetId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<AssetUnit, EF.MAssetUnit>();
        }
        public static async Task<AssetUnit?> GetByCodeAsync(string unitCode)
        {
            var code = unitCode?.Trim().ToUpperInvariant() ?? string.Empty;
            var row = await _Querys.SingleOrDefaultAsync(x => x.UnitCode == code);
            return row != null ? new AssetUnit(row) : null;
        }
        public static async Task<Response<AssetUnit>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = AddRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new AssetUnit(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AssetUnit>(msg));
        }
        public async Task<Response<AssetUnit>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(row, userId);
                await context.SaveChangesAsync();
                return new AssetUnit(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AssetUnit>(msg));
        }
        public async Task<Response<AssetUnit>> MarkLabelPrintedAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowLabelPrinted(row, userId);
                await context.SaveChangesAsync();
                return new AssetUnit(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AssetUnit>(msg));
        }
        public async Task<Response<AssetUnit>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(context, row, userId);
                await context.SaveChangesAsync();
                return new AssetUnit(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<AssetUnit>(msg));
        }
        #endregion

        #region HELPER
        // Tanpa SaveChanges: membuat count unit baru untuk aset per unit (dipakai Asset.Save dan AddRow).
        // Nomor urut melanjutkan nomor terbesar aset itu (termasuk unit yang sudah dihapus, supaya kode tidak terpakai ulang).
        internal static List<EF.MAssetUnit> CreateRows(EF.ModelContext context, EF.MAsset asset, int count, ItemCondition unitCondition, int userId)
        {
            ValidateCondition(unitCondition);
            var prefix = asset.UnitPrefix!;
            var lastNumber = context.MAssetUnits.Where(x => x.AssetId == asset.Id).Select(x => x.UnitCode).AsEnumerable()
                .Concat(context.MAssetUnits.Local.Where(x => x.AssetId == asset.Id).Select(x => x.UnitCode))
                .Select(code => int.TryParse(code.Substring(prefix.Length), out var n) ? n : 0)
                .DefaultIfEmpty(0).Max();
            var firstId = SequenceUtility.ReserveIds<EF.MAssetUnit>(context, count);

            return Enumerable.Range(0, count).Select(i => new EF.MAssetUnit
            {
                Id = firstId + i,
                AssetId = asset.Id,
                UnitCode = prefix + (lastNumber + i + 1).ToString("D" + CODE_DIGITS),
                UnitCondition = (int)unitCondition,
                Status = (int)AssetUnitStatus.Tersedia,
                InsDate = DateTime.Now,
                InsBy = userId
            }).ToList();
        }
        // Tanpa SaveChanges: semua unit yang belum dihapus ikut dihapus bersama asetnya.
        internal static void DisposeAll(EF.ModelContext context, EF.MAsset asset, string notes, int userId)
        {
            var units = context.MAssetUnits.Where(x => x.AssetId == asset.Id && x.Status != (int)AssetUnitStatus.Dihapus).ToList();
            if (units.Any(x => x.Status == (int)AssetUnitStatus.Dipinjam || x.Status == (int)AssetUnitStatus.Perawatan))
            {
                throw new ArgumentException(ExceptionMessage.ASSET_UNIT_IN_USE);
            }
            foreach (var unit in units)
            {
                unit.Status = (int)AssetUnitStatus.Dihapus;
                unit.UpdDate = DateTime.Now;
                unit.UpdBy = userId;
                context.TAssetMovements.Add(Asset.CreateMovement(context, asset.Id, unit.Id, AssetMovementType.Disposal, 1, notes, userId));
            }
        }
        private static IQueryable<EF.MAssetUnit> FilterQuery(int? assetId, AssetUnitStatus? status)
        {
            return _Querys.Where(x => (assetId == null || x.AssetId == assetId)
                    && (status == null || x.Status == (int)status))
                .OrderBy(x => x.UnitCode);
        }
        private static EF.MAssetUnit GetRow(EF.ModelContext context, int id)
        {
            return context.MAssetUnits.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Unit Aset"));
        }
        private static async Task<EF.MAssetUnit> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MAssetUnits.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Unit Aset"));
        }
        private static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        // Unit hanya Baik / Rusak Ringan / Rusak Berat; unit hilang dicatat lewat Status.
        private static void ValidateCondition(ItemCondition unitCondition)
        {
            if (unitCondition != ItemCondition.Baik && unitCondition != ItemCondition.RusakRingan && unitCondition != ItemCondition.RusakBerat)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kondisi Unit"));
            }
        }
        private static void ValidateNotDisposed(EF.MAssetUnit row)
        {
            if (row.Status == (int)AssetUnitStatus.Dihapus)
            {
                throw new ArgumentException(ExceptionMessage.ASSET_UNIT_DISPOSED);
            }
        }
        // Tanpa SaveChanges: menambah satu unit ke aset per unit, Quantity aset +1 dan mutasi Masuk.
        private static EF.MAssetUnit AddRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var asset = Asset.GetAsset(context, newrow.AssetId);
            if (asset.TrackUnit != (int)UnitTracking.PerUnit)
            {
                throw new ArgumentException(ExceptionMessage.ASSET_NOT_PER_UNIT);
            }
            var row = CreateRows(context, asset, 1, newrow.UnitCondition, userId).Single();
            row.SerialNumber = ToNullable(newrow.SerialNumber);
            row.Notes = ToNullable(newrow.Notes);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.MAssetUnits.Add(row);

            asset.Quantity += 1;
            asset.UpdDate = DateTime.Now;
            asset.UpdBy = userId;
            context.TAssetMovements.Add(Asset.CreateMovement(context, asset.Id, row.Id, AssetMovementType.In, 1, ADD_MOVEMENT_NOTE, userId));
            return row;
        }
        // Aset dan kode unit tetap; status diatur transaksi.
        private void SetRow(EF.MAssetUnit row, int userId)
        {
            ValidateNotDisposed(row);
            ValidateCondition(UnitCondition);

            row.SerialNumber = ToNullable(SerialNumber);
            row.UnitCondition = (int)UnitCondition;
            row.Notes = ToNullable(Notes);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowLabelPrinted(EF.MAssetUnit row, int userId)
        {
            ValidateNotDisposed(row);
            row.LabelPrintedAt = DateTime.Now;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        // Unit tersedia atau hilang boleh dihapus (dihapusbukukan); yang dipinjam / dirawat tidak. Quantity aset -1 dan mutasi Penghapusan.
        private static void SetRowDelete(EF.ModelContext context, EF.MAssetUnit row, int userId)
        {
            ValidateNotDisposed(row);
            if (row.Status == (int)AssetUnitStatus.Dipinjam || row.Status == (int)AssetUnitStatus.Perawatan)
            {
                throw new ArgumentException(ExceptionMessage.ASSET_UNIT_IN_USE);
            }
            var asset = Asset.GetAsset(context, row.AssetId);
            row.Status = (int)AssetUnitStatus.Dihapus;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;

            asset.Quantity -= 1;
            asset.UpdDate = DateTime.Now;
            asset.UpdBy = userId;
            context.TAssetMovements.Add(Asset.CreateMovement(context, asset.Id, row.Id, AssetMovementType.Disposal, 1, DISPOSAL_MOVEMENT_NOTE, userId));
        }
        #endregion
    }
}
