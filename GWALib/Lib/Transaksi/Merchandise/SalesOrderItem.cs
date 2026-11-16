using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Merchandise
{
    /// <summary>
    /// Item pesanan penjualan (t_sales_order_item). Dibuat bersama SalesOrder dan tidak diubah terpisah (batalkan pesanan lalu buat ulang).
    /// Harga disalin saat dipesan: harga varian, atau harga produk bila varian tanpa harga. Produk ready stock yang stoknya dilacak
    /// langsung dipotong (mutasi Terjual); produk pre-order harus masih dibuka dan tidak memotong stok.
    /// </summary>
    public partial class SalesOrderItem : BaseLib
    {
        public class NewRow
        {
            public int ProductId { get; set; }
            public int? VariantId { get; set; }
            public int Quantity { get; set; } = 1;
            // mis. nama sablon
            public string? Notes { get; set; }
        }

        public int Id { get; set; }
        public int OrderId { get; private set; }
        public int ProductId { get; private set; }
        public string ProductName { get; private set; } = null!;
        public int? VariantId { get; private set; }
        public string? VariantName { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal Subtotal { get; private set; }
        public string? Notes { get; private set; }

        private static IQueryable<EF.TSalesOrderItem> _Querys
        {
            get
            {
                return DBClass.GetContext().TSalesOrderItems
                    .Include(x => x.Product)
                    .Include(x => x.Variant)
                    .AsQueryable();
            }
        }
        public SalesOrderItem()
        {
        }
        public SalesOrderItem(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<SalesOrderItem>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal SalesOrderItem(EF.TSalesOrderItem row)
        {
            SetData(row);
        }
        private void SetData(EF.TSalesOrderItem row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<SalesOrderItem>();
            OrderId = row.OrderId;
            ProductId = row.ProductId;
            ProductName = row.Product?.Name ?? string.Empty;
            VariantId = row.VariantId;
            VariantName = row.Variant?.Name;
            Quantity = row.Quantity;
            UnitPrice = row.UnitPrice;
            Subtotal = row.Subtotal;
            Notes = row.Notes;
        }
    }
}
