using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Models;
using GWALib.Lib.Transaksi.Presensi;
using GWALib.Lib.Transaksi.Program;
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

        public class IzinInput
        {
            public int Kegiatan { get; set; }
            public int Sesi { get; set; }
            public string? Alasan { get; set; }
        }

        // Ajukan izin tidak hadir satu sesi langsung dari kartu sesi. GWALib memeriksa sesi milik kegiatan, anggota termasuk audiens,
        // sesi belum ditutup, belum presensi, dan tidak ada pengajuan ganda; anggota diambil dari user login.
        [HttpPost]
        public IActionResult AjukanIzin([FromBody] IzinInput m) => Aksi(u =>
        {
            if (m.Kegiatan <= 0 || m.Sesi <= 0)
            {
                throw new ArgumentException("Sesi tidak valid.");
            }
            ActivityAbsenceRequest.Submit(new ActivityAbsenceRequest.NewRow { ActivityId = m.Kegiatan, SessionId = m.Sesi, Reason = m.Alasan ?? "" }, u).Ok();
        }, "Izin terkirim. Tunggu persetujuan pengurus.");

        // Tarik pengajuan yang masih Menunggu (Withdraw memeriksa pemiliknya).
        [HttpPost]
        public IActionResult TarikIzin([FromBody] GwaVM.KeyInput i)
            => Aksi(u => { LibExtensions.Load<ActivityAbsenceRequest>(i.Key, "Pengajuan").Withdraw(u).Ok(); }, "Pengajuan izin ditarik.");
    }
}
