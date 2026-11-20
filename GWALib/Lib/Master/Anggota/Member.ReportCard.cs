using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Transaksi.Presensi;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;

namespace GWALib.Lib.Master.Anggota
{
    /// <summary>
    /// Rapor anggota per periode kepengurusan: identitas, kehadiran per bulan (sesi Ditutup dalam rentang periode,
    /// dibatasi sampai hari ini), poin per sumber + peringkat di periode itu, jabatan, divisi, tugas kepanitiaan
    /// (kegiatan yang mulai dalam periode, status Aktif), prestasi, dan sertifikat Terbit.
    /// GetReportCard untuk pengurus; GetMyReportCard untuk anggota user login.
    /// </summary>
    public partial class Member
    {
        public record ReportPoint(string Source, int Count, int Points);
        public record ReportItem(DateOnly? Date, string Title, string? Detail);
        public record ReportCard(int MemberId, string Fullname, string? MemberNumber, string? Nim, string? Fakultas, string? ProgramStudi, short? Angkatan,
            string? Section, MemberStatus Status, DateOnly? JoinedAt, byte[]? Photo,
            string PeriodName, DateOnly PeriodStart, DateOnly PeriodEnd, DateOnly Until,
            List<AttendanceSession.MonthStat> Attendance, List<ReportPoint> Points, int Rank, int RankedMembers,
            List<ReportItem> Positions, List<ReportItem> Divisions, List<ReportItem> Committees, List<ReportItem> Achievements, List<ReportItem> Certificates)
        {
            public int Hadir => Attendance.Sum(x => x.Hadir);
            public int Terlambat => Attendance.Sum(x => x.Terlambat);
            public int Izin => Attendance.Sum(x => x.Izin);
            public int Sakit => Attendance.Sum(x => x.Sakit);
            public int Alpa => Attendance.Sum(x => x.Alpa);
            public int Sessions => Hadir + Terlambat + Izin + Sakit + Alpa;
            public decimal Percent => Sessions == 0 ? 0 : Math.Round(100m * (Hadir + Terlambat) / Sessions, 1);
            public int TotalPoints => Points.Sum(x => x.Points);
        }

        #region SYNC
        public static ReportCard GetReportCard(int memberId, int periodId)
        {
            using var context = DBClass.GetContext();
            return BuildReportCard(context, memberId, periodId);
        }

        /// <summary>Rapor milik user login; anggota Tidak Aktif tetap boleh melihat rapor periode lamanya.</summary>
        public static ReportCard GetMyReportCard(string user, int periodId)
        {
            var userId = ValidateUtility.ValidateUser(user);
            using var context = DBClass.GetContext();
            var memberId = context.MMembers.Where(x => x.UserId == userId).Select(x => (int?)x.Id).SingleOrDefault()
                ?? throw new ArgumentException(ExceptionMessage.USER_NOT_MEMBER);
            return BuildReportCard(context, memberId, periodId);
        }
        #endregion

