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
    /// <summary>
    /// Persetujuan / tanda tangan surat keluar (t_letter_approval), unik per (surat, ApprovalLevel). Dibuat saat penyetuju memutuskan
    /// (Letter.Approve / Reject) dengan ApproverUserId = user yang memutuskan. Saat surat diajukan ulang, keputusan lama kembali Pending.
    /// Hanya-baca dari luar; tanda tangan (maksimal 2 MB) tidak dimuat di daftar (GetSignature).
    /// </summary>
    public partial class LetterApproval : BaseLib
    {
        public const int MAX_SIGNATURE_BYTES = 2 * 1024 * 1024;

        public int Id { get; set; }
        public int LetterId { get; private set; }
        public int ApproverUserId { get; private set; }
        public string ApproverName { get; private set; } = null!;
        public int? PositionId { get; private set; }
        public string? PositionName { get; private set; }
        public int ApprovalLevel { get; private set; }
        public int? FlowStepId { get; private set; }
        public LetterDecision Decision { get; private set; }
        public string? Note { get; private set; }
        public DateTime? DecidedAt { get; private set; }

        // Kolom tanda tangan (blob) tidak ikut di-select.
        private static IQueryable<EF.TLetterApproval> _Querys
        {
            get
            {
                return DBClass.GetContext().TLetterApprovals.Select(x => new EF.TLetterApproval
                {
                    Id = x.Id,
                    LetterId = x.LetterId,
                    ApproverUserId = x.ApproverUserId,
                    ApproverUser = x.ApproverUser,
                    PositionId = x.PositionId,
                    Position = x.Position,
                    ApprovalLevel = x.ApprovalLevel,
                    FlowStepId = x.FlowStepId,
                    Decision = x.Decision,
                    Note = x.Note,
                    DecidedAt = x.DecidedAt
                });
            }
        }
        public LetterApproval()
        {
        }
        public LetterApproval(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<LetterApproval>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal LetterApproval(EF.TLetterApproval row)
        {
            SetData(row);
        }
        private void SetData(EF.TLetterApproval row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<LetterApproval>();
            LetterId = row.LetterId;
            ApproverUserId = row.ApproverUserId;
            ApproverName = row.ApproverUser?.Username ?? string.Empty;
            PositionId = row.PositionId;
            PositionName = row.Position?.Name;
            ApprovalLevel = row.ApprovalLevel;
            FlowStepId = row.FlowStepId;
            Decision = (LetterDecision)row.Decision;
            Note = row.Note;
            DecidedAt = row.DecidedAt;
        }
    }
}
