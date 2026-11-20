using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;

namespace GWALib.Lib.Transaksi.Keuangan
{
    public static partial class FinancialReport
    {
        private const int MAX_DAYS = 3 * 366;

        #region SYNC
        /// <summary>Buku kas untuk satu akun, atau semua akun aktif ditambah akun nonaktif yang punya transaksi di rentang.</summary>
        public static CashBook GetCashBook(DateOnly from, DateOnly to, int? accountId = null)
        {
            ValidateRange(from, to);
            using var context = DBClass.GetContext();
            var accounts = Accounts(context, accountId, from, to);
            var ids = accounts.Select(x => x.Id).ToList();
            var before = PostedSums(context, ids, null, from);
            var rows = Posted(context)
                .Where(x => ids.Contains(x.AccountId) && x.TransactionDate >= from && x.TransactionDate <= to)
                .OrderBy(x => x.TransactionDate).ThenBy(x => x.Id)
                .Select(x => new { x.AccountId, x.TransactionDate, x.TransactionNumber, Category = x.Category.Name, x.Description, x.TransactionType, x.Amount })
                .ToList();
            var result = new List<CashBookAccount>();
            foreach (var a in accounts)
            {
                var opening = a.OpeningBalance + before.GetValueOrDefault(a.Id);
                var balance = opening;
                var lines = new List<CashLine>();
                foreach (var r in rows.Where(x => x.AccountId == a.Id))
                {
                    var income = r.TransactionType == (int)CashFlowType.Income ? r.Amount : 0;
                    var expense = r.TransactionType == (int)CashFlowType.Expense ? r.Amount : 0;
                    balance += income - expense;
                    lines.Add(new CashLine(r.TransactionDate, r.TransactionNumber, r.Category, r.Description, income, expense, balance));
                }
                result.Add(new CashBookAccount(a.Id, a.Name, a.Detail, opening, lines));
            }
            return new CashBook(from, to, result);
        }

        /// <summary>Arus kas dalam rentang: per kategori, per bulan (bulan tanpa transaksi = 0), saldo awal / akhir.</summary>
        public static CashFlow GetCashFlow(DateOnly from, DateOnly to, int? accountId = null)
        {
            ValidateRange(from, to);
            using var context = DBClass.GetContext();
            var accounts = Accounts(context, accountId, from, to);
            var ids = accounts.Select(x => x.Id).ToList();
            var opening = accounts.Sum(x => x.OpeningBalance) + PostedSums(context, ids, null, from).Values.Sum();
            var rows = Posted(context)
                .Where(x => ids.Contains(x.AccountId) && x.TransactionDate >= from && x.TransactionDate <= to)
                .Select(x => new { x.TransactionDate, x.TransactionType, x.Amount, Category = x.Category.Name })
                .ToList();
            List<AmountRow> ByCategory(CashFlowType type) => rows.Where(x => x.TransactionType == (int)type)
                .GroupBy(x => x.Category).Select(g => new AmountRow(g.Key, g.Count(), g.Sum(x => x.Amount)))
                .OrderByDescending(x => x.Amount).ThenBy(x => x.Name).ToList();
            var months = new List<MonthFlow>();
            for (var m = new DateOnly(from.Year, from.Month, 1); m <= to; m = m.AddMonths(1))
            {
                var inMonth = rows.Where(x => x.TransactionDate.Year == m.Year && x.TransactionDate.Month == m.Month).ToList();
                months.Add(new MonthFlow(m.Year, m.Month,
                    inMonth.Where(x => x.TransactionType == (int)CashFlowType.Income).Sum(x => x.Amount),
                    inMonth.Where(x => x.TransactionType == (int)CashFlowType.Expense).Sum(x => x.Amount)));
            }
            var name = accountId == null ? "Semua akun" : accounts.FirstOrDefault()?.Name ?? "-";
            return new CashFlow(from, to, name, opening, ByCategory(CashFlowType.Income), ByCategory(CashFlowType.Expense), months);
        }

        /// <summary>Realisasi satu anggaran: item RAB vs realisasi, plus pengeluaran Posted periode itu yang tidak memakai item anggaran.</summary>
        public static BudgetRealization GetBudgetRealization(int budgetId)
        {
            using var context = DBClass.GetContext();
            var budget = context.TBudgets.AsNoTracking().Where(x => x.Id == budgetId)
                .Select(x => new { x.Id, x.Name, x.PeriodId, Period = x.Period.Name, x.Status }).SingleOrDefault()
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Anggaran"));
            var realized = Posted(context).Where(x => x.BudgetItem!.BudgetId == budgetId)
                .GroupBy(x => x.BudgetItemId!.Value)
                .Select(g => new { Id = g.Key, Amount = g.Sum(x => x.TransactionType == (int)CashFlowType.Expense ? x.Amount : -x.Amount) })
                .ToDictionary(x => x.Id, x => x.Amount);
            var lines = context.TBudgetItems.AsNoTracking().Where(x => x.BudgetId == budgetId)
                .OrderBy(x => x.Category.Name).ThenBy(x => x.Id)
                .Select(x => new { x.Id, Category = x.Category.Name, x.Name, Program = x.Program != null ? x.Program.Title : null, x.Quantity, x.Unit, x.UnitPrice, x.Amount })
                .ToList()
                .Select(x => new BudgetLine(x.Category, x.Name, x.Program, x.Quantity, x.Unit, x.UnitPrice, x.Amount, realized.GetValueOrDefault(x.Id)))
                .ToList();
            var unbudgeted = Posted(context)
                .Where(x => x.PeriodId == budget.PeriodId && x.BudgetItemId == null && x.TransactionType == (int)CashFlowType.Expense)
                .GroupBy(x => x.Category.Name)
                .Select(g => new AmountRow(g.Key, g.Count(), g.Sum(x => x.Amount)))
                .ToList().OrderByDescending(x => x.Amount).ToList();
            return new BudgetRealization(budget.Id, budget.Name, budget.Period, ((BudgetStatus)budget.Status).GetDescription(), lines, unbudgeted);
        }

