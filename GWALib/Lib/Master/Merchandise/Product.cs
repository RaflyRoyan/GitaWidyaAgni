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
    /// Produk danus / merchandise organisasi (m_product). Nama unik.
    /// Stok hanya diisi saat dibuat (dicatat sebagai mutasi stok awal); selanjutnya berubah lewat t_product_stock_movement.
    /// Produk bervarian memakai stok varian. Query tidak menarik blob foto (lihat <see cref="ListRow"/>).
    /// </summary>
    public partial class Product : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public ProductType ProductType { get; set; } = ProductType.Merchandise;
            public string? Description { get; set; }
            public decimal Price { get; set; }
            public decimal? CostPrice { get; set; }
            public string Unit { get; set; } = "pcs";
            public StockTracking TrackStock { get; set; } = StockTracking.Dilacak;
            // Stok awal produk tanpa varian; diabaikan bila stok tidak dilacak.
            public int Stock { get; set; }
            public SaleMode IsPreorder { get; set; } = SaleMode.ReadyStock;
            public DateTime? PreorderCloseAt { get; set; }
            // Opsional: danus untuk kegiatan tertentu (t_activity).
            public int? ActivityId { get; set; }
            // Opsional: isi file foto asli dari upload; di-resize/kompres oleh ImageUtility.
            public byte[]? Photo { get; set; }
            public string? PhotoFileName { get; set; }
            // Opsional: varian yang langsung dibuat bersama produk (ProductId diisi otomatis).
            public List<ProductVariant.NewRow>? Variants { get; set; }
        }
        public record PhotoData(byte[] Data, string FileName, string ContentType);

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ProductType ProductType { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? CostPrice { get; set; }
        public string Unit { get; set; } = null!;
        public StockTracking TrackStock { get; set; }
        // Hanya-baca: diubah lewat mutasi stok.
        public int Stock { get; private set; }
        public int VariantCount { get; private set; }
        public SaleMode IsPreorder { get; set; }
        public DateTime? PreorderCloseAt { get; set; }
        public bool HasPhoto { get; private set; }
        public int? PhotoSize { get; private set; }
        public string? PhotoFileName { get; private set; }
        public string? PhotoContentType { get; private set; }
        public int? ActivityId { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private List<ProductVariant>? VariantList;

        // Baris hasil query tanpa kolom photo (blob): kolom produk + rantai organisasi + ukuran foto + jumlah varian aktif.
        private sealed class ListRow
        {
            public EF.MProduct Product { get; init; } = null!;
            public int? PhotoSize { get; init; }
            public int VariantCount { get; init; }
        }

        EF.ModelContext _context = null!;
        private IQueryable<ListRow> _Query
        {
            get
            {
                return Project(_context.MProducts);
            }
        }
        private static IQueryable<ListRow> _Querys
        {
            get
            {
                return Project(DBClass.GetContext().MProducts);
            }
        }
        private static IQueryable<ListRow> Project(IQueryable<EF.MProduct> query)
        {
            return query.Select(x => new ListRow
            {
                Product = new EF.MProduct
                {
                    Id = x.Id,
                    Name = x.Name,
                    ProductType = x.ProductType,
                    Description = x.Description,
                    Price = x.Price,
                    CostPrice = x.CostPrice,
                    Unit = x.Unit,
                    TrackStock = x.TrackStock,
                    Stock = x.Stock,
                    IsPreorder = x.IsPreorder,
                    PreorderCloseAt = x.PreorderCloseAt,
                    PhotoFileName = x.PhotoFileName,
                    PhotoContentType = x.PhotoContentType,
                    ActivityId = x.ActivityId,
                    IsActive = x.IsActive,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy,
                    UpdDate = x.UpdDate,
                    UpdBy = x.UpdBy
                },
                PhotoSize = x.Photo == null ? null : x.Photo.Length,
                VariantCount = x.MProductVariants.Count(v => v.IsActive == (int)ActiveStatus.Aktif)
            });
        }
        public Product()
        {
            _context = DBClass.GetContext();
        }
        public Product(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<Product>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Product.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Product(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Product.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        private Product(ListRow row)
        {
            SetData(row);
        }
        private void SetData(ListRow row)
        {
            SetData(row.Product, row.PhotoSize, row.VariantCount);
        }
        private void SetData(EF.MProduct row, int? photoSize, int variantCount)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Product>();
            Name = row.Name;
            ProductType = (ProductType)row.ProductType;
            Description = row.Description;
            Price = row.Price;
            CostPrice = row.CostPrice;
            Unit = row.Unit;
            TrackStock = (StockTracking)row.TrackStock;
            Stock = row.Stock;
            VariantCount = variantCount;
            IsPreorder = (SaleMode)row.IsPreorder;
            PreorderCloseAt = row.PreorderCloseAt;
            HasPhoto = photoSize != null;
            PhotoSize = photoSize;
            PhotoFileName = row.PhotoFileName;
            PhotoContentType = row.PhotoContentType;
            ActivityId = row.ActivityId;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