        #region HELPER
        private static ReportCard BuildReportCard(EF.ModelContext context, int memberId, int periodId)
        {
            var m = context.MMembers.AsNoTracking().Where(x => x.Id == memberId)
                .Select(x => new
                {
                    x.Id, x.Fullname, x.MemberNumber, x.Nim, x.Fakultas, x.ProgramStudi, x.Angkatan, x.Status, x.JoinedAt,
                    Section = x.TSectionMembers.Where(s => s.EndDate == null).Select(s => s.Section.Name).FirstOrDefault(),
                    Photo = x.MMemberPhotos.OrderByDescending(p => p.Id).Select(p => p.Photo).FirstOrDefault()
                }).SingleOrDefault()
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Anggota"));
            var p = context.MManagementPeriods.AsNoTracking().Where(x => x.Id == periodId)
                .Select(x => new { x.Name, x.StartDate, x.EndDate }).SingleOrDefault()
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Periode"));
            var today = DateOnly.FromDateTime(DateTime.Today);
            var until = p.EndDate < today ? p.EndDate : today;
            var start = p.StartDate.ToDateTime(TimeOnly.MinValue);
            var end = p.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var attendance = until < p.StartDate ? new List<AttendanceSession.MonthStat>() : AttendanceSession.GetStats(p.StartDate, until, null, memberId).Months;

            var totals = context.TMemberPoints.AsNoTracking().Where(x => x.PeriodId == periodId)
                .GroupBy(x => x.MemberId).Select(g => new { g.Key, Total = g.Sum(x => x.Points) }).ToList();
            var mine = totals.FirstOrDefault(x => x.Key == memberId)?.Total;
            var rank = mine == null ? 0 : totals.Count(x => x.Total > mine) + 1;
            var points = context.TMemberPoints.AsNoTracking().Where(x => x.PeriodId == periodId && x.MemberId == memberId)
                .GroupBy(x => x.SourceType).Select(g => new { g.Key, Count = g.Count(), Points = g.Sum(x => x.Points) }).ToList()
                .OrderBy(x => x.Key).Select(x => new ReportPoint(((PointSourceType)x.Key).GetDescription(), x.Count, x.Points)).ToList();

            var positions = context.TManagementAssignments.AsNoTracking().Where(x => x.MemberId == memberId && x.PeriodId == periodId)
                .OrderBy(x => x.StartDate)
                .Select(x => new { x.StartDate, x.EndDate, Position = x.Position.Name, Division = x.Division != null ? x.Division.Name : null, Section = x.Section != null ? x.Section.Name : null })
                .ToList()
                .Select(x => new ReportItem(x.StartDate, x.Position + (x.Division ?? x.Section) switch { null => "", var d => " " + d },
                    x.EndDate == null ? "Menjabat" : "s.d. " + x.EndDate.Value.ToString("dd/MM/yyyy"))).ToList();
            var divisions = context.TDivisionMembers.AsNoTracking().Where(x => x.MemberId == memberId && x.PeriodId == periodId)
                .OrderBy(x => x.JoinedAt)
                .Select(x => new { x.JoinedAt, Division = x.Division.Name, x.MemberRole, x.LeftAt })
                .ToList()
                .Select(x => new ReportItem(x.JoinedAt, x.Division, x.MemberRole + (x.LeftAt == null ? "" : ", keluar " + x.LeftAt.Value.ToString("dd/MM/yyyy")))).ToList();
            var committees = context.TCommitteeMembers.AsNoTracking()
                .Where(x => x.MemberId == memberId && x.Status == (int)CommitteeMemberStatus.Aktif && x.Activity.StartAt >= start && x.Activity.StartAt < end)
                .OrderBy(x => x.Activity.StartAt)
                .Select(x => new { x.Activity.StartAt, x.Activity.Title, x.CommitteeRole, Section = x.Section != null ? x.Section.Name : null })
                .ToList()
                .Select(x => new ReportItem(DateOnly.FromDateTime(x.StartAt), x.Title,
                    ((CommitteeRole)x.CommitteeRole).GetDescription() + (x.Section == null ? "" : " — " + x.Section))).ToList();
            var achievements = context.TAchievements.AsNoTracking()
                .Where(x => x.MemberId == memberId && x.AchievedDate >= p.StartDate && x.AchievedDate <= p.EndDate)
                .OrderBy(x => x.AchievedDate)
                .Select(x => new { x.AchievedDate, x.Title, x.RankTitle, x.Level, Competition = x.Competition != null ? x.Competition.Name : null })
                .ToList()
                .Select(x => new ReportItem(x.AchievedDate, x.Title,
                    string.Join(" · ", new[] { x.RankTitle, ((EventLevel)x.Level).GetDescription(), x.Competition }.Where(s => !string.IsNullOrWhiteSpace(s))))).ToList();
            var certificates = context.TCertificates.AsNoTracking()
                .Where(x => x.MemberId == memberId && x.Status == (int)CertificateStatus.Terbit && x.IssuedDate >= p.StartDate && x.IssuedDate <= p.EndDate)
                .OrderBy(x => x.IssuedDate)
                .Select(x => new { x.IssuedDate, x.Title, x.CertificateNumber, x.CertificateType })
                .ToList()
                .Select(x => new ReportItem(x.IssuedDate, x.Title, ((CertificateType)x.CertificateType).GetDescription() + " · " + x.CertificateNumber)).ToList();

            return new ReportCard(m.Id, m.Fullname, m.MemberNumber, m.Nim, m.Fakultas, m.ProgramStudi, m.Angkatan, m.Section, (MemberStatus)m.Status, m.JoinedAt,
                m.Photo, p.Name, p.StartDate, p.EndDate, until, attendance, points, rank, totals.Count,
                positions, divisions, committees, achievements, certificates);
        }
        #endregion
    }
}
