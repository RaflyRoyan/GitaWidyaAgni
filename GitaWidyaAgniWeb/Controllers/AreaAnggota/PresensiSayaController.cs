using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Presensi;
using Microsoft.AspNetCore.Mvc;

namespace GitaWidyaAgniWeb.Controllers.AreaAnggota
{
    /// <summary>
    /// Area Anggota › Presensi (/AreaAnggota/PresensiSaya), pola absensi aplikasi lapangan: timeline sesi hari ini, jadwal mendatang,
    /// riwayat. Check-in / check-out lewat modal kamera langsung (foto selfie) + lokasi otomatis; sesi mode Wajib Scan QR memindai QR dulu.
    /// Check-in dibuka 30 menit sebelum sesi mulai s.d. sesi selesai; lokasi dicek bila sesi punya titik dan radius.
    /// </summary>
    public class PresensiSayaController : AreaAnggotaController
    {
        public IActionResult Index()
        {
            if (SayaAtauNull() == null)
            {
                return HalamanIndex(null);
            }
            return HalamanIndex(AttendanceSession.GetMyAttendance(LibUser));
        }

        // Sesi dipilih dari jadwal (sesi, mode Tanpa QR) atau dari hasil scan QR (token).
        [HttpPost]
        public async Task<IActionResult> Kirim([FromForm] string? token, [FromForm] int? sesi, [FromForm] decimal? lat, [FromForm] decimal? lng, [FromForm] bool keluar, IFormFile? foto)
        {
            var bytes = await BacaBerkasAsync(foto);
            return Aksi(u =>
            {
                if (string.IsNullOrWhiteSpace(token) && !(sesi > 0))
                {
                    throw new ArgumentException("Pilih sesi atau scan QR sesi.");
                }
                if (bytes == null)
                {
                    throw new ArgumentException("Ambil foto selfie dulu.");
                }
                var row = new AttendanceRecord.CheckRow
                {
                    QrToken = string.IsNullOrWhiteSpace(token) ? null : token.Trim(),
                    SessionId = string.IsNullOrWhiteSpace(token) ? sesi : null,
                    Latitude = lat,
                    Longitude = lng,
                    Photo = bytes
                };
                var rec = keluar ? AttendanceRecord.CheckOut(row, u).Ok() : AttendanceRecord.CheckIn(row, u).Ok();
                return EnumHelper.Text(rec.Status);
            }, keluar ? "Check-out tercatat." : "Check-in tercatat.");
        }
    }
}
