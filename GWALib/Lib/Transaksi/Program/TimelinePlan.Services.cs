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
    public partial class TimelinePlan
    {
        private const int MIN_YEAR = 2000;

        #region SYNC
        public static List<TimelinePlan> GetDataList(int? periodId = null, short? planYear = null, int? planMonth = null, TimelinePlanStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, planYear, planMonth, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<TimelinePlan, EF.TTimelinePlan>();
        }
        public static Response<TimelinePlan> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TTimelinePlans.Add(row);
                context.SaveChanges();
                return new TimelinePlan(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<TimelinePlan>(msg));
        }
        public Response<TimelinePlan> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new TimelinePlan(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<TimelinePlan>(msg));
        }
        // Rencana dijadikan kegiatan: status Terjadwal, tanggal ikut kegiatan.
        public Response<TimelinePlan> LinkActivity(int activityId, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetLinkActivity(context, row, activityId, userId);
                context.SaveChanges();
                return new TimelinePlan(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<TimelinePlan>(msg));
        }
        public Response<TimelinePlan> SetStatus(TimelinePlanStatus status, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ApplyStatus(row, status, userId);
                context.SaveChanges();
                return new TimelinePlan(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<TimelinePlan>(msg));
        }
        public Response<TimelinePlan> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((TimelinePlanStatus)row.Status, "menghapus rencana", TimelinePlanStatus.Rencana, TimelinePlanStatus.Batal);
                var data = new TimelinePlan(Reload(row.Id));
                context.TTimelinePlans.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<TimelinePlan>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<TimelinePlan>> GetDataListAsync(int? periodId = null, short? planYear = null, int? planMonth = null, TimelinePlanStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, planYear, planMonth, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<TimelinePlan, EF.TTimelinePlan>();
        }
        public static async Task<Response<TimelinePlan>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TTimelinePlans.Add(row);
                await context.SaveChangesAsync();
                return new TimelinePlan(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<TimelinePlan>(msg));
        }
        public async Task<Response<TimelinePlan>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new TimelinePlan(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<TimelinePlan>(msg));
        }
        public async Task<Response<TimelinePlan>> LinkActivityAsync(int activityId, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetLinkActivity(context, row, activityId, userId);
                await context.SaveChangesAsync();
                return new TimelinePlan(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<TimelinePlan>(msg));
        }
        public async Task<Response<TimelinePlan>> SetStatusAsync(TimelinePlanStatus status, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ApplyStatus(row, status, userId);
                await context.SaveChangesAsync();
                return new TimelinePlan(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<TimelinePlan>(msg));
        }
        public async Task<Response<TimelinePlan>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                TransaksiHelper.ValidateStatus((TimelinePlanStatus)row.Status, "menghapus rencana", TimelinePlanStatus.Rencana, TimelinePlanStatus.Batal);
                var data = new TimelinePlan(Reload(row.Id));
                context.TTimelinePlans.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<TimelinePlan>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TTimelinePlan Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TTimelinePlan> FilterQuery(int? periodId, short? planYear, int? planMonth, TimelinePlanStatus? status)
        {
            return _Querys.Where(x => (periodId == null || x.PeriodId == periodId)
                    && (planYear == null || x.PlanYear == planYear)
                    && (planMonth == null || x.PlanMonth == planMonth)
                    && (status == null || x.Status == (int)status))
                .OrderBy(x => x.PlanYear).ThenBy(x => x.PlanMonth).ThenBy(x => x.StartDate == null).ThenBy(x => x.StartDate).ThenBy(x => x.Title);
        }
        private static EF.TTimelinePlan GetRow(EF.ModelContext context, int id)
        {
            return context.TTimelinePlans.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Rencana Timeline"));
        }
        private static async Task<EF.TTimelinePlan> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TTimelinePlans.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Rencana Timeline"));
        }
        // Tahun / bulan mengikuti StartDate bila diisi.
        private static (short Year, int Month) ValidateData(EF.ModelContext context, string title, TimelinePlanType planType, short planYear, int planMonth,
            DateOnly? startDate, DateOnly? endDate, int? programId, int? picMemberId)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Rencana"));
            }
            if (!Enum.IsDefined(planType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Rencana"));
            }
            if (startDate != null)
            {
                planYear = (short)startDate.Value.Year;
                planMonth = startDate.Value.Month;
            }
            if (planYear < MIN_YEAR || planMonth < 1 || planMonth > 12)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Bulan Rencana"));
            }
            if (endDate != null && (startDate == null || endDate < startDate))
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            if (programId != null && !context.TPrograms.Any(x => x.Id == programId))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Program Kerja"));
            }
            if (picMemberId != null)
            {
                Member.GetActiveMember(context, picMemberId.Value);
            }
            return (planYear, planMonth);
        }
        private static EF.TTimelinePlan CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var title = newrow.Title?.Trim() ?? string.Empty;
            var periodId = newrow.PeriodId > 0 ? newrow.PeriodId : null;
            if (periodId != null)
            {
                ManagementPeriod.ValidateOpenPeriod(context, periodId.Value);
            }
            var programId = newrow.ProgramId > 0 ? newrow.ProgramId : null;
            var picMemberId = newrow.PicMemberId > 0 ? newrow.PicMemberId : null;
            var month = ValidateData(context, title, newrow.PlanType, newrow.PlanYear, newrow.PlanMonth, newrow.StartDate, newrow.EndDate, programId, picMemberId);

            var row = new EF.TTimelinePlan
            {
                Id = SequenceUtility.GetNextId<EF.TTimelinePlan>(context),
                PeriodId = periodId,
                Title = title,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                PlanType = (int)newrow.PlanType,
                PlanYear = month.Year,
                PlanMonth = month.Month,
                StartDate = newrow.StartDate,
                EndDate = newrow.EndDate,
                Status = (int)TimelinePlanStatus.Rencana,
                ProgramId = programId,
                PicMemberId = picMemberId,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Periode tetap; rencana Selesai / Batal dikunci.
        private void SetRow(EF.ModelContext context, EF.TTimelinePlan row, int userId)
        {
            TransaksiHelper.ValidateStatus((TimelinePlanStatus)row.Status, "mengubah rencana", TimelinePlanStatus.Rencana, TimelinePlanStatus.Terjadwal);
            var title = Title?.Trim() ?? string.Empty;
            var programId = ProgramId > 0 ? ProgramId : null;
            var picMemberId = PicMemberId > 0 ? PicMemberId : null;
            var month = ValidateData(context, title, PlanType, PlanYear, PlanMonth, StartDate, EndDate,
                programId, picMemberId != row.PicMemberId ? picMemberId : null);

            row.Title = title;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.PlanType = (int)PlanType;
            row.PlanYear = month.Year;
            row.PlanMonth = month.Month;
            row.StartDate = StartDate;
            row.EndDate = EndDate;
            row.ProgramId = programId;
            row.PicMemberId = picMemberId;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetLinkActivity(EF.ModelContext context, EF.TTimelinePlan row, int activityId, int userId)
        {
            TransaksiHelper.ValidateStatus((TimelinePlanStatus)row.Status, "menjadwalkan rencana", TimelinePlanStatus.Rencana, TimelinePlanStatus.Terjadwal);
            var activity = Activity.GetActivity(context, activityId);
            var start = DateOnly.FromDateTime(activity.StartAt);
            row.ActivityId = activity.Id;
            row.ProgramId ??= activity.ProgramId;
            row.StartDate = start;
            row.EndDate = activity.EndAt != null ? DateOnly.FromDateTime(activity.EndAt.Value) : null;
            row.PlanYear = (short)start.Year;
            row.PlanMonth = start.Month;
            row.Status = (int)TimelinePlanStatus.Terjadwal;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        // Selesai dari Rencana / Terjadwal; Batal dari Rencana / Terjadwal. Terjadwal lewat LinkActivity.
        private static void ApplyStatus(EF.TTimelinePlan row, TimelinePlanStatus target, int userId)
        {
            var current = (TimelinePlanStatus)row.Status;
            if (target == TimelinePlanStatus.Selesai)
            {
                TransaksiHelper.ValidateStatus(current, "menyelesaikan rencana", TimelinePlanStatus.Rencana, TimelinePlanStatus.Terjadwal);
            }
            else if (target == TimelinePlanStatus.Batal)
            {
                TransaksiHelper.ValidateStatus(current, "membatalkan rencana", TimelinePlanStatus.Rencana, TimelinePlanStatus.Terjadwal);
            }
            else
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Rencana"));
            }
            row.Status = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
