using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Master.Approval;
using GWALib.Lib.Master.Surat;
using GWALib.Lib.Transaksi.Approval;
using GWALib.Lib.Transaksi.Keuangan;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Surat
{
    public partial class Letter
    {
        public const string DOCUMENT_TYPE = "LETTER";
        private const string NUMBER_FALLBACK_PREFIX = "SRT";
        public const int MAX_FILE_BYTES = 10 * 1024 * 1024;

        #region SYNC
        public List<LetterApproval> GetApprovals(bool refresh)
        {
            if (refresh || ApprovalList == null)
            {
                ApprovalList = LetterApproval.GetDataList(Id);
            }
            return ApprovalList;
        }
        public List<LetterDisposition> GetDispositions(bool refresh)
        {
            if (refresh || DispositionList == null)
            {
                DispositionList = LetterDisposition.GetDataList(Id);
            }
            return DispositionList;
        }
        public FileContent? GetFile()
        {
            using var context = DBClass.GetContext();
            var file = context.TLetters.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.ContentType }).SingleOrDefault();
            return file?.FileData == null ? null : new FileContent(file.FileData, file.FileName ?? "surat", file.ContentType);
        }
        public static List<Letter> GetDataList(LetterDirection? direction = null, LetterStatus? status = null, string? letterType = null, int? periodId = null,
            int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(direction, status, letterType, periodId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Letter, EF.TLetter>();
        }
        public static Response<Letter> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new Letter(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Letter>(msg));
        }
        public Response<Letter> Update(string user)
        {
            return Run(SetRow, user);
        }
        // file null / kosong = file dihapus.
        public Response<Letter> UpdateFile(byte[]? file, string? fileName, string? contentType, string user)
        {
            return Run((context, row, userId) => SetFileRow(row, file, fileName, contentType, userId), user);
        }
        public Response<Letter> Submit(string user)
        {
            return Run((context, row, userId) => SetSubmit(context, row, userId), user);
        }
        // Tanda tangan opsional; positionId = jabatan penandatangan bila langkah alur tidak menentukan jabatan.
        public Response<Letter> Approve(string? note, byte[]? signature, int? positionId, string user)
        {
            return RunInTransaction((context, row, userId) => SetDecision(context, row, true, note, signature, positionId, userId), user);
        }
        public Response<Letter> Reject(string? note, string user)
        {
            return Run((context, row, userId) => SetDecision(context, row, false, note, null, null, userId), user);
        }
        public Response<Letter> MarkSent(string user)
        {
            return Run((context, row, userId) => SetStatus(row, LetterStatus.Sent, userId), user);
        }
        public Response<Letter> Archive(string user)
        {
            return Run((context, row, userId) => SetStatus(row, LetterStatus.Archived, userId), user);
        }
        // Surat keluar Draft / Rejected, atau surat masuk Received; persetujuan dan disposisi ikut terhapus (ON DELETE CASCADE).
        public Response<Letter> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var data = new Letter(Reload(row.Id));
                SetDelete(context, row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Letter>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<LetterApproval>> GetApprovalsAsync(bool refresh)
        {
            if (refresh || ApprovalList == null)
            {
                ApprovalList = await LetterApproval.GetDataListAsync(Id);
            }
            return ApprovalList;
        }
        public async Task<List<LetterDisposition>> GetDispositionsAsync(bool refresh)
        {
            if (refresh || DispositionList == null)
            {
                DispositionList = await LetterDisposition.GetDataListAsync(Id);
            }
            return DispositionList;
        }
        public async Task<FileContent?> GetFileAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TLetters.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.ContentType }).SingleOrDefaultAsync();
            return file?.FileData == null ? null : new FileContent(file.FileData, file.FileName ?? "surat", file.ContentType);
        }
        public static async Task<List<Letter>> GetDataListAsync(LetterDirection? direction = null, LetterStatus? status = null, string? letterType = null, int? periodId = null,
            int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(direction, status, letterType, periodId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Letter, EF.TLetter>();
        }
        public static async Task<Response<Letter>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new Letter(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Letter>(msg));
        }
        public Task<Response<Letter>> UpdateAsync(string user)
        {
            return RunAsync(SetRow, user);
        }
        public Task<Response<Letter>> UpdateFileAsync(byte[]? file, string? fileName, string? contentType, string user)
        {
            return RunAsync((context, row, userId) => SetFileRow(row, file, fileName, contentType, userId), user);
        }
        public Task<Response<Letter>> SubmitAsync(string user)
        {
            return RunAsync((context, row, userId) => SetSubmit(context, row, userId), user);
        }
        public Task<Response<Letter>> ApproveAsync(string? note, byte[]? signature, int? positionId, string user)
        {
            return RunInTransactionAsync((context, row, userId) => SetDecision(context, row, true, note, signature, positionId, userId), user);
        }
        public Task<Response<Letter>> RejectAsync(string? note, string user)
        {
            return RunAsync((context, row, userId) => SetDecision(context, row, false, note, null, null, userId), user);
        }
        public Task<Response<Letter>> MarkSentAsync(string user)
        {
            return RunAsync((context, row, userId) => SetStatus(row, LetterStatus.Sent, userId), user);
        }
        public Task<Response<Letter>> ArchiveAsync(string user)
        {
            return RunAsync((context, row, userId) => SetStatus(row, LetterStatus.Archived, userId), user);
        }
        public async Task<Response<Letter>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var data = new Letter(Reload(row.Id));
                SetDelete(context, row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Letter>(msg));
        }
        #endregion

        #region INTERNAL
        // Surat yang boleh diberi disposisi: surat masuk (Received / Archived) atau surat keluar yang sudah disetujui.
        internal static EF.TLetter GetDispositionLetter(EF.ModelContext context, int letterId)
        {
            var row = GetRow(context, letterId);
            TransaksiHelper.ValidateStatus((LetterStatus)row.Status, "memberi disposisi surat",
                LetterStatus.Received, LetterStatus.Approved, LetterStatus.Sent, LetterStatus.Archived);
            return row;
        }
        #endregion

        #region HELPER
        private delegate void RowAction(EF.ModelContext context, EF.TLetter row, int userId);

        private Response<Letter> Run(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                return new Letter(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Letter>(msg));
        }
        private async Task<Response<Letter>> RunAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                return new Letter(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Letter>(msg));
        }
        // Persetujuan bisa membuat nomor surat: dibungkus transaksi DB supaya nomor ikut di-rollback.
        private Response<Letter> RunInTransaction(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                transaction.Commit();
                return new Letter(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Letter>(msg));
        }
        private async Task<Response<Letter>> RunInTransactionAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new Letter(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Letter>(msg));
        }
        private static EF.TLetter Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TLetter> FilterQuery(LetterDirection? direction, LetterStatus? status, string? letterType, int? periodId)
        {
            var type = string.IsNullOrWhiteSpace(letterType) ? null : LetterTemplate.ToTypeCode(letterType);
            return _Querys.Where(x => (direction == null || x.Direction == (int)direction)
                    && (status == null || x.Status == (int)status)
                    && (type == null || x.LetterType == type)
                    && (periodId == null || x.PeriodId == periodId))
                .OrderByDescending(x => x.InsDate).ThenByDescending(x => x.Id);
        }
        private static EF.TLetter GetRow(EF.ModelContext context, int id)
        {
            return context.TLetters.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Surat"));
        }
        private static async Task<EF.TLetter> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TLetters.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Surat"));
        }
        private static void ValidateUniqueNumber(EF.ModelContext context, int id, int direction, string? number)
        {
            if (number != null)
            {
                ValidateUtility.CheckDuplicateData<EF.TLetter>(context, x => x.Direction == direction && x.LetterNumber == number && x.Id != id,
                    string.Format(ExceptionMessage.DATA_DUPLICATE, "Nomor Surat"));
            }
        }
        private record Content(int? PeriodId, int? TemplateId, string LetterType, string Subject, string? Body, string? Number);

        // Template mengisi jenis dan isi bila kosong. Surat masuk wajib tanggal terima dan pengirim.
        private static Content ValidateData(EF.ModelContext context, LetterDirection direction, int? periodId, int? templateId, string? letterType, string? subject,
            string? body, string? number, DateOnly? receivedDate, string? sender)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Perihal"));
            }
            templateId = templateId > 0 ? templateId : null;
            EF.MLetterTemplate? template = null;
            if (templateId != null)
            {
                template = LetterTemplate.GetActiveTemplate(context, templateId.Value);
            }
            var type = !string.IsNullOrWhiteSpace(letterType) ? LetterTemplate.ToTypeCode(letterType) : template?.LetterType;
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Jenis Surat"));
            }
            if (direction == LetterDirection.In)
            {
                if (receivedDate == null)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Terima"));
                }
                if (string.IsNullOrWhiteSpace(sender))
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Pengirim"));
                }
            }
            periodId = (periodId > 0 ? periodId : null) ?? FinancialTransaction.GetActivePeriodId(context);
            if (periodId != null && !context.MManagementPeriods.Any(x => x.Id == periodId))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Periode Kepengurusan"));
            }
            return new Content(periodId, templateId, type, TransaksiHelper.Truncate(subject, 255)!,
                string.IsNullOrWhiteSpace(body) ? template?.Body : body, TransaksiHelper.Truncate(number, 100));
        }
        private static EF.TLetter CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            if (!Enum.IsDefined(newrow.Direction))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Arah Surat"));
            }
            var content = ValidateData(context, newrow.Direction, newrow.PeriodId, newrow.TemplateId, newrow.LetterType, newrow.Subject, newrow.Body,
                newrow.LetterNumber, newrow.ReceivedDate, newrow.Sender);
            ValidateUniqueNumber(context, 0, (int)newrow.Direction, content.Number);
            var row = new EF.TLetter
            {
                Id = SequenceUtility.GetNextId<EF.TLetter>(context),
                PeriodId = content.PeriodId,
                TemplateId = content.TemplateId,
                Direction = (int)newrow.Direction,
                LetterType = content.LetterType,
                LetterNumber = content.Number,
                Subject = content.Subject,
                LetterDate = newrow.LetterDate,
                ReceivedDate = newrow.Direction == LetterDirection.In ? newrow.ReceivedDate : null,
                Sender = TransaksiHelper.Truncate(newrow.Sender, 200),
                Recipient = TransaksiHelper.Truncate(newrow.Recipient, 200),
                Summary = TransaksiHelper.ToNullable(newrow.Summary),
                Body = content.Body,
                Status = (int)(newrow.Direction == LetterDirection.In ? LetterStatus.Received : LetterStatus.Draft),
                CreatedBy = userId,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            SetFile(row, newrow.FileData, newrow.FileName, newrow.ContentType);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TLetters.Add(row);
            return row;
        }
        private static void ValidateEditable(EF.TLetter row, string action)
        {
            if (row.Direction == (int)LetterDirection.In)
            {
                TransaksiHelper.ValidateStatus((LetterStatus)row.Status, action, LetterStatus.Received);
            }
            else
            {
                TransaksiHelper.ValidateStatus((LetterStatus)row.Status, action, LetterStatus.Draft, LetterStatus.Rejected);
            }
        }
        // Arah surat tetap; file tidak diubah di sini (UpdateFile).
        private void SetRow(EF.ModelContext context, EF.TLetter row, int userId)
        {
            ValidateEditable(row, "mengubah surat");
            var direction = (LetterDirection)row.Direction;
            var content = ValidateData(context, direction, PeriodId, TemplateId, LetterType, Subject, Body, LetterNumber, ReceivedDate, Sender);
            ValidateUniqueNumber(context, row.Id, row.Direction, content.Number);
            row.PeriodId = content.PeriodId;
            row.TemplateId = content.TemplateId;
            row.LetterType = content.LetterType;
            row.LetterNumber = content.Number;
            row.Subject = content.Subject;
            row.LetterDate = LetterDate;
            row.ReceivedDate = direction == LetterDirection.In ? ReceivedDate : null;
            row.Sender = TransaksiHelper.Truncate(Sender, 200);
            row.Recipient = TransaksiHelper.Truncate(Recipient, 200);
            row.Summary = TransaksiHelper.ToNullable(Summary);
            row.Body = content.Body;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetFile(EF.TLetter row, byte[]? file, string? fileName, string? contentType)
        {
            if (file == null || file.Length == 0)
            {
                row.FileData = null;
                row.FileName = null;
                row.ContentType = null;
                return;
            }
            row.FileName = TransaksiHelper.ValidateFile(file, fileName, "File Surat", MAX_FILE_BYTES);
            row.FileData = file;
            row.ContentType = TransaksiHelper.ToNullable(contentType);
        }
        // Surat keluar yang sudah disetujui masih boleh diberi file final (PDF bertanda tangan).
        private static void SetFileRow(EF.TLetter row, byte[]? file, string? fileName, string? contentType, int userId)
        {
            if (row.Direction == (int)LetterDirection.In)
            {
                TransaksiHelper.ValidateStatus((LetterStatus)row.Status, "mengubah file surat", LetterStatus.Received);
            }
            else
            {
                TransaksiHelper.ValidateStatus((LetterStatus)row.Status, "mengubah file surat", LetterStatus.Draft, LetterStatus.Rejected, LetterStatus.Approved);
            }
            SetFile(row, file, fileName, contentType);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void ValidateOut(EF.TLetter row)
        {
            if (row.Direction != (int)LetterDirection.Out)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.LETTER_DIRECTION_ONLY, LetterDirection.Out.GetDescription()));
            }
        }
        private static void SetSubmit(EF.ModelContext context, EF.TLetter row, int userId)
        {
            ValidateOut(row);
            TransaksiHelper.ValidateStatus((LetterStatus)row.Status, "mengajukan surat", LetterStatus.Draft, LetterStatus.Rejected);
            if (string.IsNullOrWhiteSpace(row.Body) && !context.TLetters.Any(x => x.Id == row.Id && x.FileData != null))
            {
                throw new ArgumentException(ExceptionMessage.LETTER_CONTENT_REQUIRED);
            }
            LetterApproval.ResetDecisions(context, row.Id, userId);
            row.Status = (int)LetterStatus.WaitingApproval;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        // Dengan alur LETTER aktif: langkah berjalan = langkah pertama yang belum disetujui. Tanpa alur: satu persetujuan level 1.
        private static void SetDecision(EF.ModelContext context, EF.TLetter row, bool approve, string? note, byte[]? signature, int? positionId, int userId)
        {
            ValidateOut(row);
            TransaksiHelper.ValidateStatus((LetterStatus)row.Status, approve ? "menyetujui surat" : "menolak surat", LetterStatus.WaitingApproval);
            var flow = context.MApprovalFlows.Include(x => x.MApprovalFlowSteps)
                .SingleOrDefault(x => x.DocumentType == DOCUMENT_TYPE && x.IsActive == (int)ActiveStatus.Aktif);
            var steps = flow?.MApprovalFlowSteps.OrderBy(x => x.StepLevel).ToList() ?? new();
            var approvals = context.TLetterApprovals.Where(x => x.LetterId == row.Id).ToList();
            EF.MApprovalFlowStep? step = null;
            var isLast = true;
            if (steps.Count > 0)
            {
                step = steps.First(s => !approvals.Any(a => a.ApprovalLevel == s.StepLevel && a.Decision == (int)LetterDecision.Approved));
                ApprovalHelper.ValidateApprover(context, step, userId);
                isLast = step == steps.Last();
            }
            positionId = step?.PositionId ?? (positionId > 0 ? positionId : null);
            if (positionId != null && !context.MPositions.Any(x => x.Id == positionId))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Jabatan"));
            }
            LetterApproval.SetDecision(context, approvals, row.Id, step?.StepLevel ?? 1, step?.Id, positionId,
                approve ? LetterDecision.Approved : LetterDecision.Rejected, note, signature, userId);

            var now = DateTime.Now;
            if (!approve)
            {
                row.Status = (int)LetterStatus.Rejected;
            }
            else if (isLast)
            {
                row.LetterDate ??= DateOnly.FromDateTime(now);
                if (row.LetterNumber == null)
                {
                    var date = row.LetterDate.Value.ToDateTime(TimeOnly.MinValue);
                    row.LetterNumber = context.MDocumentNumberFormats.Any(x => x.DocumentType == row.LetterType && x.IsActive == (int)ActiveStatus.Aktif)
                        ? DocumentNumberSeq.GenerateNumber(context, row.LetterType, date, userId)
                        : TransaksiHelper.GenerateNumber(context, DOCUMENT_TYPE, date, NUMBER_FALLBACK_PREFIX, row.Id, userId);
                    ValidateUniqueNumber(context, row.Id, row.Direction, row.LetterNumber);
                }
                row.Status = (int)LetterStatus.Approved;
            }
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        // Sent dari Approved (surat keluar); Archived dari Sent / Received / Approved.
        private static void SetStatus(EF.TLetter row, LetterStatus target, int userId)
        {
            if (target == LetterStatus.Sent)
            {
                ValidateOut(row);
                TransaksiHelper.ValidateStatus((LetterStatus)row.Status, "menandai surat terkirim", LetterStatus.Approved);
            }
            else
            {
                TransaksiHelper.ValidateStatus((LetterStatus)row.Status, "mengarsipkan surat", LetterStatus.Sent, LetterStatus.Received, LetterStatus.Approved);
            }
            row.Status = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetDelete(EF.ModelContext context, EF.TLetter row)
        {
            ValidateEditable(row, "menghapus surat");
            if (context.TBorrowRequests.Any(x => x.LetterId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.LETTER_IN_USE);
            }
            context.TLetters.Remove(row);
        }
        #endregion
    }
}
