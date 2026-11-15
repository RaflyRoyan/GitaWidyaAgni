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

namespace GWALib.Lib.Master.Keuangan
{
    public partial class FinancialCategory
    {
        #region SYNC
        public static List<FinancialCategory> GetDataList(FilterAktif aktif, CashFlowType? categoryType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, categoryType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<FinancialCategory, EF.MFinancialCategory>();
        }
        public static Response<FinancialCategory> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MFinancialCategories.Add(row);
                context.SaveChanges();
                return new FinancialCategory(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FinancialCategory>(msg));
        }
        public Response<FinancialCategory> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new FinancialCategory(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FinancialCategory>(msg));
        }
        public Response<FinancialCategory> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(context, row, userId);
                context.SaveChanges();
                return new FinancialCategory(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FinancialCategory>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<FinancialCategory>> GetDataListAsync(FilterAktif aktif, CashFlowType? categoryType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, categoryType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<FinancialCategory, EF.MFinancialCategory>();
        }
        public static async Task<Response<FinancialCategory>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MFinancialCategories.Add(row);
                await context.SaveChangesAsync();
                return new FinancialCategory(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FinancialCategory>(msg));
        }
        public async Task<Response<FinancialCategory>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new FinancialCategory(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FinancialCategory>(msg));
        }
        public async Task<Response<FinancialCategory>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(context, row, userId);
                await context.SaveChangesAsync();
                return new FinancialCategory(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FinancialCategory>(msg));
        }
        #endregion

        #region HELPER
        // categoryType null = jenis apa pun.
        internal static void ValidateActiveCategory(EF.ModelContext context, int categoryId, CashFlowType? categoryType)
        {
            var row = context.MFinancialCategories.SingleOrDefault(x => x.Id == categoryId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kategori Keuangan"));
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Kategori Keuangan"));
            }
            if (categoryType != null && row.CategoryType != (int)categoryType)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FINANCIAL_CATEGORY_TYPE_MISMATCH, categoryType.Value.GetDescription()));
            }
        }
        private static IQueryable<EF.MFinancialCategory> FilterQuery(FilterAktif aktif, CashFlowType? categoryType)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (categoryType == null || x.CategoryType == (int)categoryType))
                .OrderBy(x => x.CategoryType).ThenBy(x => x.Name);
        }
        private static EF.MFinancialCategory GetRow(EF.ModelContext context, int id)
        {
            return context.MFinancialCategories.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kategori Keuangan"));
        }
        private static async Task<EF.MFinancialCategory> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MFinancialCategories.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kategori Keuangan"));
        }
        private static void ValidateData(EF.ModelContext context, int id, string name, CashFlowType categoryType)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Kategori"));
            }
            if (!Enum.IsDefined(categoryType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Kategori"));
            }
            ValidateUtility.CheckDuplicateData<EF.MFinancialCategory>(context,
                x => x.CategoryType == (int)categoryType && x.Name == name && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Kategori"));
        }
        // Kategori yang masih dipakai pengaturan iuran aktif tidak boleh dinonaktifkan.
        private static void ValidateDeactivate(EF.ModelContext context, EF.MFinancialCategory row)
        {
            if (context.MDuesSettings.Any(x => x.CategoryId == row.Id && x.IsActive == (int)ActiveStatus.Aktif))
            {
                throw new ArgumentException(ExceptionMessage.FINANCIAL_CATEGORY_IN_USE);
            }
        }
        private static EF.MFinancialCategory CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            ValidateData(context, 0, name, newrow.CategoryType);

            var row = new EF.MFinancialCategory
            {
                Id = SequenceUtility.GetNextId<EF.MFinancialCategory>(context),
                Name = name,
                CategoryType = (int)newrow.CategoryType,
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Jenis (pemasukan/pengeluaran) tidak bisa diubah karena transaksi lama memakai kategori ini.
        private void SetRow(EF.ModelContext context, EF.MFinancialCategory row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            ValidateData(context, row.Id, name, (CashFlowType)row.CategoryType);
            if (IsActive != ActiveStatus.Aktif && row.IsActive == (int)ActiveStatus.Aktif)
            {
                ValidateDeactivate(context, row);
            }

            row.Name = name;
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.ModelContext context, EF.MFinancialCategory row, int userId)
        {
            ValidateDeactivate(context, row);
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
