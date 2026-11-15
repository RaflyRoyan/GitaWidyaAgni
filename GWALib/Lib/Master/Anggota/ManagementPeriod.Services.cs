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

namespace GWALib.Lib.Master.Anggota
{
    public partial class ManagementPeriod
    {
        #region SYNC
        public static List<ManagementPeriod> GetDataList(ManagementPeriodStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ManagementPeriod, EF.MManagementPeriod>();
        }
        public static ManagementPeriod? GetActive()
        {
            var row = _Querys.FirstOrDefault(x => x.Status == (int)ManagementPeriodStatus.Active);
            return row != null ? new ManagementPeriod(row) : null;
        }
        public static Response<ManagementPeriod> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MManagementPeriods.Add(row);
                context.SaveChanges();
                return new ManagementPeriod(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ManagementPeriod>(msg));
        }
        public Response<ManagementPeriod> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ManagementPeriod(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ManagementPeriod>(msg));
        }
        public Response<ManagementPeriod> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateDelete(context, row);
                context.MManagementPeriods.Remove(row);
                context.SaveChanges();
                return new ManagementPeriod(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ManagementPeriod>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ManagementPeriod>> GetDataListAsync(ManagementPeriodStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ManagementPeriod, EF.MManagementPeriod>();
        }
        public static async Task<ManagementPeriod?> GetActiveAsync()
        {
            var row = await _Querys.FirstOrDefaultAsync(x => x.Status == (int)ManagementPeriodStatus.Active);
            return row != null ? new ManagementPeriod(row) : null;
        }
        public static async Task<Response<ManagementPeriod>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MManagementPeriods.Add(row);
                await context.SaveChangesAsync();
                return new ManagementPeriod(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ManagementPeriod>(msg));
        }
        public async Task<Response<ManagementPeriod>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ManagementPeriod(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ManagementPeriod>(msg));
        }
        public async Task<Response<ManagementPeriod>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateDelete(context, row);
                context.MManagementPeriods.Remove(row);
                await context.SaveChangesAsync();
                return new ManagementPeriod(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ManagementPeriod>(msg));
        }
        #endregion

        #region HELPER
        // Periode harus ada dan belum ditutup (Draft / Active).
        internal static EF.MManagementPeriod ValidateOpenPeriod(EF.ModelContext context, int periodId)
        {
            var row = context.MManagementPeriods.SingleOrDefault(x => x.Id == periodId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Periode Kepengurusan"));
            if (row.Status == (int)ManagementPeriodStatus.Closed)
            {
                throw new ArgumentException(ExceptionMessage.PERIOD_CLOSED);
            }
            return row;
        }
        private static IQueryable<EF.MManagementPeriod> FilterQuery(ManagementPeriodStatus? status)
        {
            return _Querys.Where(x => status == null || x.Status == (int)status)
                .OrderByDescending(x => x.StartDate).ThenBy(x => x.Name);
        }
        private static EF.MManagementPeriod GetRow(EF.ModelContext context, int id)
        {
            return context.MManagementPeriods.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Periode Kepengurusan"));
        }
        private static async Task<EF.MManagementPeriod> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MManagementPeriods.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Periode Kepengurusan"));
        }
        private static void ValidateData(EF.ModelContext context, int id, string name, DateOnly startDate, DateOnly endDate, ManagementPeriodStatus status)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Periode"));
            }
            if (endDate < startDate)
            {
                throw new ArgumentException(ExceptionMessage.PERIOD_DATE_INVALID);
            }
            if (!Enum.IsDefined(status))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Periode"));
            }
            ValidateUtility.CheckDuplicateData<EF.MManagementPeriod>(context, x => x.Name == name && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Periode"));
            if (status == ManagementPeriodStatus.Active)
            {
                ValidateUtility.CheckDuplicateData<EF.MManagementPeriod>(context,
                    x => x.Status == (int)ManagementPeriodStatus.Active && x.Id != id,
                    ExceptionMessage.PERIOD_ACTIVE_EXISTS);
            }
        }
        // Hanya periode Draft yang belum dipakai tabel lain yang boleh dihapus; selebihnya cukup ditutup (Closed).
        private static void ValidateDelete(EF.ModelContext context, EF.MManagementPeriod row)
        {
            if (row.Status != (int)ManagementPeriodStatus.Draft)
            {
                throw new ArgumentException(ExceptionMessage.PERIOD_DELETE_NOT_DRAFT);
            }
            var used = context.MManagementPeriods.Where(x => x.Id == row.Id)
                .Select(x => x.MDuesSettings.Any() || x.MUserRoles.Any() || x.TBudgets.Any()
                    || x.TDebts.Any() || x.TDivisionMembers.Any() || x.TElections.Any() || x.TFinancialTransactions.Any()
                    || x.TFundingRequests.Any() || x.TLetters.Any() || x.TManagementAssignments.Any() || x.TMemberPoints.Any()
                    || x.TPeriodBalances.Any() || x.TPeriodHandoverFromPeriod != null
                    || x.TPeriodHandoverToPeriods.Any() || x.TProcurementRequests.Any() || x.TProgramTasks.Any() || x.TPrograms.Any()
                    || x.TRecruitments.Any() || x.TSalesOrders.Any() || x.TSponsorships.Any() || x.TTimelinePlans.Any())
                .Single();
            if (used)
            {
                throw new ArgumentException(ExceptionMessage.PERIOD_IN_USE);
            }
        }
        private static EF.MManagementPeriod CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            ValidateData(context, 0, name, newrow.StartDate, newrow.EndDate, ManagementPeriodStatus.Draft);

            var row = new EF.MManagementPeriod
            {
                Id = SequenceUtility.GetNextId<EF.MManagementPeriod>(context),
                Name = name,
                StartDate = newrow.StartDate,
                EndDate = newrow.EndDate,
                Status = (int)ManagementPeriodStatus.Draft,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Periode yang sudah Closed dikunci.
        private void SetRow(EF.ModelContext context, EF.MManagementPeriod row, int userId)
        {
            if (row.Status == (int)ManagementPeriodStatus.Closed)
            {
                throw new ArgumentException(ExceptionMessage.PERIOD_CLOSED);
            }
            var name = Name?.Trim() ?? string.Empty;
            ValidateData(context, row.Id, name, StartDate, EndDate, Status);

            row.Name = name;
            row.StartDate = StartDate;
            row.EndDate = EndDate;
            row.Status = (int)Status;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        #endregion
    }
}
