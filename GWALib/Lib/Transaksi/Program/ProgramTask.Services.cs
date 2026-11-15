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
    public partial class ProgramTask
    {
        #region SYNC
        public static List<ProgramTask> GetDataList(int? periodId = null, int? programId = null, int? responsibleMemberId = null, TaskProgressStatus? status = null,
            MilestoneFlag? isMilestone = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, programId, responsibleMemberId, status, isMilestone);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ProgramTask, EF.TProgramTask>();
        }
        public static Response<ProgramTask> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TProgramTasks.Add(row);
                context.SaveChanges();
                return new ProgramTask(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ProgramTask>(msg));
        }
        public Response<ProgramTask> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ProgramTask(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProgramTask>(msg));
        }
        public Response<ProgramTask> SetStatus(TaskProgressStatus status, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ApplyStatus(row, status, userId);
                context.SaveChanges();
                return new ProgramTask(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProgramTask>(msg));
        }
        public Response<ProgramTask> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var data = new ProgramTask(Reload(row.Id));
                context.TProgramTasks.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ProgramTask>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ProgramTask>> GetDataListAsync(int? periodId = null, int? programId = null, int? responsibleMemberId = null, TaskProgressStatus? status = null,
            MilestoneFlag? isMilestone = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, programId, responsibleMemberId, status, isMilestone);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ProgramTask, EF.TProgramTask>();
        }
        public static async Task<Response<ProgramTask>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TProgramTasks.Add(row);
                await context.SaveChangesAsync();
                return new ProgramTask(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ProgramTask>(msg));
        }
        public async Task<Response<ProgramTask>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ProgramTask(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProgramTask>(msg));
        }
        public async Task<Response<ProgramTask>> SetStatusAsync(TaskProgressStatus status, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ApplyStatus(row, status, userId);
                await context.SaveChangesAsync();
                return new ProgramTask(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProgramTask>(msg));
        }
        public async Task<Response<ProgramTask>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var data = new ProgramTask(Reload(row.Id));
                context.TProgramTasks.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ProgramTask>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TProgramTask Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TProgramTask> FilterQuery(int? periodId, int? programId, int? responsibleMemberId, TaskProgressStatus? status, MilestoneFlag? isMilestone)
        {
            return _Querys.Where(x => (periodId == null || x.PeriodId == periodId)
                    && (programId == null || x.ProgramId == programId)
                    && (responsibleMemberId == null || x.ResponsibleMemberId == responsibleMemberId)
                    && (status == null || x.Status == (int)status)
                    && (isMilestone == null || x.IsMilestone == (int)isMilestone))
                .OrderBy(x => x.DueDate == null).ThenBy(x => x.DueDate).ThenByDescending(x => x.Priority).ThenBy(x => x.Title);
        }
        private static EF.TProgramTask GetRow(EF.ModelContext context, int id)
        {
            return context.TProgramTasks.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Tugas"));
        }
        private static async Task<EF.TProgramTask> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TProgramTasks.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Tugas"));
        }
        private static void ValidateData(string title, Priority priority, MilestoneFlag isMilestone, DateOnly? startDate, DateOnly? dueDate)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Tugas"));
            }
            if (!Enum.IsDefined(priority))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Prioritas"));
            }
            if (!Enum.IsDefined(isMilestone))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Penanda Milestone"));
            }
            if (startDate != null && dueDate != null && dueDate < startDate)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
        }
        private static EF.TProgramTask CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var title = newrow.Title?.Trim() ?? string.Empty;
            var programId = newrow.ProgramId > 0 ? newrow.ProgramId : null;
            int periodId;
            if (programId != null)
            {
                periodId = WorkProgram.GetOpenProgram(context, programId.Value).PeriodId;
            }
            else
            {
                periodId = newrow.PeriodId > 0 ? newrow.PeriodId.Value : throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Periode"));
                ManagementPeriod.ValidateOpenPeriod(context, periodId);
            }
            var divisionId = newrow.DivisionId > 0 ? newrow.DivisionId : null;
            if (divisionId != null)
            {
                Division.ValidateActiveDivision(context, divisionId.Value);
            }
            var responsibleId = newrow.ResponsibleMemberId > 0 ? newrow.ResponsibleMemberId : null;
            if (responsibleId != null)
            {
                Member.GetActiveMember(context, responsibleId.Value);
            }
            ValidateData(title, newrow.Priority, newrow.IsMilestone, newrow.StartDate, newrow.DueDate);

            var row = new EF.TProgramTask
            {
                Id = SequenceUtility.GetNextId<EF.TProgramTask>(context),
                PeriodId = periodId,
                ProgramId = programId,
                DivisionId = divisionId,
                ResponsibleMemberId = responsibleId,
                Title = title,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                Priority = (int)newrow.Priority,
                StartDate = newrow.StartDate,
                DueDate = newrow.DueDate,
                IsMilestone = (int)newrow.IsMilestone,
                Status = (int)TaskProgressStatus.Belum,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Periode dan program tetap; tugas Selesai / Batal dikunci (buka lagi lewat SetStatus Proses).
        private void SetRow(EF.ModelContext context, EF.TProgramTask row, int userId)
        {
            TransaksiHelper.ValidateStatus((TaskProgressStatus)row.Status, "mengubah tugas", TaskProgressStatus.Belum, TaskProgressStatus.Proses);
            var title = Title?.Trim() ?? string.Empty;
            var divisionId = DivisionId > 0 ? DivisionId : null;
            if (divisionId != null && divisionId != row.DivisionId)
            {
                Division.ValidateActiveDivision(context, divisionId.Value);
            }
            var responsibleId = ResponsibleMemberId > 0 ? ResponsibleMemberId : null;
            if (responsibleId != null && responsibleId != row.ResponsibleMemberId)
            {
                Member.GetActiveMember(context, responsibleId.Value);
            }
            ValidateData(title, Priority, IsMilestone, StartDate, DueDate);

            row.DivisionId = divisionId;
            row.ResponsibleMemberId = responsibleId;
            row.Title = title;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.Priority = (int)Priority;
            row.StartDate = StartDate;
            row.DueDate = DueDate;
            row.IsMilestone = (int)IsMilestone;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void ApplyStatus(EF.TProgramTask row, TaskProgressStatus target, int userId)
        {
            var current = (TaskProgressStatus)row.Status;
            switch (target)
            {
                case TaskProgressStatus.Proses:
                    TransaksiHelper.ValidateStatus(current, "memproses tugas", TaskProgressStatus.Belum, TaskProgressStatus.Selesai);
                    row.CompletedAt = null;
                    row.DoneBy = null;
                    break;
                case TaskProgressStatus.Selesai:
                    TransaksiHelper.ValidateStatus(current, "menyelesaikan tugas", TaskProgressStatus.Belum, TaskProgressStatus.Proses);
                    row.CompletedAt = DateTime.Now;
                    row.DoneBy = userId;
                    break;
                case TaskProgressStatus.Batal:
                    TransaksiHelper.ValidateStatus(current, "membatalkan tugas", TaskProgressStatus.Belum, TaskProgressStatus.Proses);
                    break;
                default:
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Tugas"));
            }
            row.Status = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
