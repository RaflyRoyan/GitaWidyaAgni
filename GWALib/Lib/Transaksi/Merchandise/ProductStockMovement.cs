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
    /// Buku besar stok produk (t_product_stock_movement). Quantity positif = bertambah, negatif = berkurang; setiap mutasi langsung
    /// mengubah stok produk (tanpa varian) atau stok varian, dan stok tidak boleh minus. Hanya produk yang stoknya dilacak.
    /// Input manual: Masuk (jumlah positif, boleh menyebut pengadaan), Penyesuaian (positif / negatif), RusakHilang (jumlah diisi positif,
    /// disimpan negatif). Terjual / Retur dibuat otomatis oleh SalesOrder. Tidak ada ubah / hapus: koreksi dengan Penyesuaian.
    /// </summary>
    public partial class ProductStockMovement : BaseLib
    {
        public class NewRow
        {
            public int ProductId { get; set; }
            // Wajib bila produk punya varian.
            public int? VariantId { get; set; }
            public StockMovementType MovementType { get; set; } = StockMovementType.Masuk;
            public int Quantity { get; set; }
            // Kosong = sekarang.
            public DateTime? MovementDate { get; set; }
            // Hanya untuk Masuk.
            public int? ProcurementId { get; set; }
            public string? Notes { get; set; }
        }

        public int Id { get; set; }
        public int ProductId { get; private set; }
        public string ProductName { get; private set; } = null!;
        public int? VariantId { get; private set; }
        public string? VariantName { get; private set; }
        public StockMovementType MovementType { get; private set; }
        public int Quantity { get; private set; }
        public DateTime MovementDate { get; private set; }
        public int? OrderId { get; private set; }
        public int? ProcurementId { get; private set; }
        public string? Notes { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }

        private static IQueryable<EF.TProductStockMovement> _Querys
        {
            get
            {
                return DBClass.GetContext().TProductStockMovements
                    .Include(x => x.Product)
                    .Include(x => x.Variant)
                    .AsQueryable();
            }
        }
        public ProductStockMovement()
        {
        }
        public ProductStockMovement(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ProductStockMovement>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ProductStockMovement(EF.TProductStockMovement row)
        {
            SetData(row);
        }
        private void SetData(EF.TProductStockMovement row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ProductStockMovement>();
            ProductId = row.ProductId;
            ProductName = row.Product?.Name ?? string.Empty;
            VariantId = row.VariantId;
            VariantName = row.Variant?.Name;
            MovementType = (StockMovementType)row.MovementType;
            Quantity = row.Quantity;
            MovementDate = row.MovementDate;
            OrderId = row.OrderId;
            ProcurementId = row.ProcurementId;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
        }
    }
}
