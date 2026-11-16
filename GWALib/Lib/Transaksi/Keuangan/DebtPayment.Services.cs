using GWALib.General;
using GWALib.General.Utilities;
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
    public partial class DebtPayment
    {
        private const int NOTES_LENGTH = 255;

        #region SYNC
        public static List<DebtPayment> GetDataList(int debtId)
        {
            return FilterQuery(debtId).ToList().MappingData<DebtPayment, EF.TDebtPayment>();
        }
        // Transaksi, pembayaran dan status hutang disimpan dalam satu transaksi DB.
        public static Response<DebtPayment> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                transaction.Commit();
                return new DebtPayment(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<DebtPayment>(msg));
        }
        public Response<DebtPayment> Delete(string? reason, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var data = new DebtPayment(Reload(row.Id));
                SetDelete(context, row, reason, userId);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<DebtPayment>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<DebtPayment>> GetDataListAsync(int debtId)
        {
            return await (await FilterQuery(debtId).ToListAsync()).MappingDataAsync<DebtPayment, EF.TDebtPayment>();
        }
        public static async Task<Response<DebtPayment>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new DebtPayment(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<DebtPayment>(msg));
        }
        public async Task<Response<DebtPayment>> DeleteAsync(string? reason, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var data = new DebtPayment(Reload(row.Id));
                SetDelete(context, row, reason, userId);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<DebtPayment>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TDebtPayment Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TDebtPayment> FilterQuery(int debtId)
        {
            return _Querys.Where(x => x.DebtId == debtId).OrderBy(x => x.PaidDate).ThenBy(x => x.Id);
        }
        private static EF.TDebtPayment GetRow(EF.ModelContext context, int id)
        {
            return context.TDebtPayments.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pembayaran Hutang"));
        }
        private static async Task<EF.TDebtPayment> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TDebtPayments.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pembayaran Hutang"));
        }
        private static EF.TDebtPayment CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var debt = Debt.GetPayableDebt(context, newrow.DebtId);
            TransaksiHelper.ValidatePositive(newrow.Amount, "Nominal Pembayaran");
            if (newrow.PaidDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Bayar"));
            }
            if (newrow.PaidDate < debt.DebtDate)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATE_BEFORE_INVALID, "Tanggal Bayar", "Tanggal Hutang"));
            }
            var remaining = debt.Amount - debt.PaidAmount;
            if (newrow.Amount > remaining)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DEBT_PAYMENT_EXCEEDS, remaining));
            }

            var trx = FinancialTransaction.CreateRow(context, new FinancialTransaction.NewRow
            {
                AccountId = newrow.AccountId,
                CategoryId = newrow.CategoryId,
                PeriodId = FinancialTransaction.OpenPeriodOrDefault(context, debt.PeriodId),
                TransactionType = CashFlowType.Expense,
                Amount = newrow.Amount,
                TransactionDate = newrow.PaidDate,
                Description = $"Pembayaran hutang {debt.DebtNumber} - {debt.CreditorName}"
            }, null, true, userId);

            var row = new EF.TDebtPayment
            {
                Id = SequenceUtility.GetNextId<EF.TDebtPayment>(context),
                DebtId = debt.Id,
                TransactionId = trx.Id,
                Amount = newrow.Amount,
                PaidDate = newrow.PaidDate,
                Notes = TransaksiHelper.Truncate(newrow.Notes, NOTES_LENGTH),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TDebtPayments.Add(row);
            Debt.RecalculatePaid(context, debt, userId);
            return row;
        }
        private static void SetDelete(EF.ModelContext context, EF.TDebtPayment row, string? reason, int userId)
        {
            var debt = Debt.GetRow(context, row.DebtId);
            TransaksiHelper.ValidateStatus((DebtStatus)debt.Status, "membatalkan pembayaran hutang", DebtStatus.Sebagian, DebtStatus.Lunas);
            FinancialTransaction.VoidRow(context, row.TransactionId, reason ?? "Pembayaran hutang dibatalkan", userId);
            context.TDebtPayments.Remove(row);
            Debt.RecalculatePaid(context, debt, userId);
        }
        #endregion
    }
}
