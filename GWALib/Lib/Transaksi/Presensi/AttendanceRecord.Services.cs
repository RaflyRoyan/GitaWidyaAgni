using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
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
    public partial class AttendanceRecord
    {
        private const int NOTES_LENGTH = 255;

        #region SYNC
        public PhotoContent? GetCheckInPhoto()
        {
            using var context = DBClass.GetContext();
            var row = context.TAttendanceRecordIns.Where(x => x.AttendanceId == Id).Select(x => new { x.Photo, x.Latitude, x.Longitude }).SingleOrDefault();
            return row == null ? null : new PhotoContent(row.Photo, ImageUtility.GetContentType(row.Photo), row.Latitude, row.Longitude);
        }
        public PhotoContent? GetCheckOutPhoto()
        {
            using var context = DBClass.GetContext();
            var row = context.TAttendanceRecordOuts.Where(x => x.AttendanceId == Id).Select(x => new { x.Photo, x.Latitude, x.Longitude }).SingleOrDefault();
            return row == null ? null : new PhotoContent(row.Photo, ImageUtility.GetContentType(row.Photo), row.Latitude, row.Longitude);
        }
        public static List<AttendanceRecord> GetDataList(int? sessionId = null, int? memberId = null, AttendanceStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(sessionId, memberId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<AttendanceRecord, EF.TAttendanceRecord>();
        }
        // Check-in anggota yang login (user harus terhubung ke anggota).
        public static Response<AttendanceRecord> CheckIn(CheckRow input, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = AddCheckIn(context, input, userId);
                context.SaveChanges();
                return new AttendanceRecord(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AttendanceRecord>(msg));
        }
        public static Response<AttendanceRecord> CheckOut(CheckRow input, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = AddCheckOut(context, input, userId);
                context.SaveChanges();
                return new AttendanceRecord(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AttendanceRecord>(msg));
        }
        // Pencatatan manual oleh pengurus (Izin / Sakit / Alpa / Hadir), juga untuk mengoreksi ALPA otomatis.
        public static Response<AttendanceRecord> SetManual(int sessionId, int memberId, AttendanceStatus status, string? notes, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = Upsert(context, sessionId, memberId, status, notes, userId);
                context.SaveChanges();
                return new AttendanceRecord(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AttendanceRecord>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<PhotoContent?> GetCheckInPhotoAsync()
        {
            using var context = DBClass.GetContext();
            var row = await context.TAttendanceRecordIns.Where(x => x.AttendanceId == Id).Select(x => new { x.Photo, x.Latitude, x.Longitude }).SingleOrDefaultAsync();
            return row == null ? null : new PhotoContent(row.Photo, ImageUtility.GetContentType(row.Photo), row.Latitude, row.Longitude);
        }
        public async Task<PhotoContent?> GetCheckOutPhotoAsync()
        {
            using var context = DBClass.GetContext();
            var row = await context.TAttendanceRecordOuts.Where(x => x.AttendanceId == Id).Select(x => new { x.Photo, x.Latitude, x.Longitude }).SingleOrDefaultAsync();
            return row == null ? null : new PhotoContent(row.Photo, ImageUtility.GetContentType(row.Photo), row.Latitude, row.Longitude);
        }
        public static async Task<List<AttendanceRecord>> GetDataListAsync(int? sessionId = null, int? memberId = null, AttendanceStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(sessionId, memberId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<AttendanceRecord, EF.TAttendanceRecord>();
        }
        public static async Task<Response<AttendanceRecord>> CheckInAsync(CheckRow input, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = AddCheckIn(context, input, userId);
                await context.SaveChangesAsync();
                return new AttendanceRecord(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AttendanceRecord>(msg));
        }
        public static async Task<Response<AttendanceRecord>> CheckOutAsync(CheckRow input, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = AddCheckOut(context, input, userId);
                await context.SaveChangesAsync();
                return new AttendanceRecord(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AttendanceRecord>(msg));
        }
        public static async Task<Response<AttendanceRecord>> SetManualAsync(int sessionId, int memberId, AttendanceStatus status, string? notes, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = Upsert(context, sessionId, memberId, status, notes, userId);
                await context.SaveChangesAsync();
                return new AttendanceRecord(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AttendanceRecord>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TAttendanceRecord Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TAttendanceRecord> FilterQuery(int? sessionId, int? memberId, AttendanceStatus? status)
        {
            return _Querys.Where(x => (sessionId == null || x.SessionId == sessionId)
                    && (memberId == null || x.MemberId == memberId)
                    && (status == null || x.Status == (int)status))
                .OrderBy(x => x.SessionId).ThenBy(x => x.Member.Fullname);
        }
        // Sesi Open dari token QR dan masih dalam rentang OpenAt - CloseAt.
        private static EF.TAttendanceSession GetOpenSession(EF.ModelContext context, string qrToken)
        {
            var token = qrToken?.Trim().ToLowerInvariant() ?? string.Empty;
            var session = token.Length == 0 ? null : context.TAttendanceSessions.SingleOrDefault(x => x.QrToken == token);
            var now = DateTime.Now;
            if (session == null || session.Status != (int)AttendanceSessionStatus.Open || now < session.OpenAt || now > session.CloseAt)
            {
                throw new ArgumentException(ExceptionMessage.ATTENDANCE_SESSION_NOT_OPEN);
            }
            return session;
        }
        // Anggota milik user login, harus aktif.
        private static EF.MMember GetMemberOfUser(EF.ModelContext context, int userId)
        {
            var member = context.MMembers.SingleOrDefault(x => x.UserId == userId)
                ?? throw new ArgumentException(ExceptionMessage.USER_NOT_MEMBER);
            if (member.Status != (int)MemberStatus.Active)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Anggota"));
            }
            return member;
        }
        private static EF.TAttendanceRecord AddCheckIn(EF.ModelContext context, CheckRow input, int userId)
        {
            var session = GetOpenSession(context, input.QrToken);
            var member = GetMemberOfUser(context, userId);
            var activity = Activity.GetActivity(context, session.ActivityId);
            if (activity.AudienceType != (int)AudienceType.SemuaAnggota && !AttendanceSession.GetExpectedMemberIds(context, activity, session.OpenAt, false).Contains(member.Id))
            {
                throw new ArgumentException(ExceptionMessage.ATTENDANCE_NOT_IN_AUDIENCE);
            }
            GeoFence.ValidatePosition(session.Latitude, session.Longitude, session.RadiusMeter, input.Latitude, input.Longitude);
            var image = ImageUtility.Normalize(input.Photo);
            var now = DateTime.Now;

            var row = context.TAttendanceRecords.SingleOrDefault(x => x.SessionId == session.Id && x.MemberId == member.Id);
            if (row != null)
            {
                throw new ArgumentException(row.CheckInAt != null ? ExceptionMessage.ATTENDANCE_ALREADY_CHECKED_IN : ExceptionMessage.ATTENDANCE_ALREADY_RECORDED);
            }
            row = new EF.TAttendanceRecord
            {
                Id = SequenceUtility.GetNextId<EF.TAttendanceRecord>(context),
                SessionId = session.Id,
                MemberId = member.Id,
                CheckInAt = now,
                Status = (int)(now > session.OpenAt.AddMinutes(session.LateToleranceMinute) ? AttendanceStatus.Terlambat : AttendanceStatus.Hadir),
                InsDate = now,
                InsBy = userId
            };
            context.TAttendanceRecords.Add(row);
            context.TAttendanceRecordIns.Add(new EF.TAttendanceRecordIn
            {
                Id = SequenceUtility.GetNextId<EF.TAttendanceRecordIn>(context),
                AttendanceId = row.Id,
                Latitude = input.Latitude,
                Longitude = input.Longitude,
                Photo = image.Data,
                InsDate = now,
                InsBy = userId
            });
            return row;
        }
        private static EF.TAttendanceRecord AddCheckOut(EF.ModelContext context, CheckRow input, int userId)
        {
            var session = GetOpenSession(context, input.QrToken);
            var member = GetMemberOfUser(context, userId);
            var row = context.TAttendanceRecords.SingleOrDefault(x => x.SessionId == session.Id && x.MemberId == member.Id);
            if (row == null || row.CheckInAt == null)
            {
                throw new ArgumentException(ExceptionMessage.ATTENDANCE_NOT_CHECKED_IN);
            }
            if (row.CheckOutAt != null)
            {
                throw new ArgumentException(ExceptionMessage.ATTENDANCE_ALREADY_CHECKED_OUT);
            }
            GeoFence.ValidatePosition(session.Latitude, session.Longitude, session.RadiusMeter, input.Latitude, input.Longitude);
            var image = ImageUtility.Normalize(input.Photo);
            var now = DateTime.Now;

            row.CheckOutAt = now;
            row.UpdDate = now;
            row.UpdBy = userId;
            context.TAttendanceRecordOuts.Add(new EF.TAttendanceRecordOut
            {
                Id = SequenceUtility.GetNextId<EF.TAttendanceRecordOut>(context),
                AttendanceId = row.Id,
                Latitude = input.Latitude,
                Longitude = input.Longitude,
                Photo = image.Data,
                InsDate = now,
                InsBy = userId
            });
            return row;
        }
        // Sesi Open / Closed (bukan Scheduled / Cancelled). Catatan dengan check-in hanya boleh dikoreksi ke Hadir / Terlambat.
        private static EF.TAttendanceRecord Upsert(EF.ModelContext context, int sessionId, int memberId, AttendanceStatus status, string? notes, int userId)
        {
            var session = context.TAttendanceSessions.SingleOrDefault(x => x.Id == sessionId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sesi Presensi"));
            TransaksiHelper.ValidateStatus((AttendanceSessionStatus)session.Status, "mencatat presensi", AttendanceSessionStatus.Open, AttendanceSessionStatus.Closed);
            if (!Enum.IsDefined(status))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Presensi"));
            }
            Member.GetMember(context, memberId);
            var now = DateTime.Now;
            var row = context.TAttendanceRecords.SingleOrDefault(x => x.SessionId == sessionId && x.MemberId == memberId);
            if (row == null)
            {
                row = new EF.TAttendanceRecord
                {
                    Id = SequenceUtility.GetNextId<EF.TAttendanceRecord>(context),
                    SessionId = sessionId,
                    MemberId = memberId,
                    InsDate = now,
                    InsBy = userId
                };
                context.TAttendanceRecords.Add(row);
            }
            else
            {
                if (row.CheckInAt != null && status != AttendanceStatus.Hadir && status != AttendanceStatus.Terlambat)
                {
                    throw new ArgumentException(ExceptionMessage.ATTENDANCE_CHECKED_IN_STATUS);
                }
                row.UpdDate = now;
                row.UpdBy = userId;
            }
            row.Status = (int)status;
            row.Notes = TransaksiHelper.Truncate(notes, NOTES_LENGTH) ?? row.Notes;
            return row;
        }
        #endregion
    }
}
