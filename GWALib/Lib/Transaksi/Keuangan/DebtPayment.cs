using GWALib.General;
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
    /// Pembayaran hutang (t_debt_payment). Save membuat transaksi Expense Posted (satu transaksi per pembayaran) dan menghitung ulang
    /// PaidAmount / status hutang. Nominal tidak boleh melebihi sisa hutang. Tidak ada ubah: Delete membatalkan transaksinya lalu hapus,
    /// kecuali hutang sudah Dihapuskan.
    /// </summary>
    public partial class DebtPayment : BaseLib
    {
        public class NewRow
        {
            public int DebtId { get; set; }
            // Akun sumber dana dan kategori pengeluaran.
            public int AccountId { get; set; }
            public int CategoryId { get; set; }
            public decimal Amount { get; set; }
            public DateOnly PaidDate { get; set; }
            public string? Notes { get; set; }
        }

        public int Id { get; set; }
        public int DebtId { get; private set; }
        public int TransactionId { get; private set; }
        public string TransactionNumber { get; private set; } = null!;
        public string AccountName { get; private set; } = null!;
        public decimal Amount { get; private set; }
        public DateOnly PaidDate { get; private set; }
        public string? Notes { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }

        private static IQueryable<EF.TDebtPayment> _Querys
        {
            get
            {
                return DBClass.GetContext().TDebtPayments
                    .Include(x => x.Transaction).ThenInclude(x => x.Account)
                    .AsQueryable();
            }
        }
        public DebtPayment()
        {
        }
        public DebtPayment(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<DebtPayment>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal DebtPayment(EF.TDebtPayment row)
        {
            SetData(row);
        }
        private void SetData(EF.TDebtPayment row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<DebtPayment>();
            DebtId = row.DebtId;
            TransactionId = row.TransactionId;
            TransactionNumber = row.Transaction?.TransactionNumber ?? string.Empty;
            AccountName = row.Transaction?.Account?.Name ?? string.Empty;
            Amount = row.Amount;
            PaidDate = row.PaidDate;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
        }
    }
}
