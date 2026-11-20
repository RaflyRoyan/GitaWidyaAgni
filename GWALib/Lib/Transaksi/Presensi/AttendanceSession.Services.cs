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
        // Check-in sudah bisa sejak sekian menit sebelum OpenAt (sesi dibuka otomatis pada saat itu).
        public const int EARLY_CHECKIN_MINUTE = 30;
        // Sesi otomatis kegiatan non-rutin: radius dan toleransi bawaan (bisa diubah di sesi).
        private const int AUTO_RADIUS_METER = 200;
        private const int AUTO_TOLERANCE_MINUTE = 15;
        // Pengingat: check-out dikirim sekian menit sebelum selesai; kode template WhatsApp (opsional, m_message_template).
        private const int CHECKOUT_REMINDER_MINUTE = 15;
        public const string TEMPLATE_REMINDER = "ATTENDANCE_REMINDER";
        public const string TEMPLATE_CHECKOUT = "ATTENDANCE_CHECKOUT";
        // Statistik keaktifan: batas kehadiran rendah (%), minimal catatan agar dinilai, dan jumlah bulan di halaman anggota.
        public const int LOW_PERCENT = 75;
        public const int LOW_MIN_SESSIONS = 3;
        public const int MONTHS_MINE = 6;

        #region SYNC
        // Untuk job (tiap menit): buka sesi Terjadwal yang masuk jendela check-in, tutup sesi yang sudah lewat CloseAt (ALPA dicatat).
        // Sesi kegiatan yang dibatalkan tidak disentuh.
        public static Response<SyncResult> Synchronize(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var result = SyncRows(context, userId);
                context.SaveChanges();
                return result;
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SyncResult>(msg));
        }
        // Pantauan langsung sesi (pengurus): anggota sasaran + yang sudah punya catatan, status, jam, jarak check-in ke titik.
        public MonitorData GetMonitor()
        {
            using var context = DBClass.GetContext();
            return Monitor(context, Id);
        }
        // Rekap kehadiran dari sesi Ditutup dengan OpenAt dalam [from, to] (tanggal), opsional per kegiatan / section (section saat ini).
        public static List<RecapRow> GetRecap(DateOnly from, DateOnly to, int? activityId = null, int? sectionId = null)
        {
            if (to < from)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            using var context = DBClass.GetContext();
            return Recap(context, from.ToDateTime(TimeOnly.MinValue), to.AddDays(1).ToDateTime(TimeOnly.MinValue), activityId, sectionId);
        }
        // Statistik keaktifan dari sesi Ditutup dengan OpenAt dalam [from, to]: tren per bulan, per section (section saat ini),
        // dan anggota berkehadiran rendah. Opsional dibatasi satu section dan / atau satu anggota.
        public static ActivityStats GetStats(DateOnly from, DateOnly to, int? sectionId = null, int? memberId = null)
        {
            if (to < from)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            using var context = DBClass.GetContext();
            var start = from.ToDateTime(TimeOnly.MinValue);
            var end = to.AddDays(1).ToDateTime(TimeOnly.MinValue);
            var members = Recap(context, start, end, null, sectionId, memberId);
            var sections = members.GroupBy(x => x.Section ?? "Tanpa section")
                .Select(g => new GroupStat(g.Key, g.Count(), g.Sum(x => x.Hadir), g.Sum(x => x.Terlambat), g.Sum(x => x.Izin), g.Sum(x => x.Sakit), g.Sum(x => x.Alpa)))
                .OrderByDescending(x => x.Percent).ThenBy(x => x.Name).ToList();
            var low = members.Where(x => x.Sessions >= LOW_MIN_SESSIONS && x.Percent < LOW_PERCENT)
                .OrderBy(x => x.Percent).ThenByDescending(x => x.Alpa).ThenBy(x => x.Name).ToList();
            return new ActivityStats(from, to, Monthly(context, start, end, sectionId, memberId), sections, members, low);
        }
        // Jadwal presensi anggota user login: hari ini (semua sesi yang tidak dibatalkan), daysAhead hari ke depan,
        // dan historyTake catatan presensi terakhir sebelum hari ini. Anggota tanpa data / tidak aktif = daftar kosong.
        public static MyAttendance GetMyAttendance(string user, int daysAhead = 7, int historyTake = 60)
        {
            var userId = ValidateUtility.ValidateUser(user);
            using var context = DBClass.GetContext();
            return MyAttendanceData(context, userId, daysAhead, historyTake);
        }
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
        internal static DateTime CheckInStart(EF.TAttendanceSession session) => session.OpenAt.AddMinutes(-EARLY_CHECKIN_MINUTE);

        // Buka sesi (token QR baru); dipakai juga oleh check-in pertama pada sesi tanpa QR.
        internal static void OpenRow(EF.ModelContext context, EF.TAttendanceSession row, int userId) => SetOpen(context, row, userId);

        // Satu sesi otomatis untuk kegiatan non-rutin (bukan Latihan / Piket) yang punya koordinat: OpenAt = mulai,
        // CloseAt = selesai (atau akhir hari mulai). Kegiatan yang sudah lewat tidak diberi sesi. Rapat memakai QR.
        private static (DateTime OpenAt, DateTime CloseAt)? AutoTimes(EF.TActivity activity)
        {
            var type = (ActivityType)activity.ActivityType;
            if (type == ActivityType.Latihan || type == ActivityType.Piket || activity.Latitude == null || activity.Longitude == null)
            {
                return null;
            }
            var closeAt = activity.EndAt > activity.StartAt ? activity.EndAt.Value : activity.StartAt.Date.AddDays(1).AddMinutes(-1);
            return closeAt <= DateTime.Now ? null : (activity.StartAt, closeAt);
        }
        internal static void AddForActivity(EF.ModelContext context, EF.TActivity activity, int userId)
        {
            var times = AutoTimes(activity);
            if (times == null)
            {
                return;
            }
            context.TAttendanceSessions.Add(new EF.TAttendanceSession
            {
                Id = SequenceUtility.GetNextId<EF.TAttendanceSession>(context),
                ActivityId = activity.Id,
                Name = TransaksiHelper.Truncate(activity.Title, 150)!,
                OpenAt = times.Value.OpenAt,
                CloseAt = times.Value.CloseAt,
                LateToleranceMinute = AUTO_TOLERANCE_MINUTE,
                AttendanceMode = (int)(activity.ActivityType == (int)ActivityType.Rapat ? AttendanceMode.Qr : AttendanceMode.TanpaQr),
                Location = activity.Location,
                Latitude = activity.Latitude,
                Longitude = activity.Longitude,
                RadiusMeter = AUTO_RADIUS_METER,
                Status = (int)AttendanceSessionStatus.Scheduled,
                InsDate = DateTime.Now,
                InsBy = userId
            });
        }
        // Setelah kegiatan diubah: sesi otomatisnya (tanpa jadwal, OpenAt = mulai lama, masih Terjadwal tanpa presensi) mengikuti
        // waktu / lokasi baru, atau dihapus bila kegiatan tidak lagi memenuhi syarat. Kegiatan tanpa sesi sama sekali mendapat sesi baru.
        internal static void SyncForActivity(EF.ModelContext context, EF.TActivity activity, DateTime oldStartAt, int userId)
        {
            var times = AutoTimes(activity);
            var auto = context.TAttendanceSessions.FirstOrDefault(x => x.ActivityId == activity.Id && x.ScheduleId == null && x.OpenAt == oldStartAt
                && x.Status == (int)AttendanceSessionStatus.Scheduled && !x.TAttendanceRecords.Any());
            if (auto == null)
            {
                if (times != null && !context.TAttendanceSessions.Any(x => x.ActivityId == activity.Id))
                {
                    AddForActivity(context, activity, userId);
                }
                return;
            }
            if (times == null)
            {
                if (!IsReferenced(context, auto.Id))
                {
                    context.TAttendanceSessions.Remove(auto);
                }
                return;
            }
            auto.Name = TransaksiHelper.Truncate(activity.Title, 150)!;
            auto.OpenAt = times.Value.OpenAt;
            auto.CloseAt = times.Value.CloseAt;
            auto.Location = activity.Location;
            auto.Latitude = activity.Latitude;
            auto.Longitude = activity.Longitude;
            auto.UpdDate = DateTime.Now;
            auto.UpdBy = userId;
        }
        // Sebelum kegiatan dihapus: sesi Terjadwal tanpa presensi / rujukan ikut dihapus.
        internal static void RemoveForActivity(EF.ModelContext context, int activityId)
        {
            var rows = context.TAttendanceSessions
                .Where(x => x.ActivityId == activityId && x.Status == (int)AttendanceSessionStatus.Scheduled && !x.TAttendanceRecords.Any())
                .ToList();
            context.TAttendanceSessions.RemoveRange(rows.Where(x => !IsReferenced(context, x.Id)));
        }
        private static bool IsReferenced(EF.ModelContext context, int sessionId)
        {
            return context.TActivityAbsenceRequests.Any(x => x.SessionId == sessionId) || context.TMeetingMinutes.Any(x => x.SessionId == sessionId);
        }

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
        private static void ValidateData(string name, DateTime openAt, DateTime closeAt, int lateToleranceMinute, AttendanceMode mode,
            decimal? latitude, decimal? longitude, int? radiusMeter)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Sesi"));
            }
            if (!Enum.IsDefined(mode))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Mode Presensi"));
            }
            if (openAt == default || closeAt <= openAt)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            if (lateToleranceMinute < 0 || lateToleranceMinute > MAX_TOLERANCE_MINUTE)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Toleransi Terlambat"));
            }
            GeoFence.ValidateRequiredArea(latitude, longitude, radiusMeter);
        }
        private static EF.TAttendanceSession CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            Activity.GetOpenActivity(context, newrow.ActivityId);
            var name = newrow.Name?.Trim() ?? string.Empty;
            ValidateData(name, newrow.OpenAt, newrow.CloseAt, newrow.LateToleranceMinute, newrow.AttendanceMode, newrow.Latitude, newrow.Longitude, newrow.RadiusMeter);

            var row = new EF.TAttendanceSession
            {
                Id = SequenceUtility.GetNextId<EF.TAttendanceSession>(context),
                ActivityId = newrow.ActivityId,
                Name = name,
                OpenAt = newrow.OpenAt,
                CloseAt = newrow.CloseAt,
                LateToleranceMinute = newrow.LateToleranceMinute,
                AttendanceMode = (int)newrow.AttendanceMode,
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
            ValidateData(name, OpenAt, CloseAt, LateToleranceMinute, AttendanceMode, Latitude, Longitude, RadiusMeter);

            row.Name = name;
            row.OpenAt = OpenAt;
            row.CloseAt = CloseAt;
            row.LateToleranceMinute = LateToleranceMinute;
            row.AttendanceMode = (int)AttendanceMode;
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
            // Poin Hadir Kegiatan otomatis untuk yang hadir / terlambat.
            var present = context.TAttendanceRecords
                .Where(x => x.SessionId == row.Id && (x.Status == (int)AttendanceStatus.Hadir || x.Status == (int)AttendanceStatus.Terlambat)).ToList();
            foreach (var record in present)
            {
                ApplyPoint(context, record, activity, row.OpenAt, userId);
            }
            row.QrToken = null;
            row.Status = (int)AttendanceSessionStatus.Closed;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            return absent.Count;
        }
        // Pengingat check-in (saat jendela check-in dibuka, dilewati bila sudah lewat toleransi terlambat) untuk anggota sasaran yang
        // belum presensi, dan pengingat check-out (CHECKOUT_REMINDER_MINUTE sebelum selesai) untuk yang sudah check-in. Hasil = jumlah anggota.
        private static int SendReminders(EF.ModelContext context, DateTime now, int userId)
        {
            int scheduled = (int)AttendanceSessionStatus.Scheduled, open = (int)AttendanceSessionStatus.Open;
            int planned = (int)ProgramStatus.Planned, ongoing = (int)ProgramStatus.Ongoing;
            var early = now.AddMinutes(EARLY_CHECKIN_MINUTE);
            var count = 0;

            var starting = context.TAttendanceSessions.Include(x => x.Activity)
                .Where(x => (x.Status == scheduled || x.Status == open) && x.ReminderSentAt == null && x.OpenAt <= early && x.CloseAt > now
                    && (x.Activity.Status == planned || x.Activity.Status == ongoing))
                .ToList();
            foreach (var session in starting)
            {
                session.ReminderSentAt = now;
                if (now > session.OpenAt.AddMinutes(session.LateToleranceMinute))
                {
                    continue;
                }
                var recorded = context.TAttendanceRecords.Where(x => x.SessionId == session.Id).Select(x => x.MemberId).ToHashSet();
                var targets = GetExpectedMemberIds(context, session.Activity, session.OpenAt).Where(x => !recorded.Contains(x)).Distinct().ToList();
                count += Remind(context, session, targets, false, userId);
            }

            var checkoutBefore = now.AddMinutes(CHECKOUT_REMINDER_MINUTE);
            var ending = context.TAttendanceSessions
                .Where(x => x.Status == open && x.CheckoutReminderSentAt == null && x.CloseAt <= checkoutBefore && x.CloseAt > now)
                .ToList();
            foreach (var session in ending)
            {
                session.CheckoutReminderSentAt = now;
                var targets = context.TAttendanceRecords.Where(x => x.SessionId == session.Id && x.CheckInAt != null && x.CheckOutAt == null)
                    .Select(x => x.MemberId).ToList();
                count += Remind(context, session, targets, true, userId);
            }
            return count;
        }
        // Notifikasi in-app ke anggota (preferensi InApp dihormati), plus WhatsApp bila template ATTENDANCE_REMINDER / ATTENDANCE_CHECKOUT
        // saluran WhatsApp ada dan anggota punya nomor (preferensi WhatsApp dihormati oleh MessageOutbox).
        private static int Remind(EF.ModelContext context, EF.TAttendanceSession session, List<int> memberIds, bool checkout, int userId)
        {
            if (memberIds.Count == 0)
            {
                return 0;
            }
            var place = string.IsNullOrWhiteSpace(session.Location) ? "" : " di " + session.Location;
            var title = checkout ? "Jangan lupa check-out: " + session.Name : "Presensi dibuka: " + session.Name;
            var message = checkout
                ? $"Sesi selesai pukul {session.CloseAt:HH:mm}. Lakukan check-out sebelum sesi ditutup."
                : $"Sesi mulai pukul {session.OpenAt:HH:mm}{place}. Check-in dari Area Anggota › Presensi"
                    + (session.AttendanceMode == (int)AttendanceMode.Qr ? " dengan scan QR di lokasi." : ".");
            Information.NotifyHelper.ToMembers(context, memberIds.Select(x => (int?)x), Information.NotifyHelper.TYPE_ATTENDANCE, title, message,
                Information.NotifyHelper.LINK_MY_ATTENDANCE, "t_attendance_session", session.Id, userId);

            var code = checkout ? TEMPLATE_CHECKOUT : TEMPLATE_REMINDER;
            if (Master.Approval.MessageTemplate.GetTemplate(code, NotificationChannel.WhatsApp) == null)
            {
                return memberIds.Count;
            }
            var members = context.MMembers.Where(x => memberIds.Contains(x.Id) && x.Phone != null && x.Phone != "")
                .Select(x => new { x.UserId, x.Fullname, x.Phone }).ToList();
            foreach (var member in members)
            {
                try
                {
                    Approval.MessageOutbox.CreateFromTemplate(context, new Approval.MessageOutbox.TemplateRow
                    {
                        UserId = member.UserId,
                        Recipient = member.Phone!,
                        Channel = NotificationChannel.WhatsApp,
                        TemplateCode = code,
                        NotifType = Information.NotifyHelper.TYPE_ATTENDANCE,
                        Values = new()
                        {
                            ["name"] = member.Fullname,
                            ["session"] = session.Name,
                            ["time"] = (checkout ? session.CloseAt : session.OpenAt).ToString("HH:mm"),
                            ["place"] = session.Location ?? "-"
                        },
                        RefType = "t_attendance_session",
                        RefId = session.Id
                    }, userId);
                }
                catch (ArgumentException)
                {
                    // Nomor tidak valid dsb.: pengingat WhatsApp untuk anggota ini dilewati.
                }
            }
            return memberIds.Count;
        }
        // Poin Hadir Kegiatan (PointRule) untuk catatan Hadir / Terlambat pada sesi yang ditutup; status lain mencabut poinnya.
        internal static void ApplyPoint(EF.ModelContext context, EF.TAttendanceRecord record, EF.TActivity activity, DateTime openAt, int userId)
        {
            if (record.Status == (int)AttendanceStatus.Hadir || record.Status == (int)AttendanceStatus.Terlambat)
            {
                Prestasi.MemberPoint.Award(context, record.MemberId, PointSourceType.HadirKegiatan, record.Id, (ActivityType)activity.ActivityType, null,
                    DateOnly.FromDateTime(openAt), null, "Hadir: " + activity.Title, userId);
            }
            else
            {
                Prestasi.MemberPoint.Revoke(context, PointSourceType.HadirKegiatan, record.Id);
            }
        }
        private static MonitorData Monitor(EF.ModelContext context, int sessionId)
        {
            var session = context.TAttendanceSessions.Include(x => x.Activity).SingleOrDefault(x => x.Id == sessionId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sesi Presensi"));
            var expected = GetExpectedMemberIds(context, session.Activity, session.OpenAt).ToHashSet();
            var records = context.TAttendanceRecords.Where(x => x.SessionId == sessionId)
                .Select(x => new { x.Id, x.MemberId, x.Status, x.CheckInAt, x.CheckOutAt, x.Notes }).ToList();
            var recordIds = records.Select(x => x.Id).ToList();
            var points = context.TAttendanceRecordIns.Where(x => recordIds.Contains(x.AttendanceId))
                .Select(x => new { x.AttendanceId, x.Latitude, x.Longitude }).ToList().ToDictionary(x => x.AttendanceId);
            var memberIds = expected.Union(records.Select(x => x.MemberId)).ToList();
            var members = MemberInfo(context, memberIds);
            var byMember = records.ToDictionary(x => x.MemberId);

            var rows = memberIds.Select(id =>
            {
                byMember.TryGetValue(id, out var rec);
                members.TryGetValue(id, out var info);
                int? distance = null;
                if (rec != null && points.TryGetValue(rec.Id, out var p) && p.Latitude != null && p.Longitude != null
                    && session.Latitude != null && session.Longitude != null)
                {
                    distance = (int)Math.Round(GeoFence.Distance((double)session.Latitude, (double)session.Longitude, (double)p.Latitude, (double)p.Longitude));
                }
                return new MonitorRow(id, info?.Name ?? "Anggota #" + id, info?.Number, info?.Section, expected.Contains(id), rec?.Id,
                    rec == null ? null : rec.Id.ToString().EncryptString<AttendanceRecord>(), rec == null ? null : (AttendanceStatus)rec.Status,
                    rec?.CheckInAt, rec?.CheckOutAt, distance, rec?.Notes);
            }).OrderBy(x => x.Status == null).ThenBy(x => x.CheckInAt).ThenBy(x => x.Name).ToList();

            return new MonitorData(session.Id, session.Name, session.Activity.Title, session.OpenAt, session.CloseAt, (AttendanceSessionStatus)session.Status,
                (AttendanceMode)session.AttendanceMode, session.RadiusMeter, DateTime.Now, rows);
        }
        // Catatan presensi sesi Ditutup dalam [from, to), opsional per kegiatan / section saat ini / anggota.
        private static IQueryable<EF.TAttendanceRecord> ClosedRecords(EF.ModelContext context, DateTime from, DateTime to, int? activityId, int? sectionId, int? memberId)
        {
            int closed = (int)AttendanceSessionStatus.Closed;
            var query = context.TAttendanceRecords.Where(x => x.Session.Status == closed && x.Session.OpenAt >= from && x.Session.OpenAt < to
                && (activityId == null || x.Session.ActivityId == activityId) && (memberId == null || x.MemberId == memberId));
            if (sectionId != null)
            {
                query = query.Where(x => x.Member.TSectionMembers.Any(s => s.SectionId == sectionId && s.EndDate == null));
            }
            return query;
        }
        // Tren per bulan; bulan tanpa catatan di antara from..to tetap muncul dengan nilai nol.
        private static List<MonthStat> Monthly(EF.ModelContext context, DateTime from, DateTime to, int? sectionId, int? memberId)
        {
            int hadir = (int)AttendanceStatus.Hadir, telat = (int)AttendanceStatus.Terlambat, izin = (int)AttendanceStatus.Izin,
                sakit = (int)AttendanceStatus.Sakit, alpa = (int)AttendanceStatus.Alpa;
            var grouped = ClosedRecords(context, from, to, null, sectionId, memberId)
                .GroupBy(x => new { x.Session.OpenAt.Year, x.Session.OpenAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Hadir = g.Count(x => x.Status == hadir),
                    Terlambat = g.Count(x => x.Status == telat),
                    Izin = g.Count(x => x.Status == izin),
                    Sakit = g.Count(x => x.Status == sakit),
                    Alpa = g.Count(x => x.Status == alpa)
                }).ToList()
                .ToDictionary(x => (x.Year, x.Month));
            var result = new List<MonthStat>();
            for (var m = new DateTime(from.Year, from.Month, 1); m < to; m = m.AddMonths(1))
            {
                result.Add(grouped.TryGetValue((m.Year, m.Month), out var g)
                    ? new MonthStat(m.Year, m.Month, g.Hadir, g.Terlambat, g.Izin, g.Sakit, g.Alpa)
                    : new MonthStat(m.Year, m.Month, 0, 0, 0, 0, 0));
            }
            return result;
        }
        private static List<RecapRow> Recap(EF.ModelContext context, DateTime from, DateTime to, int? activityId, int? sectionId, int? memberId = null)
        {
            int hadir = (int)AttendanceStatus.Hadir, telat = (int)AttendanceStatus.Terlambat, izin = (int)AttendanceStatus.Izin,
                sakit = (int)AttendanceStatus.Sakit, alpa = (int)AttendanceStatus.Alpa;
            var query = ClosedRecords(context, from, to, activityId, sectionId, memberId);
            var grouped = query.GroupBy(x => x.MemberId).Select(g => new
            {
                MemberId = g.Key,
                Hadir = g.Count(x => x.Status == hadir),
                Terlambat = g.Count(x => x.Status == telat),
                Izin = g.Count(x => x.Status == izin),
                Sakit = g.Count(x => x.Status == sakit),
                Alpa = g.Count(x => x.Status == alpa)
            }).ToList();
            var members = MemberInfo(context, grouped.Select(x => x.MemberId).ToList());
            return grouped.Select(x =>
            {
                members.TryGetValue(x.MemberId, out var info);
                return new RecapRow(x.MemberId, info?.Name ?? "Anggota #" + x.MemberId, info?.Number, info?.Section, x.Hadir, x.Terlambat, x.Izin, x.Sakit, x.Alpa);
            }).OrderBy(x => x.Section ?? "~").ThenBy(x => x.Name).ToList();
        }
        private record MemberRow(string Name, string? Number, string? Section);
        private static Dictionary<int, MemberRow> MemberInfo(EF.ModelContext context, List<int> memberIds)
        {
            return context.MMembers.Where(x => memberIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.Fullname,
                    x.MemberNumber,
                    Section = x.TSectionMembers.Where(s => s.EndDate == null).Select(s => s.Section.Name).FirstOrDefault()
                }).ToList()
                .ToDictionary(x => x.Id, x => new MemberRow(x.Fullname, x.MemberNumber, x.Section));
        }
        private static SyncResult SyncRows(EF.ModelContext context, int userId)
        {
            var now = DateTime.Now;
            var reminded = SendReminders(context, now, userId);
            var openFrom = now.AddMinutes(EARLY_CHECKIN_MINUTE);
            int scheduled = (int)AttendanceSessionStatus.Scheduled, open = (int)AttendanceSessionStatus.Open;
            int planned = (int)ProgramStatus.Planned, ongoing = (int)ProgramStatus.Ongoing, cancelled = (int)ProgramStatus.Cancelled;

            var toOpen = context.TAttendanceSessions
                .Where(x => x.Status == scheduled && x.OpenAt <= openFrom && x.CloseAt > now && (x.Activity.Status == planned || x.Activity.Status == ongoing))
                .ToList();
            foreach (var row in toOpen)
            {
                SetOpen(context, row, userId);
            }
            var toClose = context.TAttendanceSessions
                .Where(x => (x.Status == scheduled || x.Status == open) && x.CloseAt <= now && x.Activity.Status != cancelled)
                .ToList();
            var absent = 0;
            foreach (var row in toClose)
            {
                // Sesi yang terlewat tanpa pernah dibuka (mis. job sempat mati) tetap ditutup.
                row.Status = open;
                absent += SetClose(context, row, userId);
            }
            return new SyncResult(toOpen.Count, toClose.Count, absent, reminded);
        }
        private static MyAttendance MyAttendanceData(EF.ModelContext context, int userId, int daysAhead, int historyTake)
        {
            var empty = new MyAttendance(new List<MySession>(), new List<MySession>(), new List<MySession>(), new List<MonthStat>());
            var member = context.MMembers.SingleOrDefault(x => x.UserId == userId && x.Status == (int)MemberStatus.Active);
            if (member == null)
            {
                return empty;
            }
            var now = DateTime.Now;
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var until = today.AddDays(Math.Max(daysAhead, 0) + 1);
            int cancelled = (int)AttendanceSessionStatus.Cancelled, activityCancelled = (int)ProgramStatus.Cancelled;

            // Hari ini dan mendatang: sesi yang wajib / boleh diikuti (audiens kegiatan), atau yang sudah punya catatan presensi anggota.
            var sessions = context.TAttendanceSessions.Include(x => x.Activity)
                .Where(x => x.Status != cancelled && x.OpenAt >= today && x.OpenAt < until && x.Activity.Status != activityCancelled)
                .OrderBy(x => x.OpenAt).ToList();
            var ids = sessions.Select(x => x.Id).ToList();
            var records = context.TAttendanceRecords.Where(x => x.MemberId == member.Id && ids.Contains(x.SessionId)).ToDictionary(x => x.SessionId);
            var expected = new Dictionary<(int, DateOnly), List<int>>();
            sessions = sessions.Where(x => records.ContainsKey(x.Id) || x.Activity.AudienceType == (int)AudienceType.SemuaAnggota
                || ExpectedCached(context, expected, x).Contains(member.Id)).ToList();
            // Pengajuan izin anggota (bukan yang ditarik): untuk seluruh kegiatan yang masih berlaku, atau pengajuan terakhir per sesi.
            var activityIds = sessions.Select(x => x.ActivityId).Distinct().ToList();
            int withdrawn = (int)AbsenceRequestStatus.Ditarik, waiting = (int)AbsenceRequestStatus.Menunggu, approved = (int)AbsenceRequestStatus.Disetujui;
            var requests = context.TActivityAbsenceRequests
                .Where(x => x.MemberId == member.Id && activityIds.Contains(x.ActivityId) && x.Status != withdrawn)
                .OrderByDescending(x => x.Id).ToList();
            EF.TActivityAbsenceRequest? RequestOf(EF.TAttendanceSession s)
                => requests.FirstOrDefault(r => r.ActivityId == s.ActivityId && r.SessionId == null && (r.Status == waiting || r.Status == approved))
                    ?? requests.FirstOrDefault(r => r.SessionId == s.Id);
            var current = sessions.Select(x => ToMySession(x, records.GetValueOrDefault(x.Id), now, RequestOf(x))).ToList();

            // Riwayat: catatan presensi anggota pada sesi sebelum hari ini.
            var history = context.TAttendanceRecords.Include(x => x.Session).ThenInclude(x => x.Activity)
                .Where(x => x.MemberId == member.Id && x.Session.OpenAt < today)
                .OrderByDescending(x => x.Session.OpenAt).Take(Math.Max(historyTake, 0)).ToList()
                .Select(x => ToMySession(x.Session, x, now, null)).ToList();

            var monthFrom = new DateTime(today.Year, today.Month, 1).AddMonths(1 - MONTHS_MINE);
            var monthly = Monthly(context, monthFrom, tomorrow, null, member.Id);

            return new MyAttendance(current.Where(x => x.OpenAt < tomorrow).ToList(), current.Where(x => x.OpenAt >= tomorrow).ToList(), history, monthly);
        }
        // Audiens per (kegiatan, tanggal) dihitung sekali saja untuk banyak sesi.
        private static List<int> ExpectedCached(EF.ModelContext context, Dictionary<(int, DateOnly), List<int>> cache, EF.TAttendanceSession session)
        {
            var key = (session.ActivityId, DateOnly.FromDateTime(session.OpenAt));
            if (!cache.TryGetValue(key, out var ids))
            {
                ids = GetExpectedMemberIds(context, session.Activity, session.OpenAt, false);
                cache[key] = ids;
            }
            return ids;
        }
        private static MySession ToMySession(EF.TAttendanceSession x, EF.TAttendanceRecord? rec, DateTime now, EF.TActivityAbsenceRequest? absence)
        {
            var status = (AttendanceSessionStatus)x.Status;
            var running = status == AttendanceSessionStatus.Scheduled || status == AttendanceSessionStatus.Open;
            var activityOpen = x.Activity.Status == (int)ProgramStatus.Planned || x.Activity.Status == (int)ProgramStatus.Ongoing;
            var start = CheckInStart(x);
            var inWindow = running && activityOpen && now >= start && now <= x.CloseAt;
            var absenceStatus = absence == null ? (AbsenceRequestStatus?)null : (AbsenceRequestStatus)absence.Status;
            var canRequest = running && activityOpen && now < x.CloseAt && rec == null
                && x.Activity.AudienceType != (int)AudienceType.PesertaTerdaftar
                && (absenceStatus == null || absenceStatus == AbsenceRequestStatus.Ditolak);
            return new MySession(x.Id, x.Name, x.Activity.Title, x.OpenAt, x.CloseAt, x.LateToleranceMinute, x.Location, (AttendanceMode)x.AttendanceMode,
                x.Latitude, x.Longitude, x.RadiusMeter, status, start,
                CanCheckIn: inWindow && rec == null,
                CanCheckOut: inWindow && rec?.CheckInAt != null && rec.CheckOutAt == null,
                rec?.CheckInAt, rec?.CheckOutAt, rec == null ? null : (AttendanceStatus)rec.Status,
                x.ActivityId, canRequest, absenceStatus, absence != null && absence.SessionId == null,
                absence == null ? null : absence.Id.ToString().EncryptString<ActivityAbsenceRequest>(),
                absence?.ReviewNote);
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
