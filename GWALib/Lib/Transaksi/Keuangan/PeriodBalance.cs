using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Keuangan
{
    /// <summary>
    /// Saldo akun per periode kepengurusan (t_period_balance), satu per (periode, akun). Alur status: Open → Closed; Closed bisa dibuka lagi
    /// selama akun belum punya saldo di periode sesudahnya.
    /// Saldo awal = saldo akhir periode sebelumnya yang sudah ditutup, atau saldo awal akun bila belum ada.
    /// Total pemasukan / pengeluaran dihitung dari transaksi Posted (Refresh); Close menghitung ulang, mengisi saldo akhir,
    /// dan mengunci transaksi (periode, akun) tersebut.
    /// </summary>
    public partial class PeriodBalance : BaseLib
    {
        public int Id { get; set; }
        public int PeriodId { get; private set; }
        public string PeriodName { get; private set; } = null!;
        public int AccountId { get; private set; }
        public string AccountName { get; private set; } = null!;
        public decimal OpeningBalance { get; private set; }
        public decimal TotalIncome { get; private set; }
        public decimal TotalExpense { get; private set; }
        public decimal? ClosingBalance { get; private set; }
        // Saldo berjalan = saldo awal + pemasukan - pengeluaran (per perhitungan terakhir).
        public decimal CurrentBalance => OpeningBalance + TotalIncome - TotalExpense;
        public PeriodBalanceStatus Status { get; private set; }
        public DateTime? ClosedAt { get; private set; }
        public int? ClosedBy { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TPeriodBalance> _Querys
        {
            get
            {
                return DBClass.GetContext().TPeriodBalances
                    .Include(x => x.Period)
                    .Include(x => x.Account)
                    .AsQueryable();
            }
        }
        public PeriodBalance()
        {
        }
        public PeriodBalance(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<PeriodBalance>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal PeriodBalance(EF.TPeriodBalance row)
        {
            SetData(row);
        }
        private void SetData(EF.TPeriodBalance row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<PeriodBalance>();
            PeriodId = row.PeriodId;
            PeriodName = row.Period?.Name ?? string.Empty;
            AccountId = row.AccountId;
            AccountName = row.Account?.Name ?? string.Empty;
            OpeningBalance = row.OpeningBalance;
            TotalIncome = row.TotalIncome;
            TotalExpense = row.TotalExpense;
            ClosingBalance = row.ClosingBalance;
            Status = (PeriodBalanceStatus)row.Status;
            ClosedAt = row.ClosedAt;
            ClosedBy = row.ClosedBy;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
