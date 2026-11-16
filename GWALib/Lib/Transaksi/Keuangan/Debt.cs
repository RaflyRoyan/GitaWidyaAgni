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
    /// Hutang organisasi (t_debt). Nomor dibuat saat simpan dari format DEBT, atau HT/yyyyMMdd/id bila format belum diatur. Periode kosong = periode aktif.
    /// Hutang berupa pinjaman uang (LoanAccountId + LoanCategoryId diisi saat simpan) mencatat transaksi Income Posted dan nominalnya terkunci.
    /// PaidAmount dan status (BelumBayar / Sebagian / Lunas) dihitung dari pembayaran (DebtPayment). WriteOff = Dihapuskan (final).
    /// Data hanya bisa diubah saat BelumBayar / Sebagian; hutang hanya bisa dihapus bila belum ada pembayaran (transaksi pinjamannya ikut dibatalkan).
    /// </summary>
    public partial class Debt : BaseLib
    {
        public class NewRow
        {
            // Kosong = periode kepengurusan aktif.
            public int? PeriodId { get; set; }
            public string CreditorName { get; set; } = null!;
            public string? CreditorContact { get; set; }
            public string? Description { get; set; }
            public decimal Amount { get; set; }
            public DateOnly DebtDate { get; set; }
            public DateOnly? DueDate { get; set; }
            // Opsional, diisi berdua bila hutang berupa pinjaman uang: akun penerima dan kategori pemasukan.
            public int? LoanAccountId { get; set; }
            public int? LoanCategoryId { get; set; }
        }

        public int Id { get; set; }
        public int PeriodId { get; private set; }
        public string PeriodName { get; private set; } = null!;
        public string DebtNumber { get; private set; } = null!;
        public string CreditorName { get; set; } = null!;
        public string? CreditorContact { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; private set; }
        public decimal RemainingAmount => Amount - PaidAmount;
        public DateOnly DebtDate { get; set; }
        public DateOnly? DueDate { get; set; }
        // Hanya-baca: dihitung dari pembayaran, atau WriteOff.
        public DebtStatus Status { get; private set; }
        public int? TransactionId { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private List<DebtPayment>? PaymentList;
        private List<DebtAttachment>? AttachmentList;

        private static IQueryable<EF.TDebt> _Querys
        {
            get
            {
                return DBClass.GetContext().TDebts.Include(x => x.Period).AsQueryable();
            }
        }
        public Debt()
        {
        }
        public Debt(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Debt>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Debt(EF.TDebt row)
        {
            SetData(row);
        }
        private void SetData(EF.TDebt row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Debt>();
            PeriodId = row.PeriodId;
            PeriodName = row.Period?.Name ?? string.Empty;
            DebtNumber = row.DebtNumber;
            CreditorName = row.CreditorName;
            CreditorContact = row.CreditorContact;
            Description = row.Description;
            Amount = row.Amount;
            PaidAmount = row.PaidAmount;
            DebtDate = row.DebtDate;
            DueDate = row.DueDate;
            Status = (DebtStatus)row.Status;
            TransactionId = row.TransactionId;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
