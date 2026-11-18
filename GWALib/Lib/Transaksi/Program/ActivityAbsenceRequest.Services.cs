using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Transaksi.Information;
using GWALib.Lib.Transaksi.Presensi;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;

namespace GWALib.Lib.Transaksi.Program
{
    public partial class ActivityAbsenceRequest
    {
        #region SYNC
        public static List<ActivityAbsenceRequest> GetDataList(int? activityId = null, int? memberId = null, AbsenceRequestStatus? status = null,
            int startdata = -1, int jmldata = 50)
        {
            var query = _Querys.Where(x => (activityId == null || x.ActivityId == activityId)
                    && (memberId == null || x.MemberId == memberId)
                    && (status == null || x.Status == (int)status))
                .OrderBy(x => x.Status).ThenByDescending(x => x.InsDate).AsQueryable();
            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ActivityAbsenceRequest, EF.TActivityAbsenceRequest>();
        }
        /// <summary>Pengajuan milik anggota user login (terbaru dulu); kosong bila user bukan anggota.</summary>
        public static List<ActivityAbsenceRequest> GetMyList(string user)
        {
            var userId = ValidateUtility.ValidateUser(user);
            using var context = DBClass.GetContext();
            var memberId = context.MMembers.Where(x => x.UserId == userId).Select(x => (int?)x.Id).FirstOrDefault();
            return memberId == null ? new() : GetDataList(memberId: memberId).OrderByDescending(x => x.InsDate).ToList();
        }
        /// <summary>
        /// Id kegiatan (dari activityIds) yang otomatis diikuti anggota karena termasuk audiensnya (bukan audiens Peserta Terdaftar).
        /// Persetujuan tidak ikut seluruh kegiatan tidak mengeluarkan kegiatan dari daftar ini (status pengajuan ditampilkan terpisah).
        /// </summary>
        public static HashSet<int> GetAutoActivityIds(int memberId, IEnumerable<int> activityIds)
        {
            using var context = DBClass.GetContext();
            var result = new HashSet<int>();
            foreach (var activity in context.TActivities.AsNoTracking().Where(x => activityIds.Contains(x.Id)).ToList())
            {
                if (activity.AudienceType != (int)AudienceType.PesertaTerdaftar
                    && AttendanceSession.GetExpectedMemberIds(context, activity, DateTime.Now, false).Contains(memberId))
                {
                    result.Add(activity.Id);
                }
            }
            return result;
        }
        /// <summary>Anggota user login mengajukan tidak ikut satu sesi atau seluruh kegiatan.</summary>
        public static Response<ActivityAbsenceRequest> Submit(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TActivityAbsenceRequests.Add(row);
                context.SaveChanges();
                return new ActivityAbsenceRequest(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivityAbsenceRequest>(msg));
        }
        /// <summary>Anggota menarik pengajuannya sendiri selama masih Menunggu.</summary>
        public Response<ActivityAbsenceRequest> Withdraw(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var memberId = context.MMembers.Where(x => x.UserId == userId).Select(x => (int?)x.Id).FirstOrDefault();
                if (memberId != row.MemberId)
                {
                    throw new ArgumentException("Pengajuan ini bukan milik Anda.");
                }
                TransaksiHelper.ValidateStatus((AbsenceRequestStatus)row.Status, "menarik pengajuan", AbsenceRequestStatus.Menunggu);
                row.Status = (int)AbsenceRequestStatus.Ditarik;
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                context.SaveChanges();
                return new ActivityAbsenceRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityAbsenceRequest>(msg));
        }
        /// <summary>Pengurus menyetujui. Sesi yang sudah ditutup: catatan Alpa anggota diubah menjadi Izin.</summary>
        public Response<ActivityAbsenceRequest> Approve(string? note, string user)
        {
            return Review(AbsenceRequestStatus.Disetujui, note, user);
        }
        /// <summary>Pengurus menolak; catatan wajib. Anggota tetap dihitung wajib hadir.</summary>
        public Response<ActivityAbsenceRequest> Reject(string? note, string user)
        {
            return Review(AbsenceRequestStatus.Ditolak, note, user);
        }
        #endregion

        #region INTERNAL
        /// <summary>Anggota yang disetujui tidak ikut seluruh kegiatan (dikeluarkan dari daftar wajib hadir).</summary>
        internal static IQueryable<int> ApprovedWholeMemberIds(EF.ModelContext context, int activityId)
        {
            return context.TActivityAbsenceRequests
                .Where(x => x.ActivityId == activityId && x.SessionId == null && x.Status == (int)AbsenceRequestStatus.Disetujui)
                .Select(x => x.MemberId);
        }
        /// <summary>Anggota yang disetujui izin untuk sesi ini (dicatat Izin saat sesi ditutup).</summary>
        internal static HashSet<int> ApprovedSessionMemberIds(EF.ModelContext context, int sessionId)
        {
            return context.TActivityAbsenceRequests
                .Where(x => x.SessionId == sessionId && x.Status == (int)AbsenceRequestStatus.Disetujui)
                .Select(x => x.MemberId).ToHashSet();
        }
        #endregion

