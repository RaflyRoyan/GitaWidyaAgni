using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Merchandise
{
    /// <summary>
    /// Varian produk (m_product_variant), mis. S, M, L, Hitam-L. Nama unik per produk, SKU unik.
    /// Price null = ikut harga produk. Stok hanya diisi saat dibuat (mutasi stok awal); selanjutnya lewat t_product_stock_movement.
    /// Produk disimpan sebagai ProductId + GetProduct() agar daftar varian tidak ikut menarik blob foto produk.
    /// </summary>
    public partial class ProductVariant : BaseLib
    {
        public class NewRow
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = null!;
            public decimal? Price { get; set; }
            // Stok awal; diabaikan bila stok produk tidak dilacak.
            public int Stock { get; set; }
            public string? Sku { get; set; }
            // 0 = otomatis ditaruh paling akhir.
            public int SeqNo { get; set; }
        }

        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public decimal? Price { get; set; }
        // Hanya-baca: diubah lewat mutasi stok.
        public int Stock { get; private set; }
        public string? Sku { get; set; }
        public int SeqNo { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private Product? ProductData;

        EF.ModelContext _context = null!;
        private IQueryable<EF.MProductVariant> _Query
        {
            get
            {
                return _context.MProductVariants.AsQueryable();
            }
        }
        private static IQueryable<EF.MProductVariant> _Querys
        {
            get
            {
                return DBClass.GetContext().MProductVariants.AsQueryable();
            }
        }
        public ProductVariant()
        {
            _context = DBClass.GetContext();
        }
        public ProductVariant(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<ProductVariant>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ProductVariant(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ProductVariant(EF.MProductVariant row)
        {
            SetData(row);
        }
        private void SetData(EF.MProductVariant row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ProductVariant>();
            ProductId = row.ProductId;
            Name = row.Name;
            Price = row.Price;
            Stock = row.Stock;
            Sku = row.Sku;
            SeqNo = row.SeqNo;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
