using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Transaksi.Program;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.TimPelaksana
{
    public partial class ActivityRundown
    {
        #region SYNC
        public static List<ActivityRundown> GetDataList(int activityId, DateOnly? rundownDate = null)
        {
            return FilterQuery(activityId, rundownDate).ToList().MappingData<ActivityRundown, EF.TActivityRundown>();
        }
        public static Response<ActivityRundown> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TActivityRundowns.Add(row);
                context.SaveChanges();
                return new ActivityRundown(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivityRundown>(msg));
        }
        public Response<ActivityRundown> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ActivityRundown(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityRundown>(msg));
        }
        public Response<ActivityRundown> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                Activity.GetOpenActivity(context, row.ActivityId);
                var data = new ActivityRundown(Reload(row.Id));
                context.TActivityRundowns.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ActivityRundown>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ActivityRundown>> GetDataListAsync(int activityId, DateOnly? rundownDate = null)
        {
            return await (await FilterQuery(activityId, rundownDate).ToListAsync()).MappingDataAsync<ActivityRundown, EF.TActivityRundown>();
        }
        public static async Task<Response<ActivityRundown>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TActivityRundowns.Add(row);
                await context.SaveChangesAsync();
                return new ActivityRundown(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivityRundown>(msg));
        }
        public async Task<Response<ActivityRundown>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ActivityRundown(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityRundown>(msg));
        }
        public async Task<Response<ActivityRundown>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                Activity.GetOpenActivity(context, row.ActivityId);
                var data = new ActivityRundown(Reload(row.Id));
                context.TActivityRundowns.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ActivityRundown>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TActivityRundown Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TActivityRundown> FilterQuery(int activityId, DateOnly? rundownDate)
        {
            return _Querys.Where(x => x.ActivityId == activityId && (rundownDate == null || x.RundownDate == rundownDate))
                .OrderBy(x => x.RundownDate).ThenBy(x => x.StartTime).ThenBy(x => x.SeqNo);
        }
        private static EF.TActivityRundown GetRow(EF.ModelContext context, int id)
        {
            return context.TActivityRundowns.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Rundown"));
        }
        private static async Task<EF.TActivityRundown> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TActivityRundowns.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Rundown"));
        }
        private static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        private static void ValidateData(EF.ModelContext context, int activityId, DateOnly rundownDate, TimeOnly startTime, TimeOnly? endTime,
            string title, int? sectionId, int? picMemberId)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Acara"));
            }
            if (rundownDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Rundown"));
            }
            if (endTime != null && endTime < startTime)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            if (sectionId != null)
            {
                CommitteeSection.GetSection(context, sectionId.Value, activityId);
            }
            if (picMemberId != null)
            {
                Member.GetActiveMember(context, picMemberId.Value);
            }
        }
        private static int NextSeq(EF.ModelContext context, int activityId, DateOnly rundownDate)
        {
            return ValueUtility.GetNextId<EF.TActivityRundown>(context, x => x.SeqNo, x => x.ActivityId == activityId && x.RundownDate == rundownDate);
        }
        private static EF.TActivityRundown CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            Activity.GetOpenActivity(context, newrow.ActivityId);
            var title = newrow.Title?.Trim() ?? string.Empty;
            var sectionId = newrow.SectionId > 0 ? newrow.SectionId : null;
            var picMemberId = newrow.PicMemberId > 0 ? newrow.PicMemberId : null;
            ValidateData(context, newrow.ActivityId, newrow.RundownDate, newrow.StartTime, newrow.EndTime, title, sectionId, picMemberId);

            var row = new EF.TActivityRundown
            {
                Id = SequenceUtility.GetNextId<EF.TActivityRundown>(context),
                ActivityId = newrow.ActivityId,
                RundownDate = newrow.RundownDate,
                StartTime = newrow.StartTime,
                EndTime = newrow.EndTime,
                Title = title,
                Description = ToNullable(newrow.Description),
                LocationName = ToNullable(newrow.LocationName),
                SectionId = sectionId,
                PicMemberId = picMemberId,
                SeqNo = newrow.SeqNo > 0 ? newrow.SeqNo : NextSeq(context, newrow.ActivityId, newrow.RundownDate),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Kegiatan tetap.
        private void SetRow(EF.ModelContext context, EF.TActivityRundown row, int userId)
        {
            Activity.GetOpenActivity(context, row.ActivityId);
            var title = Title?.Trim() ?? string.Empty;
            var sectionId = SectionId > 0 ? SectionId : null;
            var picMemberId = PicMemberId > 0 ? PicMemberId : null;
            ValidateData(context, row.ActivityId, RundownDate, StartTime, EndTime, title, sectionId, picMemberId != row.PicMemberId ? picMemberId : null);

            row.RundownDate = RundownDate;
            row.StartTime = StartTime;
            row.EndTime = EndTime;
            row.Title = title;
            row.Description = ToNullable(Description);
            row.LocationName = ToNullable(LocationName);
            row.SectionId = sectionId;
            row.PicMemberId = picMemberId;
            row.SeqNo = SeqNo > 0 ? SeqNo : NextSeq(context, row.ActivityId, RundownDate);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        #endregion
    }
}
