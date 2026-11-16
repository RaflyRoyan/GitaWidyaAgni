using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Pengadaan
{
    public partial class ProcurementApproval
    {
        public const int MAX_SIGNATURE_BYTES = 2 * 1024 * 1024;
        private const int NOTE_LENGTH = 500;

        #region SYNC
        public FileContent? GetSignature()
        {
            using var context = DBClass.GetContext();
            var file = context.TProcurementApprovals.Where(x => x.Id == Id).Select(x => new { x.Signature, x.SignatureFileName, x.SignatureContentType }).SingleOrDefault();
            return file?.Signature == null ? null : new FileContent(file.Signature, file.SignatureFileName ?? "ttd", file.SignatureContentType);
        }
        public static List<ProcurementApproval> GetDataList(int requestId)
        {
            return FilterQuery(requestId).ToList().MappingData<ProcurementApproval, EF.TProcurementApproval>();
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetSignatureAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TProcurementApprovals.Where(x => x.Id == Id).Select(x => new { x.Signature, x.SignatureFileName, x.SignatureContentType }).SingleOrDefaultAsync();
            return file?.Signature == null ? null : new FileContent(file.Signature, file.SignatureFileName ?? "ttd", file.SignatureContentType);
        }
        public static async Task<List<ProcurementApproval>> GetDataListAsync(int requestId)
        {
            return await (await FilterQuery(requestId).ToListAsync()).MappingDataAsync<ProcurementApproval, EF.TProcurementApproval>();
        }
        #endregion

        #region INTERNAL
        internal static EF.TProcurementApproval CreateRow(EF.ModelContext context, int requestId, EF.MApprovalFlowStep step, ProcurementDecision decision, string? note,
            byte[]? signature, string? signatureFileName, string? signatureContentType, int userId)
        {
            var now = DateTime.Now;
            var row = new EF.TProcurementApproval
            {
                Id = SequenceUtility.GetNextId<EF.TProcurementApproval>(context),
                RequestId = requestId,
                ApprovalLevel = step.StepLevel,
                FlowStepId = step.Id,
                ApproverUserId = userId,
                Decision = (int)decision,
                Note = TransaksiHelper.Truncate(note, NOTE_LENGTH),
                DecidedAt = now,
                InsDate = now,
                InsBy = userId
            };
            if (signature != null && signature.Length > 0)
            {
                row.SignatureFileName = TransaksiHelper.ValidateFile(signature, signatureFileName, "File Tanda Tangan", MAX_SIGNATURE_BYTES);
                row.Signature = signature;
                row.SignatureContentType = TransaksiHelper.ToNullable(signatureContentType);
            }
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TProcurementApprovals.Add(row);
            return row;
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TProcurementApproval> FilterQuery(int requestId)
        {
            return _Querys.Where(x => x.RequestId == requestId).OrderBy(x => x.Id);
        }
        #endregion
    }
}
