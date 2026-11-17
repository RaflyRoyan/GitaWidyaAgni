using GWALib.Lib.Transaksi.Keuangan;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.AreaAnggota
{
    /// <summary>Area Anggota › Iuran Saya (/AreaAnggota/IuranSaya): tagihan iuran milik anggota login (hanya baca; pembayaran dicatat bendahara).</summary>
    public class IuranSayaController : AreaAnggotaController
    {
        public IActionResult Index()
        {
            var saya = SayaAtauNull();
            var rows = saya == null ? null : DuesBill.GetDataList(memberId: saya.Id).OrderByDescending(x => x.BillPeriod).ToList();
            return HalamanIndex(rows);
        }
    }
}
