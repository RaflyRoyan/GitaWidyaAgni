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

namespace GWALib.Lib.Transaksi.Surat
{
    public partial class LetterApproval
    {
        #region SYNC
        public byte[]? GetSignature()
        {
            using var context = DBClass.GetContext();
            return context.TLetterApprovals.Where(x => x.Id == Id).Select(x => x.Signature).SingleOrDefault();
        }
        public static List<LetterApproval> GetDataList(int letterId)
        {
            return _Querys.Where(x => x.LetterId == letterId).OrderBy(x => x.ApprovalLevel).ToList().MappingData<LetterApproval, EF.TLetterApproval>();
        }
        #endregion

        #region ASYNC
        public async Task<byte[]?> GetSignatureAsync()
        {
            using var context = DBClass.GetContext();
            return await context.TLetterApprovals.Where(x => x.Id == Id).Select(x => x.Signature).SingleOrDefaultAsync();
        }
        public static async Task<List<LetterApproval>> GetDataListAsync(int letterId)
        {
            return await (await _Querys.Where(x => x.LetterId == letterId).OrderBy(x => x.ApprovalLevel).ToListAsync())
                .MappingDataAsync<LetterApproval, EF.TLetterApproval>();
        }
        #endregion

        #region INTERNAL
        // Baris level yang sama (dari putaran pengajuan sebelumnya) ditimpa.
        internal static void SetDecision(EF.ModelContext context, List<EF.TLetterApproval> approvals, int letterId, int level, int? flowStepId, int? positionId,
            LetterDecision decision, string? note, byte[]? signature, int userId)
        {
            if (signature != null && signature.Length > MAX_SIGNATURE_BYTES)
            {
                throw new ArgumentException(string.Format(GWAShared.Results.ExceptionMessage.FILE_TOO_LARGE, "Tanda Tangan", MAX_SIGNATURE_BYTES / 1024 / 1024));
            }
            var now = DateTime.Now;
            var row = approvals.SingleOrDefault(x => x.ApprovalLevel == level);
            if (row == null)
            {
                row = new EF.TLetterApproval
                {
                    Id = SequenceUtility.GetNextId<EF.TLetterApproval>(context),
                    LetterId = letterId,
                    ApprovalLevel = level,
                    InsDate = now,
                    InsBy = userId
                };
                context.TLetterApprovals.Add(row);
                approvals.Add(row);
            }
            else
            {
                row.UpdDate = now;
                row.UpdBy = userId;
            }
            row.ApproverUserId = userId;
            row.PositionId = positionId;
            row.FlowStepId = flowStepId;
            row.Decision = (int)decision;
            row.Note = TransaksiHelper.Truncate(note, 255);
            row.Signature = signature == null || signature.Length == 0 ? row.Signature : signature;
            row.DecidedAt = now;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        internal static void ResetDecisions(EF.ModelContext context, int letterId, int userId)
        {
            var now = DateTime.Now;
            foreach (var row in context.TLetterApprovals.Where(x => x.LetterId == letterId).ToList())
            {
                row.Decision = (int)LetterDecision.Pending;
                row.DecidedAt = null;
                row.UpdDate = now;
                row.UpdBy = userId;
            }
        }
        #endregion
    }
}
