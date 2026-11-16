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
    public partial class Debt
    {
        public const string NUMBER_DOCUMENT_TYPE = "DEBT";
        private const string NUMBER_FALLBACK_PREFIX = "HT";
        private const int DESCRIPTION_LENGTH = 500;

        #region SYNC
        public List<DebtPayment> GetPayments(bool refresh)
        {
            if (refresh || PaymentList == null)
            {
                PaymentList = DebtPayment.GetDataList(Id);
            }
            return PaymentList;
        }
        public List<DebtAttachment> GetAttachments(bool refresh)
        {
            if (refresh || AttachmentList == null)
            {
                AttachmentList = DebtAttachment.GetDataList(Id);
            }
            return AttachmentList;
        }
        public static List<Debt> GetDataList(int? periodId = null, DebtStatus? status = null, string? creditor = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, status, creditor);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Debt, EF.TDebt>();
        }
        public static Response<Debt> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                transaction.Commit();
                return new Debt(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Debt>(msg));
        }
        public Response<Debt> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(row, userId);
                context.SaveChanges();
                return new Debt(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Debt>(msg));
        }
        // Menghapuskan sisa hutang (mis. diikhlaskan pemberi hutang); final.
        public Response<Debt> WriteOff(string? reason, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetWriteOff(row, reason, userId);
                context.SaveChanges();
                return new Debt(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Debt>(msg));
        }
        // Lampiran ikut terhapus lewat ON DELETE CASCADE; transaksi pinjaman dibatalkan.
        public Response<Debt> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var data = new Debt(Reload(row.Id));
                SetDelete(context, row, userId);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Debt>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<DebtPayment>> GetPaymentsAsync(bool refresh)
        {
            if (refresh || PaymentList == null)
            {
                PaymentList = await DebtPayment.GetDataListAsync(Id);
            }
            return PaymentList;
        }
        public async Task<List<DebtAttachment>> GetAttachmentsAsync(bool refresh)
        {
            if (refresh || AttachmentList == null)
            {
                AttachmentList = await DebtAttachment.GetDataListAsync(Id);
            }
            return AttachmentList;
        }
        public static async Task<List<Debt>> GetDataListAsync(int? periodId = null, DebtStatus? status = null, string? creditor = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, status, creditor);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Debt, EF.TDebt>();
        }
        public static async Task<Response<Debt>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new Debt(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Debt>(msg));
        }
        public async Task<Response<Debt>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(row, userId);
                await context.SaveChangesAsync();
                return new Debt(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Debt>(msg));
        }
        public async Task<Response<Debt>> WriteOffAsync(string? reason, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetWriteOff(row, reason, userId);
                await context.SaveChangesAsync();
                return new Debt(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Debt>(msg));
        }
        public async Task<Response<Debt>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var data = new Debt(Reload(row.Id));
                SetDelete(context, row, userId);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Debt>(msg));
        }
        #endregion

        #region INTERNAL
        // Hutang yang masih bisa dibayar (BelumBayar / Sebagian); dipakai DebtPayment.
        internal static EF.TDebt GetPayableDebt(EF.ModelContext context, int debtId)
        {
            var row = GetRow(context, debtId);
            TransaksiHelper.ValidateStatus((DebtStatus)row.Status, "membayar hutang", DebtStatus.BelumBayar, DebtStatus.Sebagian);
            return row;
        }
        // PaidAmount = jumlah pembayaran yang tersisa setelah perubahan di context; status mengikuti, kecuali Dihapuskan.
        internal static void RecalculatePaid(EF.ModelContext context, EF.TDebt debt, int userId)
        {
            context.TDebtPayments.Where(x => x.DebtId == debt.Id).Load();
            debt.PaidAmount = context.TDebtPayments.Local.Where(x => x.DebtId == debt.Id).Sum(x => x.Amount);
            if (debt.Status != (int)DebtStatus.Dihapuskan)
            {
                debt.Status = (int)(debt.PaidAmount >= debt.Amount ? DebtStatus.Lunas
                    : debt.PaidAmount > 0 ? DebtStatus.Sebagian
                    : DebtStatus.BelumBayar);
            }
            debt.UpdDate = DateTime.Now;
            debt.UpdBy = userId;
        }
        internal static EF.TDebt GetRow(EF.ModelContext context, int id)
        {
            return context.TDebts.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Hutang"));
        }
        #endregion

        #region HELPER
        private static EF.TDebt Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TDebt> FilterQuery(int? periodId, DebtStatus? status, string? creditor)
        {
            var name = string.IsNullOrWhiteSpace(creditor) ? null : creditor.Trim();
            return _Querys.Where(x => (periodId == null || x.PeriodId == periodId)
                    && (status == null || x.Status == (int)status)
                    && (name == null || x.CreditorName.Contains(name)))
                .OrderByDescending(x => x.DebtDate).ThenByDescending(x => x.Id);
        }
        private static async Task<EF.TDebt> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TDebts.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Hutang"));
        }
        private static string ValidateData(string? creditorName, decimal amount, DateOnly debtDate, DateOnly? dueDate)
        {
            if (string.IsNullOrWhiteSpace(creditorName))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Pemberi Hutang"));
            }
            TransaksiHelper.ValidatePositive(amount, "Nominal Hutang");
            if (debtDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Hutang"));
            }
            if (dueDate != null && dueDate < debtDate)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATE_BEFORE_INVALID, "Jatuh Tempo", "Tanggal Hutang"));
            }
            return creditorName.Trim();
        }
        private static EF.TDebt CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var creditorName = ValidateData(newrow.CreditorName, newrow.Amount, newrow.DebtDate, newrow.DueDate);
            var periodId = (newrow.PeriodId > 0 ? newrow.PeriodId : FinancialTransaction.GetActivePeriodId(context))
                ?? throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Periode Kepengurusan"));
            ManagementPeriod.ValidateOpenPeriod(context, periodId);
            var isLoan = newrow.LoanAccountId > 0 || newrow.LoanCategoryId > 0;
            if (isLoan && !(newrow.LoanAccountId > 0 && newrow.LoanCategoryId > 0))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Akun dan Kategori Pinjaman"));
            }

            var id = SequenceUtility.GetNextId<EF.TDebt>(context);
            var row = new EF.TDebt
            {
                Id = id,
                PeriodId = periodId,
                DebtNumber = TransaksiHelper.GenerateNumber(context, NUMBER_DOCUMENT_TYPE, newrow.DebtDate.ToDateTime(TimeOnly.MinValue), NUMBER_FALLBACK_PREFIX, id, userId),
                CreditorName = creditorName,
                CreditorContact = TransaksiHelper.ToNullable(newrow.CreditorContact),
                Description = TransaksiHelper.Truncate(newrow.Description, DESCRIPTION_LENGTH),
                Amount = newrow.Amount,
                PaidAmount = 0,
                DebtDate = newrow.DebtDate,
                DueDate = newrow.DueDate,
                Status = (int)DebtStatus.BelumBayar,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            if (isLoan)
            {
                row.TransactionId = FinancialTransaction.CreateRow(context, new FinancialTransaction.NewRow
                {
                    AccountId = newrow.LoanAccountId!.Value,
                    CategoryId = newrow.LoanCategoryId!.Value,
                    PeriodId = periodId,
                    TransactionType = CashFlowType.Income,
                    Amount = newrow.Amount,
                    TransactionDate = newrow.DebtDate,
                    Description = $"Pinjaman {row.DebtNumber} dari {creditorName}"
                }, null, true, userId).Id;
            }
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TDebts.Add(row);
            return row;
        }
        // Periode dan nomor tetap; nominal tidak boleh di bawah yang sudah dibayar dan terkunci untuk pinjaman uang.
        private void SetRow(EF.TDebt row, int userId)
        {
            TransaksiHelper.ValidateStatus((DebtStatus)row.Status, "mengubah hutang", DebtStatus.BelumBayar, DebtStatus.Sebagian);
            var creditorName = ValidateData(CreditorName, Amount, DebtDate, DueDate);
            if (Amount != row.Amount)
            {
                if (row.TransactionId != null)
                {
                    throw new ArgumentException(ExceptionMessage.DEBT_LOAN_AMOUNT_LOCKED);
                }
                if (Amount < row.PaidAmount)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.DEBT_AMOUNT_BELOW_PAID, row.PaidAmount));
                }
            }

            row.CreditorName = creditorName;
            row.CreditorContact = TransaksiHelper.ToNullable(CreditorContact);
            row.Description = TransaksiHelper.Truncate(Description, DESCRIPTION_LENGTH);
            row.Amount = Amount;
            row.DebtDate = DebtDate;
            row.DueDate = DueDate;
            row.Status = (int)(row.PaidAmount >= row.Amount ? DebtStatus.Lunas : row.PaidAmount > 0 ? DebtStatus.Sebagian : DebtStatus.BelumBayar);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetWriteOff(EF.TDebt row, string? reason, int userId)
        {
            TransaksiHelper.ValidateStatus((DebtStatus)row.Status, "menghapuskan hutang", DebtStatus.BelumBayar, DebtStatus.Sebagian);
            row.Description = TransaksiHelper.AppendNote(row.Description, "Dihapuskan", reason, DESCRIPTION_LENGTH);
            row.Status = (int)DebtStatus.Dihapuskan;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetDelete(EF.ModelContext context, EF.TDebt row, int userId)
        {
            TransaksiHelper.ValidateStatus((DebtStatus)row.Status, "menghapus hutang", DebtStatus.BelumBayar);
            if (context.TDebtPayments.Any(x => x.DebtId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.DEBT_HAS_PAYMENTS);
            }
            if (row.TransactionId != null)
            {
                FinancialTransaction.VoidRow(context, row.TransactionId.Value, "Hutang " + row.DebtNumber + " dihapus", userId);
            }
            context.TDebts.Remove(row);
        }
        #endregion
    }
}
