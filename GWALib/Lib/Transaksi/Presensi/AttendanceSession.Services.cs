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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Presensi
{
    public partial class AttendanceSession
    {
        private const int MAX_TOLERANCE_MINUTE = 240;
        private const int REASON_LENGTH = 255;

        #region SYNC
        public static List<AttendanceSession> GetDataList(int? activityId = null, AttendanceSessionStatus? status = null, DateTime? from = null, DateTime? to = null,
            int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(activityId, status, from, to);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<AttendanceSession, EF.TAttendanceSession>();
        }
        public static Response<AttendanceSession> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TAttendanceSessions.Add(row);
                context.SaveChanges();
                return new AttendanceSession(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AttendanceSession>(msg));
        }
        public Response<AttendanceSession> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(row, userId);
                context.SaveChanges();
                return new AttendanceSession(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AttendanceSession>(msg));
        }
        // Membuka sesi dan membuat token QR baru; dipanggil lagi saat Open = ganti token (mis. QR tersebar).
        public Response<AttendanceSession> Open(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetOpen(context, row, userId);
                context.SaveChanges();
                return new AttendanceSession(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AttendanceSession>(msg));
        }
        // Menutup sesi; anggota sasaran yang belum presensi dicatat ALPA. Hasil = jumlah ALPA baru.
        public Response<int> Close(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var count = SetClose(context, row, userId);
                context.SaveChanges();
                return count;
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<int>(msg));
        }
        public Response<AttendanceSession> Cancel(string reason, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetCancel(context, row, reason, userId);
                context.SaveChanges();
                return new AttendanceSession(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AttendanceSession>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<AttendanceSession>> GetDataListAsync(int? activityId = null, AttendanceSessionStatus? status = null, DateTime? from = null, DateTime? to = null,
            int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(activityId, status, from, to);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<AttendanceSession, EF.TAttendanceSession>();
        }
        public static async Task<Response<AttendanceSession>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TAttendanceSessions.Add(row);
                await context.SaveChangesAsync();
                return new AttendanceSession(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AttendanceSession>(msg));
        }
        public async Task<Response<AttendanceSession>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(row, userId);
                await context.SaveChangesAsync();
                return new AttendanceSession(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AttendanceSession>(msg));
        }
        public async Task<Response<AttendanceSession>> OpenAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetOpen(context, row, userId);
                await context.SaveChangesAsync();
                return new AttendanceSession(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AttendanceSession>(msg));
        }
        public async Task<Response<int>> CloseAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var count = SetClose(context, row, userId);
                await context.SaveChangesAsync();
                return count;
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<int>(msg));
        }
        public async Task<Response<AttendanceSession>> CancelAsync(string reason, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetCancel(context, row, reason, userId);
                await context.SaveChangesAsync();
                return new AttendanceSession(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AttendanceSession>(msg));
        }
        #endregion

        #region INTERNAL
        // Anggota yang wajib hadir sesuai audiens kegiatan (penentu siapa yang dicatat ALPA). Section memakai riwayat section pada tanggal sesi.
        // excludeAbsent: buang anggota yang disetujui tidak ikut seluruh kegiatan (ActivityAbsenceRequest); false untuk cek audiens (check-in).
        internal static List<int> GetExpectedMemberIds(EF.ModelContext context, EF.TActivity activity, DateTime at, bool excludeAbsent = true)
        {
            var ids = AudienceMemberIds(context, activity, at);
            if (excludeAbsent)
            {
                var absent = Program.ActivityAbsenceRequest.ApprovedWholeMemberIds(context, activity.Id).ToHashSet();
                ids = ids.Where(x => !absent.Contains(x)).ToList();
            }
            return ids;
        }
        private static List<int> AudienceMemberIds(EF.ModelContext context, EF.TActivity activity, DateTime at)
        {
            var active = (int)MemberStatus.Active;
            return (AudienceType)activity.AudienceType switch
            {
                AudienceType.Pengurus => context.MMembers.Where(x => x.Status == active && x.MemberType == (int)MemberType.Pengurus).Select(x => x.Id).ToList(),
                AudienceType.PesertaTerdaftar => context.TActivityParticipants
                    .Where(x => x.ActivityId == activity.Id && x.RegistrationStatus != (int)RegistrationStatus.Cancelled && x.Member.Status == active)
                    .Select(x => x.MemberId).ToList(),
                AudienceType.Divisi => context.TDivisionMembers
                    .Where(x => x.DivisionId == activity.DivisionId && x.IsActive == (int)ActiveStatus.Aktif && x.Member.Status == active)
                    .Select(x => x.MemberId).Distinct().ToList(),
                AudienceType.Section => Anggota.SectionMember.MemberIdsAt(context, activity.SectionId ?? 0, DateOnly.FromDateTime(at)).Distinct().ToList(),
                AudienceType.TimPelaksana => context.TCommitteeMembers
                    .Where(x => x.ActivityId == activity.ParentActivityId && x.Status == (int)CommitteeMemberStatus.Aktif && x.Member.Status == active)
                    .Select(x => x.MemberId).ToList(),
                _ => context.MMembers.Where(x => x.Status == active).Select(x => x.Id).ToList()
            };
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TAttendanceSession> FilterQuery(int? activityId, AttendanceSessionStatus? status, DateTime? from, DateTime? to)
        {
            return _Querys.Where(x => (activityId == null || x.ActivityId == activityId)
                    && (status == null || x.Status == (int)status)
                    && (from == null || x.OpenAt >= from)
                    && (to == null || x.OpenAt < to))
                .OrderBy(x => x.OpenAt).ThenBy(x => x.Id);
        }
        private static EF.TAttendanceSession GetRow(EF.ModelContext context, int id)
        {
            return context.TAttendanceSessions.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sesi Presensi"));
        }
        private static async Task<EF.TAttendanceSession> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TAttendanceSessions.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sesi Presensi"));
        }
        private static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        private static void ValidateData(string name, DateTime openAt, DateTime closeAt, int lateToleranceMinute, decimal? latitude, decimal? longitude, int? radiusMeter)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Sesi"));
            }
            if (openAt == default || closeAt <= openAt)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            if (lateToleranceMinute < 0 || lateToleranceMinute > MAX_TOLERANCE_MINUTE)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Toleransi Terlambat"));
            }
            GeoFence.ValidateArea(latitude, longitude, radiusMeter);
        }
        private static EF.TAttendanceSession CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            Activity.GetOpenActivity(context, newrow.ActivityId);
            var name = newrow.Name?.Trim() ?? string.Empty;
            ValidateData(name, newrow.OpenAt, newrow.CloseAt, newrow.LateToleranceMinute, newrow.Latitude, newrow.Longitude, newrow.RadiusMeter);

            var row = new EF.TAttendanceSession
            {
                Id = SequenceUtility.GetNextId<EF.TAttendanceSession>(context),
                ActivityId = newrow.ActivityId,
                Name = name,
                OpenAt = newrow.OpenAt,
                CloseAt = newrow.CloseAt,
                LateToleranceMinute = newrow.LateToleranceMinute,
                Location = ToNullable(newrow.Location),
                Latitude = newrow.Latitude,
                Longitude = newrow.Longitude,
                RadiusMeter = newrow.RadiusMeter,
                Status = (int)AttendanceSessionStatus.Scheduled,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Kegiatan / jadwal tetap; sesi Closed / Cancelled dikunci.
        private void SetRow(EF.TAttendanceSession row, int userId)
        {
            TransaksiHelper.ValidateStatus((AttendanceSessionStatus)row.Status, "mengubah sesi", AttendanceSessionStatus.Scheduled, AttendanceSessionStatus.Open);
            var name = Name?.Trim() ?? string.Empty;
            ValidateData(name, OpenAt, CloseAt, LateToleranceMinute, Latitude, Longitude, RadiusMeter);

            row.Name = name;
            row.OpenAt = OpenAt;
            row.CloseAt = CloseAt;
            row.LateToleranceMinute = LateToleranceMinute;
            row.Location = ToNullable(Location);
            row.Latitude = Latitude;
            row.Longitude = Longitude;
            row.RadiusMeter = RadiusMeter;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetOpen(EF.ModelContext context, EF.TAttendanceSession row, int userId)
        {
            TransaksiHelper.ValidateStatus((AttendanceSessionStatus)row.Status, "membuka sesi", AttendanceSessionStatus.Scheduled, AttendanceSessionStatus.Open);
            Activity.GetOpenActivity(context, row.ActivityId);
            row.QrToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
            row.Status = (int)AttendanceSessionStatus.Open;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static int SetClose(EF.ModelContext context, EF.TAttendanceSession row, int userId)
        {
            TransaksiHelper.ValidateStatus((AttendanceSessionStatus)row.Status, "menutup sesi", AttendanceSessionStatus.Open);
            var activity = Activity.GetActivity(context, row.ActivityId);
            var recorded = context.TAttendanceRecords.Where(x => x.SessionId == row.Id).Select(x => x.MemberId).ToHashSet();
            var absent = GetExpectedMemberIds(context, activity, row.OpenAt).Where(x => !recorded.Contains(x)).Distinct().ToList();
            // Izin sesi yang disetujui (ActivityAbsenceRequest) dicatat Izin, sisanya Alpa.
            var excused = Program.ActivityAbsenceRequest.ApprovedSessionMemberIds(context, row.Id);
            if (absent.Count > 0)
            {
                var firstId = SequenceUtility.ReserveIds<EF.TAttendanceRecord>(context, absent.Count);
                var now = DateTime.Now;
                context.TAttendanceRecords.AddRange(absent.Select((memberId, i) => new EF.TAttendanceRecord
                {
                    Id = firstId + i,
                    SessionId = row.Id,
                    MemberId = memberId,
                    Status = excused.Contains(memberId) ? (int)AttendanceStatus.Izin : (int)AttendanceStatus.Alpa,
                    InsDate = now,
                    InsBy = userId
                }));
            }
            row.QrToken = null;
            row.Status = (int)AttendanceSessionStatus.Closed;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            return absent.Count;
        }
        private static void SetCancel(EF.ModelContext context, EF.TAttendanceSession row, string reason, int userId)
        {
            TransaksiHelper.ValidateStatus((AttendanceSessionStatus)row.Status, "membatalkan sesi", AttendanceSessionStatus.Scheduled, AttendanceSessionStatus.Open);
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Alasan Pembatalan"));
            }
            if (context.TAttendanceRecords.Any(x => x.SessionId == row.Id && x.CheckInAt != null))
            {
                throw new ArgumentException(ExceptionMessage.ATTENDANCE_SESSION_HAS_CHECKIN);
            }
            row.CancelReason = TransaksiHelper.Truncate(reason, REASON_LENGTH);
            row.QrToken = null;
            row.Status = (int)AttendanceSessionStatus.Cancelled;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
