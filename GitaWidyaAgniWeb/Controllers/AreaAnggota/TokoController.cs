using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GitaWidyaAgniWeb.Models.Landing;
using GWALib.Lib.Master.Merchandise;
using GWALib.Lib.Transaksi.Merchandise;
using GWAShared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.AreaAnggota
{
    /// <summary>
    /// Area Anggota › Toko (/AreaAnggota/Toko): katalog merchandise, pesan atas nama anggota login, dan riwayat pesanan.
    /// Pembayaran dicatat pengurus (danus); pesanan bisa dibatalkan sendiri selama belum ada pembayaran.
    /// </summary>
    public class TokoController : AreaAnggotaController
    {
        public class Model
        {
            public List<TokoProduk> Katalog { get; set; } = new();
            public List<SalesOrder> Pesanan { get; set; } = new();
            public bool Anggota { get; set; }
        }

        public class PesanInput
        {
            public string? Phone { get; set; }
            public string? Notes { get; set; }
            public List<ItemInput> Items { get; set; } = new();
        }

        public class ItemInput
        {
            public int ProductId { get; set; }
            public int? VariantId { get; set; }
            public int Quantity { get; set; }
        }

        public IActionResult Index()
        {
            var saya = SayaAtauNull();
            var model = new Model
            {
                Anggota = saya != null,
                Katalog = TokoProduk.LoadKatalog(),
                Pesanan = saya == null ? new() : SalesOrder.GetDataList(buyerMemberId: saya.Id).OrderByDescending(x => x.InsDate).Take(20).ToList()
            };
            return HalamanIndex(model);
        }

        [HttpPost]
        public IActionResult Pesan([FromBody] PesanInput m) => Aksi(u =>
        {
            var saya = Saya();
            var order = SalesOrder.Save(new SalesOrder.NewRow
            {
                BuyerMemberId = saya.Id,
                BuyerPhone = string.IsNullOrWhiteSpace(m.Phone) ? saya.Phone : m.Phone,
                OrderDate = DateOnly.FromDateTime(DateTime.Today),
                Notes = m.Notes,
                Items = m.Items.Where(i => i.Quantity > 0)
                    .Select(i => new SalesOrderItem.NewRow { ProductId = i.ProductId, VariantId = i.VariantId, Quantity = i.Quantity }).ToList()
            }, u).Ok();
            return new { order.OrderNumber, order.TotalAmount };
        }, "Pesanan dibuat. Lakukan pembayaran ke pengurus danus.");

        [HttpPost]
        public IActionResult Batal([FromBody] GwaVM.KeyInput i) => Aksi(u =>
        {
            var order = LibExtensions.Load<SalesOrder>(i.Key, "Pesanan");
            if (order.BuyerMemberId != Saya().Id)
            {
                throw new ArgumentException(ACCESS_DENIED_MESSAGE);
            }
            order.Cancel("Dibatalkan pembeli", u).Ok();
        }, "Pesanan dibatalkan.");

        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
        public IActionResult Foto(string key)
        {
            try
            {
                var f = LibExtensions.Load<Product>(key, "Produk").GetPhoto();
                return f == null ? NotFound() : File(f.Data, f.ContentType ?? "image/jpeg");
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
