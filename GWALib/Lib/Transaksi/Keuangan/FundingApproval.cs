using GWALib.General;
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
    /// <summary>
    /// Keputusan per langkah persetujuan pengajuan dana (t_funding_approval), unik per (pengajuan, ApprovalLevel = step_level alur FUNDING_REQUEST).
    /// Baris dibuat saat penyetuju memutuskan (Approve / Reject / RequestRevision di FundingRequest) dengan ApproverUserId = user yang memutuskan.
    /// Saat pengajuan diajukan ulang, keputusan lama kembali Pending (penyetuju dan catatan terakhir tetap tersimpan). Hanya-baca dari luar.
    /// </summary>
    public partial class FundingApproval : BaseLib
    {
        public int Id { get; set; }
        public int RequestId { get; private set; }
        public int ApprovalLevel { get; private set; }
        public int? FlowStepId { get; private set; }
        public string? StepName { get; private set; }
        public int ApproverUserId { get; private set; }
        public string ApproverName { get; private set; } = null!;
        public FundingDecision Decision { get; private set; }
        public string? Note { get; private set; }
        public DateTime? DecidedAt { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TFundingApproval> _Querys
        {
            get
            {
                return DBClass.GetContext().TFundingApprovals
                    .Include(x => x.FlowStep)
                    .Include(x => x.ApproverUser)
                    .AsQueryable();
            }
        }
        public FundingApproval()
        {
        }
        public FundingApproval(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<FundingApproval>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal FundingApproval(EF.TFundingApproval row)
        {
            SetData(row);
        }
        private void SetData(EF.TFundingApproval row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<FundingApproval>();
            RequestId = row.RequestId;
            ApprovalLevel = row.ApprovalLevel;
            FlowStepId = row.FlowStepId;
            StepName = row.FlowStep?.StepName;
            ApproverUserId = row.ApproverUserId;
            ApproverName = row.ApproverUser?.Username ?? string.Empty;
            Decision = (FundingDecision)row.Decision;
            Note = row.Note;
            DecidedAt = row.DecidedAt;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
