using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Master.Prestasi;
using GWALib.Lib.Transaksi.Keuangan;
using GWALib.Lib.Transaksi.Program;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Prestasi
{
    public partial class MemberPoint
    {
        #region SYNC
        public static List<MemberPoint> GetDataList(int? memberId = null, int? periodId = null, PointSourceType? sourceType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(memberId, periodId, sourceType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<MemberPoint, EF.TMemberPoint>();
        }
        // Peringkat total poin anggota (periode null = semua periode).
        public static List<LeaderboardRow> GetLeaderboard(int? periodId = null, int top = 10)
        {
            return LeaderboardQuery(periodId).Take(top).ToList();
        }
        public static Response<MemberPoint> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateManualRow(context, newrow, userId);
                context.SaveChanges();
                return new MemberPoint(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<MemberPoint>(msg));
        }
        // Hanya poin, tanggal, periode dan catatan.
        public Response<MemberPoint> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new MemberPoint(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MemberPoint>(msg));
        }
        public Response<MemberPoint> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var data = new MemberPoint(Reload(row.Id));
                context.TMemberPoints.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<MemberPoint>(msg));
        }
        // Poin Hadir dan Panitia satu kegiatan; mengembalikan jumlah poin yang dibuat / diperbarui.
        public static Response<int> GenerateForActivity(int activityId, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = AwardActivity(context, activityId, userId);
                context.SaveChanges();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        // Poin Pengurus untuk penugasan pada periode.
        public static Response<int> GenerateForPeriod(int periodId, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = AwardPeriod(context, periodId, userId);
                context.SaveChanges();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<MemberPoint>> GetDataListAsync(int? memberId = null, int? periodId = null, PointSourceType? sourceType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(memberId, periodId, sourceType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<MemberPoint, EF.TMemberPoint>();
        }
        public static async Task<List<LeaderboardRow>> GetLeaderboardAsync(int? periodId = null, int top = 10)
        {
            return await LeaderboardQuery(periodId).Take(top).ToListAsync();
        }
        public static async Task<Response<MemberPoint>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateManualRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new MemberPoint(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<MemberPoint>(msg));
        }
        public async Task<Response<MemberPoint>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new MemberPoint(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MemberPoint>(msg));
        }
        public async Task<Response<MemberPoint>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var data = new MemberPoint(Reload(row.Id));
                context.TMemberPoints.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<MemberPoint>(msg));
        }
        public static async Task<Response<int>> GenerateForActivityAsync(int activityId, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = AwardActivity(context, activityId, userId);
                await context.SaveChangesAsync();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        public static async Task<Response<int>> GenerateForPeriodAsync(int periodId, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = AwardPeriod(context, periodId, userId);
                await context.SaveChangesAsync();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        #endregion

        #region INTERNAL
        // Membuat / memperbarui poin dari aturan efektif; tanpa aturan aktif tidak ada poin (baris lama tetap). true bila ada poin.
        internal static bool Award(EF.ModelContext context, int memberId, PointSourceType sourceType, int refId, ActivityType? activityType, EventLevel? level,
            DateOnly earnedDate, int? periodId, string? notes, int userId)
        {
            var rule = PointRule.GetEffectiveRow(context, sourceType, activityType, level);
            if (rule == null)
            {
                return false;
            }
            var now = DateTime.Now;
            var row = context.TMemberPoints.Local.SingleOrDefault(x => x.MemberId == memberId && x.SourceType == (int)sourceType && x.RefId == refId)
                ?? context.TMemberPoints.SingleOrDefault(x => x.MemberId == memberId && x.SourceType == (int)sourceType && x.RefId == refId);
            if (row == null)
            {
                row = new EF.TMemberPoint
                {
                    Id = SequenceUtility.GetNextId<EF.TMemberPoint>(context),
                    MemberId = memberId,
                    SourceType = (int)sourceType,
                    RefId = refId,
                    InsDate = now,
                    InsBy = userId
                };
                context.TMemberPoints.Add(row);
            }
            else
            {
                row.UpdDate = now;
                row.UpdBy = userId;
            }
            row.PeriodId = periodId ?? FinancialTransaction.GetActivePeriodId(context);
            row.Points = rule.Points;
            row.EarnedDate = earnedDate;
            row.Notes = TransaksiHelper.Truncate(notes, 255);
            return true;
        }
        internal static void Revoke(EF.ModelContext context, PointSourceType sourceType, int refId)
        {
            context.TMemberPoints.RemoveRange(context.TMemberPoints.Where(x => x.SourceType == (int)sourceType && x.RefId == refId));
        }
        #endregion

        #region HELPER
        private static EF.TMemberPoint Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TMemberPoint> FilterQuery(int? memberId, int? periodId, PointSourceType? sourceType)
        {
            return _Querys.Where(x => (memberId == null || x.MemberId == memberId)
                    && (periodId == null || x.PeriodId == periodId)
                    && (sourceType == null || x.SourceType == (int)sourceType))
                .OrderByDescending(x => x.EarnedDate).ThenByDescending(x => x.Id);
        }
        private static IQueryable<LeaderboardRow> LeaderboardQuery(int? periodId)
        {
            return DBClass.GetContext().TMemberPoints
                .Where(x => periodId == null || x.PeriodId == periodId)
                .GroupBy(x => new { x.MemberId, x.Member.Fullname })
                .Select(g => new LeaderboardRow(g.Key.MemberId, g.Key.Fullname, g.Sum(x => x.Points)))
                .OrderByDescending(x => x.TotalPoints).ThenBy(x => x.MemberName);
        }
        private static EF.TMemberPoint GetRow(EF.ModelContext context, int id)
        {
            return context.TMemberPoints.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Poin Anggota"));
        }
        private static async Task<EF.TMemberPoint> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TMemberPoints.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Poin Anggota"));
        }
        private static int? ValidateData(EF.ModelContext context, int? periodId, int points, DateOnly earnedDate)
        {
            if (points == 0)
            {
                throw new ArgumentException(ExceptionMessage.POINT_ZERO);
            }
            if (earnedDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Poin"));
            }
            periodId = (periodId > 0 ? periodId : null) ?? FinancialTransaction.GetActivePeriodId(context);
            if (periodId != null && !context.MManagementPeriods.Any(x => x.Id == periodId))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Periode Kepengurusan"));
            }
            return periodId;
        }
        private static EF.TMemberPoint CreateManualRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            if (!Enum.IsDefined(newrow.SourceType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Sumber Poin"));
            }
            Member.GetMember(context, newrow.MemberId);
            var periodId = ValidateData(context, newrow.PeriodId, newrow.Points, newrow.EarnedDate);
            var refId = newrow.RefId > 0 ? newrow.RefId : null;
            if (refId != null)
            {
                ValidateUtility.CheckDuplicateData<EF.TMemberPoint>(context, x => x.MemberId == newrow.MemberId && x.SourceType == (int)newrow.SourceType && x.RefId == refId,
                    string.Format(ExceptionMessage.DATA_DUPLICATE, "anggota, sumber dan referensi"));
            }
            var row = new EF.TMemberPoint
            {
                Id = SequenceUtility.GetNextId<EF.TMemberPoint>(context),
                MemberId = newrow.MemberId,
                PeriodId = periodId,
                SourceType = (int)newrow.SourceType,
                RefId = refId,
                Points = newrow.Points,
                EarnedDate = newrow.EarnedDate,
                Notes = TransaksiHelper.Truncate(newrow.Notes, 255),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TMemberPoints.Add(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.TMemberPoint row, int userId)
        {
            row.PeriodId = ValidateData(context, PeriodId, Points, EarnedDate);
            row.Points = Points;
            row.EarnedDate = EarnedDate;
            row.Notes = TransaksiHelper.Truncate(Notes, 255);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static int AwardActivity(EF.ModelContext context, int activityId, int userId)
        {
            var activity = Activity.GetActivity(context, activityId);
            var activityType = (ActivityType)activity.ActivityType;
            var earned = DateOnly.FromDateTime(activity.StartAt);
            var count = 0;
            var attendance = context.TAttendanceRecords
                .Where(x => x.Session.ActivityId == activityId && (x.Status == (int)AttendanceStatus.Hadir || x.Status == (int)AttendanceStatus.Terlambat))
                .Select(x => new { x.Id, x.MemberId, x.Session.OpenAt })
                .ToList();
            foreach (var record in attendance)
            {
                if (Award(context, record.MemberId, PointSourceType.HadirKegiatan, record.Id, activityType, null, DateOnly.FromDateTime(record.OpenAt),
                    null, "Hadir: " + activity.Title, userId))
                {
                    count++;
                }
            }
            var committee = context.TCommitteeMembers.Where(x => x.ActivityId == activityId && x.Status == (int)CommitteeMemberStatus.Aktif)
                .Select(x => new { x.Id, x.MemberId }).ToList();
            foreach (var member in committee)
            {
                if (Award(context, member.MemberId, PointSourceType.Panitia, member.Id, activityType, null, earned, null, "Panitia: " + activity.Title, userId))
                {
                    count++;
                }
            }
            return count;
        }
        private static int AwardPeriod(EF.ModelContext context, int periodId, int userId)
        {
            if (!context.MManagementPeriods.Any(x => x.Id == periodId))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Periode Kepengurusan"));
            }
            var count = 0;
            var assignments = context.TManagementAssignments.Where(x => x.PeriodId == periodId)
                .Select(x => new { x.Id, x.MemberId, x.StartDate, PositionName = x.Position.Name }).ToList();
            foreach (var assignment in assignments)
            {
                if (Award(context, assignment.MemberId, PointSourceType.Pengurus, assignment.Id, null, null, assignment.StartDate, periodId,
                    "Pengurus: " + assignment.PositionName, userId))
                {
                    count++;
                }
            }
            return count;
        }
        #endregion
    }
}
