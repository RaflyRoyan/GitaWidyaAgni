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

namespace GWALib.Lib.Transaksi.Keuangan
{
    public partial class FundingApproval
    {
        private const int NOTE_LENGTH = 255;

        #region SYNC
        public static List<FundingApproval> GetDataList(int requestId)
        {
            return FilterQuery(requestId).ToList().MappingData<FundingApproval, EF.TFundingApproval>();
        }
        #endregion

        #region ASYNC
        public static async Task<List<FundingApproval>> GetDataListAsync(int requestId)
        {
            return await (await FilterQuery(requestId).ToListAsync()).MappingDataAsync<FundingApproval, EF.TFundingApproval>();
        }
        #endregion

        #region INTERNAL
        // Menyimpan keputusan langkah; baris level yang sama (dari putaran pengajuan sebelumnya) ditimpa.
        internal static void SetDecision(EF.ModelContext context, List<EF.TFundingApproval> approvals, int requestId, EF.MApprovalFlowStep step,
            FundingDecision decision, string? note, int userId)
        {
            var now = DateTime.Now;
            var row = approvals.SingleOrDefault(x => x.ApprovalLevel == step.StepLevel);
            if (row == null)
            {
                row = new EF.TFundingApproval
                {
                    Id = SequenceUtility.GetNextId<EF.TFundingApproval>(context),
                    RequestId = requestId,
                    ApprovalLevel = step.StepLevel,
                    InsDate = now,
                    InsBy = userId
                };
                context.TFundingApprovals.Add(row);
                approvals.Add(row);
            }
            else
            {
                row.UpdDate = now;
                row.UpdBy = userId;
            }
            row.FlowStepId = step.Id;
            row.ApproverUserId = userId;
            row.Decision = (int)decision;
            row.Note = TransaksiHelper.Truncate(note, NOTE_LENGTH);
            row.DecidedAt = now;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        // Pengajuan ulang: semua keputusan kembali Pending, penyetuju dan catatan terakhir tetap sebagai riwayat.
        internal static void ResetDecisions(EF.ModelContext context, int requestId, int userId)
        {
            var now = DateTime.Now;
            foreach (var row in context.TFundingApprovals.Where(x => x.RequestId == requestId).ToList())
            {
                row.Decision = (int)FundingDecision.Pending;
                row.DecidedAt = null;
                row.UpdDate = now;
                row.UpdBy = userId;
            }
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TFundingApproval> FilterQuery(int requestId)
        {
            return _Querys.Where(x => x.RequestId == requestId).OrderBy(x => x.ApprovalLevel);
        }
        #endregion
    }
}
