using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Inventaris
{
    /// <summary>
    /// Peminjaman aset (t_borrow_request). Nomor dibuat saat simpan dari format BORROW, atau PJM/yyyyMMdd/id bila format belum diatur.
    /// Peminjam: Anggota (MemberId), Organisasi / Instansi (ExternalPartyId aktif + kontak opsional), atau Perorangan Luar (BorrowerName).
    /// Skema sewa: TarifTetap (sewa = hari x tarif x jumlah), Seikhlasnya (hanya anggota, sewa tidak ditagih), Gratis. Tarif disalin dari aset.
    /// Alur status: Pending → Approved / Rejected; Pending / Approved → Cancelled; Approved → Borrowed lewat Checkout (unit dipindai);
    /// Borrowed / Overdue → Returned setelah semua aset kembali (BorrowItem.ReturnQuantity / BorrowItemUnit.ReturnUnit). MarkOverdue (job) menandai Overdue.
    /// Denda dihitung saat pengembalian; WaiveLateFee memberi potongan. Pembayaran lewat ReceivePayment (Income dengan borrow_request_id).
    /// </summary>
    public partial class BorrowRequest : BaseLib
    {
        public class NewRow
        {
            public BorrowerType BorrowerType { get; set; } = BorrowerType.Anggota;
            public int? MemberId { get; set; }
            public int? ExternalPartyId { get; set; }
            public int? ContactId { get; set; }
            // Wajib untuk Perorangan Luar; anggota / mitra memakai namanya bila kosong.
            public string? BorrowerName { get; set; }
            public string? BorrowerInstitution { get; set; }
            public string? BorrowerPhone { get; set; }
            public string? BorrowerEmail { get; set; }
            public string? Guarantee { get; set; }
            public int? LetterId { get; set; }
            public string Purpose { get; set; } = null!;
            public DateTime BorrowDate { get; set; }
            public DateTime ExpectedReturnDate { get; set; }
            public RentalScheme RentalScheme { get; set; } = RentalScheme.TarifTetap;
            public string? Notes { get; set; }
            public List<BorrowItem.NewRow> Items { get; set; } = new();
        }

        public int Id { get; set; }
        public string RequestNumber { get; private set; } = null!;
        public BorrowerType BorrowerType { get; set; }
        public int? MemberId { get; set; }
        public int? ExternalPartyId { get; set; }
        public int? ContactId { get; set; }
        public string? BorrowerName { get; set; }
        public string? BorrowerInstitution { get; set; }
        public string? BorrowerPhone { get; set; }
        public string? BorrowerEmail { get; set; }
        public string? Guarantee { get; set; }
        public int? LetterId { get; set; }
        public string Purpose { get; set; } = null!;
        public DateTime BorrowDate { get; set; }
        public DateTime ExpectedReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; private set; }
        public RentalScheme RentalScheme { get; set; }
        public decimal TotalRentalFee { get; private set; }
        public decimal TotalLateFee { get; private set; }
        public decimal LateFeeWaived { get; private set; }
        public string? WaiveNote { get; private set; }
        public decimal PaidAmount { get; private set; }
        public decimal TotalDue => TotalRentalFee + TotalLateFee - LateFeeWaived;
        // Hanya-baca: dihitung dari tagihan dan pembayaran.
        public PaymentStatus PaymentStatus { get; private set; }
        public int? ApprovedBy { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        // Hanya-baca: diubah lewat Approve / Reject / Cancel / Checkout / pengembalian / MarkOverdue.
        public BorrowRequestStatus Status { get; private set; }
        public string? Notes { get; set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private List<BorrowItem>? ItemList;

        private static IQueryable<EF.TBorrowRequest> _Querys
        {
            get
            {
                return DBClass.GetContext().TBorrowRequests.AsQueryable();
            }
        }
        public BorrowRequest()
        {
        }
        public BorrowRequest(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<BorrowRequest>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal BorrowRequest(EF.TBorrowRequest row)
        {
            SetData(row);
        }
        private void SetData(EF.TBorrowRequest row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<BorrowRequest>();
            RequestNumber = row.RequestNumber;
            BorrowerType = (BorrowerType)row.BorrowerType;
            MemberId = row.MemberId;
            ExternalPartyId = row.ExternalPartyId;
            ContactId = row.ContactId;
            BorrowerName = row.BorrowerName;
            BorrowerInstitution = row.BorrowerInstitution;
            BorrowerPhone = row.BorrowerPhone;
            BorrowerEmail = row.BorrowerEmail;
            Guarantee = row.Guarantee;
            LetterId = row.LetterId;
            Purpose = row.Purpose;
            BorrowDate = row.BorrowDate;
            ExpectedReturnDate = row.ExpectedReturnDate;
            ActualReturnDate = row.ActualReturnDate;
            RentalScheme = (RentalScheme)row.RentalScheme;
            TotalRentalFee = row.TotalRentalFee;
            TotalLateFee = row.TotalLateFee;
            LateFeeWaived = row.LateFeeWaived;
            WaiveNote = row.WaiveNote;
            PaidAmount = row.PaidAmount;
            PaymentStatus = (PaymentStatus)row.PaymentStatus;
            ApprovedBy = row.ApprovedBy;
            ApprovedAt = row.ApprovedAt;
            Status = (BorrowRequestStatus)row.Status;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
