using GWALib.General;
using GWALib.Lib.Transaksi.Presensi;
using GWAShared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GWALib.Lib.Transaksi.Information
{
    /// <summary>
    /// Ringkasan pribadi anggota user login untuk dashboard (read-only): kehadiran bulan ini dan periode aktif, poin + peringkat periode aktif,
    /// tagihan iuran, tugas kepanitiaan mendatang, peminjaman berjalan, prestasi dan sertifikat. User tanpa anggota = null.
    /// </summary>
    public partial class Dashboard
    {
        public record MySummary(int MemberId, string Fullname, string? Section, MemberStatus Status, MemberType MemberType,
            string? PeriodName, DateOnly AttendanceFrom, AttendanceSession.MonthStat ThisMonth, int PeriodSessions, decimal PeriodPercent,
            int Points, int Rank, int RankedMembers, decimal UnpaidDues, int UnpaidDuesCount, DateOnly? NextDueDate,
            int CommitteeDuties, int BorrowActive, int Achievements, int Certificates);

        public static MySummary? GetMine(string user)
        {
            if (!int.TryParse(user, out var userId))
            {
                return null;
            }
            using var context = DBClass.GetContext();
            var m = context.MMembers.AsNoTracking().Where(x => x.UserId == userId)
                .Select(x => new
                {
                    x.Id, x.Fullname, x.Status, x.MemberType,
                    Section = x.TSectionMembers.Where(s => s.EndDate == null).Select(s => s.Section.Name).FirstOrDefault()
                }).FirstOrDefault();
            if (m == null)
            {
                return null;
            }
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);
            var monthStart = new DateOnly(today.Year, today.Month, 1);
            var period = context.MManagementPeriods.AsNoTracking().Where(x => x.Status == (int)ManagementPeriodStatus.Active)
                .Select(x => new { x.Id, x.Name, x.StartDate }).FirstOrDefault();

            // Kehadiran: sejak awal periode aktif (tanpa periode: 6 bulan terakhir), bulan ini dipisah.
            var from = period != null && period.StartDate <= today ? period.StartDate : monthStart.AddMonths(-5);
            if (from > monthStart)
            {
                from = monthStart;
            }
            var months = AttendanceSession.GetStats(from, today, null, m.Id).Months;
            var thisMonth = months.FirstOrDefault(x => x.Year == today.Year && x.Month == today.Month)
                ?? new AttendanceSession.MonthStat(today.Year, today.Month, 0, 0, 0, 0, 0);
            var sessions = months.Sum(x => x.Sessions);
            var present = months.Sum(x => x.Hadir + x.Terlambat);
            var percent = sessions == 0 ? 0 : Math.Round(100m * present / sessions, 1);

            // Poin + peringkat di antara anggota yang punya poin pada periode aktif.
            var points = 0;
            var rank = 0;
            var ranked = 0;
            if (period != null)
            {
                var totals = context.TMemberPoints.AsNoTracking().Where(x => x.PeriodId == period.Id)
                    .GroupBy(x => x.MemberId).Select(g => new { MemberId = g.Key, Total = g.Sum(x => x.Points) }).ToList();
                ranked = totals.Count;
                points = totals.Where(x => x.MemberId == m.Id).Sum(x => x.Total);
                if (totals.Any(x => x.MemberId == m.Id))
                {
                    rank = totals.Count(x => x.Total > points) + 1;
                }
            }

            var unpaid = context.TDuesBills.AsNoTracking().Where(x => x.MemberId == m.Id && x.Status == (int)DuesBillStatus.Unpaid)
                .Select(x => new { x.Amount, x.DueDate }).ToList();
            var duties = context.TCommitteeMembers.Count(x => x.MemberId == m.Id && x.Status == (int)CommitteeMemberStatus.Aktif
                && (x.Activity.EndAt ?? x.Activity.StartAt) >= now && x.Activity.Status != (int)ActivityStatus.Cancelled);
            var borrows = context.TBorrowRequests.Count(x => x.MemberId == m.Id
                && (x.Status == (int)BorrowRequestStatus.Pending || x.Status == (int)BorrowRequestStatus.Approved
                    || x.Status == (int)BorrowRequestStatus.Borrowed || x.Status == (int)BorrowRequestStatus.Overdue));
            var achievements = context.TAchievements.Count(x => x.MemberId == m.Id);
            var certificates = context.TCertificates.Count(x => x.MemberId == m.Id && x.Status == (int)CertificateStatus.Terbit);

            return new MySummary(m.Id, m.Fullname, m.Section, (MemberStatus)m.Status, (MemberType)m.MemberType,
                period?.Name, from, thisMonth, sessions, percent, points, rank, ranked,
                unpaid.Sum(x => x.Amount), unpaid.Count, unpaid.Where(x => x.DueDate != null).Min(x => x.DueDate),
                duties, borrows, achievements, certificates);
        }
    }
}
