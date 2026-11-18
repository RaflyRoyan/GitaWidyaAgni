using GWALib.General;
using GWAShared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GWALib.Lib.Transaksi.Information
{
    /// <summary>
    /// Ringkasan dashboard (read-only, query agregat): periode aktif, anggota, agenda mendatang, kas, arus kas 6 bulan, anggota per section,
    /// anggota per divisi, persetujuan tertunda, inventaris, papan poin, dan tugas pribadi anggota login.
    /// Web memilih bagian yang boleh tampil sesuai hak akses user.
    /// </summary>
    public class Dashboard
    {
        public record AgendaLine(int Id, string Title, ActivityType Type, DateTime StartAt, string? Location);
        public record MonthLine(string Month, decimal Income, decimal Expense);
        public record DivisionLine(string Name, int Count);
        public record PointLine(string Name, int Points);
        public record TaskLine(string Kind, string Title, string? Note, DateOnly? DueDate);

        public string? PeriodName { get; set; }
        public DateOnly? PeriodEnd { get; set; }
        public int ActiveMembers { get; set; }
        public int Officers { get; set; }
        public int Alumni { get; set; }
        public int UpcomingActivities { get; set; }
        public int ActivitiesThisMonth { get; set; }
        public List<AgendaLine> Agenda { get; set; } = new();

        public decimal CashBalance { get; set; }
        public decimal IncomeThisMonth { get; set; }
        public decimal ExpenseThisMonth { get; set; }
        public decimal UnpaidDues { get; set; }
        public int UnpaidDuesCount { get; set; }
        public List<MonthLine> CashFlow { get; set; } = new();

        /// <summary>Anggota aktif per section saat ini (semua anggota punya section).</summary>
        public List<DivisionLine> Sections { get; set; } = new();
        /// <summary>Anggota divisi periode aktif (hanya anggota pilihan Ketua Umum).</summary>
        public List<DivisionLine> Divisions { get; set; } = new();
        /// <summary>Pengajuan tidak ikut kegiatan yang menunggu persetujuan.</summary>
        public int AbsenceWaiting { get; set; }
        public int LettersWaiting { get; set; }
        public int LettersInThisMonth { get; set; }
        public int FundingWaiting { get; set; }
        public int ProcurementWaiting { get; set; }
        public int BorrowPending { get; set; }
        public int BorrowOut { get; set; }
        public int BorrowOverdue { get; set; }
        public int AssetCount { get; set; }
        public List<PointLine> TopPoints { get; set; } = new();

        /// <summary>Tugas anggota login: disposisi terbuka + tagihan iuran belum bayar (kosong bila user bukan anggota).</summary>
        public List<TaskLine> MyTasks { get; set; } = new();

        public static Dashboard GetSummary(string user)
        {
            var context = DBClass.GetContext();
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);
            var monthStart = new DateOnly(today.Year, today.Month, 1);
            var d = new Dashboard();

            var period = context.MManagementPeriods.AsNoTracking().Where(x => x.Status == (int)ManagementPeriodStatus.Active)
                .Select(x => new { x.Id, x.Name, x.EndDate }).FirstOrDefault();
            d.PeriodName = period?.Name;
            d.PeriodEnd = period?.EndDate;

            // Anggota.
            var members = context.MMembers.AsNoTracking().GroupBy(x => x.Status).Select(g => new { g.Key, Count = g.Count() }).ToList();
            d.ActiveMembers = members.Where(x => x.Key == (int)MemberStatus.Active).Sum(x => x.Count);
            d.Alumni = members.Where(x => x.Key == (int)MemberStatus.Alumni).Sum(x => x.Count);
            d.Officers = period == null ? 0 : context.TManagementAssignments.Where(x => x.PeriodId == period.Id && x.IsCurrent == 1).Select(x => x.MemberId).Distinct().Count();
            d.Divisions = context.MDivisions.AsNoTracking().Where(x => x.IsActive == 1).OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(x => new DivisionLine(x.Name, x.TDivisionMembers.Count(m => m.IsActive == 1 && (m.PeriodId == null || period == null || m.PeriodId == period.Id))))
                .ToList();

            d.Sections = context.MSections.AsNoTracking().Where(x => x.IsActive == 1).OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(x => new DivisionLine(x.Name, x.TSectionMembers.Count(m => m.EndDate == null && m.Member.Status == (int)MemberStatus.Active)))
                .ToList();

            // Kegiatan.
            var open = new[] { (int)ActivityStatus.Planned, (int)ActivityStatus.Ongoing };
            var activities = context.TActivities.AsNoTracking().Where(x => open.Contains(x.Status) && x.ParentActivityId == null);
            d.UpcomingActivities = activities.Count(x => x.StartAt >= now);
            var monthStartDt = monthStart.ToDateTime(TimeOnly.MinValue);
            d.ActivitiesThisMonth = context.TActivities.Count(x => x.StartAt >= monthStartDt && x.StartAt < monthStartDt.AddMonths(1) && x.Status != (int)ActivityStatus.Cancelled);
            // Agenda: sesi presensi mendatang (kegiatan rutin yang sedang berjalan) + kegiatan yang belum mulai, urut waktu.
            var sessions = context.TAttendanceSessions.AsNoTracking()
                .Where(x => x.CloseAt >= now && (x.Status == (int)AttendanceSessionStatus.Scheduled || x.Status == (int)AttendanceSessionStatus.Open)
                    && x.Activity.Status != (int)ActivityStatus.Cancelled)
                .OrderBy(x => x.OpenAt).Take(6)
                .Select(x => new AgendaLine(x.ActivityId, x.Activity.Title, (ActivityType)x.Activity.ActivityType, x.OpenAt, x.Location ?? x.Activity.Location))
                .ToList();
            var starting = activities.Where(x => x.StartAt >= now).OrderBy(x => x.StartAt).Take(6)
                .Select(x => new AgendaLine(x.Id, x.Title, (ActivityType)x.ActivityType, x.StartAt, x.Location)).ToList();
            d.Agenda = sessions.Concat(starting)
                .GroupBy(x => new { x.Id, x.StartAt.Date }).Select(g => g.First())
                .OrderBy(x => x.StartAt).Take(6).ToList();

            // Keuangan: saldo = saldo awal akun aktif + pemasukan − pengeluaran Posted.
            var accounts = context.MFinancialAccounts.Where(x => x.IsActive == 1);
            var posted = context.TFinancialTransactions.AsNoTracking().Where(x => x.Status == (int)TransactionStatus.Posted);
            var sums = posted.Where(x => accounts.Any(a => a.Id == x.AccountId)).GroupBy(x => x.TransactionType)
                .Select(g => new { g.Key, Sum = g.Sum(x => x.Amount) }).ToList();
            d.CashBalance = (accounts.Sum(x => (decimal?)x.OpeningBalance) ?? 0)
                + sums.Where(x => x.Key == (int)CashFlowType.Income).Sum(x => x.Sum)
                - sums.Where(x => x.Key == (int)CashFlowType.Expense).Sum(x => x.Sum);
            var from = monthStart.AddMonths(-5);
            var monthly = posted.Where(x => x.TransactionDate >= from)
                .GroupBy(x => new { x.TransactionDate.Year, x.TransactionDate.Month, x.TransactionType })
                .Select(g => new { g.Key.Year, g.Key.Month, g.Key.TransactionType, Sum = g.Sum(x => x.Amount) }).ToList();
            var id = new System.Globalization.CultureInfo("id-ID");
            for (var m = from; m <= monthStart; m = m.AddMonths(1))
            {
                decimal Of(CashFlowType t) => monthly.Where(x => x.Year == m.Year && x.Month == m.Month && x.TransactionType == (int)t).Sum(x => x.Sum);
                d.CashFlow.Add(new MonthLine(m.ToString("MMM yy", id), Of(CashFlowType.Income), Of(CashFlowType.Expense)));
            }
            d.IncomeThisMonth = d.CashFlow[^1].Income;
            d.ExpenseThisMonth = d.CashFlow[^1].Expense;
            var unpaid = context.TDuesBills.Where(x => x.Status == (int)DuesBillStatus.Unpaid);
            d.UnpaidDuesCount = unpaid.Count();
            d.UnpaidDues = unpaid.Sum(x => (decimal?)x.Amount) ?? 0;

            // Persetujuan & operasional.
            d.LettersWaiting = context.TLetters.Count(x => x.Status == (int)LetterStatus.WaitingApproval);
            d.AbsenceWaiting = context.TActivityAbsenceRequests.Count(x => x.Status == (int)AbsenceRequestStatus.Menunggu);
            d.LettersInThisMonth = context.TLetters.Count(x => x.Direction == (int)LetterDirection.In && x.InsDate >= monthStartDt);
            d.FundingWaiting = context.TFundingRequests.Count(x => x.Status == (int)FundingRequestStatus.Submitted);
            d.ProcurementWaiting = context.TProcurementRequests.Count(x => x.Status == (int)ProcurementStatus.Diajukan);
            d.BorrowPending = context.TBorrowRequests.Count(x => x.Status == (int)BorrowRequestStatus.Pending);
            d.BorrowOut = context.TBorrowRequests.Count(x => x.Status == (int)BorrowRequestStatus.Borrowed);
            d.BorrowOverdue = context.TBorrowRequests.Count(x => x.Status == (int)BorrowRequestStatus.Overdue);
            d.AssetCount = context.MAssets.Count(x => x.Status != (int)AssetStatus.Disposed);

            // Papan poin periode aktif (else semua periode).
            var points = context.TMemberPoints.AsNoTracking().Where(x => period == null || x.PeriodId == period.Id);
            if (period != null && !points.Any())
            {
                points = context.TMemberPoints.AsNoTracking();
            }
            d.TopPoints = points.GroupBy(x => new { x.MemberId, x.Member.Fullname })
                .Select(g => new { g.Key.Fullname, Total = g.Sum(x => x.Points) })
                .OrderByDescending(x => x.Total).Take(5).ToList()
                .Select(x => new PointLine(x.Fullname, x.Total)).ToList();

            // Tugas pribadi.
            if (int.TryParse(user, out var userId))
            {
                var memberId = context.MMembers.Where(x => x.UserId == userId).Select(x => (int?)x.Id).FirstOrDefault();
                if (memberId != null)
                {
                    d.MyTasks.AddRange(context.TLetterDispositions.AsNoTracking().Where(x => x.ToMemberId == memberId && x.Status == (int)DispositionStatus.Open)
                        .OrderBy(x => x.DueDate == null).ThenBy(x => x.DueDate).Take(5)
                        .Select(x => new TaskLine("Disposisi", x.Letter.Subject, x.Instruction, x.DueDate)).ToList());
                    d.MyTasks.AddRange(context.TDuesBills.AsNoTracking().Where(x => x.MemberId == memberId && x.Status == (int)DuesBillStatus.Unpaid)
                        .OrderBy(x => x.DueDate).Take(5)
                        .Select(x => new TaskLine("Iuran", x.DuesSetting.Name + " " + x.BillPeriod, "Rp " + x.Amount.ToString("N0"), x.DueDate)).ToList());
                }
            }
            return d;
        }
    }
}