        /// <summary>Rekap iuran satu pengaturan iuran, opsional satu section (section anggota saat ini). Tagihan Dibebaskan bukan tunggakan.</summary>
        public static DuesRecap GetDuesRecap(int duesSettingId, int? sectionId = null)
        {
            using var context = DBClass.GetContext();
            var setting = context.MDuesSettings.AsNoTracking().Where(x => x.Id == duesSettingId)
                .Select(x => new { x.Id, x.Name, x.Amount, x.Frequency, Period = x.Period != null ? x.Period.Name : null }).SingleOrDefault()
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pengaturan Iuran"));
            int paid = (int)DuesBillStatus.Paid, waived = (int)DuesBillStatus.Waived, unpaid = (int)DuesBillStatus.Unpaid;
            var bills = context.TDuesBills.AsNoTracking()
                .Where(x => x.DuesSettingId == duesSettingId
                    && (sectionId == null || x.Member.TSectionMembers.Any(s => s.EndDate == null && s.SectionId == sectionId)))
                .Select(x => new
                {
                    x.MemberId,
                    x.Member.Fullname,
                    x.Member.MemberNumber,
                    Section = x.Member.TSectionMembers.Where(s => s.EndDate == null).Select(s => s.Section.Name).FirstOrDefault(),
                    x.BillPeriod,
                    x.Amount,
                    x.Status
                }).ToList();
            var periods = bills.GroupBy(x => x.BillPeriod).OrderBy(g => g.Key)
                .Select(g => new DuesPeriodRow(g.Key, g.Count(), g.Count(x => x.Status == paid), g.Count(x => x.Status == waived), g.Count(x => x.Status == unpaid),
                    g.Where(x => x.Status == paid).Sum(x => x.Amount), g.Where(x => x.Status == unpaid).Sum(x => x.Amount)))
                .ToList();
            var members = bills.GroupBy(x => x.MemberId)
                .Select(g =>
                {
                    var f = g.First();
                    return new DuesMemberRow(g.Key, f.Fullname, f.MemberNumber, f.Section, g.Count(), g.Count(x => x.Status == paid), g.Count(x => x.Status == waived),
                        g.Count(x => x.Status == unpaid), g.Where(x => x.Status == paid).Sum(x => x.Amount), g.Where(x => x.Status == unpaid).Sum(x => x.Amount),
                        g.Where(x => x.Status == unpaid).Select(x => x.BillPeriod).OrderBy(x => x).ToList());
                })
                .OrderByDescending(x => x.Outstanding).ThenBy(x => x.Section).ThenBy(x => x.Name).ToList();
            return new DuesRecap(setting.Id, setting.Name, setting.Amount, ((DuesFrequency)setting.Frequency).GetDescription(), setting.Period, periods, members);
        }
        #endregion

        #region HELPER
        private record AccountInfo(int Id, string Name, string? Detail, decimal OpeningBalance);

        private static void ValidateRange(DateOnly from, DateOnly to)
        {
            if (to < from)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            if (to.DayNumber - from.DayNumber > MAX_DAYS)
            {
                throw new ArgumentException("Rentang laporan maksimal 3 tahun.");
            }
        }

        private static IQueryable<EF.TFinancialTransaction> Posted(EF.ModelContext context)
        {
            return context.TFinancialTransactions.AsNoTracking().Where(x => x.Status == (int)TransactionStatus.Posted);
        }

        // Satu akun (harus ada), atau akun aktif + akun nonaktif yang punya transaksi Posted di rentang.
        private static List<AccountInfo> Accounts(EF.ModelContext context, int? accountId, DateOnly from, DateOnly to)
        {
            var posted = (int)TransactionStatus.Posted;
            var list = context.MFinancialAccounts.AsNoTracking()
                .Where(x => accountId != null
                    ? x.Id == accountId
                    : x.IsActive == 1 || x.TFinancialTransactions.Any(t => t.Status == posted && t.TransactionDate >= from && t.TransactionDate <= to))
                .OrderBy(x => x.AccountType).ThenBy(x => x.Name)
                .Select(x => new { x.Id, x.Name, x.BankName, x.AccountNumber, x.OpeningBalance })
                .ToList()
                .Select(x => new AccountInfo(x.Id, x.Name,
                    string.Join(" ", new[] { x.BankName, x.AccountNumber }.Where(s => !string.IsNullOrWhiteSpace(s))) is { Length: > 0 } d ? d : null,
                    x.OpeningBalance))
                .ToList();
            if (accountId != null && list.Count == 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Akun Kas"));
            }
            return list;
        }

        // Jumlah bersih (pemasukan − pengeluaran) transaksi Posted per akun sebelum tanggal `before`.
        private static Dictionary<int, decimal> PostedSums(EF.ModelContext context, List<int> accountIds, DateOnly? from, DateOnly before)
        {
            return Posted(context)
                .Where(x => accountIds.Contains(x.AccountId) && x.TransactionDate < before && (from == null || x.TransactionDate >= from))
                .GroupBy(x => x.AccountId)
                .Select(g => new { g.Key, Net = g.Sum(x => x.TransactionType == (int)CashFlowType.Income ? x.Amount : -x.Amount) })
                .ToDictionary(x => x.Key, x => x.Net);
        }
        #endregion
    }
}
