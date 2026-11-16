using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Transaksi.Approval;
using GWALib.Lib.Transaksi.Keuangan;
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

namespace GWALib.Lib.Transaksi.Pengadaan
{
    public partial class ProcurementRequest
    {
        public const string DOCUMENT_TYPE = "PROCUREMENT";
        private const string NUMBER_FALLBACK_PREFIX = "PGD";
        private const int TITLE_LENGTH = 200;
        private const int REASON_LENGTH = 500;

        #region SYNC
        public List<ProcurementItem> GetItems(bool refresh)
        {
            if (refresh || ItemList == null)
            {
                ItemList = ProcurementItem.GetDataList(Id);
            }
            return ItemList;
        }
        public List<ProcurementQuotation> GetQuotations(bool refresh)
        {
            if (refresh || QuotationList == null)
            {
                QuotationList = ProcurementQuotation.GetDataList(Id);
            }
            return QuotationList;
        }
        public List<ProcurementApproval> GetApprovals(bool refresh)
        {
            if (refresh || ApprovalList == null)
            {
                ApprovalList = ProcurementApproval.GetDataList(Id);
            }
            return ApprovalList;
        }
        public List<ProcurementReceipt> GetReceipts(bool refresh)
        {
            if (refresh || ReceiptList == null)
            {
                ReceiptList = ProcurementReceipt.GetDataList(Id);
            }
            return ReceiptList;
        }
        public static List<ProcurementRequest> GetDataList(int? periodId = null, ProcurementStatus? status = null, int? requestedByMemberId = null,
            int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, status, requestedByMemberId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ProcurementRequest, EF.TProcurementRequest>();
        }
        public static Response<ProcurementRequest> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                transaction.Commit();
                return new ProcurementRequest(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ProcurementRequest>(msg));
        }
        public Response<ProcurementRequest> Update(string user)
        {
            return Run(SetRow, user);
        }
        public Response<ProcurementRequest> Submit(string user)
        {
            return Run((context, row, userId) => SetSubmit(context, row, userId), user);
        }
        // Menyetujui langkah yang sedang berjalan; tanda tangan opsional. Status Disetujui setelah langkah terakhir.
        public Response<ProcurementRequest> Approve(string? note, byte[]? signature, string? signatureFileName, string? signatureContentType, string user)
        {
            return Run((context, row, userId) => SetDecision(context, row, ProcurementDecision.Setuju, note, signature, signatureFileName, signatureContentType, userId), user);
        }
        public Response<ProcurementRequest> RequestRevision(string? note, string user)
        {
            return Run((context, row, userId) => SetDecision(context, row, ProcurementDecision.Revisi, note, null, null, null, userId), user);
        }
        public Response<ProcurementRequest> Reject(string? note, string user)
        {
            return Run((context, row, userId) => SetDecision(context, row, ProcurementDecision.Tolak, note, null, null, null, userId), user);
        }
        // fundingRequestId wajib untuk metode PengajuanDana (pengajuan Approved / Disbursed).
        public Response<ProcurementRequest> Purchase(int? fundingRequestId, string user)
        {
            return Run((context, row, userId) => SetPurchase(context, row, fundingRequestId, userId), user);
        }
        // Metode DibayarLangsung: transaksi Expense Posted dengan procurement_id; TotalActual dihitung ulang.
        public Response<ProcurementRequest> RecordPayment(int accountId, int categoryId, decimal amount, DateOnly paidDate, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = GetRow(context, Id);
                SetPayment(context, row, accountId, categoryId, amount, paidDate, userId);
                context.SaveChanges();
                transaction.Commit();
                return new ProcurementRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementRequest>(msg));
        }
        public Response<ProcurementRequest> CancelPayment(int transactionId, string? reason, string user)
        {
            return Run((context, row, userId) => SetCancelPayment(context, row, transactionId, reason, userId), user);
        }
        public Response<ProcurementRequest> Cancel(string? reason, string user)
        {
            return Run((context, row, userId) => SetCancel(context, row, reason, userId), user);
        }
        // Hanya Draft; item dan penawaran ikut dihapus.
        public Response<ProcurementRequest> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var data = new ProcurementRequest(Reload(row.Id));
                SetDelete(context, row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ProcurementRequest>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<ProcurementItem>> GetItemsAsync(bool refresh)
        {
            if (refresh || ItemList == null)
            {
                ItemList = await ProcurementItem.GetDataListAsync(Id);
            }
            return ItemList;
        }
        public async Task<List<ProcurementQuotation>> GetQuotationsAsync(bool refresh)
        {
            if (refresh || QuotationList == null)
            {
                QuotationList = await ProcurementQuotation.GetDataListAsync(Id);
            }
            return QuotationList;
        }
        public async Task<List<ProcurementApproval>> GetApprovalsAsync(bool refresh)
        {
            if (refresh || ApprovalList == null)
            {
                ApprovalList = await ProcurementApproval.GetDataListAsync(Id);
            }
            return ApprovalList;
        }
        public async Task<List<ProcurementReceipt>> GetReceiptsAsync(bool refresh)
        {
            if (refresh || ReceiptList == null)
            {
                ReceiptList = await ProcurementReceipt.GetDataListAsync(Id);
            }
            return ReceiptList;
        }
        public static async Task<List<ProcurementRequest>> GetDataListAsync(int? periodId = null, ProcurementStatus? status = null, int? requestedByMemberId = null,
            int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, status, requestedByMemberId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ProcurementRequest, EF.TProcurementRequest>();
        }
        public static async Task<Response<ProcurementRequest>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new ProcurementRequest(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ProcurementRequest>(msg));
        }
        public Task<Response<ProcurementRequest>> UpdateAsync(string user)
        {
            return RunAsync(SetRow, user);
        }
        public Task<Response<ProcurementRequest>> SubmitAsync(string user)
        {
            return RunAsync((context, row, userId) => SetSubmit(context, row, userId), user);
        }
        public Task<Response<ProcurementRequest>> ApproveAsync(string? note, byte[]? signature, string? signatureFileName, string? signatureContentType, string user)
        {
            return RunAsync((context, row, userId) => SetDecision(context, row, ProcurementDecision.Setuju, note, signature, signatureFileName, signatureContentType, userId), user);
        }
        public Task<Response<ProcurementRequest>> RequestRevisionAsync(string? note, string user)
        {
            return RunAsync((context, row, userId) => SetDecision(context, row, ProcurementDecision.Revisi, note, null, null, null, userId), user);
        }
        public Task<Response<ProcurementRequest>> RejectAsync(string? note, string user)
        {
            return RunAsync((context, row, userId) => SetDecision(context, row, ProcurementDecision.Tolak, note, null, null, null, userId), user);
        }
        public Task<Response<ProcurementRequest>> PurchaseAsync(int? fundingRequestId, string user)
        {
            return RunAsync((context, row, userId) => SetPurchase(context, row, fundingRequestId, userId), user);
        }
        public async Task<Response<ProcurementRequest>> RecordPaymentAsync(int accountId, int categoryId, decimal amount, DateOnly paidDate, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = await GetRowAsync(context, Id);
                SetPayment(context, row, accountId, categoryId, amount, paidDate, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new ProcurementRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementRequest>(msg));
        }
        public Task<Response<ProcurementRequest>> CancelPaymentAsync(int transactionId, string? reason, string user)
        {
            return RunAsync((context, row, userId) => SetCancelPayment(context, row, transactionId, reason, userId), user);
        }
        public Task<Response<ProcurementRequest>> CancelAsync(string? reason, string user)
        {
            return RunAsync((context, row, userId) => SetCancel(context, row, reason, userId), user);
        }
        public async Task<Response<ProcurementRequest>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var data = new ProcurementRequest(Reload(row.Id));
                SetDelete(context, row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ProcurementRequest>(msg));
        }
        #endregion

        #region INTERNAL
        // Usulan yang item-nya masih boleh diubah (Draft / Revisi); dipakai ProcurementItem.
        // Local: usulan yang baru di-Add dalam command yang sama (Save dengan Items) belum ada di DB.
        internal static EF.TProcurementRequest GetEditableRequest(EF.ModelContext context, int requestId)
        {
            var row = GetLocalOrDb(context, requestId);
            TransaksiHelper.ValidateStatus((ProcurementStatus)row.Status, "mengubah usulan pengadaan", ProcurementStatus.Draft, ProcurementStatus.Revisi);
            return row;
        }
        internal static EF.TProcurementRequest GetRequest(EF.ModelContext context, int requestId)
        {
            return GetLocalOrDb(context, requestId);
        }
        // TotalEstimate = jumlah quantity x harga estimasi item yang tersisa setelah perubahan di context.
        internal static void RecalculateEstimate(EF.ModelContext context, EF.TProcurementRequest request, int userId)
        {
            if (context.Entry(request).State != EntityState.Added)
            {
                context.TProcurementItems.Where(x => x.RequestId == request.Id).Load();
            }
            request.TotalEstimate = context.TProcurementItems.Local.Where(x => x.RequestId == request.Id).Sum(x => x.Quantity * x.EstimatedUnitPrice);
            request.UpdDate = DateTime.Now;
            request.UpdBy = userId;
        }
        // Status penerimaan dari jumlah diterima semua item: Dibeli (belum ada), DiterimaSebagian, Selesai (semua lengkap).
        internal static void RecalculateReceiveStatus(EF.ModelContext context, EF.TProcurementRequest request, int userId)
        {
            context.TProcurementItems.Where(x => x.RequestId == request.Id).Load();
            var items = context.TProcurementItems.Local.Where(x => x.RequestId == request.Id).ToList();
            request.Status = (int)(items.All(x => x.ReceivedQuantity >= x.Quantity) ? ProcurementStatus.Selesai
                : items.Any(x => x.ReceivedQuantity > 0) ? ProcurementStatus.DiterimaSebagian
                : ProcurementStatus.Dibeli);
            request.UpdDate = DateTime.Now;
            request.UpdBy = userId;
        }
        #endregion

        #region HELPER
        private delegate void RowAction(EF.ModelContext context, EF.TProcurementRequest row, int userId);

        private Response<ProcurementRequest> Run(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                return new ProcurementRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementRequest>(msg));
        }
        private async Task<Response<ProcurementRequest>> RunAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                return new ProcurementRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ProcurementRequest>(msg));
        }
        private static EF.TProcurementRequest GetLocalOrDb(EF.ModelContext context, int requestId)
        {
            return context.TProcurementRequests.Local.SingleOrDefault(x => x.Id == requestId)
                ?? context.TProcurementRequests.SingleOrDefault(x => x.Id == requestId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Usulan Pengadaan"));
        }
        private static EF.TProcurementRequest Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TProcurementRequest> FilterQuery(int? periodId, ProcurementStatus? status, int? requestedByMemberId)
        {
            return _Querys.Where(x => (periodId == null || x.PeriodId == periodId)
                    && (status == null || x.Status == (int)status)
                    && (requestedByMemberId == null || x.RequestedByMemberId == requestedByMemberId))
                .OrderByDescending(x => x.RequestDate).ThenByDescending(x => x.Id);
        }
        private static EF.TProcurementRequest GetRow(EF.ModelContext context, int id)
        {
            return context.TProcurementRequests.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Usulan Pengadaan"));
        }
        private static async Task<EF.TProcurementRequest> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TProcurementRequests.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Usulan Pengadaan"));
        }
        private record Refs(int? PeriodId, int RequestedByMemberId, int? ActivityId, int? ProgramId, int? BudgetItemId, string Title, string Reason);

        private static Refs ValidateData(EF.ModelContext context, int? periodId, int? requestedByMemberId, int userId, DateOnly requestDate, string? title,
            string? reason, int? activityId, int? programId, int? budgetItemId, ProcurementPaymentMethod paymentMethod, DateOnly? neededByDate)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Pengadaan"));
            }
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Alasan Kebutuhan"));
            }
            if (requestDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Usulan"));
            }
            if (neededByDate != null && neededByDate < requestDate)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATE_BEFORE_INVALID, "Tanggal Dibutuhkan", "Tanggal Usulan"));
            }
            if (!Enum.IsDefined(paymentMethod))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Metode Pembayaran"));
            }
            activityId = activityId > 0 ? activityId : null;
            programId = programId > 0 ? programId : null;
            budgetItemId = budgetItemId > 0 ? budgetItemId : null;
            periodId = periodId > 0 ? periodId : null;
            if (activityId != null && programId != null)
            {
                throw new ArgumentException(ExceptionMessage.PROCUREMENT_TARGET_ONE);
            }
            if (activityId != null)
            {
                Activity.GetActivity(context, activityId.Value);
            }
            if (programId != null && !context.TPrograms.Any(x => x.Id == programId))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Program Kerja"));
            }
            if (budgetItemId != null)
            {
                periodId ??= BudgetItem.GetApprovedItem(context, budgetItemId.Value).Budget.PeriodId;
            }
            periodId ??= FinancialTransaction.GetActivePeriodId(context);
            if (periodId != null)
            {
                ManagementPeriod.ValidateOpenPeriod(context, periodId.Value);
            }

            var memberId = requestedByMemberId > 0 ? requestedByMemberId!.Value
                : context.MMembers.Where(x => x.UserId == userId).Select(x => (int?)x.Id).FirstOrDefault()
                    ?? throw new ArgumentException(ExceptionMessage.USER_NOT_MEMBER);
            Member.GetActiveMember(context, memberId);
            return new Refs(periodId, memberId, activityId, programId, budgetItemId, TransaksiHelper.Truncate(title, TITLE_LENGTH)!, reason.Trim());
        }
        private static EF.TProcurementRequest CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var refs = ValidateData(context, newrow.PeriodId, newrow.RequestedByMemberId, userId, newrow.RequestDate, newrow.Title, newrow.Reason,
                newrow.ActivityId, newrow.ProgramId, newrow.BudgetItemId, newrow.PaymentMethod, newrow.NeededByDate);
            var id = SequenceUtility.GetNextId<EF.TProcurementRequest>(context);
            var row = new EF.TProcurementRequest
            {
                Id = id,
                RequestNumber = TransaksiHelper.GenerateNumber(context, DOCUMENT_TYPE, newrow.RequestDate.ToDateTime(TimeOnly.MinValue), NUMBER_FALLBACK_PREFIX, id, userId),
                PeriodId = refs.PeriodId,
                RequestedByMemberId = refs.RequestedByMemberId,
                RequestDate = newrow.RequestDate,
                Title = refs.Title,
                Reason = refs.Reason,
                ActivityId = refs.ActivityId,
                ProgramId = refs.ProgramId,
                BudgetItemId = refs.BudgetItemId,
                PaymentMethod = (int)newrow.PaymentMethod,
                NeededByDate = newrow.NeededByDate,
                TotalEstimate = 0,
                TotalActual = 0,
                IsOverBudget = (int)BudgetCheck.SesuaiAnggaran,
                Status = (int)ProcurementStatus.Draft,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TProcurementRequests.Add(row);

            foreach (var item in newrow.Items ?? new())
            {
                item.RequestId = row.Id;
                ProcurementItem.CreateRow(context, item, userId);
            }
            RecalculateEstimate(context, row, userId);
            row.UpdDate = null;
            row.UpdBy = null;
            return row;
        }
        // Hanya Draft / Revisi; nomor tetap.
        private void SetRow(EF.ModelContext context, EF.TProcurementRequest row, int userId)
        {
            TransaksiHelper.ValidateStatus((ProcurementStatus)row.Status, "mengubah usulan pengadaan", ProcurementStatus.Draft, ProcurementStatus.Revisi);
            var refs = ValidateData(context, PeriodId, RequestedByMemberId, userId, RequestDate, Title, Reason, ActivityId, ProgramId, BudgetItemId, PaymentMethod, NeededByDate);

            row.PeriodId = refs.PeriodId;
            row.RequestedByMemberId = refs.RequestedByMemberId;
            row.RequestDate = RequestDate;
            row.Title = refs.Title;
            row.Reason = refs.Reason;
            row.ActivityId = refs.ActivityId;
            row.ProgramId = refs.ProgramId;
            row.BudgetItemId = refs.BudgetItemId;
            row.PaymentMethod = (int)PaymentMethod;
            row.NeededByDate = NeededByDate;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        // Sisa item anggaran = nilai item - realisasi (pengeluaran - pemasukan Posted yang memakai item).
        private static void SetSubmit(EF.ModelContext context, EF.TProcurementRequest row, int userId)
        {
            TransaksiHelper.ValidateStatus((ProcurementStatus)row.Status, "mengajukan pengadaan", ProcurementStatus.Draft, ProcurementStatus.Revisi);
            if (!context.TProcurementItems.Any(x => x.RequestId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.PROCUREMENT_ITEM_REQUIRED);
            }
            ApprovalHelper.GetActiveSteps(context, DOCUMENT_TYPE);
            RecalculateEstimate(context, row, userId);
            if (row.BudgetItemId != null)
            {
                var item = BudgetItem.GetApprovedItem(context, row.BudgetItemId.Value);
                var realized = context.TFinancialTransactions
                    .Where(x => x.BudgetItemId == item.Id && x.Status == (int)TransactionStatus.Posted)
                    .Sum(x => x.TransactionType == (int)CashFlowType.Expense ? x.Amount : -x.Amount);
                row.BudgetRemaining = item.Amount - realized;
                row.IsOverBudget = (int)(row.TotalEstimate > row.BudgetRemaining ? BudgetCheck.MelebihiAnggaran : BudgetCheck.SesuaiAnggaran);
            }
            else
            {
                row.BudgetRemaining = null;
                row.IsOverBudget = (int)BudgetCheck.SesuaiAnggaran;
            }
            row.Status = (int)ProcurementStatus.Diajukan;
        }
        // Langkah berjalan = langkah pertama yang belum Setuju sejak permintaan revisi terakhir.
        private static void SetDecision(EF.ModelContext context, EF.TProcurementRequest row, ProcurementDecision decision, string? note,
            byte[]? signature, string? signatureFileName, string? signatureContentType, int userId)
        {
            var action = decision switch
            {
                ProcurementDecision.Setuju => "menyetujui pengadaan",
                ProcurementDecision.Tolak => "menolak pengadaan",
                _ => "meminta revisi pengadaan"
            };
            TransaksiHelper.ValidateStatus((ProcurementStatus)row.Status, action, ProcurementStatus.Diajukan);
            var steps = ApprovalHelper.GetActiveSteps(context, DOCUMENT_TYPE);
            var approvals = context.TProcurementApprovals.Where(x => x.RequestId == row.Id).Select(x => new { x.Id, x.ApprovalLevel, x.Decision }).ToList();
            var lastRevision = approvals.Where(x => x.Decision == (int)ProcurementDecision.Revisi).Select(x => x.Id).DefaultIfEmpty(0).Max();
            var step = steps.First(s => !approvals.Any(a => a.Id > lastRevision && a.ApprovalLevel == s.StepLevel && a.Decision == (int)ProcurementDecision.Setuju));
            ApprovalHelper.ValidateApprover(context, step, userId);
            if (decision == ProcurementDecision.Revisi && step.AllowRevision != (int)RevisionPermission.Boleh)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.APPROVAL_REVISION_NOT_ALLOWED, step.StepName));
            }

            ProcurementApproval.CreateRow(context, row.Id, step, decision, note, signature, signatureFileName, signatureContentType, userId);
            switch (decision)
            {
                case ProcurementDecision.Tolak:
                    row.Status = (int)ProcurementStatus.Ditolak;
                    row.RejectReason = TransaksiHelper.Truncate(note, REASON_LENGTH);
                    break;
                case ProcurementDecision.Revisi:
                    row.Status = (int)ProcurementStatus.Revisi;
                    break;
                default:
                    if (step == steps.Last())
                    {
                        row.Status = (int)ProcurementStatus.Disetujui;
                    }
                    break;
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetPurchase(EF.ModelContext context, EF.TProcurementRequest row, int? fundingRequestId, int userId)
        {
            TransaksiHelper.ValidateStatus((ProcurementStatus)row.Status, "mencatat pembelian", ProcurementStatus.Disetujui);
            if (row.PaymentMethod == (int)ProcurementPaymentMethod.PengajuanDana)
            {
                if (!(fundingRequestId > 0))
                {
                    throw new ArgumentException(ExceptionMessage.PROCUREMENT_FUNDING_REQUIRED);
                }
                var status = context.TFundingRequests.Where(x => x.Id == fundingRequestId).Select(x => (int?)x.Status).SingleOrDefault()
                    ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pengajuan Dana"));
                if (status != (int)FundingRequestStatus.Approved && status != (int)FundingRequestStatus.Disbursed)
                {
                    throw new ArgumentException(ExceptionMessage.PROCUREMENT_FUNDING_REQUIRED);
                }
                if (context.TProcurementRequests.Any(x => x.Id != row.Id && x.FundingRequestId == fundingRequestId && x.Status != (int)ProcurementStatus.Batal))
                {
                    throw new ArgumentException(ExceptionMessage.PROCUREMENT_FUNDING_USED);
                }
                row.FundingRequestId = fundingRequestId;
            }
            row.Status = (int)ProcurementStatus.Dibeli;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        // TotalActual = transaksi Expense Posted pengadaan ini; excludeTransactionId untuk transaksi yang sedang dibatalkan.
        private static decimal GetActualTotal(EF.ModelContext context, int requestId, int? excludeTransactionId)
        {
            return context.TFinancialTransactions
                .Where(x => x.ProcurementId == requestId && x.Status == (int)TransactionStatus.Posted
                    && x.TransactionType == (int)CashFlowType.Expense && x.Id != excludeTransactionId)
                .Sum(x => x.Amount);
        }
        private static void SetPayment(EF.ModelContext context, EF.TProcurementRequest row, int accountId, int categoryId, decimal amount, DateOnly paidDate, int userId)
        {
            TransaksiHelper.ValidateStatus((ProcurementStatus)row.Status, "mencatat pembayaran pengadaan",
                ProcurementStatus.Dibeli, ProcurementStatus.DiterimaSebagian, ProcurementStatus.Selesai);
            if (row.PaymentMethod != (int)ProcurementPaymentMethod.DibayarLangsung)
            {
                throw new ArgumentException(ExceptionMessage.PROCUREMENT_PAID_BY_FUNDING);
            }
            FinancialTransaction.CreateRow(context, new FinancialTransaction.NewRow
            {
                AccountId = accountId,
                CategoryId = categoryId,
                PeriodId = FinancialTransaction.OpenPeriodOrDefault(context, row.PeriodId),
                TransactionType = CashFlowType.Expense,
                Amount = amount,
                TransactionDate = paidDate,
                Description = $"Pengadaan {row.RequestNumber}: {row.Title}",
                BudgetItemId = row.BudgetItemId,
                ActivityId = row.ActivityId,
                ProgramId = row.ProgramId,
                ProcurementId = row.Id
            }, null, true, userId);
            row.TotalActual = GetActualTotal(context, row.Id, null) + amount;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetCancelPayment(EF.ModelContext context, EF.TProcurementRequest row, int transactionId, string? reason, int userId)
        {
            var trx = context.TFinancialTransactions.SingleOrDefault(x => x.Id == transactionId && x.ProcurementId == row.Id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pembayaran Pengadaan"));
            FinancialTransaction.VoidRow(context, trx, reason ?? "Pembayaran pengadaan dibatalkan", userId);
            row.TotalActual = GetActualTotal(context, row.Id, trx.Id);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetCancel(EF.ModelContext context, EF.TProcurementRequest row, string? reason, int userId)
        {
            TransaksiHelper.ValidateStatus((ProcurementStatus)row.Status, "membatalkan pengadaan", ProcurementStatus.Draft, ProcurementStatus.Diajukan,
                ProcurementStatus.Revisi, ProcurementStatus.Disetujui, ProcurementStatus.Dibeli);
            if (row.Status == (int)ProcurementStatus.Dibeli
                && (GetActualTotal(context, row.Id, null) > 0 || context.TProcurementReceipts.Any(x => x.RequestId == row.Id)))
            {
                throw new ArgumentException(ExceptionMessage.PROCUREMENT_HAS_PROGRESS);
            }
            row.RejectReason = TransaksiHelper.AppendNote(row.RejectReason, "Batal", reason, REASON_LENGTH);
            row.Status = (int)ProcurementStatus.Batal;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetDelete(EF.ModelContext context, EF.TProcurementRequest row)
        {
            TransaksiHelper.ValidateStatus((ProcurementStatus)row.Status, "menghapus usulan pengadaan", ProcurementStatus.Draft);
            context.TProcurementQuotations.RemoveRange(context.TProcurementQuotations.Where(x => x.RequestId == row.Id));
            context.TProcurementItems.RemoveRange(context.TProcurementItems.Where(x => x.RequestId == row.Id));
            context.TProcurementRequests.Remove(row);
        }
        #endregion
    }
}
