using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Presensi;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Kegiatan
{
    public partial class PresensiVM
    {
        public class Method
        {
            private static Dictionary<int, string> NamaKegiatan()
                => GWALib.Lib.Transaksi.Program.Activity.GetDataList().ToDictionary(x => x.Id, x => x.Title);

            // ---------- Sesi
            public static IEnumerable<SesiRow> GetSesiList()
            {
                var kegiatan = NamaKegiatan();
                var rekap = AttendanceRecord.GetDataList().GroupBy(x => x.SessionId).ToDictionary(g => g.Key, g => (
                    Hadir: g.Count(x => x.Status == AttendanceStatus.Hadir || x.Status == AttendanceStatus.Terlambat),
                    Tidak: g.Count(x => x.Status != AttendanceStatus.Hadir && x.Status != AttendanceStatus.Terlambat)));
                var notulen = MeetingMinutes.GetDataList().Select(x => x.SessionId).ToHashSet();
                return AttendanceSession.GetDataList().Select(x => new SesiRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Id = x.Id,
                    Name = x.Name,
                    Kegiatan = kegiatan.TryGetValue(x.ActivityId, out var k) ? k : "",
                    OpenAt = x.OpenAt,
                    CloseAt = x.CloseAt,
                    Location = x.Location,
                    Geofence = x.Latitude.HasValue && x.RadiusMeter.HasValue,
                    DariJadwal = x.ScheduleId.HasValue,
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    Hadir = rekap.TryGetValue(x.Id, out var r) ? r.Hadir : 0,
                    TidakHadir = rekap.TryGetValue(x.Id, out var r2) ? r2.Tidak : 0,
                    AdaNotulen = notulen.Contains(x.Id)
                }).OrderByDescending(x => x.OpenAt);
            }

            public static void SimpanSesi(NewSesi input, string user)
                => AttendanceSession.Save(new AttendanceSession.NewRow
                {
                    ActivityId = input.ActivityId ?? 0,
                    Name = input.Name ?? "",
                    OpenAt = input.OpenAt ?? DateTime.Now,
                    CloseAt = input.CloseAt ?? DateTime.Now,
                    LateToleranceMinute = input.LateToleranceMinute,
                    Location = input.Location,
                    Latitude = input.Latitude,
                    Longitude = input.Longitude,
                    RadiusMeter = input.RadiusMeter
                }, user).Ok();

            public static void UpdateSesi(string key, NewSesi input, string user)
            {
                var obj = LoadSesi(key);
                obj.Name = input.Name ?? "";
                obj.OpenAt = input.OpenAt ?? obj.OpenAt;
                obj.CloseAt = input.CloseAt ?? obj.CloseAt;
                obj.LateToleranceMinute = input.LateToleranceMinute;
                obj.Location = input.Location;
                obj.Latitude = input.Latitude;
                obj.Longitude = input.Longitude;
                obj.RadiusMeter = input.RadiusMeter;
                obj.Update(user).Ok();
            }

            public static AttendanceSession LoadSesi(string? key) => LibExtensions.Load<AttendanceSession>(key, "Sesi presensi");

            public static void Manual(ManualInput input, string user)
            {
                var sesi = LoadSesi(input.Key);
                AttendanceRecord.SetManual(sesi.Id, input.MemberId ?? 0, (AttendanceStatus)input.Status, input.Notes, user).Ok();
            }

            public static AttendanceRecord.PhotoContent? Foto(string? key, bool keluar)
            {
                var obj = LibExtensions.Load<AttendanceRecord>(key, "Presensi");
                return keluar ? obj.GetCheckOutPhoto() : obj.GetCheckInPhoto();
            }

            // ---------- Jadwal
            public static IEnumerable<JadwalRow> GetJadwalList()
            {
                var kegiatan = NamaKegiatan();
                return ActivitySchedule.GetDataList(FilterAktif.Semua).Select(x => new JadwalRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Kegiatan = kegiatan.TryGetValue(x.ActivityId, out var k) ? k : "",
                    Hari = (int)x.DayOfWeek,
                    HariText = EnumHelper.Text(x.DayOfWeek),
                    Jam = x.StartTime.ToString("HH:mm") + " – " + x.EndTime.ToString("HH:mm"),
                    Location = x.Location,
                    ValidFrom = x.ValidFrom.ToDateTime(TimeOnly.MinValue),
                    ValidUntil = Opsi.KeDateTime(x.ValidUntil),
                    LateToleranceMinute = x.LateToleranceMinute,
                    Aktif = x.IsActive == ActiveStatus.Aktif
                }).OrderBy(x => x.Kegiatan).ThenBy(x => x.Hari);
            }

            private static TimeOnly Jam(DateTime? value, string nama)
                => value.HasValue ? TimeOnly.FromDateTime(value.Value) : throw new ArgumentException(nama + " harus diisi");

            public static void SimpanJadwal(NewJadwal input, string user)
                => ActivitySchedule.Save(new ActivitySchedule.NewRow
                {
                    ActivityId = input.ActivityId ?? 0,
                    DayOfWeek = (ScheduleDay)input.DayOfWeek,
                    StartTime = Jam(input.StartTime, "Jam mulai"),
                    EndTime = Jam(input.EndTime, "Jam selesai"),
                    Location = input.Location,
                    Latitude = input.Latitude,
                    Longitude = input.Longitude,
                    RadiusMeter = input.RadiusMeter,
                    LateToleranceMinute = input.LateToleranceMinute,
                    ValidFrom = Opsi.TanggalWajib(input.ValidFrom, "Berlaku mulai"),
                    ValidUntil = Opsi.Tanggal(input.ValidUntil)
                }, user).Ok();

            public static void UpdateJadwal(string key, NewJadwal input, string user)
            {
                var obj = LibExtensions.Load<ActivitySchedule>(key, "Jadwal");
                obj.DayOfWeek = (ScheduleDay)input.DayOfWeek;
                obj.StartTime = Jam(input.StartTime, "Jam mulai");
                obj.EndTime = Jam(input.EndTime, "Jam selesai");
                obj.Location = input.Location;
                obj.Latitude = input.Latitude;
                obj.Longitude = input.Longitude;
                obj.RadiusMeter = input.RadiusMeter;
                obj.LateToleranceMinute = input.LateToleranceMinute;
                obj.ValidFrom = Opsi.TanggalWajib(input.ValidFrom, "Berlaku mulai");
                obj.ValidUntil = Opsi.Tanggal(input.ValidUntil);
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }

            public static void HapusJadwal(string? key, string user) => LibExtensions.Load<ActivitySchedule>(key, "Jadwal").Delete(user).Ok();

            public static int Generate(GenerateInput input, string user)
                => ActivitySchedule.GenerateSessions(Opsi.TanggalWajib(input.From, "Tanggal awal"), Opsi.TanggalWajib(input.To, "Tanggal akhir"), input.ActivityId, user).Ok();

            // ---------- Notulen
            public static IEnumerable<NotulenRow> GetNotulenList()
            {
                var sesi = AttendanceSession.GetDataList().ToDictionary(x => x.Id);
                return MeetingMinutes.GetDataList().Select(x => new NotulenRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Sesi = sesi.TryGetValue(x.SessionId, out var s) ? s.Name : "",
                    Tanggal = sesi.TryGetValue(x.SessionId, out var s2) ? s2.OpenAt : null,
                    Agenda = x.Agenda,
                    Pimpinan = x.ChairedByName,
                    Notulis = x.NotulisName,
                    FileName = x.FileName,
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    FinalizedAt = x.FinalizedAt
                }).OrderByDescending(x => x.Tanggal);
            }

            public static MeetingMinutes LoadNotulen(string? key) => LibExtensions.Load<MeetingMinutes>(key, "Notulen");

            public static void SimpanNotulen(NewNotulen input, byte[]? file, IFormFile? meta, string user)
            {
                var obj = MeetingMinutes.Save(new MeetingMinutes.NewRow
                {
                    SessionId = input.SessionId ?? 0,
                    ChairedByMemberId = input.ChairedByMemberId,
                    NotulisMemberId = input.NotulisMemberId,
                    Agenda = input.Agenda,
                    Content = input.Content ?? "",
                    Decisions = input.Decisions,
                    FollowUp = input.FollowUp
                }, user).Ok();
                if (file != null)
                {
                    obj.UpdateFile(file, meta!.FileName, meta.ContentType, user).Ok();
                }
            }

            public static void UpdateNotulen(string key, NewNotulen input, byte[]? file, IFormFile? meta, bool hapusFile, string user)
            {
                var obj = LoadNotulen(key);
                obj.ChairedByMemberId = input.ChairedByMemberId;
                obj.NotulisMemberId = input.NotulisMemberId;
                obj.Agenda = input.Agenda;
                obj.Content = input.Content ?? "";
                obj.Decisions = input.Decisions;
                obj.FollowUp = input.FollowUp;
                obj.Update(user).Ok();
                if (file != null)
                {
                    obj.UpdateFile(file, meta!.FileName, meta.ContentType, user).Ok();
                }
                else if (hapusFile)
                {
                    obj.UpdateFile(null, null, null, user).Ok();
                }
            }
        }
    }
}
