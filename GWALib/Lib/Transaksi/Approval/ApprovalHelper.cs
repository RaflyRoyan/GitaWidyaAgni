using GWAShared.Enums;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Approval
{
    /// <summary>
    /// Pembantu persetujuan bertingkat dokumen transaksi (pengajuan dana, pengadaan, surat) berdasarkan m_approval_flow.
    /// </summary>
    internal static class ApprovalHelper
    {
        // Alur aktif jenis dokumen beserta langkahnya (urut step_level); error bila belum diatur atau tanpa langkah.
        internal static List<EF.MApprovalFlowStep> GetActiveSteps(EF.ModelContext context, string documentType)
        {
            var flow = context.MApprovalFlows.Include(x => x.MApprovalFlowSteps)
                .SingleOrDefault(x => x.DocumentType == documentType && x.IsActive == (int)ActiveStatus.Aktif);
            var steps = flow?.MApprovalFlowSteps.OrderBy(x => x.StepLevel).ToList();
            if (steps == null || steps.Count == 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.APPROVAL_FLOW_NOT_SET, documentType));
            }
            return steps;
        }
        // Role: user punya role itu (tanpa periode, atau periode yang sedang aktif).
        // Jabatan: user terhubung ke anggota yang sedang menjabat posisi itu pada periode aktif.
        // User tertentu: user langkahnya sendiri.
        internal static bool IsApprover(EF.ModelContext context, EF.MApprovalFlowStep step, int userId)
        {
            return (ApproverType)step.ApproverType switch
            {
                ApproverType.Role => context.MUserRoles.Any(x => x.UserId == userId && x.RoleId == step.RoleId
                    && (x.PeriodId == null || x.Period!.Status == (int)ManagementPeriodStatus.Active)),
                ApproverType.Jabatan => context.TManagementAssignments.Any(x => x.PositionId == step.PositionId
                    && x.IsCurrent == (int)AssignmentPeriod.Berjalan && x.Member.UserId == userId
                    && x.Period.Status == (int)ManagementPeriodStatus.Active),
                ApproverType.UserTertentu => step.UserId == userId,
                _ => false
            };
        }
        internal static void ValidateApprover(EF.ModelContext context, EF.MApprovalFlowStep step, int userId)
        {
            if (!IsApprover(context, step, userId))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.APPROVAL_NOT_APPROVER, step.StepName));
            }
        }
    }
}
