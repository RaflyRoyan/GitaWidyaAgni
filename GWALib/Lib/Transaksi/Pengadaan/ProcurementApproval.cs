using GWALib.General;
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
    /// <summary>
    /// Riwayat keputusan persetujuan pengadaan (t_procurement_approval), satu baris per keputusan (tidak ada unique per level).
    /// ApprovalLevel = step_level alur PROCUREMENT. Dibuat oleh ProcurementRequest.Approve / RequestRevision / Reject; hanya-baca dari luar.
    /// Putaran persetujuan dimulai lagi setelah keputusan Revisi terakhir. Tanda tangan (opsional, maksimal 2 MB) tidak dimuat di daftar (GetSignature).
    /// </summary>
    public partial class ProcurementApproval : BaseLib
    {
        public record FileContent(byte[] Data, string FileName, string? ContentType);

        public int Id { get; set; }
        public int RequestId { get; private set; }
        public int ApprovalLevel { get; private set; }
        public int? FlowStepId { get; private set; }
        public string? StepName { get; private set; }
        public int ApproverUserId { get; private set; }
        public string ApproverName { get; private set; } = null!;
        public ProcurementDecision Decision { get; private set; }
        public string? Note { get; private set; }
        // Terisi = tanda tangan ada.
        public string? SignatureFileName { get; private set; }
        public DateTime DecidedAt { get; private set; }

        // Kolom tanda tangan (blob) tidak ikut di-select.
        private static IQueryable<EF.TProcurementApproval> _Querys
        {
            get
            {
                return DBClass.GetContext().TProcurementApprovals.Select(x => new EF.TProcurementApproval
                {
                    Id = x.Id,
                    RequestId = x.RequestId,
                    ApprovalLevel = x.ApprovalLevel,
                    FlowStepId = x.FlowStepId,
                    FlowStep = x.FlowStep,
                    ApproverUserId = x.ApproverUserId,
                    ApproverUser = x.ApproverUser,
                    Decision = x.Decision,
                    Note = x.Note,
                    SignatureFileName = x.SignatureFileName,
                    DecidedAt = x.DecidedAt
                });
            }
        }
        public ProcurementApproval()
        {
        }
        public ProcurementApproval(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ProcurementApproval>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ProcurementApproval(EF.TProcurementApproval row)
        {
            SetData(row);
        }
        private void SetData(EF.TProcurementApproval row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ProcurementApproval>();
            RequestId = row.RequestId;
            ApprovalLevel = row.ApprovalLevel;
            FlowStepId = row.FlowStepId;
            StepName = row.FlowStep?.StepName;
            ApproverUserId = row.ApproverUserId;
            ApproverName = row.ApproverUser?.Username ?? string.Empty;
            Decision = (ProcurementDecision)row.Decision;
            Note = row.Note;
            SignatureFileName = row.SignatureFileName;
            DecidedAt = row.DecidedAt;
        }
    }
}
