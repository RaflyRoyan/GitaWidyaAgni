using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GWALib.EF;

[Table("t_borrow_request")]
[Index("ApprovedBy", Name = "fk_borrow_approver")]
[Index("MemberId", Name = "fk_borrow_member")]
[Index("ContactId", Name = "fk_t_borrow_request_contact_id")]
[Index("ExternalPartyId", Name = "fk_t_borrow_request_external_party_id")]
[Index("LetterId", Name = "fk_t_borrow_request_letter_id")]
[Index("Status", Name = "ix_borrow_status")]
[Index("RequestNumber", Name = "uq_borrow_number", IsUnique = true)]
public partial class TBorrowRequest
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 1=ANGGOTA (member_id), 2=ORGANISASI/INSTANSI (external_party_id), 3=PERORANGAN_LUAR (borrower_name)
    /// </summary>
    [Column("borrower_type")]
    public int BorrowerType { get; set; }

    /// <summary>
    /// Diisi jika borrower_type=1
    /// </summary>
    [Column("member_id")]
    public int? MemberId { get; set; }

    /// <summary>
    /// Organisasi peminjam, diisi jika borrower_type=2
    /// </summary>
    [Column("external_party_id")]
    public int? ExternalPartyId { get; set; }

    /// <summary>
    /// PIC dari organisasi peminjam
    /// </summary>
    [Column("contact_id")]
    public int? ContactId { get; set; }

    /// <summary>
    /// Nama peminjam saat transaksi (wajib untuk borrower_type=3)
    /// </summary>
    [Column("borrower_name")]
    [StringLength(150)]
    public string? BorrowerName { get; set; }

    /// <summary>
    /// Asal instansi peminjam
    /// </summary>
    [Column("borrower_institution")]
    [StringLength(150)]
    public string? BorrowerInstitution { get; set; }

    /// <summary>
    /// Kontak peminjam
    /// </summary>
    [Column("borrower_phone")]
    [StringLength(30)]
    public string? BorrowerPhone { get; set; }

    /// <summary>
    /// Email peminjam
    /// </summary>
    [Column("borrower_email")]
    [StringLength(150)]
    public string? BorrowerEmail { get; set; }

    /// <summary>
    /// Jaminan, mis. KTM/KTP
    /// </summary>
    [Column("guarantee")]
    [StringLength(255)]
    public string? Guarantee { get; set; }

    /// <summary>
    /// Surat permohonan peminjaman (opsional)
    /// </summary>
    [Column("letter_id")]
    public int? LetterId { get; set; }

    [Column("request_number")]
    [StringLength(50)]
    public string RequestNumber { get; set; } = null!;

    [Column("purpose")]
    [StringLength(255)]
    public string Purpose { get; set; } = null!;

    [Column("borrow_date", TypeName = "datetime")]
    public DateTime BorrowDate { get; set; }

    [Column("expected_return_date", TypeName = "datetime")]
    public DateTime ExpectedReturnDate { get; set; }

    [Column("actual_return_date", TypeName = "datetime")]
    public DateTime? ActualReturnDate { get; set; }

    /// <summary>
    /// 1=TARIF_TETAP, 2=SEIKHLASNYA (anggota, sewa tidak ditagih), 3=GRATIS
    /// </summary>
    [Column("rental_scheme")]
    public int RentalScheme { get; set; }

    /// <summary>
    /// SUM(t_borrow_item.rental_fee); 0 untuk skema 2 dan 3
    /// </summary>
    [Column("total_rental_fee")]
    [Precision(15, 2)]
    public decimal TotalRentalFee { get; set; }

    /// <summary>
    /// SUM(t_borrow_item.late_fee), dihitung otomatis saat pengembalian
    /// </summary>
    [Column("total_late_fee")]
    [Precision(15, 2)]
    public decimal TotalLateFee { get; set; }

    /// <summary>
    /// Potongan/penghapusan denda oleh pengurus
    /// </summary>
    [Column("late_fee_waived")]
    [Precision(15, 2)]
    public decimal LateFeeWaived { get; set; }

    /// <summary>
    /// Alasan potongan denda
    /// </summary>
    [Column("waive_note")]
    [StringLength(255)]
    public string? WaiveNote { get; set; }

    /// <summary>
    /// SUM(t_financial_transaction.amount) dengan borrow_request_id ini
    /// </summary>
    [Column("paid_amount")]
    [Precision(15, 2)]
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// 1=BELUM_BAYAR, 2=SEBAGIAN, 3=LUNAS, 4=TIDAK_ADA_TAGIHAN. Lunas jika paid_amount &gt;= total_rental_fee + total_late_fee - late_fee_waived
    /// </summary>
    [Column("payment_status")]
    public int PaymentStatus { get; set; }

    [Column("approved_by")]
    public int? ApprovedBy { get; set; }

    [Column("approved_at", TypeName = "datetime")]
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// 1=PENDING, 2=APPROVED, 3=REJECTED, 4=BORROWED, 5=RETURNED, 6=OVERDUE, 7=CANCELLED
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    [Column("notes")]
    [StringLength(255)]
    public string? Notes { get; set; }

    [Column("ins_date", TypeName = "datetime")]
    public DateTime InsDate { get; set; }

    [Column("ins_by")]
    public int? InsBy { get; set; }

    [Column("upd_date", TypeName = "datetime")]
    public DateTime? UpdDate { get; set; }

    [Column("upd_by")]
    public int? UpdBy { get; set; }

    [ForeignKey("ApprovedBy")]
    [InverseProperty("TBorrowRequests")]
    public virtual MUser? ApprovedByNavigation { get; set; }

    [ForeignKey("ContactId")]
    [InverseProperty("TBorrowRequests")]
    public virtual MExternalPartyContact? Contact { get; set; }

    [ForeignKey("ExternalPartyId")]
    [InverseProperty("TBorrowRequests")]
    public virtual MExternalParty? ExternalParty { get; set; }

    [ForeignKey("LetterId")]
    [InverseProperty("TBorrowRequests")]
    public virtual TLetter? Letter { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("TBorrowRequests")]
    public virtual MMember? Member { get; set; }

    [InverseProperty("BorrowRequest")]
    public virtual ICollection<TBorrowItem> TBorrowItems { get; set; } = new List<TBorrowItem>();

    [InverseProperty("BorrowRequest")]
    public virtual ICollection<TFinancialTransaction> TFinancialTransactions { get; set; } = new List<TFinancialTransaction>();
}
