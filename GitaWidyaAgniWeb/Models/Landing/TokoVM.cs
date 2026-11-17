using GWALib.Lib.Master.Merchandise;
using GWALib.Lib.Transaksi.Merchandise;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Landing
{
    /// <summary>Produk aktif beserta varian aktifnya (urut SeqNo).</summary>
    public record TokoProduk(Product Data, List<ProductVariant> Varian)
    {
        // Ready stock yang dilacak dan stok habis; produk bervarian dicek per varian.
        public bool Habis => Data.TrackStock == StockTracking.Dilacak && Data.IsPreorder != SaleMode.PreOrder && Varian.Count == 0 && Data.Stock <= 0;
        public bool VarianHabis(ProductVariant v) => Data.TrackStock == StockTracking.Dilacak && Data.IsPreorder != SaleMode.PreOrder && v.Stock <= 0;
        public bool StokTerlihat => Data.TrackStock == StockTracking.Dilacak && Data.IsPreorder != SaleMode.PreOrder;

        /// <summary>Katalog toko: produk aktif, pre-order yang masih dibuka.</summary>
        public static List<TokoProduk> LoadKatalog()
        {
            return Product.GetDataList(FilterAktif.Aktif).OrderBy(x => x.Name)
                .Where(p => p.IsPreorder != SaleMode.PreOrder || p.PreorderCloseAt == null || p.PreorderCloseAt > DateTime.Now)
                .Select(p => new TokoProduk(p, p.VariantCount > 0
                    ? p.GetVariants(true).Where(v => v.IsActive == ActiveStatus.Aktif).OrderBy(v => v.SeqNo).ToList()
                    : new()))
                .ToList();
        }
    }

    /// <summary>Form pesanan tamu. Qty: kunci "{productId}-{variantId}" (variantId 0 = tanpa varian) → jumlah.</summary>
    public class TokoInput
    {
        public string? BuyerName { get; set; }
        public string? BuyerPhone { get; set; }
        public string? Notes { get; set; }
        // Honeypot anti-bot: harus kosong.
        public string? Website { get; set; }
        public Dictionary<string, int> Qty { get; set; } = new();

        public List<SalesOrderItem.NewRow> ToItems()
        {
            var items = new List<SalesOrderItem.NewRow>();
            foreach (var (key, qty) in Qty)
            {
                var parts = key.Split('-');
                if (qty <= 0 || parts.Length != 2 || !int.TryParse(parts[0], out var productId) || !int.TryParse(parts[1], out var variantId))
                {
                    continue;
                }
                items.Add(new SalesOrderItem.NewRow { ProductId = productId, VariantId = variantId > 0 ? variantId : null, Quantity = qty });
            }
            return items;
        }
    }

    public class TokoPageVM
    {
        public List<TokoProduk> Katalog { get; set; } = new();
        public TokoInput Input { get; set; } = new();
        public string? Error { get; set; }
        public SalesOrder? Dibuat { get; set; }
        public string? WhatsAppAdmin { get; set; }
        // Lacak pesanan tamu.
        public string? LacakNomor { get; set; }
        public string? LacakWa { get; set; }
        public bool LacakDicari => !string.IsNullOrWhiteSpace(LacakNomor) || !string.IsNullOrWhiteSpace(LacakWa);
        public SalesOrder? Lacak { get; set; }
        public List<SalesOrderItem> LacakItem { get; set; } = new();
        public bool AnggotaLogin { get; set; }
        public int MaxBaris => SalesOrder.GUEST_MAX_LINES;
        public int MaxQty => SalesOrder.GUEST_MAX_QUANTITY;
    }
}
