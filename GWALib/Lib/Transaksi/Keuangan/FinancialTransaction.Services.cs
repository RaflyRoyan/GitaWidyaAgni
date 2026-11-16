using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Master.Keuangan;
using GWALib.Lib.Transaksi.Program;
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
    public partial class FinancialTransaction
    {
        public const string NUMBER_DOCUMENT_TYPE = "FIN_TRX";
        private const string NUMBER_FALLBACK_PREFIX = "TRX";
        private const int DESCRIPTION_LENGTH = 255;

        #region SYNC
        public List<FinancialAttachment> GetAttachments(bool refresh)
        {
            if (refresh || AttachmentList == null)
            {
                AttachmentList = FinancialAttachment.GetDataList(Id);
            }
            return AttachmentList;
        }
        public static List<FinancialTransaction> GetDataList(int? periodId = null, int? accountId = null, CashFlowType? transactionType = null,
            TransactionStatus? status = null, DateOnly? from = null, DateOnly? to = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, accountId, transactionType, status, from, to);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<FinancialTransaction, EF.TFinancialTransaction>();
        }
        // Nomor transaksi ikut di-rollback bila simpan gagal.
        public static Response<FinancialTransaction> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                ValidateManualRefs(newrow);
                var row = CreateRow(context, newrow, null, false, userId);
                context.SaveChanges();
                transaction.Commit();
                return new FinancialTransaction(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FinancialTransaction>(msg));
        }
        public Response<FinancialTransaction> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new FinancialTransaction(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FinancialTransaction>(msg));
        }
        public Response<FinancialTransaction> Post(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetPost(context, row, userId);
                context.SaveChanges();
                return new FinancialTransaction(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FinancialTransaction>(msg));
        }
        public Response<FinancialTransaction> Void(string? reason, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateNotLinked(context, row);
                VoidRow(context, row, reason, userId);
                context.SaveChanges();
                return new FinancialTransaction(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FinancialTransaction>(msg));
        }
        // Hanya Draft yang bisa dihapus (lampiran ikut terhapus lewat ON DELETE CASCADE).
        public Response<FinancialTransaction> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((TransactionStatus)row.Status, "menghapus transaksi", TransactionStatus.Draft);
                var data = new FinancialTransaction(Reload(row.Id));
                context.TFinancialTransactions.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FinancialTransaction>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<FinancialAttachment>> GetAttachmentsAsync(bool refresh)
        {
            if (refresh || AttachmentList == null)
            {
                AttachmentList = await FinancialAttachment.GetDataListAsync(Id);
            }
            return AttachmentList;
        }
        public static async Task<List<FinancialTransaction>> GetDataListAsync(int? periodId = null, int? accountId = null, CashFlowType? transactionType = null,
            TransactionStatus? status = null, DateOnly? from = null, DateOnly? to = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, accountId, transactionType, status, from, to);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<FinancialTransaction, EF.TFinancialTransaction>();
        }
        public static async Task<Response<FinancialTransaction>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                ValidateManualRefs(newrow);
                var row = CreateRow(context, newrow, null, false, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new FinancialTransaction(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FinancialTransaction>(msg));
        }
        public async Task<Response<FinancialTransaction>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new FinancialTransaction(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FinancialTransaction>(msg));
        }
        public async Task<Response<FinancialTransaction>> PostAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetPost(context, row, userId);
                await context.SaveChangesAsync();
                return new FinancialTransaction(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FinancialTransaction>(msg));
        }
        public async Task<Response<FinancialTransaction>> VoidAsync(string? reason, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateNotLinked(context, row);
                VoidRow(context, row, reason, userId);
                await context.SaveChangesAsync();
                return new FinancialTransaction(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FinancialTransaction>(msg));
        }
        public async Task<Response<FinancialTransaction>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                TransaksiHelper.ValidateStatus((TransactionStatus)row.Status, "menghapus transaksi", TransactionStatus.Draft);
                var data = new FinancialTransaction(Reload(row.Id));
                context.TFinancialTransactions.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FinancialTransaction>(msg));
        }
        #endregion

        #region INTERNAL
        // Dipakai juga oleh tagihan iuran, pengajuan dana dan hutang (posted = true, langsung Posted).
        // Panggil di dalam BeginTransaction supaya nomor transaksi ikut di-rollback.
        internal static EF.TFinancialTransaction CreateRow(EF.ModelContext context, NewRow newrow, int? fundingRequestId, bool posted, int userId)
        {
            var periodId = newrow.PeriodId > 0 ? newrow.PeriodId : GetActivePeriodId(context);
            var refs = NormalizeRefs(newrow.BudgetItemId, newrow.ActivityId, newrow.ProgramId, newrow.CompetitionId,
                newrow.SponsorshipId, newrow.ProcurementId, newrow.SalesOrderId, newrow.BorrowRequestId);
            ValidateData(context, newrow.AccountId, newrow.CategoryId, periodId, newrow.TransactionType, newrow.Amount, newrow.TransactionDate, refs);

            var id = SequenceUtility.GetNextId<EF.TFinancialTransaction>(context);
            var now = DateTime.Now;
            var row = new EF.TFinancialTransaction
            {
                Id = id,
                AccountId = newrow.AccountId,
                CategoryId = newrow.CategoryId,
                PeriodId = periodId,
                BudgetItemId = refs.BudgetItemId,
                FundingRequestId = fundingRequestId,
                ActivityId = refs.ActivityId,
                ProgramId = refs.ProgramId,
                CompetitionId = refs.CompetitionId,
                SponsorshipId = refs.SponsorshipId,
                ProcurementId = refs.ProcurementId,
                SalesOrderId = refs.SalesOrderId,
                BorrowRequestId = refs.BorrowRequestId,
                TransactionNumber = TransaksiHelper.GenerateNumber(context, NUMBER_DOCUMENT_TYPE, newrow.TransactionDate.ToDateTime(TimeOnly.MinValue), NUMBER_FALLBACK_PREFIX, id, userId),
                TransactionType = (int)newrow.TransactionType,
                Amount = newrow.Amount,
                TransactionDate = newrow.TransactionDate,
                Description = TransaksiHelper.Truncate(newrow.Description, DESCRIPTION_LENGTH),
                Status = (int)(posted ? TransactionStatus.Posted : TransactionStatus.Draft),
                CreatedBy = userId,
                ApprovedBy = posted ? userId : null,
                ApprovedAt = posted ? now : null,
                InsDate = now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TFinancialTransactions.Add(row);
            return row;
        }
        // Pembatalan tanpa cek dokumen asal; dipakai dokumen asal (iuran, pengajuan dana, hutang) saat membatalkan.
        internal static void VoidRow(EF.ModelContext context, EF.TFinancialTransaction row, string? reason, int userId)
        {
            TransaksiHelper.ValidateStatus((TransactionStatus)row.Status, "membatalkan transaksi", TransactionStatus.Draft, TransactionStatus.Posted);
            ValidateBalanceOpen(context, row.PeriodId, row.AccountId);
            row.Description = TransaksiHelper.AppendNote(row.Description, "Dibatalkan", reason, DESCRIPTION_LENGTH);
            row.Status = (int)TransactionStatus.Void;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        internal static void VoidRow(EF.ModelContext context, int transactionId, string? reason, int userId)
        {
            var row = context.TFinancialTransactions.Single(x => x.Id == transactionId);
            if (row.Status != (int)TransactionStatus.Void)
            {
                VoidRow(context, row, reason, userId);
            }
        }
        // Periode dokumen asal bila belum ditutup; null (= periode aktif saat CreateRow) bila sudah ditutup, mis. tunggakan periode lalu.
        internal static int? OpenPeriodOrDefault(EF.ModelContext context, int? periodId)
        {
            return periodId != null && context.MManagementPeriods.Any(x => x.Id == periodId && x.Status != (int)ManagementPeriodStatus.Closed)
                ? periodId : null;
        }
        internal static int? GetActivePeriodId(EF.ModelContext context)
        {
            return context.MManagementPeriods.Where(x => x.Status == (int)ManagementPeriodStatus.Active).Select(x => (int?)x.Id).FirstOrDefault();
        }
        // Saldo periode (periode, akun) yang sudah ditutup mengunci transaksinya.
        internal static void ValidateBalanceOpen(EF.ModelContext context, int? periodId, int accountId)
        {
            if (periodId != null && context.TPeriodBalances.Any(x => x.PeriodId == periodId && x.AccountId == accountId && x.Status == (int)PeriodBalanceStatus.Closed))
            {
                var name = context.MFinancialAccounts.Where(x => x.Id == accountId).Select(x => x.Name).Single();
                throw new ArgumentException(string.Format(ExceptionMessage.FINANCIAL_BALANCE_CLOSED, name));
            }
        }
        #endregion

        #region HELPER
        private record RefIds(int? BudgetItemId, int? ActivityId, int? ProgramId, int? CompetitionId,
            int? SponsorshipId, int? ProcurementId, int? SalesOrderId, int? BorrowRequestId);

        private static EF.TFinancialTransaction Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TFinancialTransaction> FilterQuery(int? periodId, int? accountId, CashFlowType? transactionType,
            TransactionStatus? status, DateOnly? from, DateOnly? to)
        {
            return _Querys.Where(x => (periodId == null || x.PeriodId == periodId)
                    && (accountId == null || x.AccountId == accountId)
                    && (transactionType == null || x.TransactionType == (int)transactionType)
                    && (status == null || x.Status == (int)status)
                    && (from == null || x.TransactionDate >= from)
                    && (to == null || x.TransactionDate <= to))
                .OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.Id);
        }
        private static EF.TFinancialTransaction GetRow(EF.ModelContext context, int id)
        {
            return context.TFinancialTransactions.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Transaksi Keuangan"));
        }
        private static async Task<EF.TFinancialTransaction> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TFinancialTransactions.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Transaksi Keuangan"));
        }
        private static int? ToNullableId(int? id)
        {
            return id > 0 ? id : null;
        }
        private static RefIds NormalizeRefs(int? budgetItemId, int? activityId, int? programId, int? competitionId,
            int? sponsorshipId, int? procurementId, int? salesOrderId, int? borrowRequestId)
        {
            return new RefIds(ToNullableId(budgetItemId), ToNullableId(activityId), ToNullableId(programId), ToNullableId(competitionId),
                ToNullableId(sponsorshipId), ToNullableId(procurementId), ToNullableId(salesOrderId), ToNullableId(borrowRequestId));
        }
        private static void ValidateData(EF.ModelContext context, int accountId, int categoryId, int? periodId, CashFlowType transactionType,
            decimal amount, DateOnly transactionDate, RefIds refs)
        {
            if (!Enum.IsDefined(transactionType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Transaksi"));
            }
            TransaksiHelper.ValidatePositive(amount, "Nominal Transaksi");
            if (transactionDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Transaksi"));
            }
            FinancialAccount.ValidateActiveAccount(context, accountId);
            FinancialCategory.ValidateActiveCategory(context, categoryId, transactionType);
            if (periodId != null)
            {
                ManagementPeriod.ValidateOpenPeriod(context, periodId.Value);
            }
            ValidateBalanceOpen(context, periodId, accountId);

            if (refs.BudgetItemId != null)
            {
                BudgetItem.GetApprovedItem(context, refs.BudgetItemId.Value);
            }
            if (refs.ActivityId != null)
            {
                Activity.GetActivity(context, refs.ActivityId.Value);
            }
            RequireExists(refs.ProgramId, id => context.TPrograms.Any(x => x.Id == id), "Program Kerja");
            RequireExists(refs.CompetitionId, id => context.TCompetitions.Any(x => x.Id == id), "Lomba");
            RequireExists(refs.SponsorshipId, id => context.TSponsorships.Any(x => x.Id == id), "Sponsorship");
            RequireExists(refs.ProcurementId, id => context.TProcurementRequests.Any(x => x.Id == id), "Pengadaan");
            RequireExists(refs.SalesOrderId, id => context.TSalesOrders.Any(x => x.Id == id), "Pesanan Penjualan");
            RequireExists(refs.BorrowRequestId, id => context.TBorrowRequests.Any(x => x.Id == id), "Peminjaman");
        }
        private static void RequireExists(int? id, Func<int, bool> exists, string label)
        {
            if (id != null && !exists(id.Value))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, label));
            }
        }
        // Pembayaran pengadaan hanya lewat ProcurementRequest supaya total_actual tetap sinkron.
        private static void ValidateManualRefs(NewRow newrow)
        {
            if (newrow.ProcurementId > 0)
            {
                throw new ArgumentException(ExceptionMessage.PROCUREMENT_PAYMENT_VIA_REQUEST);
            }
            if (newrow.SalesOrderId > 0)
            {
                throw new ArgumentException(ExceptionMessage.SALES_PAYMENT_VIA_ORDER);
            }
            if (newrow.BorrowRequestId > 0)
            {
                throw new ArgumentException(ExceptionMessage.BORROW_PAYMENT_VIA_REQUEST);
            }
        }
        // Transaksi buatan dokumen lain hanya dibatalkan lewat dokumen asalnya supaya status dokumen tetap sinkron.
        private static void ValidateNotLinked(EF.ModelContext context, EF.TFinancialTransaction row)
        {
            string? source = row.FundingRequestId != null ? "pengajuan dana"
                : row.ProcurementId != null ? "pengadaan"
                : row.SalesOrderId != null ? "pesanan penjualan"
                : row.BorrowRequestId != null ? "peminjaman aset"
                : context.TMaintenanceLogs.Any(x => x.TransactionId == row.Id) ? "perawatan aset"
                : context.TDuesBills.Any(x => x.TransactionId == row.Id) ? "tagihan iuran"
                : context.TDebtPayments.Any(x => x.TransactionId == row.Id) ? "pembayaran hutang"
                : context.TDebts.Any(x => x.TransactionId == row.Id) ? "hutang"
                : null;
            if (source != null)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FINANCIAL_TRANSACTION_LINKED, source));
            }
        }
        // Hanya Draft; nomor dan pengajuan dana tetap.
        private void SetRow(EF.ModelContext context, EF.TFinancialTransaction row, int userId)
        {
            TransaksiHelper.ValidateStatus((TransactionStatus)row.Status, "mengubah transaksi", TransactionStatus.Draft);
            var periodId = PeriodId > 0 ? PeriodId : GetActivePeriodId(context);
            var refs = NormalizeRefs(BudgetItemId, ActivityId, ProgramId, CompetitionId, SponsorshipId, row.ProcurementId, row.SalesOrderId, row.BorrowRequestId);
            ValidateData(context, AccountId, CategoryId, periodId, TransactionType, Amount, TransactionDate, refs);

            row.AccountId = AccountId;
            row.CategoryId = CategoryId;
            row.PeriodId = periodId;
            row.BudgetItemId = refs.BudgetItemId;
            row.ActivityId = refs.ActivityId;
            row.ProgramId = refs.ProgramId;
            row.CompetitionId = refs.CompetitionId;
            row.SponsorshipId = refs.SponsorshipId;
            row.ProcurementId = refs.ProcurementId;
            row.SalesOrderId = refs.SalesOrderId;
            row.BorrowRequestId = refs.BorrowRequestId;
            row.TransactionType = (int)TransactionType;
            row.Amount = Amount;
            row.TransactionDate = TransactionDate;
            row.Description = TransaksiHelper.Truncate(Description, DESCRIPTION_LENGTH);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetPost(EF.ModelContext context, EF.TFinancialTransaction row, int userId)
        {
            TransaksiHelper.ValidateStatus((TransactionStatus)row.Status, "memposting transaksi", TransactionStatus.Draft);
            FinancialAccount.ValidateActiveAccount(context, row.AccountId);
            if (row.PeriodId != null)
            {
                ManagementPeriod.ValidateOpenPeriod(context, row.PeriodId.Value);
            }
            ValidateBalanceOpen(context, row.PeriodId, row.AccountId);
            var now = DateTime.Now;
            row.Status = (int)TransactionStatus.Posted;
            row.ApprovedBy = userId;
            row.ApprovedAt = now;
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
