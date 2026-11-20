namespace GWALib.Lib.Transaksi.Keuangan
{
    /// <summary>
    /// Data laporan keuangan (read-only, dari transaksi Posted saja):
    /// - Buku kas (GetCashBook): per akun, saldo awal sebelum tanggal mulai (saldo awal akun + transaksi Posted sebelumnya),
    ///   setiap transaksi dalam rentang dengan saldo berjalan, lalu saldo akhir.
    /// - Arus kas (GetCashFlow): pemasukan dan pengeluaran per kategori dan per bulan dalam rentang, saldo awal / akhir semua akun.
    /// - Realisasi anggaran (GetBudgetRealization): item RAB vs realisasi (pengeluaran dikurangi pemasukan yang memakai item,
    ///   sama dengan BudgetItem.RealizedAmount), plus pengeluaran periode anggaran yang tidak memakai item anggaran.
    /// - Rekap iuran (GetDuesRecap): per pengaturan iuran, ringkasan per periode tagihan dan per anggota (tunggakan).
    /// </summary>
    public static partial class FinancialReport
    {
        public record CashLine(DateOnly Date, string Number, string Category, string? Description, decimal Income, decimal Expense, decimal Balance);
        public record CashBookAccount(int AccountId, string Name, string? Detail, decimal Opening, List<CashLine> Lines)
        {
            public decimal Income => Lines.Sum(x => x.Income);
            public decimal Expense => Lines.Sum(x => x.Expense);
            public decimal Closing => Opening + Income - Expense;
        }
        public record CashBook(DateOnly From, DateOnly To, List<CashBookAccount> Accounts)
        {
            public decimal Opening => Accounts.Sum(x => x.Opening);
            public decimal Income => Accounts.Sum(x => x.Income);
            public decimal Expense => Accounts.Sum(x => x.Expense);
            public decimal Closing => Accounts.Sum(x => x.Closing);
        }

        public record AmountRow(string Name, int Count, decimal Amount);
        public record MonthFlow(int Year, int Month, decimal Income, decimal Expense)
        {
            public decimal Net => Income - Expense;
        }
        public record CashFlow(DateOnly From, DateOnly To, string Account, decimal Opening, List<AmountRow> Income, List<AmountRow> Expense, List<MonthFlow> Months)
        {
            public decimal TotalIncome => Income.Sum(x => x.Amount);
            public decimal TotalExpense => Expense.Sum(x => x.Amount);
            public decimal Closing => Opening + TotalIncome - TotalExpense;
        }

        public record BudgetLine(string Category, string Name, string? Program, decimal Quantity, string? Unit, decimal UnitPrice, decimal Amount, decimal Realized)
        {
            public decimal Remaining => Amount - Realized;
            public decimal Percent => Amount == 0 ? 0 : Math.Round(100m * Realized / Amount, 1);
        }
        public record BudgetRealization(int BudgetId, string Name, string Period, string Status, List<BudgetLine> Lines, List<AmountRow> Unbudgeted)
        {
            public decimal Amount => Lines.Sum(x => x.Amount);
            public decimal Realized => Lines.Sum(x => x.Realized);
            public decimal Remaining => Amount - Realized;
            public decimal Percent => Amount == 0 ? 0 : Math.Round(100m * Realized / Amount, 1);
        }

        public record DuesPeriodRow(string BillPeriod, int Bills, int Paid, int Waived, int Unpaid, decimal Received, decimal Outstanding);
        public record DuesMemberRow(int MemberId, string Name, string? MemberNumber, string? Section, int Bills, int Paid, int Waived, int Unpaid,
            decimal Received, decimal Outstanding, List<string> UnpaidPeriods);
        public record DuesRecap(int SettingId, string Name, decimal Amount, string Frequency, string? Period, List<DuesPeriodRow> Periods, List<DuesMemberRow> Members)
        {
            public decimal Received => Periods.Sum(x => x.Received);
            public decimal Outstanding => Periods.Sum(x => x.Outstanding);
        }
    }
}
