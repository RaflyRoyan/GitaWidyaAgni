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
    /// Transaksi kas masuk / keluar (t_financial_transaction). Alur status: Draft → Posted → Void; Draft juga bisa langsung Void.
    /// Nomor dibuat saat simpan dari format FIN_TRX (DocumentNumberSeq), atau TRX/yyyyMMdd/id bila format belum diatur.
    /// Kategori harus sejenis dengan jenis transaksi. Periode kosong = periode aktif. Transaksi tidak bisa dibuat / diposting / dibatalkan
    /// bila saldo periode akunnya sudah ditutup. Transaksi dari tagihan iuran, pengajuan dana, hutang dan pembayaran hutang
    /// langsung Posted dan hanya bisa dibatalkan lewat dokumen asalnya.
    /// </summary>
    public partial class FinancialTransaction : BaseLib
    {
        public class NewRow
        {
            public int AccountId { get; set; }
            public int CategoryId { get; set; }
            // Kosong = periode kepengurusan aktif.
            public int? PeriodId { get; set; }
            public CashFlowType TransactionType { get; set; }
            public decimal Amount { get; set; }
            public DateOnly TransactionDate { get; set; }
            public string? Description { get; set; }
            // Referensi opsional.
            public int? BudgetItemId { get; set; }
            public int? ActivityId { get; set; }
            public int? ProgramId { get; set; }
            public int? CompetitionId { get; set; }
            public int? SponsorshipId { get; set; }
            // Hanya untuk pemanggil internal (ProcurementRequest); Save menolak bila diisi.
            // Hanya untuk pemanggil internal (ProcurementRequest / SalesOrder); Save menolak bila diisi.
            public int? ProcurementId { get; set; }
            public int? SalesOrderId { get; set; }
            public int? BorrowRequestId { get; set; }
        }

        public int Id { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; private set; } = null!;
        public int CategoryId { get; set; }
        public string CategoryName { get; private set; } = null!;
        public int? PeriodId { get; set; }
        public string? PeriodName { get; private set; }
        public int? BudgetItemId { get; set; }
        // Hanya-baca: diisi saat pencairan / pengembalian dana pengajuan.
        public int? FundingRequestId { get; private set; }
        public int? ActivityId { get; set; }
        public int? ProgramId { get; set; }
        public int? CompetitionId { get; set; }
        public int? SponsorshipId { get; set; }
        // Hanya-baca: diisi saat pembayaran pengadaan (ProcurementRequest.RecordPayment).
        public int? ProcurementId { get; private set; }
        // Hanya-baca: diisi saat pembayaran pesanan penjualan (SalesOrder.ReceivePayment).
        public int? SalesOrderId { get; private set; }
        // Hanya-baca: diisi saat pembayaran peminjaman (BorrowRequest.ReceivePayment).
        public int? BorrowRequestId { get; private set; }
        public string TransactionNumber { get; private set; } = null!;
        public CashFlowType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateOnly TransactionDate { get; set; }
        public string? Description { get; set; }
        // Hanya-baca: diubah lewat Post / Void.
        public TransactionStatus Status { get; private set; }
        public int CreatedBy { get; private set; }
        public int? ApprovedBy { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private List<FinancialAttachment>? AttachmentList;

        private static IQueryable<EF.TFinancialTransaction> _Querys
        {
            get
            {
                return DBClass.GetContext().TFinancialTransactions
                    .Include(x => x.Account)
                    .Include(x => x.Category)
                    .Include(x => x.Period)
                    .AsQueryable();
            }
        }
        public FinancialTransaction()
        {
        }
        public FinancialTransaction(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<FinancialTransaction>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal FinancialTransaction(EF.TFinancialTransaction row)
        {
            SetData(row);
        }
        private void SetData(EF.TFinancialTransaction row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<FinancialTransaction>();
            AccountId = row.AccountId;
            AccountName = row.Account?.Name ?? string.Empty;
            CategoryId = row.CategoryId;
            CategoryName = row.Category?.Name ?? string.Empty;
            PeriodId = row.PeriodId;
            PeriodName = row.Period?.Name;
            BudgetItemId = row.BudgetItemId;
            FundingRequestId = row.FundingRequestId;
            ActivityId = row.ActivityId;
            ProgramId = row.ProgramId;
            CompetitionId = row.CompetitionId;
            SponsorshipId = row.SponsorshipId;
            ProcurementId = row.ProcurementId;
            SalesOrderId = row.SalesOrderId;
            BorrowRequestId = row.BorrowRequestId;
            TransactionNumber = row.TransactionNumber;
            TransactionType = (CashFlowType)row.TransactionType;
            Amount = row.Amount;
            TransactionDate = row.TransactionDate;
            Description = row.Description;
            Status = (TransactionStatus)row.Status;
            CreatedBy = row.CreatedBy;
            ApprovedBy = row.ApprovedBy;
            ApprovedAt = row.ApprovedAt;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
