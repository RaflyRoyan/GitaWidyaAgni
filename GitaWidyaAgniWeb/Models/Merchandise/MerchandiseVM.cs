using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Merchandise;
using GWALib.Lib.Transaksi.Keuangan;
using GWALib.Lib.Transaksi.Merchandise;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Merchandise
{
    /// <summary>
    /// Merchandise / danus: tab Produk (m_product + varian + foto + mutasi stok), Pesanan (t_sales_order: Dipesan → Siap Diambil → Selesai,
    /// atau Batal; pembayaran = transaksi Pemasukan), Mutasi Stok (riwayat). Stok produk ber-varian disimpan di varian.
    /// </summary>
    public partial class MerchandiseVM
    {
        public class Index
        {
        }

        // Pilihan "produk[:varian]" untuk pesanan.
        public class ProdukOpsi
        {
            public string Value { get; set; } = "";
            public string Text { get; set; } = "";
        }

        public static List<ProdukOpsi> OpsiProduk()
        {
            var list = new List<ProdukOpsi>();
            foreach (var p in Product.GetDataList(FilterAktif.Aktif).OrderBy(x => x.Name))
            {
                var varian = p.VariantCount > 0 ? p.GetVariants(true).Where(v => v.IsActive == ActiveStatus.Aktif).ToList() : new();
                if (varian.Count == 0)
                {
                    list.Add(new ProdukOpsi { Value = p.Id.ToString(), Text = $"{p.Name} · Rp {p.Price:#,##0}" + (p.TrackStock == StockTracking.Dilacak ? $" · stok {p.Stock}" : "") });
                    continue;
                }
                list.AddRange(varian.OrderBy(v => v.SeqNo).Select(v => new ProdukOpsi
                {
                    Value = p.Id + ":" + v.Id,
                    Text = $"{p.Name} — {v.Name} · Rp {(v.Price ?? p.Price):#,##0}" + (p.TrackStock == StockTracking.Dilacak ? $" · stok {v.Stock}" : "")
                }));
            }
            return list;
        }

        // ================= Produk
        public class ListProduk
        {
            public List<ProdukRow> Rows { get; set; } = Product.GetDataList(FilterAktif.Semua).Select(x => new ProdukRow
            {
                EncDecrKey = x.EncDecrKey,
                Name = x.Name,
                Jenis = EnumHelper.Text(x.ProductType),
                Price = x.Price,
                CostPrice = x.CostPrice,
                Stock = x.Stock,
                Unit = x.Unit,
                Dilacak = x.TrackStock == StockTracking.Dilacak,
                PreOrder = x.IsPreorder == SaleMode.PreOrder,
                PreorderCloseAt = x.PreorderCloseAt,
                VariantCount = x.VariantCount,
                HasPhoto = x.HasPhoto,
                Aktif = x.IsActive == ActiveStatus.Aktif
            }).OrderBy(x => x.Name).ToList();
        }

        public class ProdukRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public string Jenis { get; set; } = "";
            public decimal Price { get; set; }
            public decimal? CostPrice { get; set; }
            public int Stock { get; set; }
            public string Unit { get; set; } = "";
            public bool Dilacak { get; set; }
            public bool PreOrder { get; set; }
            public DateTime? PreorderCloseAt { get; set; }
            public int VariantCount { get; set; }
            public bool HasPhoto { get; set; }
            public bool Aktif { get; set; }
        }

        public class AddProduk
        {
            public NewProduk Row { get; set; } = new();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = GwaVM.Opsi<ProductType>();
            public List<GwaVM.OpsiItem> LacakOpsi { get; set; } = GwaVM.Opsi<StockTracking>();
            public List<GwaVM.OpsiItem> ModeOpsi { get; set; } = GwaVM.Opsi<SaleMode>();
            public List<GwaVM.OpsiItem> KegiatanOpsi { get; set; } = Opsi.Kegiatan();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
            public bool HasPhoto { get; set; }
        }

        public class EditProduk : AddProduk
        {
            public EditProduk(string encKey)
            {
                var obj = LibExtensions.Load<Product>(encKey, "Produk");
                HasPhoto = obj.HasPhoto;
                Row = new NewProduk
                {
                    Name = obj.Name,
                    ProductType = (int)obj.ProductType,
                    Description = obj.Description,
                    Price = obj.Price,
                    CostPrice = obj.CostPrice,
                    Unit = obj.Unit,
                    TrackStock = (int)obj.TrackStock,
                    IsPreorder = (int)obj.IsPreorder,
                    PreorderCloseAt = obj.PreorderCloseAt,
                    ActivityId = obj.ActivityId,
                    IsActive = (int)obj.IsActive
                };
            }
        }

        public class NewProduk
        {
            [Required(ErrorMessage = "Nama produk harus diisi")]
            [StringLength(150, ErrorMessage = "Nama maksimal 150 karakter")]
            [Display(Name = "Nama Produk")]
            public string? Name { get; set; }

            [Display(Name = "Jenis")]
            public int ProductType { get; set; } = (int)GWAShared.Enums.ProductType.Merchandise;

            [Display(Name = "Deskripsi")]
            public string? Description { get; set; }

            [Range(0, 999999999, ErrorMessage = "Harga tidak boleh negatif")]
            [Display(Name = "Harga Jual")]
            public decimal Price { get; set; }

            [Display(Name = "Harga Pokok")]
            public decimal? CostPrice { get; set; }

            [StringLength(30, ErrorMessage = "Satuan maksimal 30 karakter")]
            [Display(Name = "Satuan")]
            public string? Unit { get; set; } = "pcs";

            [Display(Name = "Stok")]
            public int TrackStock { get; set; } = (int)StockTracking.Dilacak;

            [Range(0, 1000000, ErrorMessage = "Stok awal tidak boleh negatif")]
            [Display(Name = "Stok Awal (tanpa varian)")]
            public int Stock { get; set; }

            [Display(Name = "Mode Jual")]
            public int IsPreorder { get; set; } = (int)SaleMode.ReadyStock;

            [Display(Name = "Pre-order Ditutup")]
            public DateTime? PreorderCloseAt { get; set; }

            [Display(Name = "Kegiatan")]
            public int? ActivityId { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        // Varian (modal _Varian).
        public class Varian
        {
            public string Key { get; set; } = "";
            public string Nama { get; set; } = "";
            public bool Dilacak { get; set; }
            public VarianInput Input { get; set; } = new();
            public List<VarianRow> Rows { get; set; }

            public Varian(string encKey)
            {
                var obj = LibExtensions.Load<Product>(encKey, "Produk");
                Key = obj.EncDecrKey;
                Nama = obj.Name;
                Dilacak = obj.TrackStock == StockTracking.Dilacak;
                Rows = obj.GetVariants(true).Select(x => new VarianRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Name = x.Name,
                    Price = x.Price,
                    Stock = x.Stock,
                    Sku = x.Sku,
                    SeqNo = x.SeqNo,
                    Aktif = x.IsActive == ActiveStatus.Aktif
                }).OrderBy(x => x.SeqNo).ToList();
            }
        }

        public class VarianRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public decimal? Price { get; set; }
            public int Stock { get; set; }
            public string? Sku { get; set; }
            public int SeqNo { get; set; }
            public bool Aktif { get; set; }
        }

        public class VarianInput
        {
            public string Key { get; set; } = "";

            [Required(ErrorMessage = "Nama varian harus diisi")]
            [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
            [Display(Name = "Varian (mis. XL)")]
            public string? Name { get; set; }

            [Display(Name = "Harga (kosong = harga produk)")]
            public decimal? Price { get; set; }

            [Range(0, 1000000, ErrorMessage = "Stok tidak boleh negatif")]
            [Display(Name = "Stok Awal")]
            public int Stock { get; set; }

            [StringLength(50, ErrorMessage = "SKU maksimal 50 karakter")]
            [Display(Name = "SKU")]
            public string? Sku { get; set; }
        }

        // Mutasi stok manual (modal _Stok).
        public class Stok
        {
            public string Nama { get; set; } = "";
            public StokInput Row { get; set; } = new();
            public List<GwaVM.OpsiItem> VarianOpsi { get; set; } = new();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = new()
            {
                new((int)StockMovementType.Masuk, "Masuk (produksi / beli)"), new((int)StockMovementType.Penyesuaian, "Penyesuaian (+/-)"), new((int)StockMovementType.RusakHilang, "Rusak / Hilang")
            };

            public Stok(string encKey)
            {
                var obj = LibExtensions.Load<Product>(encKey, "Produk");
                if (obj.TrackStock != StockTracking.Dilacak)
                {
                    throw new ArgumentException("Stok produk ini tidak dilacak.");
                }
                Nama = obj.Name;
                Row.Key = obj.EncDecrKey;
                VarianOpsi = obj.GetVariants(true).Where(v => v.IsActive == ActiveStatus.Aktif).Select(v => new GwaVM.OpsiItem(v.Id, v.Name + " (stok " + v.Stock + ")")).ToList();
            }
        }

        public class StokInput
        {
            public string Key { get; set; } = "";

            [Display(Name = "Varian")]
            public int? VariantId { get; set; }

            [Display(Name = "Jenis")]
            public int MovementType { get; set; } = (int)StockMovementType.Masuk;

            [Display(Name = "Jumlah")]
            public int Quantity { get; set; } = 1;

            [StringLength(255, ErrorMessage = "Catatan maksimal 255 karakter")]
            [Display(Name = "Catatan")]
            public string? Notes { get; set; }
        }

        // ================= Mutasi stok
        public class ListMutasi
        {
            public List<MutasiRow> Rows { get; set; } = ProductStockMovement.GetDataList().Select(x => new MutasiRow
            {
                MovementDate = x.MovementDate,
                Produk = x.ProductName + (x.VariantName != null ? " — " + x.VariantName : ""),
                Jenis = EnumHelper.Text(x.MovementType),
                Quantity = x.Quantity,
                Referensi = x.OrderId.HasValue ? "Pesanan #" + x.OrderId : x.ProcurementId.HasValue ? "Pengadaan #" + x.ProcurementId : null,
                Notes = x.Notes
            }).OrderByDescending(x => x.MovementDate).ToList();
        }

        public class MutasiRow
        {
            public DateTime MovementDate { get; set; }
            public string Produk { get; set; } = "";
            public string Jenis { get; set; } = "";
            public int Quantity { get; set; }
            public string? Referensi { get; set; }
            public string? Notes { get; set; }
        }

        // ================= Pesanan
        public class ListPesanan
        {
            public List<PesananRow> Rows { get; set; } = SalesOrder.GetDataList().Select(x => new PesananRow
            {
                EncDecrKey = x.EncDecrKey,
                OrderNumber = x.OrderNumber,
                OrderDate = x.OrderDate.ToDateTime(TimeOnly.MinValue),
                Pembeli = x.BuyerName ?? "",
                BuyerPhone = x.BuyerPhone,
                TotalAmount = x.TotalAmount,
                PaidAmount = x.PaidAmount,
                Bayar = (int)x.PaymentStatus,
                BayarText = EnumHelper.Text(x.PaymentStatus),
                Status = (int)x.FulfillmentStatus,
                StatusText = EnumHelper.Text(x.FulfillmentStatus),
                Rincian = string.Join(", ", x.GetItems(true).Select(i => i.ProductName + (i.VariantName != null ? " " + i.VariantName : "") + " × " + i.Quantity))
            }).OrderByDescending(x => x.OrderDate).ThenByDescending(x => x.OrderNumber).ToList();
        }

        public class PesananRow
        {
            public string EncDecrKey { get; set; } = "";
            public string OrderNumber { get; set; } = "";
            public DateTime OrderDate { get; set; }
            public string Pembeli { get; set; } = "";
            public string? BuyerPhone { get; set; }
            public string Rincian { get; set; } = "";
            public decimal TotalAmount { get; set; }
            public decimal PaidAmount { get; set; }
            public int Bayar { get; set; }
            public string BayarText { get; set; } = "";
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
        }

        public class AddPesanan
        {
            public NewPesanan Row { get; set; } = new() { OrderDate = DateTime.Today };
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public List<GwaVM.OpsiItem> KegiatanOpsi { get; set; } = Opsi.Kegiatan(true);
            public List<ProdukOpsi> ProdukOpsi { get; set; } = OpsiProduk();
        }

        public class NewPesanan
        {
            [Display(Name = "Pembeli (anggota)")]
            public int? BuyerMemberId { get; set; }

            [StringLength(150, ErrorMessage = "Nama pembeli maksimal 150 karakter")]
            [Display(Name = "Nama Pembeli (bukan anggota)")]
            public string? BuyerName { get; set; }

            [StringLength(30, ErrorMessage = "Telepon maksimal 30 karakter")]
            [Display(Name = "Telepon")]
            public string? BuyerPhone { get; set; }

            [Required(ErrorMessage = "Tanggal harus diisi")]
            [Display(Name = "Tanggal Pesan")]
            public DateTime? OrderDate { get; set; }

            [Display(Name = "Kegiatan / Event")]
            public int? ActivityId { get; set; }

            [Display(Name = "PIC")]
            public int? PicMemberId { get; set; }

            [StringLength(500, ErrorMessage = "Catatan maksimal 500 karakter")]
            [Display(Name = "Catatan")]
            public string? Notes { get; set; }

            public List<PesananLine> Items { get; set; } = new();
        }

        public class PesananLine
        {
            // "produkId" atau "produkId:varianId".
            public string Product { get; set; } = "";
            public int Quantity { get; set; } = 1;
        }

        public class Detail
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public string Ringkasan { get; set; } = "";
            public List<SalesOrderItem> Items { get; set; }
            public List<BayarRow> Bayar { get; set; }

            public Detail(string encKey)
            {
                var obj = LibExtensions.Load<SalesOrder>(encKey, "Pesanan");
                Key = obj.EncDecrKey;
                Judul = obj.OrderNumber + " · " + obj.BuyerName;
                Ringkasan = $"Total Rp {obj.TotalAmount:#,##0} · dibayar Rp {obj.PaidAmount:#,##0} · {EnumHelper.Text(obj.PaymentStatus)} · {EnumHelper.Text(obj.FulfillmentStatus)}";
                Items = obj.GetItems(true);
                Bayar = FinancialTransaction.GetDataList(transactionType: CashFlowType.Income, status: TransactionStatus.Posted)
                    .Where(t => t.SalesOrderId == obj.Id)
                    .Select(t => new BayarRow { TransactionId = t.Id, Nomor = t.TransactionNumber, Tanggal = t.TransactionDate.ToDateTime(TimeOnly.MinValue), Akun = t.AccountName, Amount = t.Amount })
                    .ToList();
            }
        }

        public class BayarRow
        {
            public int TransactionId { get; set; }
            public string Nomor { get; set; } = "";
            public DateTime Tanggal { get; set; }
            public string Akun { get; set; } = "";
            public decimal Amount { get; set; }
        }

        public class BatalBayarInput
        {
            public string Key { get; set; } = "";
            public int TransactionId { get; set; }
            public string? Keterangan { get; set; }
        }
    }
}
