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
    public partial class AssetCategory
    {
        #region SYNC
        public List<Asset> GetAssets(bool refresh)
        {
            if (refresh || AssetList == null)
            {
                AssetList = Asset.GetDataList(categoryId: Id);
            }
            return AssetList;
        }
        public static List<AssetCategory> GetDataList(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<AssetCategory, EF.MAssetCategory>();
        }
        public static Response<AssetCategory> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MAssetCategories.Add(row);
                context.SaveChanges();
                return new AssetCategory(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AssetCategory>(msg));
        }
        public Response<AssetCategory> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new AssetCategory(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AssetCategory>(msg));
        }
        public Response<AssetCategory> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(context, row, userId);
                context.SaveChanges();
                return new AssetCategory(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<AssetCategory>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<Asset>> GetAssetsAsync(bool refresh)
        {
            if (refresh || AssetList == null)
            {
                AssetList = await Asset.GetDataListAsync(categoryId: Id);
            }
            return AssetList;
        }
        public static async Task<List<AssetCategory>> GetDataListAsync(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<AssetCategory, EF.MAssetCategory>();
        }
        public static async Task<Response<AssetCategory>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MAssetCategories.Add(row);
                await context.SaveChangesAsync();
                return new AssetCategory(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AssetCategory>(msg));
        }
        public async Task<Response<AssetCategory>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new AssetCategory(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AssetCategory>(msg));
        }
        public async Task<Response<AssetCategory>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(context, row, userId);
                await context.SaveChangesAsync();
                return new AssetCategory(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<AssetCategory>(msg));
        }
        #endregion

        #region HELPER
        internal static void ValidateActiveAssetCategory(EF.ModelContext context, int categoryId)
        {
            var row = context.MAssetCategories.SingleOrDefault(x => x.Id == categoryId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kategori Aset"));
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Kategori Aset"));
            }
        }
        private static IQueryable<EF.MAssetCategory> FilterQuery(FilterAktif aktif)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua))
                .OrderBy(x => x.Name);
        }
        private static EF.MAssetCategory GetRow(EF.ModelContext context, int id)
        {
            return context.MAssetCategories.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kategori Aset"));
        }
        private static async Task<EF.MAssetCategory> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MAssetCategories.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kategori Aset"));
        }
        private static void ValidateData(EF.ModelContext context, int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Kategori Aset"));
            }
            ValidateUtility.CheckDuplicateData<EF.MAssetCategory>(context, x => x.Name == name && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Kategori Aset"));
        }
        // Kategori yang masih dipakai aset yang belum dihapuskan tidak boleh dinonaktifkan.
        private static void ValidateDeactivate(EF.ModelContext context, EF.MAssetCategory row)
        {
            if (context.MAssets.Any(x => x.CategoryId == row.Id && x.Status != (int)AssetStatus.Disposed))
            {
                throw new ArgumentException(ExceptionMessage.ASSET_CATEGORY_IN_USE);
            }
        }
        private static EF.MAssetCategory CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            ValidateData(context, 0, name);

            var row = new EF.MAssetCategory
            {
                Id = SequenceUtility.GetNextId<EF.MAssetCategory>(context),
                Name = name,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.MAssetCategory row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            ValidateData(context, row.Id, name);
            if (IsActive != ActiveStatus.Aktif && row.IsActive == (int)ActiveStatus.Aktif)
            {
                ValidateDeactivate(context, row);
            }

            row.Name = name;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.ModelContext context, EF.MAssetCategory row, int userId)
        {
            ValidateDeactivate(context, row);
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
