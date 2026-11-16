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
    /// Tagihan iuran anggota (t_dues_bill), unik per (pengaturan iuran, anggota, periode tagihan). Dibuat massal lewat Generate dari m_dues_setting
    /// dengan nominal disalin dari pengaturan. Format periode tagihan mengikuti frekuensi: Bulanan yyyy-MM, Semester yyyy-GANJIL / yyyy-GENAP,
    /// Tahunan yyyy, Sekali bebas (default SEKALI).
    /// Alur status: Unpaid → Paid (Pay membuat transaksi Income Posted) / Waived. CancelPayment membatalkan transaksinya dan kembali Unpaid;
    /// Unwaive mengembalikan Waived ke Unpaid. Nominal / jatuh tempo hanya bisa diubah dan tagihan hanya bisa dihapus saat Unpaid.
    /// </summary>
    public partial class DuesBill : BaseLib
    {
        public int Id { get; set; }
        public int DuesSettingId { get; private set; }
        public string DuesSettingName { get; private set; } = null!;
        public int MemberId { get; private set; }
        public string MemberName { get; private set; } = null!;
        public string BillPeriod { get; private set; } = null!;
        public decimal Amount { get; set; }
        public DateOnly? DueDate { get; set; }
        // Hanya-baca: diubah lewat Pay / CancelPayment / Waive / Unwaive.
        public DuesBillStatus Status { get; private set; }
        public DateTime? PaidAt { get; private set; }
        public int? TransactionId { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TDuesBill> _Querys
        {
            get
            {
                return DBClass.GetContext().TDuesBills
                    .Include(x => x.DuesSetting)
                    .Include(x => x.Member)
                    .AsQueryable();
            }
        }
        public DuesBill()
        {
        }
        public DuesBill(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<DuesBill>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal DuesBill(EF.TDuesBill row)
        {
            SetData(row);
        }
        private void SetData(EF.TDuesBill row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<DuesBill>();
            DuesSettingId = row.DuesSettingId;
            DuesSettingName = row.DuesSetting?.Name ?? string.Empty;
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname ?? string.Empty;
            BillPeriod = row.BillPeriod;
            Amount = row.Amount;
            DueDate = row.DueDate;
            Status = (DuesBillStatus)row.Status;
            PaidAt = row.PaidAt;
            TransactionId = row.TransactionId;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
