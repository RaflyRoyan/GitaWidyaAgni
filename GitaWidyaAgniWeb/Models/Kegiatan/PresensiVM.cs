using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Presensi;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Kegiatan
{
    /// <summary>
    /// Kegiatan › Presensi: tab Sesi (t_attendance_session: buka dengan QR / tutup / batal + rekap & presensi manual),
    /// tab Jadwal Rutin (t_activity_schedule + generate sesi), tab Notulen (t_meeting_minutes).
    /// Anggota check-in / check-out dari Area Anggota › Presensi (scan QR + foto + lokasi).
    /// </summary>
    public partial class PresensiVM
    {
        public class Index
        {
            // Pilihan untuk cetak rekap kehadiran.
            public List<GwaVM.OpsiItem> KegiatanOpsi { get; set; } = Opsi.Kegiatan();
            public List<GwaVM.OpsiItem> SectionOpsi { get; set; } = Opsi.Section();
        }

        // Pantauan langsung satu sesi (dimuat ulang berkala oleh Presensi.js).
        public class Pantau
        {
            public string Key { get; set; } = "";
            public AttendanceSession.MonitorData Data { get; set; }

            public Pantau(string encKey)
            {
                var obj = LibExtensions.Load<AttendanceSession>(encKey, "Sesi presensi");
                Key = obj.EncDecrKey;
                Data = obj.GetMonitor();
            }
        }

        // ================= Sesi
        public class ListSesi
        {
            public List<SesiRow> Rows { get; set; } = Method.GetSesiList().ToList();
        }

        public class SesiRow
        {
            public string EncDecrKey { get; set; } = "";
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Kegiatan { get; set; } = "";
            public DateTime OpenAt { get; set; }
            public DateTime CloseAt { get; set; }
            public string? Location { get; set; }
            public bool Geofence { get; set; }
            public bool DariJadwal { get; set; }
            public string ModeText { get; set; } = "";
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public int Hadir { get; set; }
            public int TidakHadir { get; set; }
            public bool AdaNotulen { get; set; }
        }

        public class AddSesi
        {
            public NewSesi Row { get; set; } = new();
            public List<GwaVM.OpsiItem> KegiatanOpsi { get; set; } = Opsi.Kegiatan(true);
            public List<GwaVM.OpsiItem> ModeOpsi { get; set; } = GwaVM.Opsi<AttendanceMode>();
            public Dictionary<int, decimal[]> KoordinatKegiatan { get; set; } = Opsi.KoordinatKegiatan();

            public AddSesi()
            {
                var start = DateTime.Today.AddHours(DateTime.Now.Hour + 1);
                Row.OpenAt = start;
                Row.CloseAt = start.AddHours(2);
            }
        }

        public class EditSesi : AddSesi
        {
            public string Kegiatan { get; set; } = "";
            public EditSesi(string encKey)
            {
                var obj = LibExtensions.Load<AttendanceSession>(encKey, "Sesi presensi");
                Kegiatan = KegiatanOpsi.FirstOrDefault(x => x.Value == obj.ActivityId)?.Text ?? Opsi.Kegiatan().FirstOrDefault(x => x.Value == obj.ActivityId)?.Text ?? "";
                Row = new NewSesi
                {
                    ActivityId = obj.ActivityId,
                    Name = obj.Name,
                    OpenAt = obj.OpenAt,
                    CloseAt = obj.CloseAt,
                    LateToleranceMinute = obj.LateToleranceMinute,
                    AttendanceMode = (int)obj.AttendanceMode,
                    Location = obj.Location,
                    Latitude = obj.Latitude,
                    Longitude = obj.Longitude,
                    RadiusMeter = obj.RadiusMeter
                };
            }
        }

        public class NewSesi
        {
            [Required(ErrorMessage = "Pilih kegiatan")]
            [Display(Name = "Kegiatan")]
            public int? ActivityId { get; set; }

            [Required(ErrorMessage = "Nama sesi harus diisi")]
            [StringLength(150, ErrorMessage = "Nama maksimal 150 karakter")]
            [Display(Name = "Nama Sesi")]
            public string? Name { get; set; }

            [Required(ErrorMessage = "Waktu buka harus diisi")]
            [Display(Name = "Dibuka")]
            public DateTime? OpenAt { get; set; }

            [Required(ErrorMessage = "Waktu tutup harus diisi")]
            [Display(Name = "Ditutup")]
            public DateTime? CloseAt { get; set; }

            [Range(0, 240, ErrorMessage = "Toleransi 0 - 240 menit")]
            [Display(Name = "Toleransi Terlambat (menit)")]
            public int LateToleranceMinute { get; set; } = 10;

            [Display(Name = "Mode Presensi")]
            public int AttendanceMode { get; set; } = (int)GWAShared.Enums.AttendanceMode.TanpaQr;

            [StringLength(255, ErrorMessage = "Lokasi maksimal 255 karakter")]
            [Display(Name = "Lokasi")]
            public string? Location { get; set; }

            [Required(ErrorMessage = "Latitude harus diisi")]
            [Range(-90, 90, ErrorMessage = "Latitude -90 sampai 90")]
            [Display(Name = "Latitude")]
            public decimal? Latitude { get; set; }

            [Required(ErrorMessage = "Longitude harus diisi")]
            [Range(-180, 180, ErrorMessage = "Longitude -180 sampai 180")]
            [Display(Name = "Longitude")]
            public decimal? Longitude { get; set; }

            [Required(ErrorMessage = "Radius harus diisi")]
            [Range(10, 100000, ErrorMessage = "Radius 10 - 100000 meter")]
            [Display(Name = "Radius (meter)")]
            public int? RadiusMeter { get; set; }
        }

        // QR sesi yang sedang dibuka.
        public class Qr
        {
            public string Key { get; set; } = "";
            public string Nama { get; set; } = "";
            public string Kegiatan { get; set; } = "";
            public string Token { get; set; } = "";
            public DateTime CloseAt { get; set; }

            public Qr(string encKey)
            {
                var obj = LibExtensions.Load<AttendanceSession>(encKey, "Sesi presensi");
                if (obj.Status != AttendanceSessionStatus.Open || string.IsNullOrEmpty(obj.QrToken))
                {
                    throw new ArgumentException("Sesi belum dibuka.");
                }
                Key = obj.EncDecrKey;
                Nama = obj.Name;
                Kegiatan = Opsi.Kegiatan().FirstOrDefault(x => x.Value == obj.ActivityId)?.Text ?? "";
                Token = obj.QrToken;
                CloseAt = obj.CloseAt;
            }
        }

        // Rekap kehadiran satu sesi + presensi manual.
        public class Rekap
        {
            public string Key { get; set; } = "";
            public string Nama { get; set; } = "";
            public int Status { get; set; }
            public ManualInput Input { get; set; } = new();
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public List<GwaVM.OpsiItem> StatusOpsi { get; set; } = GwaVM.Opsi<AttendanceStatus>();
            public RekapList List { get; set; }

            public Rekap(string encKey)
            {
                var obj = LibExtensions.Load<AttendanceSession>(encKey, "Sesi presensi");
                Key = obj.EncDecrKey;
                Nama = obj.Name;
                Status = (int)obj.Status;
                List = new RekapList(obj.Id, obj.EncDecrKey);
            }
        }

        public class RekapList
        {
            public string Key { get; set; } = "";
            public List<RekapRow> Rows { get; set; }

            public RekapList(int sessionId, string key)
            {
                Key = key;
                Rows = AttendanceRecord.GetDataList(sessionId).Select(x => new RekapRow
                {
                    EncDecrKey = x.EncDecrKey,
                    MemberId = x.MemberId,
                    Nama = x.MemberName,
                    CheckInAt = x.CheckInAt,
                    CheckOutAt = x.CheckOutAt,
                    StatusText = EnumHelper.Text(x.Status),
                    Notes = x.Notes
                }).OrderBy(x => x.Nama).ToList();
            }
            public RekapList(string encKey) : this(LibExtensions.Load<AttendanceSession>(encKey, "Sesi presensi").Id, encKey)
            {
            }
        }

        public class RekapRow
        {
            public string EncDecrKey { get; set; } = "";
            public int MemberId { get; set; }
            public string Nama { get; set; } = "";
            public DateTime? CheckInAt { get; set; }
            public DateTime? CheckOutAt { get; set; }
            public string StatusText { get; set; } = "";
            public string? Notes { get; set; }
            public bool AdaFoto => CheckInAt.HasValue;
        }

        public class ManualInput
        {
            public string Key { get; set; } = "";

            [Required(ErrorMessage = "Pilih anggota")]
            [Display(Name = "Anggota")]
            public int? MemberId { get; set; }

            [Display(Name = "Status")]
            public int Status { get; set; } = (int)AttendanceStatus.Izin;

            [StringLength(255, ErrorMessage = "Keterangan maksimal 255 karakter")]
            [Display(Name = "Keterangan")]
            public string? Notes { get; set; }
        }

        // ================= Jadwal rutin
        public class ListJadwal
        {
            public List<JadwalRow> Rows { get; set; } = Method.GetJadwalList().ToList();
        }

        public class JadwalRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Kegiatan { get; set; } = "";
            public int Hari { get; set; }
            public string HariText { get; set; } = "";
            public string Jam { get; set; } = "";
            public string? Location { get; set; }
            public DateTime ValidFrom { get; set; }
            public DateTime? ValidUntil { get; set; }
            public int LateToleranceMinute { get; set; }
            public string ModeText { get; set; } = "";
            public bool Aktif { get; set; }
        }

        public class AddJadwal
        {
            public NewJadwal Row { get; set; } = new();
            public List<GwaVM.OpsiItem> KegiatanOpsi { get; set; } = Opsi.Kegiatan(true);
            public List<GwaVM.OpsiItem> ModeOpsi { get; set; } = GwaVM.Opsi<AttendanceMode>();
            public Dictionary<int, decimal[]> KoordinatKegiatan { get; set; } = Opsi.KoordinatKegiatan();
            public List<GwaVM.OpsiItem> HariOpsi { get; set; } = GwaVM.Opsi<ScheduleDay>();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;

            public AddJadwal()
            {
                Row.ValidFrom = DateTime.Today;
                Row.StartTime = DateTime.Today.AddHours(16);
                Row.EndTime = DateTime.Today.AddHours(18);
            }
        }

        public class EditJadwal : AddJadwal
        {
            public EditJadwal(string encKey)
            {
                var obj = LibExtensions.Load<ActivitySchedule>(encKey, "Jadwal");
                Row = new NewJadwal
                {
                    ActivityId = obj.ActivityId,
                    DayOfWeek = (int)obj.DayOfWeek,
                    StartTime = DateTime.Today.Add(obj.StartTime.ToTimeSpan()),
                    EndTime = DateTime.Today.Add(obj.EndTime.ToTimeSpan()),
                    Location = obj.Location,
                    Latitude = obj.Latitude,
                    Longitude = obj.Longitude,
                    RadiusMeter = obj.RadiusMeter,
                    LateToleranceMinute = obj.LateToleranceMinute,
                    AttendanceMode = (int)obj.AttendanceMode,
                    ValidFrom = Opsi.KeDateTime(obj.ValidFrom),
                    ValidUntil = Opsi.KeDateTime(obj.ValidUntil),
                    IsActive = (int)obj.IsActive
                };
            }
        }

        public class NewJadwal
        {
            [Required(ErrorMessage = "Pilih kegiatan")]
            [Display(Name = "Kegiatan")]
            public int? ActivityId { get; set; }

            [Display(Name = "Hari")]
            public int DayOfWeek { get; set; } = (int)ScheduleDay.Sabtu;

            [Required(ErrorMessage = "Jam mulai harus diisi")]
            [Display(Name = "Mulai")]
            public DateTime? StartTime { get; set; }

            [Required(ErrorMessage = "Jam selesai harus diisi")]
            [Display(Name = "Selesai")]
            public DateTime? EndTime { get; set; }

            [StringLength(255, ErrorMessage = "Lokasi maksimal 255 karakter")]
            [Display(Name = "Lokasi")]
            public string? Location { get; set; }

            [Required(ErrorMessage = "Latitude harus diisi")]
            [Range(-90, 90, ErrorMessage = "Latitude -90 sampai 90")]
            [Display(Name = "Latitude")]
            public decimal? Latitude { get; set; }

            [Required(ErrorMessage = "Longitude harus diisi")]
            [Range(-180, 180, ErrorMessage = "Longitude -180 sampai 180")]
            [Display(Name = "Longitude")]
            public decimal? Longitude { get; set; }

            [Required(ErrorMessage = "Radius harus diisi")]
            [Range(10, 100000, ErrorMessage = "Radius 10 - 100000 meter")]
            [Display(Name = "Radius (meter)")]
            public int? RadiusMeter { get; set; }

            [Range(0, 240, ErrorMessage = "Toleransi 0 - 240 menit")]
            [Display(Name = "Toleransi Terlambat (menit)")]
            public int LateToleranceMinute { get; set; } = 10;

            [Display(Name = "Mode Presensi")]
            public int AttendanceMode { get; set; } = (int)GWAShared.Enums.AttendanceMode.TanpaQr;

            [Required(ErrorMessage = "Berlaku mulai harus diisi")]
            [Display(Name = "Berlaku Mulai")]
            public DateTime? ValidFrom { get; set; }

            [Display(Name = "Berlaku Sampai")]
            public DateTime? ValidUntil { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        public class GenerateInput
        {
            public DateTime? From { get; set; }
            public DateTime? To { get; set; }
            public int? ActivityId { get; set; }
        }

        // ================= Notulen
        public class ListNotulen
        {
            public List<NotulenRow> Rows { get; set; } = Method.GetNotulenList().ToList();
        }

        public class NotulenRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Sesi { get; set; } = "";
            public DateTime? Tanggal { get; set; }
            public string? Agenda { get; set; }
            public string? Pimpinan { get; set; }
            public string? Notulis { get; set; }
            public string? FileName { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public DateTime? FinalizedAt { get; set; }
        }

        public class AddNotulen
        {
            public NewNotulen Row { get; set; } = new();
            public List<GwaVM.OpsiItem> SesiOpsi { get; set; }
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public string? FileName { get; set; }

            public AddNotulen()
            {
                // Sesi Dibuka / Ditutup yang belum punya notulen.
                var sudah = MeetingMinutes.GetDataList().Select(x => x.SessionId).ToHashSet();
                SesiOpsi = AttendanceSession.GetDataList()
                    .Where(x => (x.Status == AttendanceSessionStatus.Open || x.Status == AttendanceSessionStatus.Closed) && !sudah.Contains(x.Id))
                    .OrderByDescending(x => x.OpenAt)
                    .Select(x => new GwaVM.OpsiItem(x.Id, x.Name + " · " + x.OpenAt.ToString("dd MMM yyyy HH:mm"))).ToList();
            }
        }

        public class EditNotulen : AddNotulen
        {
            public string Sesi { get; set; } = "";
            public EditNotulen(string encKey)
            {
                var obj = LibExtensions.Load<MeetingMinutes>(encKey, "Notulen");
                if (obj.Status == MeetingMinuteStatus.Final)
                {
                    throw new ArgumentException("Notulen Final tidak bisa diubah.");
                }
                Sesi = AttendanceSession.GetDataList().FirstOrDefault(x => x.Id == obj.SessionId)?.Name ?? "";
                FileName = obj.FileName;
                Row = new NewNotulen
                {
                    SessionId = obj.SessionId,
                    ChairedByMemberId = obj.ChairedByMemberId,
                    NotulisMemberId = obj.NotulisMemberId,
                    Agenda = obj.Agenda,
                    Content = obj.Content,
                    Decisions = obj.Decisions,
                    FollowUp = obj.FollowUp
                };
            }
        }

        public class NewNotulen
        {
            [Required(ErrorMessage = "Pilih sesi")]
            [Display(Name = "Sesi / Rapat")]
            public int? SessionId { get; set; }

            [Display(Name = "Pimpinan Rapat")]
            public int? ChairedByMemberId { get; set; }

            [Display(Name = "Notulis")]
            public int? NotulisMemberId { get; set; }

            [Display(Name = "Agenda")]
            public string? Agenda { get; set; }

            [Required(ErrorMessage = "Isi notulen harus diisi")]
            [Display(Name = "Isi / Pembahasan")]
            public string? Content { get; set; }

            [Display(Name = "Keputusan")]
            public string? Decisions { get; set; }

            [Display(Name = "Tindak Lanjut")]
            public string? FollowUp { get; set; }
        }
    }
}
