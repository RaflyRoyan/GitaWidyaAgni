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
    /// Pengajuan dana (t_funding_request). Nomor dibuat saat simpan dari format FUNDING_REQUEST, atau PD/yyyyMMdd/id bila format belum diatur.
    /// Alur status: Draft → Submitted → Approved → Disbursed; Submitted → Rejected (final) atau kembali Draft bila langkah meminta revisi.
    /// Persetujuan mengikuti alur aktif m_approval_flow FUNDING_REQUEST langkah demi langkah (lihat FundingApproval); Approved setelah langkah terakhir.
    /// Disburse membuat transaksi Expense Posted. Pertanggungjawaban: realisasi (FundingRealization) diisi saat Disbursed,
    /// SettlementStatus Belum → Diajukan → Selesai; Settle mencatat sisa dana yang dikembalikan (Income) atau kekurangan (Expense).
    /// </summary>
    public partial class FundingRequest : BaseLib
    {
        public class NewRow
        {
            // Kosong = periode item anggaran, atau periode aktif.
            public int? PeriodId { get; set; }
            public int? BudgetItemId { get; set; }
            public int? ProgramId { get; set; }
            public int? ActivityId { get; set; }
            public int? CompetitionId { get; set; }
            public string Purpose { get; set; } = null!;
            public decimal Amount { get; set; }
        }

        public int Id { get; set; }
        public int? PeriodId { get; set; }
        public string? PeriodName { get; private set; }
        public int? BudgetItemId { get; set; }
        public int? ProgramId { get; set; }
        public int? ActivityId { get; set; }
        public int? CompetitionId { get; set; }
        public int RequestedBy { get; private set; }
        public string RequestedByName { get; private set; } = null!;
        public string RequestNumber { get; private set; } = null!;
        public string Purpose { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal RealizedAmount { get; private set; }
        public decimal ReturnedAmount { get; private set; }
        // Hanya-baca: diubah lewat Submit / Approve / Reject / RequestRevision / Disburse / Settle.
        public FundingRequestStatus Status { get; private set; }
        public SettlementStatus SettlementStatus { get; private set; }
        public int? SettledBy { get; private set; }
        public DateTime? SettledAt { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private List<FundingApproval>? ApprovalList;
        private List<FundingRealization>? RealizationList;

        private static IQueryable<EF.TFundingRequest> _Querys
        {
            get
            {
                return DBClass.GetContext().TFundingRequests
                    .Include(x => x.Period)
                    .Include(x => x.RequestedByNavigation)
                    .AsQueryable();
            }
        }
        public FundingRequest()
        {
        }
        public FundingRequest(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<FundingRequest>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal FundingRequest(EF.TFundingRequest row)
        {
            SetData(row);
        }
        private void SetData(EF.TFundingRequest row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<FundingRequest>();
            PeriodId = row.PeriodId;
            PeriodName = row.Period?.Name;
            BudgetItemId = row.BudgetItemId;
            ProgramId = row.ProgramId;
            ActivityId = row.ActivityId;
            CompetitionId = row.CompetitionId;
            RequestedBy = row.RequestedBy;
            RequestedByName = row.RequestedByNavigation?.Username ?? string.Empty;
            RequestNumber = row.RequestNumber;
            Purpose = row.Purpose;
            Amount = row.Amount;
            RealizedAmount = row.RealizedAmount;
            ReturnedAmount = row.ReturnedAmount;
            Status = (FundingRequestStatus)row.Status;
            SettlementStatus = (SettlementStatus)row.SettlementStatus;
            SettledBy = row.SettledBy;
            SettledAt = row.SettledAt;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
