using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Presensi
{
    /// <summary>
    /// Sesi presensi kegiatan (t_attendance_session), dibuat manual atau dari jadwal rutin (ScheduleId).
    /// Alur status: Scheduled → Open (QrToken dibuat) → Closed; Scheduled / Open → Cancelled (CancelReason).
    /// Close mencatat ALPA untuk anggota sasaran (sesuai audiens kegiatan) yang belum punya catatan presensi.
    /// Sesi dibuka otomatis EARLY_CHECKIN_MINUTE menit sebelum OpenAt dan ditutup otomatis setelah CloseAt (Synchronize, dipanggil job);
    /// mode TanpaQr = anggota check-in dari daftar sesi (GetMyToday), mode Qr = wajib scan QR. Titik lokasi + radius wajib.
    /// Kegiatan non-rutin (bukan Latihan / Piket) yang punya koordinat otomatis mendapat satu sesi dari jam mulai s.d. selesai.
    /// </summary>
    public partial class AttendanceSession : BaseLib
    {
        public class NewRow
        {
            public int ActivityId { get; set; }
            public string Name { get; set; } = null!;
            public DateTime OpenAt { get; set; }
            public DateTime CloseAt { get; set; }
            public int LateToleranceMinute { get; set; } = 10;
            public AttendanceMode AttendanceMode { get; set; } = AttendanceMode.TanpaQr;
            public string? Location { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public int? RadiusMeter { get; set; }
        }
        // Hasil Synchronize (job): sesi yang dibuka, ditutup, jumlah ALPA baru, dan anggota yang dikirimi pengingat.
        public record SyncResult(int Opened, int Closed, int Absent, int Reminded);
        // Pantauan langsung satu sesi (pengurus): satu baris per anggota sasaran / yang punya catatan. Distance = jarak check-in ke titik (m).
        public record MonitorRow(int MemberId, string Name, string? MemberNumber, string? Section, bool Expected, int? RecordId, string? RecordKey,
            AttendanceStatus? Status, DateTime? CheckInAt, DateTime? CheckOutAt, int? Distance, string? Notes);
        public record MonitorData(int SessionId, string Name, string Activity, DateTime OpenAt, DateTime CloseAt, AttendanceSessionStatus Status,
            AttendanceMode Mode, int? RadiusMeter, DateTime GeneratedAt, List<MonitorRow> Rows)
        {
            public int Present => Rows.Count(x => x.Status == AttendanceStatus.Hadir || x.Status == AttendanceStatus.Terlambat);
            public int Late => Rows.Count(x => x.Status == AttendanceStatus.Terlambat);
            public int Excused => Rows.Count(x => x.Status == AttendanceStatus.Izin || x.Status == AttendanceStatus.Sakit);
            public int Absent => Rows.Count(x => x.Status == AttendanceStatus.Alpa);
            public int NotYet => Rows.Count(x => x.Status == null);
        }
        // Rekap kehadiran per anggota dari sesi yang sudah ditutup (laporan).
        public record RecapRow(int MemberId, string Name, string? MemberNumber, string? Section, int Hadir, int Terlambat, int Izin, int Sakit, int Alpa)
        {
            public int Sessions => Hadir + Terlambat + Izin + Sakit + Alpa;
            public decimal Percent => Sessions == 0 ? 0 : Math.Round(100m * (Hadir + Terlambat) / Sessions, 1);
        }
        // Satu sesi di jadwal anggota yang login (Area Anggota › Presensi). CanCheckIn / CanCheckOut = tombol yang boleh tampil sekarang;
        // CheckInFrom = awal jendela check-in (OpenAt - EARLY_CHECKIN_MINUTE).
        // Izin: CanRequestAbsence = tombol "Ajukan izin" boleh tampil (sesi belum selesai, belum presensi, kegiatan bukan Peserta Terdaftar,
        // tidak ada pengajuan Menunggu / Disetujui); AbsenceStatus / AbsenceWhole / AbsenceKey = pengajuan anggota untuk sesi ini
        // atau seluruh kegiatan (AbsenceKey = encKey ActivityAbsenceRequest, untuk menarik pengajuan Menunggu).
        public record MySession(int Id, string Name, string Activity, DateTime OpenAt, DateTime CloseAt, int LateToleranceMinute, string? Location,
            AttendanceMode Mode, decimal? Latitude, decimal? Longitude, int? RadiusMeter, AttendanceSessionStatus SessionStatus,
            DateTime CheckInFrom, bool CanCheckIn, bool CanCheckOut, DateTime? CheckInAt, DateTime? CheckOutAt, AttendanceStatus? RecordStatus,
            int ActivityId = 0, bool CanRequestAbsence = false, AbsenceRequestStatus? AbsenceStatus = null, bool AbsenceWhole = false,
            string? AbsenceKey = null, string? AbsenceNote = null);
        // Jadwal presensi anggota: sesi hari ini, sesi mendatang, riwayat (sesi lampau yang punya catatan presensi),
        // dan kehadiran per bulan (MONTHS_MINE bulan terakhir, sesi Ditutup).
        public record MyAttendance(List<MySession> Today, List<MySession> Upcoming, List<MySession> History, List<MonthStat> Monthly);

        // Statistik keaktifan dari sesi Ditutup: Percent = (Hadir + Terlambat) / semua catatan.
        public record MonthStat(int Year, int Month, int Hadir, int Terlambat, int Izin, int Sakit, int Alpa)
        {
            public int Sessions => Hadir + Terlambat + Izin + Sakit + Alpa;
            public decimal Percent => Sessions == 0 ? 0 : Math.Round(100m * (Hadir + Terlambat) / Sessions, 1);
        }
        public record GroupStat(string Name, int Members, int Hadir, int Terlambat, int Izin, int Sakit, int Alpa)
        {
            public int Sessions => Hadir + Terlambat + Izin + Sakit + Alpa;
            public decimal Percent => Sessions == 0 ? 0 : Math.Round(100m * (Hadir + Terlambat) / Sessions, 1);
        }
        // Months = tren per bulan (bulan tanpa sesi tetap ada, nol); Sections = per section saat ini;
        // LowMembers = anggota dengan minimal LOW_MIN_SESSIONS catatan dan kehadiran di bawah LOW_PERCENT, terendah dulu.
        public record ActivityStats(DateOnly From, DateOnly To, List<MonthStat> Months, List<GroupStat> Sections, List<RecapRow> Members,
            List<RecapRow> LowMembers)
        {
            public int Hadir => Months.Sum(x => x.Hadir);
            public int Terlambat => Months.Sum(x => x.Terlambat);
            public int Izin => Months.Sum(x => x.Izin);
            public int Sakit => Months.Sum(x => x.Sakit);
            public int Alpa => Months.Sum(x => x.Alpa);
            public int Sessions => Hadir + Terlambat + Izin + Sakit + Alpa;
            public decimal Percent => Sessions == 0 ? 0 : Math.Round(100m * (Hadir + Terlambat) / Sessions, 1);
        }

        public int Id { get; set; }
        public int ActivityId { get; private set; }
        public int? ScheduleId { get; private set; }
        public string Name { get; set; } = null!;
        public DateTime OpenAt { get; set; }
        public DateTime CloseAt { get; set; }
        public int LateToleranceMinute { get; set; }
        public AttendanceMode AttendanceMode { get; set; }
        public string? Location { get; set; }
        // Isi QR yang dipindai anggota; hanya ada saat sesi dibuka. Tidak ikut diserialisasi ke daftar umum.
        [JsonIgnore]
        public string? QrToken { get; private set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? RadiusMeter { get; set; }
        // Hanya-baca: diubah lewat Open / Close / Cancel.
        public AttendanceSessionStatus Status { get; private set; }
        public string? CancelReason { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TAttendanceSession> _Querys
        {
            get
            {
                return DBClass.GetContext().TAttendanceSessions.AsQueryable();
            }
        }
        public AttendanceSession()
        {
        }
        public AttendanceSession(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<AttendanceSession>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal AttendanceSession(EF.TAttendanceSession row)
        {
            SetData(row);
        }
        private void SetData(EF.TAttendanceSession row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<AttendanceSession>();
            ActivityId = row.ActivityId;
            ScheduleId = row.ScheduleId;
            Name = row.Name;
            OpenAt = row.OpenAt;
            CloseAt = row.CloseAt;
            LateToleranceMinute = row.LateToleranceMinute;
            AttendanceMode = (AttendanceMode)row.AttendanceMode;
            Location = row.Location;
            QrToken = row.QrToken;
            Latitude = row.Latitude;
            Longitude = row.Longitude;
            RadiusMeter = row.RadiusMeter;
            Status = (AttendanceSessionStatus)row.Status;
            CancelReason = row.CancelReason;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
