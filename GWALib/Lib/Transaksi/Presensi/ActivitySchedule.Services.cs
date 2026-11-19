using GWALib.General;
using GWALib.General.Utilities;
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

namespace GWALib.Lib.Transaksi.Presensi
{
    public partial class ActivitySchedule
    {
        private const int MAX_TOLERANCE_MINUTE = 240;
        private const int MAX_GENERATE_DAYS = 366;
        // Sesi dibuat sejauh ini ke depan saat jadwal disimpan (job GENERATE_ATTENDANCE_SESSIONS melanjutkan berkala).
        public const int SESSION_DAYS_AHEAD = 7;

        #region SYNC
        public static List<ActivitySchedule> GetDataList(FilterAktif aktif, int? activityId = null)
        {
            return FilterQuery(aktif, activityId).ToList().MappingData<ActivitySchedule, EF.TActivitySchedule>();
        }
        // Jadwal tersimpan langsung punya sesi presensi untuk SESSION_DAYS_AHEAD hari ke depan.
        public static Response<ActivitySchedule> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TActivitySchedules.Add(row);
                context.SaveChanges();
                Refresh(context, row, userId);
                return new ActivitySchedule(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivitySchedule>(msg));
        }
        public Response<ActivitySchedule> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                Refresh(context, row, userId);
                return new ActivitySchedule(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivitySchedule>(msg));
        }
        // Nonaktif; sesi mendatang yang belum dipakai ikut dihapus / dibatalkan.
        public Response<ActivitySchedule> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                row.IsActive = (int)ActiveStatus.TidakAktif;
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                Refresh(context, row, userId);
                return new ActivitySchedule(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ActivitySchedule>(msg));
        }
        // Membuat sesi presensi dari jadwal aktif untuk rentang tanggal (activityId null = semua kegiatan); hasil = jumlah sesi baru.
        // skipEnded: lewati sesi yang jam selesainya sudah lewat (job), supaya tidak langsung ditutup dengan Alpa.
        public static Response<int> GenerateSessions(DateOnly from, DateOnly to, int? activityId, string user, bool skipEnded = false)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = Generate(context, from, to, activityId, userId, skipEnded);
                context.SaveChanges();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ActivitySchedule>> GetDataListAsync(FilterAktif aktif, int? activityId = null)
        {
            return await (await FilterQuery(aktif, activityId).ToListAsync()).MappingDataAsync<ActivitySchedule, EF.TActivitySchedule>();
        }
        public static async Task<Response<ActivitySchedule>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TActivitySchedules.Add(row);
                await context.SaveChangesAsync();
                Refresh(context, row, userId);
                return new ActivitySchedule(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivitySchedule>(msg));
        }
        public async Task<Response<ActivitySchedule>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                Refresh(context, row, userId);
                return new ActivitySchedule(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivitySchedule>(msg));
        }
        public async Task<Response<ActivitySchedule>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                row.IsActive = (int)ActiveStatus.TidakAktif;
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                Refresh(context, row, userId);
                return new ActivitySchedule(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ActivitySchedule>(msg));
        }
        public static async Task<Response<int>> GenerateSessionsAsync(DateOnly from, DateOnly to, int? activityId, string user, bool skipEnded = false)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = Generate(context, from, to, activityId, userId, skipEnded);
                await context.SaveChangesAsync();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TActivitySchedule> FilterQuery(FilterAktif aktif, int? activityId)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (activityId == null || x.ActivityId == activityId))
                .OrderBy(x => x.ActivityId).ThenBy(x => x.DayOfWeek).ThenBy(x => x.StartTime);
        }
        private static EF.TActivitySchedule GetRow(EF.ModelContext context, int id)
        {
            return context.TActivitySchedules.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Jadwal Rutin"));
        }
        private static async Task<EF.TActivitySchedule> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TActivitySchedules.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Jadwal Rutin"));
        }
        // ScheduleDay 1=Senin..7=Minggu, bukan System.DayOfWeek (0=Minggu).
        private static ScheduleDay ToScheduleDay(DateOnly date)
        {
            return date.DayOfWeek == System.DayOfWeek.Sunday ? ScheduleDay.Minggu : (ScheduleDay)(int)date.DayOfWeek;
        }
        private static void ValidateData(ScheduleDay dayOfWeek, TimeOnly startTime, TimeOnly endTime, decimal? latitude, decimal? longitude, int? radiusMeter,
            int lateToleranceMinute, AttendanceMode mode, DateOnly validFrom, DateOnly? validUntil)
        {
            if (!Enum.IsDefined(dayOfWeek))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Hari"));
            }
            if (!Enum.IsDefined(mode))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Mode Presensi"));
            }
            if (endTime <= startTime)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            if (lateToleranceMinute < 0 || lateToleranceMinute > MAX_TOLERANCE_MINUTE)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Toleransi Terlambat"));
            }
            if (validFrom == default || (validUntil != null && validUntil < validFrom))
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            GeoFence.ValidateRequiredArea(latitude, longitude, radiusMeter);
        }
        private static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        private static EF.TActivitySchedule CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            Activity.GetOpenActivity(context, newrow.ActivityId);
            ValidateData(newrow.DayOfWeek, newrow.StartTime, newrow.EndTime, newrow.Latitude, newrow.Longitude, newrow.RadiusMeter,
                newrow.LateToleranceMinute, newrow.AttendanceMode, newrow.ValidFrom, newrow.ValidUntil);

            var row = new EF.TActivitySchedule
            {
                Id = SequenceUtility.GetNextId<EF.TActivitySchedule>(context),
                ActivityId = newrow.ActivityId,
                DayOfWeek = (int)newrow.DayOfWeek,
                StartTime = newrow.StartTime,
                EndTime = newrow.EndTime,
                Location = ToNullable(newrow.Location),
                Latitude = newrow.Latitude,
                Longitude = newrow.Longitude,
                RadiusMeter = newrow.RadiusMeter,
                LateToleranceMinute = newrow.LateToleranceMinute,
                AttendanceMode = (int)newrow.AttendanceMode,
                ValidFrom = newrow.ValidFrom,
                ValidUntil = newrow.ValidUntil,
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Kegiatan tetap. Sesi mendatang yang belum dipakai disesuaikan oleh Refresh.
        private void SetRow(EF.ModelContext context, EF.TActivitySchedule row, int userId)
        {
            Activity.GetOpenActivity(context, row.ActivityId);
            ValidateData(DayOfWeek, StartTime, EndTime, Latitude, Longitude, RadiusMeter, LateToleranceMinute, AttendanceMode, ValidFrom, ValidUntil);
            if (!Enum.IsDefined(IsActive))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status"));
            }

            row.DayOfWeek = (int)DayOfWeek;
            row.StartTime = StartTime;
            row.EndTime = EndTime;
            row.Location = ToNullable(Location);
            row.Latitude = Latitude;
            row.Longitude = Longitude;
            row.RadiusMeter = RadiusMeter;
            row.LateToleranceMinute = LateToleranceMinute;
            row.AttendanceMode = (int)AttendanceMode;
            row.ValidFrom = ValidFrom;
            row.ValidUntil = ValidUntil;
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        // Setelah jadwal disimpan: sesuaikan sesi mendatang, lalu buat sesi SESSION_DAYS_AHEAD hari ke depan (keduanya disimpan).
        private static void Refresh(EF.ModelContext context, EF.TActivitySchedule row, int userId)
        {
            SyncFutureSessions(context, row, userId);
            context.SaveChanges();
            var today = DateOnly.FromDateTime(DateTime.Today);
            Generate(context, today, today.AddDays(SESSION_DAYS_AHEAD), row.ActivityId, userId, skipEnded: true);
            context.SaveChanges();
        }
        // Sesi mendatang dari jadwal ini yang masih Terjadwal dan belum ada presensinya mengikuti jadwal terbaru: diubah bila
        // tanggalnya masih cocok; selain itu dihapus, atau dibatalkan bila sudah dirujuk pengajuan izin / notulen.
        private static void SyncFutureSessions(EF.ModelContext context, EF.TActivitySchedule row, int userId)
        {
            var now = DateTime.Now;
            var sessions = context.TAttendanceSessions
                .Where(x => x.ScheduleId == row.Id && x.Status == (int)AttendanceSessionStatus.Scheduled && x.OpenAt > now && !x.TAttendanceRecords.Any())
                .ToList();
            if (sessions.Count == 0)
            {
                return;
            }
            var ids = sessions.Select(x => x.Id).ToList();
            var used = context.TActivityAbsenceRequests.Where(x => x.SessionId != null && ids.Contains(x.SessionId.Value)).Select(x => x.SessionId!.Value).ToList()
                .Concat(context.TMeetingMinutes.Where(x => ids.Contains(x.SessionId)).Select(x => x.SessionId).ToList())
                .ToHashSet();
            foreach (var session in sessions)
            {
                var date = DateOnly.FromDateTime(session.OpenAt);
                if (row.IsActive == (int)ActiveStatus.Aktif && (int)ToScheduleDay(date) == row.DayOfWeek && row.ValidFrom <= date
                    && (row.ValidUntil == null || row.ValidUntil >= date))
                {
                    session.OpenAt = date.ToDateTime(row.StartTime);
                    session.CloseAt = date.ToDateTime(row.EndTime);
                    session.LateToleranceMinute = row.LateToleranceMinute;
                    session.AttendanceMode = row.AttendanceMode;
                    session.Location = row.Location;
                    session.Latitude = row.Latitude;
                    session.Longitude = row.Longitude;
                    session.RadiusMeter = row.RadiusMeter;
                }
                else if (used.Contains(session.Id))
                {
                    session.Status = (int)AttendanceSessionStatus.Cancelled;
                    session.CancelReason = "Jadwal rutin diubah / dinonaktifkan";
                }
                else
                {
                    context.TAttendanceSessions.Remove(session);
                    continue;
                }
                session.UpdDate = now;
                session.UpdBy = userId;
            }
        }
        // Hanya kegiatan Planned / Ongoing; tanggal di luar ValidFrom..ValidUntil dilewati, sesi yang sudah ada tidak dibuat ulang.
        private static int Generate(EF.ModelContext context, DateOnly from, DateOnly to, int? activityId, int userId, bool skipEnded)
        {
            var now = DateTime.Now;
            if (to < from || to.DayNumber - from.DayNumber > MAX_GENERATE_DAYS)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            var schedules = context.TActivitySchedules.Include(x => x.Activity)
                .Where(x => x.IsActive == (int)ActiveStatus.Aktif && (activityId == null || x.ActivityId == activityId)
                    && (x.Activity.Status == (int)ProgramStatus.Planned || x.Activity.Status == (int)ProgramStatus.Ongoing))
                .ToList();
            var scheduleIds = schedules.Select(x => x.Id).ToList();
            var fromAt = from.ToDateTime(TimeOnly.MinValue);
            var toAt = to.ToDateTime(TimeOnly.MaxValue);
            var existing = context.TAttendanceSessions
                .Where(x => x.ScheduleId != null && scheduleIds.Contains(x.ScheduleId.Value) && x.OpenAt >= fromAt && x.OpenAt <= toAt)
                .Select(x => new { ScheduleId = x.ScheduleId!.Value, x.OpenAt }).ToList()
                .Select(x => (x.ScheduleId, x.OpenAt)).ToHashSet();

            var rows = new List<EF.TAttendanceSession>();
            for (var date = from; date <= to; date = date.AddDays(1))
            {
                var day = (int)ToScheduleDay(date);
                foreach (var schedule in schedules.Where(x => x.DayOfWeek == day && x.ValidFrom <= date && (x.ValidUntil == null || x.ValidUntil >= date)))
                {
                    var openAt = date.ToDateTime(schedule.StartTime);
                    var closeAt = date.ToDateTime(schedule.EndTime);
                    if (existing.Contains((schedule.Id, openAt)) || (skipEnded && closeAt <= now))
                    {
                        continue;
                    }
                    rows.Add(new EF.TAttendanceSession
                    {
                        ActivityId = schedule.ActivityId,
                        ScheduleId = schedule.Id,
                        Name = TransaksiHelper.Truncate($"{schedule.Activity.Title} - {date:dd/MM/yyyy}", 150)!,
                        OpenAt = openAt,
                        CloseAt = closeAt,
                        LateToleranceMinute = schedule.LateToleranceMinute,
                        AttendanceMode = schedule.AttendanceMode,
                        Location = schedule.Location,
                        Latitude = schedule.Latitude,
                        Longitude = schedule.Longitude,
                        RadiusMeter = schedule.RadiusMeter,
                        Status = (int)AttendanceSessionStatus.Scheduled,
                        InsDate = DateTime.Now,
                        InsBy = userId
                    });
                }
            }
            if (rows.Count == 0)
            {
                return 0;
            }
            var firstId = SequenceUtility.ReserveIds<EF.TAttendanceSession>(context, rows.Count);
            for (var i = 0; i < rows.Count; i++)
            {
                rows[i].Id = firstId + i;
            }
            context.TAttendanceSessions.AddRange(rows);
            return rows.Count;
        }
        #endregion
    }
}
