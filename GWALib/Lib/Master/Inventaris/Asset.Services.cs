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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Inventaris
{
    public partial class Asset
    {
        private const string INITIAL_MOVEMENT_NOTE = "Aset baru";
        private const string DISPOSAL_MOVEMENT_NOTE = "Aset dihapuskan";

        #region SYNC
        public List<AssetUnit> GetUnits(bool refresh)
        {
            if (refresh || UnitList == null)
            {
                UnitList = AssetUnit.GetDataList(assetId: Id);
            }
            return UnitList;
        }
        public List<AssetPhoto.Info> GetPhotos(bool refresh)
        {
            if (refresh || PhotoList == null)
            {
                PhotoList = AssetPhoto.GetInfoList(Id);
            }
            return PhotoList;
        }
        public static List<Asset> GetDataList(AssetStatus? status = null, int? categoryId = null, ItemCondition? assetCondition = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, categoryId, assetCondition);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Asset, EF.MAsset>();
        }
        public static Response<Asset> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new Asset(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Asset>(msg));
        }
        public Response<Asset> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new Asset(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Asset>(msg));
        }
        public Response<Asset> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(context, row, userId);
                context.SaveChanges();
                return new Asset(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Asset>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<AssetUnit>> GetUnitsAsync(bool refresh)
        {
            if (refresh || UnitList == null)
            {
                UnitList = await AssetUnit.GetDataListAsync(assetId: Id);
            }
            return UnitList;
        }
        public async Task<List<AssetPhoto.Info>> GetPhotosAsync(bool refresh)
        {
            if (refresh || PhotoList == null)
            {
                PhotoList = await AssetPhoto.GetInfoListAsync(Id);
            }
            return PhotoList;
        }
        public static async Task<List<Asset>> GetDataListAsync(AssetStatus? status = null, int? categoryId = null, ItemCondition? assetCondition = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, categoryId, assetCondition);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Asset, EF.MAsset>();
        }
        public static async Task<Response<Asset>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new Asset(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Asset>(msg));
        }
        public async Task<Response<Asset>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new Asset(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Asset>(msg));
        }
        public async Task<Response<Asset>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(context, row, userId);
                await context.SaveChangesAsync();
                return new Asset(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Asset>(msg));
        }
        #endregion

        #region HELPER
        // Aset yang belum dihapuskan (status apa pun selain Disposed).
        internal static EF.MAsset GetAsset(EF.ModelContext context, int assetId)
        {
            var row = context.MAssets.SingleOrDefault(x => x.Id == assetId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Aset"));
            if (row.Status == (int)AssetStatus.Disposed)
            {
                throw new ArgumentException(ExceptionMessage.ASSET_DISPOSED);
            }
            return row;
        }
        // Aset siap dipinjam: status Tersedia.
        internal static EF.MAsset GetAvailableAsset(EF.ModelContext context, int assetId)
        {
            var row = GetAsset(context, assetId);
            if (row.Status != (int)AssetStatus.Available)
            {
                throw new ArgumentException(ExceptionMessage.ASSET_IN_USE);
            }
            return row;
        }
        private static IQueryable<EF.MAsset> FilterQuery(AssetStatus? status, int? categoryId, ItemCondition? assetCondition)
        {
            return _Querys.Where(x => (status == null || x.Status == (int)status)
                    && (categoryId == null || x.CategoryId == categoryId)
                    && (assetCondition == null || x.AssetCondition == (int)assetCondition))
                .OrderBy(x => x.AssetCode);
        }
        private static EF.MAsset GetRow(EF.ModelContext context, int id)
        {
            return context.MAssets.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Aset"));
        }
        private static async Task<EF.MAsset> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MAssets.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Aset"));
        }
        private static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        private static void ValidateData(EF.ModelContext context, int id, string assetCode, string name, string unit,
            DateOnly? acquisitionDate, decimal? acquisitionCost, AcquisitionSource acquisitionSource, ItemCondition assetCondition,
            decimal rentalFeePerDay, decimal lateFeePerDay)
        {
            if (string.IsNullOrWhiteSpace(assetCode))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Kode Aset"));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Aset"));
            }
            if (string.IsNullOrWhiteSpace(unit))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Satuan"));
            }
            if (acquisitionDate != null && acquisitionDate > DateOnly.FromDateTime(DateTime.Today))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Tanggal Perolehan"));
            }
            if (acquisitionCost != null && acquisitionCost < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Nilai Perolehan"));
            }
            if (!Enum.IsDefined(acquisitionSource))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Sumber Perolehan"));
            }
            if (!Enum.IsDefined(assetCondition))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kondisi Aset"));
            }
            if (rentalFeePerDay < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Tarif Sewa"));
            }
            if (lateFeePerDay < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Denda Telat"));
            }
            ValidateUtility.CheckDuplicateData<EF.MAsset>(context, x => x.AssetCode == assetCode && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Kode Aset"));
        }
        // Awalan kode unit: wajib untuk PerUnit (huruf / angka, unik antar aset), selalu NULL untuk CukupJumlah.
        private static string? ResolveUnitPrefix(EF.ModelContext context, UnitTracking trackUnit, string? unitPrefix)
        {
            if (!Enum.IsDefined(trackUnit))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Pencatatan Unit"));
            }
            if (trackUnit == UnitTracking.CukupJumlah)
            {
                return null;
            }
            var prefix = unitPrefix?.Trim().ToUpperInvariant() ?? string.Empty;
            if (prefix.Length == 0)
            {
                throw new ArgumentException(ExceptionMessage.ASSET_UNIT_PREFIX_REQUIRED);
            }
            if (!Regex.IsMatch(prefix, "^[A-Z0-9]+$"))
            {
                throw new ArgumentException(ExceptionMessage.ASSET_UNIT_PREFIX_INVALID);
            }
            ValidateUtility.CheckDuplicateData<EF.MAsset>(context, x => x.UnitPrefix == prefix,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Awalan Kode Unit"));
            return prefix;
        }
        internal static EF.TAssetMovement CreateMovement(EF.ModelContext context, int assetId, int? assetUnitId, AssetMovementType movementType, int quantity, string notes, int userId)
        {
            return new EF.TAssetMovement
            {
                Id = SequenceUtility.GetNextId<EF.TAssetMovement>(context),
                AssetId = assetId,
                AssetUnitId = assetUnitId,
                MovementType = (int)movementType,
                Quantity = quantity,
                MovementDate = DateTime.Now,
                Notes = notes,
                CreatedBy = userId,
                InsDate = DateTime.Now,
                InsBy = userId
            };
        }
        private static EF.MAsset CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var assetCode = newrow.AssetCode?.Trim().ToUpperInvariant() ?? string.Empty;
            var name = newrow.Name?.Trim() ?? string.Empty;
            var unit = newrow.Unit?.Trim() ?? string.Empty;
            AssetCategory.ValidateActiveAssetCategory(context, newrow.CategoryId);
            if (newrow.Quantity <= 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jumlah"));
            }
            ValidateData(context, 0, assetCode, name, unit, newrow.AcquisitionDate, newrow.AcquisitionCost,
                newrow.AcquisitionSource, newrow.AssetCondition, newrow.RentalFeePerDay, newrow.LateFeePerDay);
            var unitPrefix = ResolveUnitPrefix(context, newrow.TrackUnit, newrow.UnitPrefix);

            var row = new EF.MAsset
            {
                Id = SequenceUtility.GetNextId<EF.MAsset>(context),
                CategoryId = newrow.CategoryId,
                AssetCode = assetCode,
                Name = name,
                Brand = ToNullable(newrow.Brand),
                Specification = ToNullable(newrow.Specification),
                Quantity = newrow.Quantity,
                Unit = unit,
                TrackUnit = (int)newrow.TrackUnit,
                UnitPrefix = unitPrefix,
                AcquisitionDate = newrow.AcquisitionDate,
                AcquisitionCost = newrow.AcquisitionCost,
                AcquisitionSource = (int)newrow.AcquisitionSource,
                Location = ToNullable(newrow.Location),
                AssetCondition = (int)newrow.AssetCondition,
                RentalFeePerDay = newrow.RentalFeePerDay,
                LateFeePerDay = newrow.LateFeePerDay,
                Status = (int)AssetStatus.Available,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.MAssets.Add(row);
            if (newrow.TrackUnit == UnitTracking.PerUnit)
            {
                // Satu unit + satu mutasi Masuk per unit; kondisi unit mengikuti kondisi aset (unit hanya Baik / Rusak).
                var unitCondition = newrow.AssetCondition == ItemCondition.Hilang ? ItemCondition.Baik : newrow.AssetCondition;
                foreach (var unitRow in AssetUnit.CreateRows(context, row, newrow.Quantity, unitCondition, userId))
                {
                    context.MAssetUnits.Add(unitRow);
                    context.TAssetMovements.Add(CreateMovement(context, row.Id, unitRow.Id, AssetMovementType.In, 1, INITIAL_MOVEMENT_NOTE, userId));
                }
            }
            else
            {
                context.TAssetMovements.Add(CreateMovement(context, row.Id, null, AssetMovementType.In, row.Quantity, INITIAL_MOVEMENT_NOTE, userId));
            }
            return row;
        }
        // Kategori hanya divalidasi bila diganti. Quantity, Status, TrackUnit dan UnitPrefix tidak diubah di sini.
        private void SetRow(EF.ModelContext context, EF.MAsset row, int userId)
        {
            if (row.Status == (int)AssetStatus.Disposed)
            {
                throw new ArgumentException(ExceptionMessage.ASSET_DISPOSED);
            }
            var assetCode = AssetCode?.Trim().ToUpperInvariant() ?? string.Empty;
            var name = Name?.Trim() ?? string.Empty;
            var unit = Unit?.Trim() ?? string.Empty;
            var categoryId = Category?.Id ?? 0;
            if (categoryId != row.CategoryId)
            {
                AssetCategory.ValidateActiveAssetCategory(context, categoryId);
            }
            ValidateData(context, row.Id, assetCode, name, unit, AcquisitionDate, AcquisitionCost, AcquisitionSource, AssetCondition,
                RentalFeePerDay, LateFeePerDay);

            row.CategoryId = categoryId;
            row.AssetCode = assetCode;
            row.Name = name;
            row.Brand = ToNullable(Brand);
            row.Specification = ToNullable(Specification);
            row.Unit = unit;
            row.AcquisitionDate = AcquisitionDate;
            row.AcquisitionCost = AcquisitionCost;
            row.AcquisitionSource = (int)AcquisitionSource;
            row.Location = ToNullable(Location);
            row.AssetCondition = (int)AssetCondition;
            row.RentalFeePerDay = RentalFeePerDay;
            row.LateFeePerDay = LateFeePerDay;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        // Penghapusan aset: hanya bila tidak sedang dipinjam / dirawat; dicatat sebagai mutasi Penghapusan.
        // Aset per unit: unit yang belum dihapus ikut dihapus (unit yang sedang dipinjam / dirawat menolak penghapusan).
        private static void SetRowDelete(EF.ModelContext context, EF.MAsset row, int userId)
        {
            if (row.Status == (int)AssetStatus.Disposed)
            {
                throw new ArgumentException(ExceptionMessage.ASSET_DISPOSED);
            }
            if (row.Status != (int)AssetStatus.Available)
            {
                throw new ArgumentException(ExceptionMessage.ASSET_IN_USE);
            }
            if (row.TrackUnit == (int)UnitTracking.PerUnit)
            {
                AssetUnit.DisposeAll(context, row, DISPOSAL_MOVEMENT_NOTE, userId);
            }
            else
            {
                context.TAssetMovements.Add(CreateMovement(context, row.Id, null, AssetMovementType.Disposal, row.Quantity, DISPOSAL_MOVEMENT_NOTE, userId));
            }
            row.Status = (int)AssetStatus.Disposed;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
