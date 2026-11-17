using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GWALib.Lib.Master.Inventaris;
using GWALib.Lib.Transaksi.Inventaris;
using GWAShared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.AreaAnggota
{
    /// <summary>
    /// Area Anggota › Peminjaman (/AreaAnggota/PeminjamanSaya): ajukan peminjaman aset atas nama anggota login dan pantau statusnya.
    /// Persetujuan, serah terima, dan pengembalian dicatat pengurus inventaris.
    /// </summary>
    public class PeminjamanSayaController : AreaAnggotaController
    {
        public class Model
        {
            public List<BorrowRequest> Riwayat { get; set; } = new();
            public List<Asset> Aset { get; set; } = new();
        }

        public class AjukanInput
        {
            public string? Purpose { get; set; }
            public DateTime? BorrowDate { get; set; }
            public DateTime? ExpectedReturnDate { get; set; }
            public int RentalScheme { get; set; } = (int)GWAShared.Enums.RentalScheme.Gratis;
            public string? Notes { get; set; }
            public List<BarisInput> Items { get; set; } = new();
        }

        public class BarisInput
        {
            public int AssetId { get; set; }
            public int Quantity { get; set; }
        }

        public IActionResult Index()
        {
            var saya = SayaAtauNull();
            if (saya == null)
            {
                return HalamanIndex(null);
            }
            return HalamanIndex(new Model
            {
                Riwayat = BorrowRequest.GetDataList(memberId: saya.Id).OrderByDescending(x => x.InsDate).Take(30).ToList(),
                Aset = Asset.GetDataList(AssetStatus.Available).OrderBy(x => x.Name).ToList()
            });
        }

        [HttpPost]
        public IActionResult Ajukan([FromBody] AjukanInput m) => Aksi(u =>
        {
            var saya = Saya();
            var req = BorrowRequest.Save(new BorrowRequest.NewRow
            {
                BorrowerType = BorrowerType.Anggota,
                MemberId = saya.Id,
                BorrowerPhone = saya.Phone,
                BorrowerEmail = saya.Email,
                Purpose = m.Purpose ?? "",
                BorrowDate = m.BorrowDate ?? DateTime.MinValue,
                ExpectedReturnDate = m.ExpectedReturnDate ?? DateTime.MinValue,
                RentalScheme = (RentalScheme)m.RentalScheme,
                Notes = m.Notes,
                Items = m.Items.Where(i => i.Quantity > 0).Select(i => new BorrowItem.NewRow { AssetId = i.AssetId, Quantity = i.Quantity }).ToList()
            }, u).Ok();
            return req.RequestNumber;
        }, "Pengajuan peminjaman terkirim. Tunggu persetujuan pengurus.");

        [HttpPost]
        public IActionResult Batal([FromBody] GwaVM.KeyInput i) => Aksi(u =>
        {
            var req = LibExtensions.Load<BorrowRequest>(i.Key, "Peminjaman");
            if (req.MemberId != Saya().Id)
            {
                throw new ArgumentException(ACCESS_DENIED_MESSAGE);
            }
            req.Cancel("Dibatalkan peminjam", u).Ok();
        }, "Pengajuan dibatalkan.");
    }
}
