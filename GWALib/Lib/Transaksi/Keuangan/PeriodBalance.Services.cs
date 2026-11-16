using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Keuangan
{
    public partial class PeriodBalance
    {
        #region SYNC
        public static List<PeriodBalance> GetDataList(int? periodId = null, int? accountId = null, PeriodBalanceStatus? status = null)
        {
            return FilterQuery(periodId, accountId, status).ToList().MappingData<PeriodBalance, EF.TPeriodBalance>();
        }
        // accountId null = semua akun aktif yang belum punya saldo di periode ini. Mengembalikan jumlah saldo yang dibuat.
        public static Response<int> Open(int periodId, int? accountId, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = CreateRows(context, periodId, accountId, userId);
                context.SaveChanges();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        public Response<PeriodBalance> Refresh(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((PeriodBalanceStatus)row.Status, "menghitung ulang saldo", PeriodBalanceStatus.Open);
                SetTotals(context, row, userId);
                context.SaveChanges();
                return new PeriodBalance(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodBalance>(msg));
        }
        public Response<PeriodBalance> Close(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetClose(context, row, userId);
                context.SaveChanges();
                return new PeriodBalance(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodBalance>(msg));
        }
        public Response<PeriodBalance> Reopen(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetReopen(context, row, userId);
                context.SaveChanges();
                return new PeriodBalance(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodBalance>(msg));
        }
        public Response<PeriodBalance> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((PeriodBalanceStatus)row.Status, "menghapus saldo periode", PeriodBalanceStatus.Open);
                var data = new PeriodBalance(Reload(row.Id));
                context.TPeriodBalances.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<PeriodBalance>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<PeriodBalance>> GetDataListAsync(int? periodId = null, int? accountId = null, PeriodBalanceStatus? status = null)
        {
            return await (await FilterQuery(periodId, accountId, status).ToListAsync()).MappingDataAsync<PeriodBalance, EF.TPeriodBalance>();
        }
        public static async Task<Response<int>> OpenAsync(int periodId, int? accountId, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = CreateRows(context, periodId, accountId, userId);
                await context.SaveChangesAsync();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        public async Task<Response<PeriodBalance>> RefreshAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                TransaksiHelper.ValidateStatus((PeriodBalanceStatus)row.Status, "menghitung ulang saldo", PeriodBalanceStatus.Open);
                SetTotals(context, row, userId);
                await context.SaveChangesAsync();
                return new PeriodBalance(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodBalance>(msg));
        }
        public async Task<Response<PeriodBalance>> CloseAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetClose(context, row, userId);
                await context.SaveChangesAsync();
                return new PeriodBalance(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodBalance>(msg));
        }
        public async Task<Response<PeriodBalance>> ReopenAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetReopen(context, row, userId);
                await context.SaveChangesAsync();
                return new PeriodBalance(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodBalance>(msg));
        }
        public async Task<Response<PeriodBalance>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                TransaksiHelper.ValidateStatus((PeriodBalanceStatus)row.Status, "menghapus saldo periode", PeriodBalanceStatus.Open);
                var data = new PeriodBalance(Reload(row.Id));
                context.TPeriodBalances.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<PeriodBalance>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TPeriodBalance Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TPeriodBalance> FilterQuery(int? periodId, int? accountId, PeriodBalanceStatus? status)
        {
            return _Querys.Where(x => (periodId == null || x.PeriodId == periodId)
                    && (accountId == null || x.AccountId == accountId)
                    && (status == null || x.Status == (int)status))
                .OrderByDescending(x => x.Period.StartDate).ThenBy(x => x.Account.Name);
        }
        private static EF.TPeriodBalance GetRow(EF.ModelContext context, int id)
        {
            return context.TPeriodBalances.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Saldo Periode"));
        }
        private static async Task<EF.TPeriodBalance> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TPeriodBalances.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Saldo Periode"));
        }
        private static int CreateRows(EF.ModelContext context, int periodId, int? accountId, int userId)
        {
            var period = ManagementPeriod.ValidateOpenPeriod(context, periodId);
            List<EF.MFinancialAccount> accounts;
            if (accountId > 0)
            {
                var account = context.MFinancialAccounts.SingleOrDefault(x => x.Id == accountId)
                    ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Akun Keuangan"));
                ValidateUtility.CheckDuplicateData<EF.TPeriodBalance>(context, x => x.PeriodId == periodId && x.AccountId == account.Id,
                    string.Format(ExceptionMessage.DATA_DUPLICATE, "periode dan akun"));
                accounts = new() { account };
            }
            else
            {
                accounts = context.MFinancialAccounts
                    .Where(x => x.IsActive == (int)ActiveStatus.Aktif && !context.TPeriodBalances.Any(b => b.PeriodId == periodId && b.AccountId == x.Id))
                    .ToList();
            }
            if (accounts.Count == 0)
            {
                return 0;
            }

            var nextId = SequenceUtility.ReserveIds<EF.TPeriodBalance>(context, accounts.Count);
            var now = DateTime.Now;
            foreach (var account in accounts)
            {
                // Saldo akhir periode sebelumnya (berdasarkan tanggal mulai periode) yang sudah ditutup.
                var previousClosing = context.TPeriodBalances
                    .Where(x => x.AccountId == account.Id && x.Status == (int)PeriodBalanceStatus.Closed && x.Period.StartDate < period.StartDate)
                    .OrderByDescending(x => x.Period.StartDate)
                    .Select(x => x.ClosingBalance)
                    .FirstOrDefault();
                var row = new EF.TPeriodBalance
                {
                    Id = nextId++,
                    PeriodId = periodId,
                    AccountId = account.Id,
                    OpeningBalance = previousClosing ?? account.OpeningBalance,
                    Status = (int)PeriodBalanceStatus.Open,
                    InsDate = now,
                    InsBy = userId
                };
                SetTotals(context, row, null);
                GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
                context.TPeriodBalances.Add(row);
            }
            return accounts.Count;
        }
        // userId null = baris baru (kolom upd tidak diisi).
        private static void SetTotals(EF.ModelContext context, EF.TPeriodBalance row, int? userId)
        {
            var totals = context.TFinancialTransactions
                .Where(x => x.PeriodId == row.PeriodId && x.AccountId == row.AccountId && x.Status == (int)TransactionStatus.Posted)
                .GroupBy(x => x.TransactionType)
                .Select(g => new { Type = g.Key, Amount = g.Sum(x => x.Amount) })
                .ToList();
            row.TotalIncome = totals.Where(x => x.Type == (int)CashFlowType.Income).Sum(x => x.Amount);
            row.TotalExpense = totals.Where(x => x.Type == (int)CashFlowType.Expense).Sum(x => x.Amount);
            if (userId != null)
            {
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
            }
        }
        private static void SetClose(EF.ModelContext context, EF.TPeriodBalance row, int userId)
        {
            TransaksiHelper.ValidateStatus((PeriodBalanceStatus)row.Status, "menutup saldo periode", PeriodBalanceStatus.Open);
            if (context.TFinancialTransactions.Any(x => x.PeriodId == row.PeriodId && x.AccountId == row.AccountId && x.Status == (int)TransactionStatus.Draft))
            {
                throw new ArgumentException(ExceptionMessage.PERIOD_BALANCE_HAS_DRAFT);
            }
            SetTotals(context, row, userId);
            row.ClosingBalance = row.OpeningBalance + row.TotalIncome - row.TotalExpense;
            row.Status = (int)PeriodBalanceStatus.Closed;
            row.ClosedAt = DateTime.Now;
            row.ClosedBy = userId;
        }
        private static void SetReopen(EF.ModelContext context, EF.TPeriodBalance row, int userId)
        {
            TransaksiHelper.ValidateStatus((PeriodBalanceStatus)row.Status, "membuka kembali saldo periode", PeriodBalanceStatus.Closed);
            var startDate = context.MManagementPeriods.Where(x => x.Id == row.PeriodId).Select(x => x.StartDate).Single();
            if (context.TPeriodBalances.Any(x => x.AccountId == row.AccountId && x.Period.StartDate > startDate))
            {
                throw new ArgumentException(ExceptionMessage.PERIOD_BALANCE_NEXT_EXISTS);
            }
            row.ClosingBalance = null;
            row.Status = (int)PeriodBalanceStatus.Open;
            row.ClosedAt = null;
            row.ClosedBy = null;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
