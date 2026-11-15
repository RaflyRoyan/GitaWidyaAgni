using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
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
    public partial class DuesSetting
    {
        #region SYNC
        public static List<DuesSetting> GetDataList(FilterAktif aktif, int? periodId = null, DuesFrequency? frequency = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, periodId, frequency);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<DuesSetting, EF.MDuesSetting>();
        }
        public static Response<DuesSetting> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MDuesSettings.Add(row);
                context.SaveChanges();
                return new DuesSetting(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<DuesSetting>(msg));
        }
        public Response<DuesSetting> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new DuesSetting(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DuesSetting>(msg));
        }
        public Response<DuesSetting> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new DuesSetting(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<DuesSetting>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<DuesSetting>> GetDataListAsync(FilterAktif aktif, int? periodId = null, DuesFrequency? frequency = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, periodId, frequency);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<DuesSetting, EF.MDuesSetting>();
        }
        public static async Task<Response<DuesSetting>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MDuesSettings.Add(row);
                await context.SaveChangesAsync();
                return new DuesSetting(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<DuesSetting>(msg));
        }
        public async Task<Response<DuesSetting>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new DuesSetting(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DuesSetting>(msg));
        }
        public async Task<Response<DuesSetting>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new DuesSetting(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<DuesSetting>(msg));
        }
        #endregion

        #region HELPER
        internal static EF.MDuesSetting GetActiveDuesSetting(EF.ModelContext context, int duesSettingId)
        {
            var row = context.MDuesSettings.SingleOrDefault(x => x.Id == duesSettingId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pengaturan Iuran"));
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Pengaturan Iuran"));
            }
            return row;
        }
        private static IQueryable<EF.MDuesSetting> FilterQuery(FilterAktif aktif, int? periodId, DuesFrequency? frequency)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (periodId == null || x.PeriodId == periodId)
                    && (frequency == null || x.Frequency == (int)frequency))
                .OrderBy(x => x.Name);
        }
        private static EF.MDuesSetting GetRow(EF.ModelContext context, int id)
        {
            return context.MDuesSettings.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pengaturan Iuran"));
        }
        private static async Task<EF.MDuesSetting> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MDuesSettings.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pengaturan Iuran"));
        }
        private static void ValidateData(EF.ModelContext context, int id, int? periodId, string name, decimal amount, DuesFrequency frequency)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Iuran"));
            }
            if (amount <= 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Nominal Iuran"));
            }
            if (!Enum.IsDefined(frequency))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Frekuensi Iuran"));
            }
            ValidateUtility.CheckDuplicateData<EF.MDuesSetting>(context,
                x => x.PeriodId == periodId && x.Name == name && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Iuran"));
        }
        private static EF.MDuesSetting CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            var periodId = newrow.PeriodId > 0 ? newrow.PeriodId : null;
            var categoryId = newrow.CategoryId > 0 ? newrow.CategoryId : null;
            if (periodId != null)
            {
                ManagementPeriod.ValidateOpenPeriod(context, periodId.Value);
            }
            if (categoryId != null)
            {
                FinancialCategory.ValidateActiveCategory(context, categoryId.Value, CashFlowType.Income);
            }
            ValidateData(context, 0, periodId, name, newrow.Amount, newrow.Frequency);

            var row = new EF.MDuesSetting
            {
                Id = SequenceUtility.GetNextId<EF.MDuesSetting>(context),
                PeriodId = periodId,
                Name = name,
                Amount = newrow.Amount,
                Frequency = (int)newrow.Frequency,
                CategoryId = categoryId,
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // organisasi tetap. Periode / kategori hanya divalidasi bila diganti; periode dan frekuensi dikunci bila tagihan sudah ada.
        private void SetRow(EF.ModelContext context, EF.MDuesSetting row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            int? periodId = Period != null && Period.Id > 0 ? Period.Id : null;
            int? categoryId = Category != null && Category.Id > 0 ? Category.Id : null;
            if ((periodId != row.PeriodId || (int)Frequency != row.Frequency) && context.TDuesBills.Any(x => x.DuesSettingId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.DUES_SETTING_HAS_BILLS);
            }
            if (periodId != null && periodId != row.PeriodId)
            {
                ManagementPeriod.ValidateOpenPeriod(context, periodId.Value);
            }
            if (categoryId != null && categoryId != row.CategoryId)
            {
                FinancialCategory.ValidateActiveCategory(context, categoryId.Value, CashFlowType.Income);
            }
            ValidateData(context, row.Id, periodId, name, Amount, Frequency);

            row.PeriodId = periodId;
            row.Name = name;
            row.Amount = Amount;
            row.Frequency = (int)Frequency;
            row.CategoryId = categoryId;
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MDuesSetting row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
