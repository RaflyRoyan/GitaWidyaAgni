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
    /// Usulan pengadaan barang (t_procurement_request). Nomor dibuat saat simpan dari format PROCUREMENT, atau PGD/yyyyMMdd/id bila format belum diatur.
    /// Alur status: Draft → Diajukan → Disetujui / Revisi / Ditolak; Revisi diperbaiki lalu diajukan lagi. Disetujui → Dibeli → DiterimaSebagian → Selesai
    /// (mengikuti penerimaan). Draft / Diajukan / Revisi / Disetujui, dan Dibeli tanpa pembayaran / penerimaan, bisa Batal.
    /// Persetujuan mengikuti alur aktif m_approval_flow PROCUREMENT langkah demi langkah (lihat ProcurementApproval).
    /// Saat diajukan, total estimasi dibandingkan sisa item anggaran: IsOverBudget / BudgetRemaining hanya peringatan.
    /// Pembayaran: DibayarLangsung lewat RecordPayment (transaksi Expense Posted, TotalActual = jumlahnya); PengajuanDana wajib dihubungkan
    /// ke pengajuan dana yang sudah disetujui saat Purchase.
    /// </summary>
    public partial class ProcurementRequest : BaseLib
    {
        public class NewRow
        {
            // Kosong = periode item anggaran, atau periode aktif.
            public int? PeriodId { get; set; }
            // Kosong = anggota milik user login.
            public int? RequestedByMemberId { get; set; }
            public DateOnly RequestDate { get; set; }
            public string Title { get; set; } = null!;
            public string Reason { get; set; } = null!;
            // Maksimal satu dari kegiatan / program.
            public int? ActivityId { get; set; }
            public int? ProgramId { get; set; }
            public int? BudgetItemId { get; set; }
            public ProcurementPaymentMethod PaymentMethod { get; set; } = ProcurementPaymentMethod.DibayarLangsung;
            public DateOnly? NeededByDate { get; set; }
            // Opsional: item yang langsung dibuat bersama usulan (RequestId diisi otomatis).
            public List<ProcurementItem.NewRow>? Items { get; set; }
        }

        public int Id { get; set; }
        public string RequestNumber { get; private set; } = null!;
        public int? PeriodId { get; set; }
        public string? PeriodName { get; private set; }
        public int RequestedByMemberId { get; set; }
        public string RequestedByName { get; private set; } = null!;
        public DateOnly RequestDate { get; set; }
        public string Title { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public int? ActivityId { get; set; }
        public int? ProgramId { get; set; }
        public int? BudgetItemId { get; set; }
        public ProcurementPaymentMethod PaymentMethod { get; set; }
        public int? FundingRequestId { get; private set; }
        public DateOnly? NeededByDate { get; set; }
        public decimal TotalEstimate { get; private set; }
        public decimal TotalActual { get; private set; }
        public BudgetCheck IsOverBudget { get; private set; }
        public decimal? BudgetRemaining { get; private set; }
        // Hanya-baca: diubah lewat Submit / Approve / Purchase / penerimaan / Cancel.
        public ProcurementStatus Status { get; private set; }
        public string? RejectReason { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private List<ProcurementItem>? ItemList;
        private List<ProcurementQuotation>? QuotationList;
        private List<ProcurementApproval>? ApprovalList;
        private List<ProcurementReceipt>? ReceiptList;

        private static IQueryable<EF.TProcurementRequest> _Querys
        {
            get
            {
                return DBClass.GetContext().TProcurementRequests
                    .Include(x => x.Period)
                    .Include(x => x.RequestedByMember)
                    .AsQueryable();
            }
        }
        public ProcurementRequest()
        {
        }
        public ProcurementRequest(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ProcurementRequest>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ProcurementRequest(EF.TProcurementRequest row)
        {
            SetData(row);
        }
        private void SetData(EF.TProcurementRequest row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ProcurementRequest>();
            RequestNumber = row.RequestNumber;
            PeriodId = row.PeriodId;
            PeriodName = row.Period?.Name;
            RequestedByMemberId = row.RequestedByMemberId;
            RequestedByName = row.RequestedByMember?.Fullname ?? string.Empty;
            RequestDate = row.RequestDate;
            Title = row.Title;
            Reason = row.Reason;
            ActivityId = row.ActivityId;
            ProgramId = row.ProgramId;
            BudgetItemId = row.BudgetItemId;
            PaymentMethod = (ProcurementPaymentMethod)row.PaymentMethod;
            FundingRequestId = row.FundingRequestId;
            NeededByDate = row.NeededByDate;
            TotalEstimate = row.TotalEstimate;
            TotalActual = row.TotalActual;
            IsOverBudget = (BudgetCheck)row.IsOverBudget;
            BudgetRemaining = row.BudgetRemaining;
            Status = (ProcurementStatus)row.Status;
            RejectReason = row.RejectReason;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
