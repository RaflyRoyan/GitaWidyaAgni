using GWALib.General;
using GWALib.Lib.Transaksi.Information;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Transaksi.Approval;
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
    public partial class FundingRequest
    {
        public const string DOCUMENT_TYPE = "FUNDING_REQUEST";
        private const string NUMBER_FALLBACK_PREFIX = "PD";
        private const int PURPOSE_LENGTH = 255;

        #region SYNC
        public List<FundingApproval> GetApprovals(bool refresh)
        {
            if (refresh || ApprovalList == null)
            {
                ApprovalList = FundingApproval.GetDataList(Id);
            }
            return ApprovalList;
        }
        public List<FundingRealization> GetRealizations(bool refresh)
        {
            if (refresh || RealizationList == null)
            {
                RealizationList = FundingRealization.GetDataList(Id);
            }
            return RealizationList;
        }
        // requestedBy diisi untuk daftar "pengajuan saya".
        public static List<FundingRequest> GetDataList(int? periodId = null, FundingRequestStatus? status = null, SettlementStatus? settlementStatus = null,
            int? requestedBy = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, status, settlementStatus, requestedBy);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<FundingRequest, EF.TFundingRequest>();
        }
        public static Response<FundingRequest> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                transaction.Commit();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FundingRequest>(msg));
        }
        public Response<FundingRequest> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRequest>(msg));
        }
        public Response<FundingRequest> Submit(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetSubmit(context, row, userId);
                context.SaveChanges();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRequest>(msg));
        }
        // Menyetujui langkah yang sedang berjalan; status Approved setelah langkah terakhir.
        public Response<FundingRequest> Approve(string? note, string user)
        {
            return Decide(FundingDecision.Approved, note, user);
        }
        public Response<FundingRequest> Reject(string? note, string user)
        {
            return Decide(FundingDecision.Rejected, note, user);
        }
        // Mengembalikan ke Draft untuk diperbaiki; hanya bila langkahnya mengizinkan revisi.
        public Response<FundingRequest> RequestRevision(string? note, string user)
        {
            return Decide(FundingDecision.Revision, note, user);
        }
        public Response<FundingRequest> Disburse(int accountId, int categoryId, DateOnly disburseDate, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = GetRow(context, Id);
                SetDisburse(context, row, accountId, categoryId, disburseDate, userId);
                context.SaveChanges();
                transaction.Commit();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRequest>(msg));
        }
        public Response<FundingRequest> SubmitSettlement(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetSettlementStatus(row, SettlementStatus.Diajukan, userId);
                context.SaveChanges();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRequest>(msg));
        }
        // Bendahara mengembalikan pertanggungjawaban untuk diperbaiki.
        public Response<FundingRequest> ReturnSettlement(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetSettlementStatus(row, SettlementStatus.Belum, userId);
                context.SaveChanges();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRequest>(msg));
        }
        // Akun dan kategori wajib bila realisasi berbeda dengan dana: sisa → Income (dikembalikan), kurang → Expense (diganti).
        public Response<FundingRequest> Settle(int? accountId, int? categoryId, DateOnly settleDate, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = GetRow(context, Id);
                SetSettle(context, row, accountId, categoryId, settleDate, userId);
                context.SaveChanges();
                transaction.Commit();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRequest>(msg));
        }
        // Hanya Draft yang bisa dihapus (riwayat persetujuan ikut terhapus lewat ON DELETE CASCADE).
        public Response<FundingRequest> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((FundingRequestStatus)row.Status, "menghapus pengajuan dana", FundingRequestStatus.Draft);
                var data = new FundingRequest(Reload(row.Id));
                context.TFundingRequests.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FundingRequest>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<FundingApproval>> GetApprovalsAsync(bool refresh)
        {
            if (refresh || ApprovalList == null)
            {
                ApprovalList = await FundingApproval.GetDataListAsync(Id);
            }
            return ApprovalList;
        }
        public async Task<List<FundingRealization>> GetRealizationsAsync(bool refresh)
        {
            if (refresh || RealizationList == null)
            {
                RealizationList = await FundingRealization.GetDataListAsync(Id);
            }
            return RealizationList;
        }
        public static async Task<List<FundingRequest>> GetDataListAsync(int? periodId = null, FundingRequestStatus? status = null, SettlementStatus? settlementStatus = null,
            int? requestedBy = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, status, settlementStatus, requestedBy);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<FundingRequest, EF.TFundingRequest>();
        }
        public static async Task<Response<FundingRequest>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<FundingRequest>(msg));
        }
        public async Task<Response<FundingRequest>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRequest>(msg));
        }
        public async Task<Response<FundingRequest>> SubmitAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetSubmit(context, row, userId);
                await context.SaveChangesAsync();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRequest>(msg));
        }
        public Task<Response<FundingRequest>> ApproveAsync(string? note, string user)
        {
            return DecideAsync(FundingDecision.Approved, note, user);
        }
        public Task<Response<FundingRequest>> RejectAsync(string? note, string user)
        {
            return DecideAsync(FundingDecision.Rejected, note, user);
        }
        public Task<Response<FundingRequest>> RequestRevisionAsync(string? note, string user)
        {
            return DecideAsync(FundingDecision.Revision, note, user);
        }
        public async Task<Response<FundingRequest>> DisburseAsync(int accountId, int categoryId, DateOnly disburseDate, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = await GetRowAsync(context, Id);
                SetDisburse(context, row, accountId, categoryId, disburseDate, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRequest>(msg));
        }
        public async Task<Response<FundingRequest>> SubmitSettlementAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetSettlementStatus(row, SettlementStatus.Diajukan, userId);
                await context.SaveChangesAsync();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRequest>(msg));
        }
        public async Task<Response<FundingRequest>> ReturnSettlementAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetSettlementStatus(row, SettlementStatus.Belum, userId);
                await context.SaveChangesAsync();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRequest>(msg));
        }
        public async Task<Response<FundingRequest>> SettleAsync(int? accountId, int? categoryId, DateOnly settleDate, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = await GetRowAsync(context, Id);
                SetSettle(context, row, accountId, categoryId, settleDate, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRequest>(msg));
        }
        public async Task<Response<FundingRequest>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                TransaksiHelper.ValidateStatus((FundingRequestStatus)row.Status, "menghapus pengajuan dana", FundingRequestStatus.Draft);
                var data = new FundingRequest(Reload(row.Id));
                context.TFundingRequests.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<FundingRequest>(msg));
        }
        #endregion

        #region INTERNAL
        // Pengajuan yang realisasinya masih boleh diubah (Disbursed, pertanggungjawaban Belum); dipakai FundingRealization.
        internal static EF.TFundingRequest GetSettlingRequest(EF.ModelContext context, int requestId)
        {
            var row = context.TFundingRequests.SingleOrDefault(x => x.Id == requestId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pengajuan Dana"));
            TransaksiHelper.ValidateStatus((FundingRequestStatus)row.Status, "mengubah realisasi dana", FundingRequestStatus.Disbursed);
            TransaksiHelper.ValidateStatus((SettlementStatus)row.SettlementStatus, "mengubah realisasi dana", SettlementStatus.Belum);
            return row;
        }
        // RealizedAmount = jumlah realisasi yang tersisa setelah perubahan di context.
        internal static void RecalculateRealized(EF.ModelContext context, EF.TFundingRequest request, int userId)
        {
            context.TFundingRealizations.Where(x => x.FundingRequestId == request.Id).Load();
            request.RealizedAmount = context.TFundingRealizations.Local.Where(x => x.FundingRequestId == request.Id).Sum(x => x.Amount);
            request.UpdDate = DateTime.Now;
            request.UpdBy = userId;
        }
        #endregion

        #region HELPER
        private record Refs(int? PeriodId, int? BudgetItemId, int? ProgramId, int? ActivityId, int? CompetitionId);

        private static EF.TFundingRequest Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TFundingRequest> FilterQuery(int? periodId, FundingRequestStatus? status, SettlementStatus? settlementStatus, int? requestedBy)
        {
            return _Querys.Where(x => (periodId == null || x.PeriodId == periodId)
                    && (status == null || x.Status == (int)status)
                    && (settlementStatus == null || x.SettlementStatus == (int)settlementStatus)
                    && (requestedBy == null || x.RequestedBy == requestedBy))
                .OrderByDescending(x => x.Id);
        }
        private static EF.TFundingRequest GetRow(EF.ModelContext context, int id)
        {
            return context.TFundingRequests.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pengajuan Dana"));
        }
        private static async Task<EF.TFundingRequest> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TFundingRequests.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pengajuan Dana"));
        }
        private Response<FundingRequest> Decide(FundingDecision decision, string? note, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetDecision(context, row, decision, note, userId);
                context.SaveChanges();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRequest>(msg));
        }
        private async Task<Response<FundingRequest>> DecideAsync(FundingDecision decision, string? note, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetDecision(context, row, decision, note, userId);
                await context.SaveChangesAsync();
                return new FundingRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<FundingRequest>(msg));
        }
        // Periode kosong diambil dari anggaran item (bila ada) lalu periode aktif; program diambil dari kegiatan bila kosong.
        private static Refs ValidateData(EF.ModelContext context, int? periodId, int? budgetItemId, int? programId, int? activityId, int? competitionId,
            string? purpose, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(purpose))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Keperluan"));
            }
            TransaksiHelper.ValidatePositive(amount, "Nominal Pengajuan");
            budgetItemId = budgetItemId > 0 ? budgetItemId : null;
            programId = programId > 0 ? programId : null;
            activityId = activityId > 0 ? activityId : null;
            competitionId = competitionId > 0 ? competitionId : null;
            periodId = periodId > 0 ? periodId : null;

            if (budgetItemId != null)
            {
                var item = BudgetItem.GetApprovedItem(context, budgetItemId.Value);
                periodId ??= item.Budget.PeriodId;
                programId ??= item.ProgramId;
            }
            if (activityId != null)
            {
                programId ??= Activity.GetActivity(context, activityId.Value).ProgramId;
            }
            if (programId != null && !context.TPrograms.Any(x => x.Id == programId))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Program Kerja"));
            }
            if (competitionId != null && !context.TCompetitions.Any(x => x.Id == competitionId))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Lomba"));
            }
            periodId ??= FinancialTransaction.GetActivePeriodId(context);
            if (periodId != null)
            {
                ManagementPeriod.ValidateOpenPeriod(context, periodId.Value);
            }
            return new Refs(periodId, budgetItemId, programId, activityId, competitionId);
        }
        private static EF.TFundingRequest CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var refs = ValidateData(context, newrow.PeriodId, newrow.BudgetItemId, newrow.ProgramId, newrow.ActivityId, newrow.CompetitionId,
                newrow.Purpose, newrow.Amount);
            var id = SequenceUtility.GetNextId<EF.TFundingRequest>(context);
            var now = DateTime.Now;
            var row = new EF.TFundingRequest
            {
                Id = id,
                PeriodId = refs.PeriodId,
                BudgetItemId = refs.BudgetItemId,
                ProgramId = refs.ProgramId,
                ActivityId = refs.ActivityId,
                CompetitionId = refs.CompetitionId,
                RequestedBy = userId,
                RequestNumber = TransaksiHelper.GenerateNumber(context, DOCUMENT_TYPE, now, NUMBER_FALLBACK_PREFIX, id, userId),
                Purpose = TransaksiHelper.Truncate(newrow.Purpose, PURPOSE_LENGTH)!,
                Amount = newrow.Amount,
                RealizedAmount = 0,
                ReturnedAmount = 0,
                Status = (int)FundingRequestStatus.Draft,
                SettlementStatus = (int)SettlementStatus.Belum,
                InsDate = now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TFundingRequests.Add(row);
            return row;
        }
        // Hanya Draft (termasuk setelah diminta revisi); nomor dan pemohon tetap.
        private void SetRow(EF.ModelContext context, EF.TFundingRequest row, int userId)
        {
            TransaksiHelper.ValidateStatus((FundingRequestStatus)row.Status, "mengubah pengajuan dana", FundingRequestStatus.Draft);
            var refs = ValidateData(context, PeriodId, BudgetItemId, ProgramId, ActivityId, CompetitionId, Purpose, Amount);

            row.PeriodId = refs.PeriodId;
            row.BudgetItemId = refs.BudgetItemId;
            row.ProgramId = refs.ProgramId;
            row.ActivityId = refs.ActivityId;
            row.CompetitionId = refs.CompetitionId;
            row.Purpose = TransaksiHelper.Truncate(Purpose, PURPOSE_LENGTH)!;
            row.Amount = Amount;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetSubmit(EF.ModelContext context, EF.TFundingRequest row, int userId)
        {
            TransaksiHelper.ValidateStatus((FundingRequestStatus)row.Status, "mengajukan dana", FundingRequestStatus.Draft);
            var steps = ApprovalHelper.GetActiveSteps(context, DOCUMENT_TYPE);
            if (row.PeriodId != null)
            {
                ManagementPeriod.ValidateOpenPeriod(context, row.PeriodId.Value);
            }
            FundingApproval.ResetDecisions(context, row.Id, userId);
            row.Status = (int)FundingRequestStatus.Submitted;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            NotifyHelper.ToApprovers(context, steps[0], $"Pengajuan dana {row.RequestNumber} menunggu persetujuan", row.Purpose,
                NotifyHelper.LINK_FUNDING, "t_funding_request", row.Id, userId);
        }
        // Langkah berjalan = langkah pertama (urut step_level) yang belum Approved pada putaran pengajuan ini.
        private static void SetDecision(EF.ModelContext context, EF.TFundingRequest row, FundingDecision decision, string? note, int userId)
        {
            var action = decision switch
            {
                FundingDecision.Approved => "menyetujui pengajuan dana",
                FundingDecision.Rejected => "menolak pengajuan dana",
                _ => "meminta revisi pengajuan dana"
            };
            TransaksiHelper.ValidateStatus((FundingRequestStatus)row.Status, action, FundingRequestStatus.Submitted);
            var steps = ApprovalHelper.GetActiveSteps(context, DOCUMENT_TYPE);
            var approvals = context.TFundingApprovals.Where(x => x.RequestId == row.Id).ToList();
            var step = steps.First(s => !approvals.Any(a => a.ApprovalLevel == s.StepLevel && a.Decision == (int)FundingDecision.Approved));
            ApprovalHelper.ValidateApprover(context, step, userId);
            if (decision == FundingDecision.Revision && step.AllowRevision != (int)RevisionPermission.Boleh)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.APPROVAL_REVISION_NOT_ALLOWED, step.StepName));
            }
            if (decision == FundingDecision.Approved && row.PeriodId != null)
            {
                ManagementPeriod.ValidateOpenPeriod(context, row.PeriodId.Value);
            }

            FundingApproval.SetDecision(context, approvals, row.Id, step, decision, note, userId);
            row.Status = (int)(decision switch
            {
                FundingDecision.Rejected => FundingRequestStatus.Rejected,
                FundingDecision.Revision => FundingRequestStatus.Draft,
                _ => step == steps.Last() ? FundingRequestStatus.Approved : FundingRequestStatus.Submitted
            });
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;

            if (row.Status == (int)FundingRequestStatus.Submitted)
            {
                var next = steps[steps.IndexOf(step) + 1];
                NotifyHelper.ToApprovers(context, next, $"Pengajuan dana {row.RequestNumber} menunggu persetujuan", row.Purpose,
                    NotifyHelper.LINK_FUNDING, "t_funding_request", row.Id, userId);
            }
            else
            {
                var result = decision switch
                {
                    FundingDecision.Approved => "disetujui",
                    FundingDecision.Rejected => "ditolak",
                    _ => "perlu direvisi"
                };
                NotifyHelper.ToUser(context, row.InsBy, NotifyHelper.TYPE_APPROVAL_RESULT, $"Pengajuan dana {row.RequestNumber} {result}",
                    note ?? row.Purpose, NotifyHelper.LINK_FUNDING, "t_funding_request", row.Id, userId);
            }
        }
        private static void SetDisburse(EF.ModelContext context, EF.TFundingRequest row, int accountId, int categoryId, DateOnly disburseDate, int userId)
        {
            TransaksiHelper.ValidateStatus((FundingRequestStatus)row.Status, "mencairkan dana", FundingRequestStatus.Approved);
            CreateTransaction(context, row, accountId, categoryId, CashFlowType.Expense, row.Amount, disburseDate,
                $"Pencairan {row.RequestNumber}: {row.Purpose}", userId);
            row.Status = (int)FundingRequestStatus.Disbursed;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetSettlementStatus(EF.TFundingRequest row, SettlementStatus target, int userId)
        {
            TransaksiHelper.ValidateStatus((FundingRequestStatus)row.Status, "mengubah pertanggungjawaban dana", FundingRequestStatus.Disbursed);
            if (target == SettlementStatus.Diajukan)
            {
                TransaksiHelper.ValidateStatus((SettlementStatus)row.SettlementStatus, "mengajukan pertanggungjawaban dana", SettlementStatus.Belum);
            }
            else
            {
                TransaksiHelper.ValidateStatus((SettlementStatus)row.SettlementStatus, "mengembalikan pertanggungjawaban dana", SettlementStatus.Diajukan);
            }
            row.SettlementStatus = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetSettle(EF.ModelContext context, EF.TFundingRequest row, int? accountId, int? categoryId, DateOnly settleDate, int userId)
        {
            TransaksiHelper.ValidateStatus((FundingRequestStatus)row.Status, "menyelesaikan pertanggungjawaban dana", FundingRequestStatus.Disbursed);
            TransaksiHelper.ValidateStatus((SettlementStatus)row.SettlementStatus, "menyelesaikan pertanggungjawaban dana", SettlementStatus.Diajukan);
            RecalculateRealized(context, row, userId);
            var difference = row.Amount - row.RealizedAmount;
            if (difference != 0)
            {
                if (!(accountId > 0) || !(categoryId > 0))
                {
                    throw new ArgumentException(ExceptionMessage.FUNDING_SETTLEMENT_ACCOUNT_REQUIRED);
                }
                var isReturn = difference > 0;
                CreateTransaction(context, row, accountId!.Value, categoryId!.Value, isReturn ? CashFlowType.Income : CashFlowType.Expense,
                    Math.Abs(difference), settleDate,
                    (isReturn ? "Pengembalian sisa dana " : "Kekurangan dana ") + row.RequestNumber, userId);
            }
            var now = DateTime.Now;
            row.ReturnedAmount = Math.Max(difference, 0);
            row.SettlementStatus = (int)SettlementStatus.Selesai;
            row.SettledBy = userId;
            row.SettledAt = now;
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        private static void CreateTransaction(EF.ModelContext context, EF.TFundingRequest row, int accountId, int categoryId, CashFlowType type,
            decimal amount, DateOnly date, string description, int userId)
        {
            FinancialTransaction.CreateRow(context, new FinancialTransaction.NewRow
            {
                AccountId = accountId,
                CategoryId = categoryId,
                PeriodId = FinancialTransaction.OpenPeriodOrDefault(context, row.PeriodId),
                TransactionType = type,
                Amount = amount,
                TransactionDate = date,
                Description = description,
                BudgetItemId = row.BudgetItemId,
                ActivityId = row.ActivityId,
                ProgramId = row.ProgramId,
                CompetitionId = row.CompetitionId
            }, row.Id, true, userId);
        }
        #endregion
    }
}