        #region HELPER
        private Response<ActivityAbsenceRequest> Review(AbsenceRequestStatus target, string? note, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((AbsenceRequestStatus)row.Status, "memproses pengajuan", AbsenceRequestStatus.Menunggu);
                var text = TransaksiHelper.Truncate(TransaksiHelper.ToNullable(note), REASON_LENGTH);
                if (target == AbsenceRequestStatus.Ditolak && text == null)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Alasan Penolakan"));
                }
                if (target == AbsenceRequestStatus.Disetujui && row.SessionId != null)
                {
                    // Sesi yang sudah ditutup: Alpa otomatis berubah menjadi Izin.
                    var record = context.TAttendanceRecords.SingleOrDefault(x => x.SessionId == row.SessionId && x.MemberId == row.MemberId);
                    if (record != null && record.Status == (int)AttendanceStatus.Alpa && record.CheckInAt == null)
                    {
                        record.Status = (int)AttendanceStatus.Izin;
                        record.UpdDate = DateTime.Now;
                        record.UpdBy = userId;
                    }
                }
                row.Status = (int)target;
                row.ReviewNote = text;
                row.ReviewedBy = userId;
                row.ReviewedAt = DateTime.Now;
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                var title = context.TActivities.Where(x => x.Id == row.ActivityId).Select(x => x.Title).Single();
                NotifyHelper.ToMember(context, row.MemberId, NotifyHelper.TYPE_ABSENCE,
                    $"Pengajuan tidak ikut {title} {(target == AbsenceRequestStatus.Disetujui ? "disetujui" : "ditolak")}", text,
                    NotifyHelper.LINK_MY_ACTIVITY, "t_activity_absence_request", row.Id, userId);
                context.SaveChanges();
                return new ActivityAbsenceRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityAbsenceRequest>(msg));
        }
        private static EF.TActivityAbsenceRequest CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var member = context.MMembers.SingleOrDefault(x => x.UserId == userId)
                ?? throw new ArgumentException(ExceptionMessage.USER_NOT_MEMBER);
            if (member.Status != (int)MemberStatus.Active)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Anggota"));
            }
            var reason = TransaksiHelper.ToNullable(newrow.Reason)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Alasan"));
            if (reason.Length > REASON_LENGTH)
            {
                throw new ArgumentException($"Alasan maksimal {REASON_LENGTH} karakter.");
            }
            var activity = Activity.GetOpenActivity(context, newrow.ActivityId);
            if (activity.AudienceType == (int)AudienceType.PesertaTerdaftar)
            {
                throw new ArgumentException("Kegiatan ini memakai pendaftaran peserta; batalkan pendaftaran Anda saja.");
            }
            var sessionId = newrow.SessionId > 0 ? newrow.SessionId : null;
            var at = DateTime.Now;
            if (sessionId != null)
            {
                var session = context.TAttendanceSessions.SingleOrDefault(x => x.Id == sessionId && x.ActivityId == activity.Id)
                    ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sesi"));
                if (session.Status != (int)AttendanceSessionStatus.Scheduled && session.Status != (int)AttendanceSessionStatus.Open)
                {
                    throw new ArgumentException("Izin hanya bisa diajukan untuk sesi yang belum ditutup.");
                }
                if (context.TAttendanceRecords.Any(x => x.SessionId == sessionId && x.MemberId == member.Id && x.CheckInAt != null))
                {
                    throw new ArgumentException("Anda sudah presensi di sesi ini.");
                }
                at = session.OpenAt;
            }
            if (!AttendanceSession.GetExpectedMemberIds(context, activity, at, false).Contains(member.Id))
            {
                throw new ArgumentException("Anda tidak termasuk peserta kegiatan ini.");
            }
            var open = new[] { (int)AbsenceRequestStatus.Menunggu, (int)AbsenceRequestStatus.Disetujui };
            if (context.TActivityAbsenceRequests.Any(x => x.ActivityId == activity.Id && x.MemberId == member.Id && x.SessionId == null && open.Contains(x.Status)))
            {
                throw new ArgumentException("Anda sudah mengajukan tidak ikut seluruh kegiatan ini.");
            }
            if (sessionId != null && context.TActivityAbsenceRequests.Any(x => x.SessionId == sessionId && x.MemberId == member.Id && open.Contains(x.Status)))
            {
                throw new ArgumentException("Anda sudah mengajukan izin untuk sesi ini.");
            }
            return new EF.TActivityAbsenceRequest
            {
                Id = SequenceUtility.GetNextId<EF.TActivityAbsenceRequest>(context),
                ActivityId = activity.Id,
                MemberId = member.Id,
                SessionId = sessionId,
                Reason = reason,
                Status = (int)AbsenceRequestStatus.Menunggu,
                InsDate = DateTime.Now,
                InsBy = userId
            };
        }
        private static EF.TActivityAbsenceRequest Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static EF.TActivityAbsenceRequest GetRow(EF.ModelContext context, int id)
        {
            return context.TActivityAbsenceRequests.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pengajuan"));
        }
        #endregion
    }
}
