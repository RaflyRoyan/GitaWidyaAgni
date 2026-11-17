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

namespace GWALib.Lib.Transaksi.Program
{
    public partial class WorkProgram
    {
        #region SYNC
        public static List<WorkProgram> GetDataList(int? periodId = null, ProgramStatus? status = null, int? picMemberId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, status, picMemberId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<WorkProgram, EF.TProgram>();
        }
        public static Response<WorkProgram> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TPrograms.Add(row);
                context.SaveChanges();
                return new WorkProgram(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<WorkProgram>(msg));
        }
        public Response<WorkProgram> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new WorkProgram(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<WorkProgram>(msg));
        }
        public Response<WorkProgram> SetStatus(ProgramStatus status, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ApplyStatus(row, status, userId);
                context.SaveChanges();
                return new WorkProgram(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<WorkProgram>(msg));
        }
        public Response<WorkProgram> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateDelete(context, row);
                var data = new WorkProgram(Reload(row.Id));
                context.TPrograms.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<WorkProgram>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<WorkProgram>> GetDataListAsync(int? periodId = null, ProgramStatus? status = null, int? picMemberId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, status, picMemberId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<WorkProgram, EF.TProgram>();
        }
        public static async Task<Response<WorkProgram>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TPrograms.Add(row);
                await context.SaveChangesAsync();
                return new WorkProgram(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<WorkProgram>(msg));
        }
        public async Task<Response<WorkProgram>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new WorkProgram(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<WorkProgram>(msg));
        }
        public async Task<Response<WorkProgram>> SetStatusAsync(ProgramStatus status, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ApplyStatus(row, status, userId);
                await context.SaveChangesAsync();
                return new WorkProgram(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<WorkProgram>(msg));
        }
        public async Task<Response<WorkProgram>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateDelete(context, row);
                var data = new WorkProgram(Reload(row.Id));
                context.TPrograms.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<WorkProgram>(msg));
        }
        #endregion

        #region INTERNAL
        // Program yang masih berjalan (Planned / Ongoing); dipakai kegiatan, tugas, laporan, timeline.
        internal static EF.TProgram GetOpenProgram(EF.ModelContext context, int programId)
        {
            var row = context.TPrograms.SingleOrDefault(x => x.Id == programId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Program Kerja"));
            TransaksiHelper.ValidateStatus((ProgramStatus)row.Status, "memakai program kerja", ProgramStatus.Planned, ProgramStatus.Ongoing);
            return row;
        }
        #endregion

        #region HELPER
        private static EF.TProgram Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TProgram> FilterQuery(int? periodId, ProgramStatus? status, int? picMemberId)
        {
            return _Querys.Where(x => (periodId == null || x.PeriodId == periodId)
                    && (status == null || x.Status == (int)status)
                    && (picMemberId == null || x.PicMemberId == picMemberId))
                .OrderBy(x => x.StartDate == null).ThenBy(x => x.StartDate).ThenBy(x => x.Title);
        }
        private static EF.TProgram GetRow(EF.ModelContext context, int id)
        {
            return context.TPrograms.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Program Kerja"));
        }
        private static async Task<EF.TProgram> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TPrograms.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Program Kerja"));
        }
        private static void ValidateData(EF.ModelContext context, int id, int periodId, int? previousProgramId, int? picMemberId,
            string title, DateOnly? startDate, DateOnly? endDate, decimal budgetAmount)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Program"));
            }
            if (startDate != null && endDate != null && endDate < startDate)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            if (budgetAmount < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Anggaran Program"));
            }
            if (previousProgramId != null)
            {
                var previousPeriodId = context.TPrograms.Where(x => x.Id == previousProgramId && x.Id != id).Select(x => (int?)x.PeriodId).SingleOrDefault()
                    ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Program Sebelumnya"));
                if (previousPeriodId == periodId)
                {
                    throw new ArgumentException(ExceptionMessage.PROGRAM_PREVIOUS_SAME_PERIOD);
                }
            }
            if (picMemberId != null)
            {
                Member.GetActiveMember(context, picMemberId.Value);
            }
        }
        // Bidang harus divisi aktif (dicek hanya saat berubah); jangka harus nilai enum.
        private static void ValidateGrouping(EF.ModelContext context, int? divisionId, ProgramTerm? term)
        {
            if (divisionId != null)
            {
                Division.ValidateActiveDivision(context, divisionId.Value);
            }
            if (term != null && !Enum.IsDefined(term.Value))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jangka Program"));
            }
        }
        private static void ValidateDelete(EF.ModelContext context, EF.TProgram row)
        {
            TransaksiHelper.ValidateStatus((ProgramStatus)row.Status, "menghapus program kerja", ProgramStatus.Planned);
            var used = context.TPrograms.Where(x => x.Id == row.Id)
                .Select(x => x.TActivities.Any() || x.TProgramTasks.Any() || x.TActivityReports.Any() || x.TBudgetItems.Any()
                    || x.TFinancialTransactions.Any() || x.TFundingRequests.Any() || x.TProcurementRequests.Any() || x.TTimelinePlans.Any()
                    || x.InversePreviousProgram.Any())
                .Single();
            if (used)
            {
                throw new ArgumentException(ExceptionMessage.PROGRAM_IN_USE);
            }
        }
        private static EF.TProgram CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var title = newrow.Title?.Trim() ?? string.Empty;
            var previousProgramId = newrow.PreviousProgramId > 0 ? newrow.PreviousProgramId : null;
            var picMemberId = newrow.PicMemberId > 0 ? newrow.PicMemberId : null;
            ManagementPeriod.ValidateOpenPeriod(context, newrow.PeriodId);
            ValidateData(context, 0, newrow.PeriodId, previousProgramId, picMemberId, title, newrow.StartDate, newrow.EndDate, newrow.BudgetAmount);
            var divisionId = newrow.DivisionId > 0 ? newrow.DivisionId : null;
            ValidateGrouping(context, divisionId, newrow.Term);

            var row = new EF.TProgram
            {
                Id = SequenceUtility.GetNextId<EF.TProgram>(context),
                PeriodId = newrow.PeriodId,
                PreviousProgramId = previousProgramId,
                PicMemberId = picMemberId,
                DivisionId = divisionId,
                Term = (int?)newrow.Term,
                Title = title,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                StartDate = newrow.StartDate,
                EndDate = newrow.EndDate,
                BudgetAmount = newrow.BudgetAmount,
                Status = (int)ProgramStatus.Planned,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Periode tetap; status lewat SetStatus.
        private void SetRow(EF.ModelContext context, EF.TProgram row, int userId)
        {
            TransaksiHelper.ValidateStatus((ProgramStatus)row.Status, "mengubah program kerja", ProgramStatus.Planned, ProgramStatus.Ongoing);
            var title = Title?.Trim() ?? string.Empty;
            var previousProgramId = PreviousProgramId > 0 ? PreviousProgramId : null;
            var picMemberId = PicMemberId > 0 ? PicMemberId : null;
            ValidateData(context, row.Id, row.PeriodId, previousProgramId, picMemberId != row.PicMemberId ? picMemberId : null,
                title, StartDate, EndDate, BudgetAmount);
            var divisionId = DivisionId > 0 ? DivisionId : null;
            ValidateGrouping(context, divisionId != row.DivisionId ? divisionId : null, Term);

            row.PreviousProgramId = previousProgramId;
            row.PicMemberId = picMemberId;
            row.DivisionId = divisionId;
            row.Term = (int?)Term;
            row.Title = title;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.StartDate = StartDate;
            row.EndDate = EndDate;
            row.BudgetAmount = BudgetAmount;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        // Planned → Ongoing → Done; Planned / Ongoing → Cancelled.
        private static void ApplyStatus(EF.TProgram row, ProgramStatus target, int userId)
        {
            var current = (ProgramStatus)row.Status;
            switch (target)
            {
                case ProgramStatus.Ongoing:
                    TransaksiHelper.ValidateStatus(current, "memulai program", ProgramStatus.Planned);
                    break;
                case ProgramStatus.Done:
                    TransaksiHelper.ValidateStatus(current, "menyelesaikan program", ProgramStatus.Ongoing);
                    break;
                case ProgramStatus.Cancelled:
                    TransaksiHelper.ValidateStatus(current, "membatalkan program", ProgramStatus.Planned, ProgramStatus.Ongoing);
                    break;
                default:
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Program"));
            }
            row.Status = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
