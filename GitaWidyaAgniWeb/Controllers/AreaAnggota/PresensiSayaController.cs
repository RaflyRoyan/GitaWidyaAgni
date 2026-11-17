using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Presensi;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.AreaAnggota
{
    /// <summary>
    /// Area Anggota › Presensi (/AreaAnggota/PresensiSaya): scan QR sesi, foto selfie, dan lokasi untuk check-in / check-out; riwayat presensi sendiri.
    /// Sesi harus Dibuka dan dalam jendela waktu; lokasi dicek bila sesi punya titik dan radius.
    /// </summary>
    public class PresensiSayaController : AreaAnggotaController
    {
        public record Riwayat(string Sesi, DateTime OpenAt, AttendanceRecord Record);

        public IActionResult Index()
        {
            var saya = SayaAtauNull();
            if (saya == null)
            {
                return HalamanIndex(null);
            }
            var records = AttendanceRecord.GetDataList(memberId: saya.Id, jmldata: 50, startdata: 0);
            var ids = records.Select(r => r.SessionId).ToHashSet();
            var sesi = AttendanceSession.GetDataList().Where(s => ids.Contains(s.Id)).ToDictionary(s => s.Id);
            var rows = records
                .Select(r => sesi.TryGetValue(r.SessionId, out var s) ? new Riwayat(s.Name, s.OpenAt, r) : new Riwayat("Sesi #" + r.SessionId, r.InsDate, r))
                .OrderByDescending(x => x.OpenAt).ToList();
            return HalamanIndex(rows);
        }

        [HttpPost]
        public async Task<IActionResult> Kirim([FromForm] string? token, [FromForm] decimal? lat, [FromForm] decimal? lng, [FromForm] bool keluar, IFormFile? foto)
        {
            var bytes = await BacaBerkasAsync(foto);
            return Aksi(u =>
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new ArgumentException("Scan QR sesi atau isi kodenya.");
                }
                if (bytes == null)
                {
                    throw new ArgumentException("Ambil foto selfie dulu.");
                }
                var row = new AttendanceRecord.CheckRow { QrToken = token.Trim(), Latitude = lat, Longitude = lng, Photo = bytes };
                var rec = keluar ? AttendanceRecord.CheckOut(row, u).Ok() : AttendanceRecord.CheckIn(row, u).Ok();
                return EnumHelper.Text(rec.Status);
            }, keluar ? "Check-out tercatat." : "Check-in tercatat.");
        }
    }
}
