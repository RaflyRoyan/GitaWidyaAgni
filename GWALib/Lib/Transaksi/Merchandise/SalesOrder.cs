using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Merchandise
{
    /// <summary>
    /// Pesanan penjualan danus / merchandise (t_sales_order). Nomor dibuat saat simpan dari format DANUS, atau DNS/yyyyMMdd/id bila format belum diatur.
    /// Pembeli: anggota atau nama pembeli. Item dibuat bersama pesanan (minimal 1) dan memotong stok (lihat SalesOrderItem); TotalAmount = jumlah subtotal.
    /// Pengambilan: Dipesan → SiapDiambil → Selesai (harus Lunas); Dipesan / SiapDiambil → Batal (belum ada pembayaran) mengembalikan stok (mutasi Retur).
    /// Pembayaran lewat ReceivePayment (transaksi Income Posted dengan sales_order_id) dan CancelPayment; PaidAmount / PaymentStatus dihitung ulang.
    /// </summary>
    public partial class SalesOrder : BaseLib
    {
        public class NewRow
        {
            // Kosong = periode kepengurusan aktif.
            public int? PeriodId { get; set; }
            public int? ActivityId { get; set; }
            public int? BuyerMemberId { get; set; }
            public string? BuyerName { get; set; }
            public string? BuyerPhone { get; set; }
            public DateOnly OrderDate { get; set; }
            public int? PicMemberId { get; set; }
            public string? Notes { get; set; }
            public List<SalesOrderItem.NewRow> Items { get; set; } = new();
        }

        // Pesanan tamu (tanpa login) dari toko publik: nama dan nomor WhatsApp wajib, tanggal = hari ini, periode = periode aktif.
        public class GuestRow
        {
            public string? BuyerName { get; set; }
            public string? BuyerPhone { get; set; }
            public string? Notes { get; set; }
            public List<SalesOrderItem.NewRow> Items { get; set; } = new();
        }

        public int Id { get; set; }
        public string OrderNumber { get; private set; } = null!;
        public int? PeriodId { get; private set; }
        public int? ActivityId { get; set; }
        public int? BuyerMemberId { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerPhone { get; set; }
        public DateOnly OrderDate { get; private set; }
        public decimal TotalAmount { get; private set; }
        public decimal PaidAmount { get; private set; }
        public decimal RemainingAmount => TotalAmount - PaidAmount;
        // Hanya-baca: dihitung dari pembayaran.
        public PaymentStatus PaymentStatus { get; private set; }
        // Hanya-baca: diubah lewat SetReady / Finish / Cancel.
        public FulfillmentStatus FulfillmentStatus { get; private set; }
        public int? PicMemberId { get; set; }
        public string? Notes { get; set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private List<SalesOrderItem>? ItemList;

        private static IQueryable<EF.TSalesOrder> _Querys
        {
            get
            {
                return DBClass.GetContext().TSalesOrders.Include(x => x.BuyerMember).AsQueryable();
            }
        }
        public SalesOrder()
        {
        }
        public SalesOrder(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<SalesOrder>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal SalesOrder(EF.TSalesOrder row)
        {
            SetData(row);
        }
        private void SetData(EF.TSalesOrder row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<SalesOrder>();
            OrderNumber = row.OrderNumber;
            PeriodId = row.PeriodId;
            ActivityId = row.ActivityId;
            BuyerMemberId = row.BuyerMemberId;
            BuyerName = row.BuyerName ?? row.BuyerMember?.Fullname;
            BuyerPhone = row.BuyerPhone;
            OrderDate = row.OrderDate;
            TotalAmount = row.TotalAmount;
            PaidAmount = row.PaidAmount;
            PaymentStatus = (PaymentStatus)row.PaymentStatus;
            FulfillmentStatus = (FulfillmentStatus)row.FulfillmentStatus;
            PicMemberId = row.PicMemberId;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
