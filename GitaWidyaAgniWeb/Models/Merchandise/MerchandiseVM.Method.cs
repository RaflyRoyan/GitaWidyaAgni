using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Merchandise;
using GWALib.Lib.Transaksi.Merchandise;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Merchandise
{
    public partial class MerchandiseVM
    {
        public class Method
        {
            public static Product LoadProduk(string? key) => LibExtensions.Load<Product>(key, "Produk");
            public static SalesOrder LoadPesanan(string? key) => LibExtensions.Load<SalesOrder>(key, "Pesanan");

            // ---------- Produk
            public static void SimpanProduk(NewProduk input, byte[]? foto, string? fotoName, string user)
                => Product.Save(new Product.NewRow
                {
                    Name = input.Name ?? "",
                    ProductType = (ProductType)input.ProductType,
                    Description = input.Description,
                    Price = input.Price,
                    CostPrice = input.CostPrice,
                    Unit = string.IsNullOrWhiteSpace(input.Unit) ? "pcs" : input.Unit,
                    TrackStock = (StockTracking)input.TrackStock,
                    Stock = input.Stock,
                    IsPreorder = (SaleMode)input.IsPreorder,
                    PreorderCloseAt = input.PreorderCloseAt,
                    ActivityId = input.ActivityId,
                    Photo = foto,
                    PhotoFileName = fotoName
                }, user).Ok();

            public static void UpdateProduk(string key, NewProduk input, byte[]? foto, string? fotoName, bool hapusFoto, string user)
            {
                var obj = LoadProduk(key);
                obj.Name = input.Name ?? "";
                obj.ProductType = (ProductType)input.ProductType;
                obj.Description = input.Description;
                obj.Price = input.Price;
                obj.CostPrice = input.CostPrice;
                obj.Unit = string.IsNullOrWhiteSpace(input.Unit) ? "pcs" : input.Unit;
                obj.TrackStock = (StockTracking)input.TrackStock;
                obj.IsPreorder = (SaleMode)input.IsPreorder;
                obj.PreorderCloseAt = input.PreorderCloseAt;
                obj.ActivityId = input.ActivityId;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
                if (foto != null)
                {
                    obj.UpdatePhoto(foto, fotoName, user).Ok();
                }
                else if (hapusFoto)
                {
                    obj.UpdatePhoto(null, null, user).Ok();
                }
            }

            public static void TambahVarian(VarianInput input, string user)
            {
                var obj = LoadProduk(input.Key);
                ProductVariant.Save(new ProductVariant.NewRow { ProductId = obj.Id, Name = input.Name ?? "", Price = input.Price, Stock = input.Stock, Sku = input.Sku }, user).Ok();
            }

            public static void HapusVarian(string? key, string user) => LibExtensions.Load<ProductVariant>(key, "Varian").Delete(user).Ok();

            public static void SimpanStok(StokInput input, string user)
            {
                var obj = LoadProduk(input.Key);
                ProductStockMovement.Save(new ProductStockMovement.NewRow
                {
                    ProductId = obj.Id,
                    VariantId = input.VariantId,
                    MovementType = (StockMovementType)input.MovementType,
                    Quantity = input.Quantity,
                    Notes = input.Notes
                }, user).Ok();
            }

            // ---------- Pesanan
            public static void SimpanPesanan(NewPesanan input, string user)
            {
                var items = new List<SalesOrderItem.NewRow>();
                foreach (var l in (input.Items ?? new()).Where(x => !string.IsNullOrWhiteSpace(x.Product) && x.Quantity > 0))
                {
                    var parts = l.Product.Split(':');
                    items.Add(new SalesOrderItem.NewRow
                    {
                        ProductId = int.TryParse(parts[0], out var pid) ? pid : 0,
                        VariantId = parts.Length > 1 && int.TryParse(parts[1], out var vid) ? vid : null,
                        Quantity = l.Quantity
                    });
                }
                SalesOrder.Save(new SalesOrder.NewRow
                {
                    BuyerMemberId = input.BuyerMemberId,
                    BuyerName = input.BuyerName,
                    BuyerPhone = input.BuyerPhone,
                    OrderDate = Opsi.TanggalWajib(input.OrderDate, "Tanggal pesan"),
                    ActivityId = input.ActivityId,
                    PicMemberId = input.PicMemberId,
                    Notes = input.Notes,
                    Items = items
                }, user).Ok();
            }

            public static GwaVM.BayarForm FormBayar(string encKey)
            {
                var obj = LoadPesanan(encKey);
                return new GwaVM.BayarForm
                {
                    Judul = obj.OrderNumber + " · " + obj.BuyerName,
                    Info = $"Sisa Rp {Math.Max(0, obj.TotalAmount - obj.PaidAmount):#,##0}. Dicatat sebagai transaksi Pemasukan.",
                    OnSave = "Merchandise.pay()",
                    Tombol = "Catat Pembayaran",
                    KategoriOpsi = Opsi.Kategori(CashFlowType.Income),
                    Row = new GwaVM.BayarInput { Key = obj.EncDecrKey, Amount = Math.Max(0, obj.TotalAmount - obj.PaidAmount) }
                };
            }

            public static void Bayar(GwaVM.BayarInput input, string user)
                => LoadPesanan(input.Key).ReceivePayment(input.AccountId ?? 0, input.CategoryId ?? 0, input.Amount ?? 0, Opsi.TanggalWajib(input.Date, "Tanggal"), user).Ok();

            public static void BatalBayar(BatalBayarInput input, string user) => LoadPesanan(input.Key).CancelPayment(input.TransactionId, input.Keterangan, user).Ok();
        }
    }
}
