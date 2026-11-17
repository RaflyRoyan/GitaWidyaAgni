using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Program;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Kegiatan
{
    /// <summary>
    /// Kegiatan › Daftar Kegiatan: tab Kegiatan (t_activity + peserta + kolaborator) dan tab Laporan (t_activity_report).
    /// Kegiatan: Direncanakan → Berjalan → Selesai / Dibatalkan. Laporan: Draft → Diajukan → Disetujui / Revisi.
    /// </summary>
    public class KegiatanVM
    {
        public class Index
        {
        }

        // ================= Kegiatan
        public class ListKegiatan
        {
            public List<KegiatanRow> Rows { get; set; } = Method.GetKegiatanList().ToList();
        }

        public class KegiatanRow
        {
            public string EncDecrKey { get; set; } = "";
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public string Jenis { get; set; } = "";
            public string Audiens { get; set; } = "";
            public string? Program { get; set; }
            public string? Induk { get; set; }
            public string? Location { get; set; }
            public DateTime StartAt { get; set; }
            public DateTime? EndAt { get; set; }
            public bool Publik { get; set; }
            public bool Disorot { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public int JumlahPeserta { get; set; }
        }

        public class AddKegiatan
        {
            public NewKegiatan Row { get; set; } = new();
            public List<GwaVM.OpsiItem> ProgramOpsi { get; set; } = Opsi.Program(hanyaTerbuka: true);
            public List<GwaVM.OpsiItem> IndukOpsi { get; set; }
            public List<GwaVM.OpsiItem> LombaOpsi { get; set; } = Opsi.Lomba();
            public List<GwaVM.OpsiItem> DivisiOpsi { get; set; } = Opsi.Divisi();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = GwaVM.Opsi<ActivityType>();
            public List<GwaVM.OpsiItem> AudiensOpsi { get; set; } = GwaVM.Opsi<AudienceType>();
            public List<GwaVM.OpsiItem> PublikOpsi { get; set; } = GwaVM.Opsi<PublicVisibility>();
            public List<GwaVM.OpsiItem> SorotOpsi { get; set; } = GwaVM.Opsi<FeaturedStatus>();

            public AddKegiatan() : this(0)
            {
                var start = DateTime.Today.AddDays(7).AddHours(16);
                Row.StartAt = start;
                Row.EndAt = start.AddHours(3);
            }

            protected AddKegiatan(int kecualiId)
            {
                // Induk hanya kegiatan level teratas (satu tingkat).
                IndukOpsi = Activity.GetDataList().Where(x => x.ParentActivityId == null && x.Id != kecualiId
                        && (x.Status == ProgramStatus.Planned || x.Status == ProgramStatus.Ongoing))
                    .OrderByDescending(x => x.StartAt)
                    .Select(x => new GwaVM.OpsiItem(x.Id, x.Title + " · " + x.StartAt.ToString("dd MMM yyyy"))).ToList();
            }
        }

        public class EditKegiatan : AddKegiatan
        {
            public string StatusText { get; set; } = "";

            public EditKegiatan(string encKey) : this(LibExtensions.Load<Activity>(encKey, "Kegiatan"))
            {
            }

            private EditKegiatan(Activity obj) : base(obj.Id)
            {
                StatusText = EnumHelper.Text(obj.Status);
                Row = new NewKegiatan
                {
                    ProgramId = obj.ProgramId,
                    ParentActivityId = obj.ParentActivityId,
                    CompetitionId = obj.CompetitionId,
                    ActivityType = (int)obj.ActivityType,
                    AudienceType = (int)obj.AudienceType,
                    DivisionId = obj.DivisionId,
                    Title = obj.Title,
                    Description = obj.Description,
                    Location = obj.Location,
                    Latitude = obj.Latitude,
                    Longitude = obj.Longitude,
                    StartAt = obj.StartAt,
                    EndAt = obj.EndAt,
                    IsPublic = (int)obj.IsPublic,
                    IsFeatured = (int)obj.IsFeatured
                };
            }
        }

        public class NewKegiatan
        {
            [Required(ErrorMessage = "Nama kegiatan harus diisi")]
            [StringLength(200, ErrorMessage = "Nama kegiatan maksimal 200 karakter")]
            [Display(Name = "Nama Kegiatan")]
            public string? Title { get; set; }

            [Display(Name = "Jenis")]
            public int ActivityType { get; set; } = (int)GWAShared.Enums.ActivityType.Event;

            [Display(Name = "Audiens / Wajib Hadir")]
            public int AudienceType { get; set; } = (int)GWAShared.Enums.AudienceType.SemuaAnggota;

            [Display(Name = "Program")]
            public int? ProgramId { get; set; }

            [Display(Name = "Kegiatan Induk")]
            public int? ParentActivityId { get; set; }

            [Display(Name = "Lomba")]
            public int? CompetitionId { get; set; }

            [Display(Name = "Divisi")]
            public int? DivisionId { get; set; }

            [Display(Name = "Deskripsi")]
            public string? Description { get; set; }

            [StringLength(255, ErrorMessage = "Lokasi maksimal 255 karakter")]
            [Display(Name = "Lokasi")]
            public string? Location { get; set; }

            [Range(-90, 90, ErrorMessage = "Latitude -90 sampai 90")]
            [Display(Name = "Latitude")]
            public decimal? Latitude { get; set; }

            [Range(-180, 180, ErrorMessage = "Longitude -180 sampai 180")]
            [Display(Name = "Longitude")]
            public decimal? Longitude { get; set; }

            [Required(ErrorMessage = "Waktu mulai harus diisi")]
            [Display(Name = "Mulai")]
            public DateTime? StartAt { get; set; }

            [Display(Name = "Selesai")]
            public DateTime? EndAt { get; set; }

            [Display(Name = "Tampil di Landing Page")]
            public int IsPublic { get; set; } = (int)PublicVisibility.Private;

            [Display(Name = "Sorotan")]
            public int IsFeatured { get; set; } = (int)FeaturedStatus.Biasa;
        }

        // ---------- Peserta (modal _Peserta + _PesertaList)
        public class Peserta
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public PesertaInput Input { get; set; } = new();
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public PesertaList List { get; set; }

            public Peserta(string encKey)
            {
                var obj = LibExtensions.Load<Activity>(encKey, "Kegiatan");
                Key = obj.EncDecrKey;
                Judul = obj.Title;
                List = new PesertaList(obj.Id);
            }
        }

        public class PesertaList
        {
            public List<PesertaRow> Rows { get; set; }
            public PesertaList(int activityId)
            {
                Rows = ActivityParticipant.GetDataList(activityId).Select(x => new PesertaRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Nama = x.MemberName,
                    Peran = x.ParticipantRole,
                    Status = (int)x.RegistrationStatus,
                    StatusText = EnumHelper.Text(x.RegistrationStatus),
                    InsDate = x.InsDate
                }).OrderBy(x => x.Nama).ToList();
            }
            public PesertaList(string encKey) : this(LibExtensions.Load<Activity>(encKey, "Kegiatan").Id)
            {
            }
        }

        public class PesertaRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Nama { get; set; } = "";
            public string Peran { get; set; } = "";
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public DateTime InsDate { get; set; }
            public bool Aktif => Status != (int)RegistrationStatus.Cancelled;
        }

        public class PesertaInput
        {
            public string Key { get; set; } = "";

            [Required(ErrorMessage = "Pilih anggota")]
            [Display(Name = "Anggota")]
            public int? MemberId { get; set; }

            [StringLength(50, ErrorMessage = "Peran maksimal 50 karakter")]
            [Display(Name = "Peran")]
            public string? ParticipantRole { get; set; } = ActivityParticipant.DEFAULT_ROLE;
        }

        // ---------- Kolaborator (modal _Kolaborator + _KolaboratorList)
        public class Kolaborator
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public KolaboratorInput Input { get; set; } = new();
            public List<GwaVM.OpsiItem> MitraOpsi { get; set; } = Opsi.Mitra();
            public List<GwaVM.OpsiItem> PeranOpsi { get; set; } = GwaVM.Opsi<CollaboratorRole>();
            public KolaboratorList List { get; set; }

            public Kolaborator(string encKey)
            {
                var obj = LibExtensions.Load<Activity>(encKey, "Kegiatan");
                Key = obj.EncDecrKey;
                Judul = obj.Title;
                List = new KolaboratorList(obj.Id);
            }
        }

        public class KolaboratorList
        {
            public List<KolaboratorRow> Rows { get; set; }
            public KolaboratorList(int activityId)
            {
                Rows = ActivityCollaborator.GetDataList(activityId).Select(x => new KolaboratorRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Nama = x.ExternalPartyName,
                    Peran = EnumHelper.Text(x.CollabRole),
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    Notes = x.Notes,
                    RespondedAt = x.RespondedAt
                }).OrderBy(x => x.Nama).ToList();
            }
            public KolaboratorList(string encKey) : this(LibExtensions.Load<Activity>(encKey, "Kegiatan").Id)
            {
            }
        }

        public class KolaboratorRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Nama { get; set; } = "";
            public string Peran { get; set; } = "";
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public string? Notes { get; set; }
            public DateTime? RespondedAt { get; set; }
        }

        public class KolaboratorInput
        {
            public string Key { get; set; } = "";

            [Required(ErrorMessage = "Pilih mitra")]
            [Display(Name = "Mitra")]
            public int? ExternalPartyId { get; set; }

            [Display(Name = "Peran")]
            public int CollabRole { get; set; } = (int)CollaboratorRole.PenyelenggaraBersama;

            [StringLength(255, ErrorMessage = "Catatan maksimal 255 karakter")]
            [Display(Name = "Catatan")]
            public string? Notes { get; set; }
        }

        // ================= Laporan
        public class ListLaporan
        {
            public List<LaporanRow> Rows { get; set; } = Method.GetLaporanList().ToList();
        }

        public class LaporanRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string? Kegiatan { get; set; }
            public string? Program { get; set; }
            public string? FileName { get; set; }
            public DateTime? SubmittedAt { get; set; }
            public DateTime? ApprovedAt { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
        }

        public class AddLaporan
        {
            public NewLaporan Row { get; set; } = new();
            public List<GwaVM.OpsiItem> KegiatanOpsi { get; set; } = Opsi.Kegiatan();
            public List<GwaVM.OpsiItem> ProgramOpsi { get; set; } = Opsi.Program();
            public string? FileName { get; set; }
        }

        public class EditLaporan : AddLaporan
        {
            public string StatusText { get; set; } = "";

            public EditLaporan(string encKey)
            {
                var obj = LibExtensions.Load<ActivityReport>(encKey, "Laporan");
                StatusText = EnumHelper.Text(obj.Status);
                FileName = obj.FileName;
                Row = new NewLaporan { ActivityId = obj.ActivityId, ProgramId = obj.ProgramId, Title = obj.Title, Summary = obj.Summary };
            }
        }

        public class NewLaporan
        {
            [Display(Name = "Kegiatan")]
            public int? ActivityId { get; set; }

            [Display(Name = "Program (bila tanpa kegiatan)")]
            public int? ProgramId { get; set; }

            [Required(ErrorMessage = "Judul laporan harus diisi")]
            [StringLength(200, ErrorMessage = "Judul maksimal 200 karakter")]
            [Display(Name = "Judul Laporan")]
            public string? Title { get; set; }

            [Display(Name = "Ringkasan")]
            public string? Summary { get; set; }
        }

        public class Method
        {
            public static IEnumerable<KegiatanRow> GetKegiatanList()
            {
                var list = Activity.GetDataList();
                var judul = list.ToDictionary(x => x.Id, x => x.Title);
                var peserta = ActivityParticipant.GetDataList().Where(x => x.RegistrationStatus != RegistrationStatus.Cancelled)
                    .GroupBy(x => x.ActivityId).ToDictionary(g => g.Key, g => g.Count());
                return list.Select(x => new KegiatanRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Id = x.Id,
                    Title = x.Title,
                    Jenis = EnumHelper.Text(x.ActivityType),
                    Audiens = EnumHelper.Text(x.AudienceType),
                    Program = x.ProgramTitle,
                    Induk = x.ParentActivityId.HasValue && judul.TryGetValue(x.ParentActivityId.Value, out var j) ? j : null,
                    Location = x.Location,
                    StartAt = x.StartAt,
                    EndAt = x.EndAt,
                    Publik = x.IsPublic == PublicVisibility.Public,
                    Disorot = x.IsFeatured == FeaturedStatus.Disorot,
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    JumlahPeserta = peserta.TryGetValue(x.Id, out var n) ? n : 0
                }).OrderByDescending(x => x.StartAt);
            }

            private static void Isi(Activity obj, NewKegiatan input)
            {
                obj.ProgramId = input.ProgramId;
                obj.ParentActivityId = input.ParentActivityId;
                obj.CompetitionId = input.CompetitionId;
                obj.ActivityType = (ActivityType)input.ActivityType;
                obj.AudienceType = (AudienceType)input.AudienceType;
                obj.DivisionId = input.DivisionId;
                obj.Title = input.Title ?? "";
                obj.Description = input.Description;
                obj.Location = input.Location;
                obj.Latitude = input.Latitude;
                obj.Longitude = input.Longitude;
                obj.StartAt = input.StartAt ?? DateTime.Now;
                obj.EndAt = input.EndAt;
                obj.IsPublic = (PublicVisibility)input.IsPublic;
                obj.IsFeatured = (FeaturedStatus)input.IsFeatured;
            }

            public static void SimpanKegiatan(NewKegiatan input, string user)
            {
                Activity.Save(new Activity.NewRow
                {
                    ProgramId = input.ProgramId,
                    ParentActivityId = input.ParentActivityId,
                    CompetitionId = input.CompetitionId,
                    ActivityType = (ActivityType)input.ActivityType,
                    AudienceType = (AudienceType)input.AudienceType,
                    DivisionId = input.DivisionId,
                    Title = input.Title ?? "",
                    Description = input.Description,
                    Location = input.Location,
                    Latitude = input.Latitude,
                    Longitude = input.Longitude,
                    StartAt = input.StartAt ?? DateTime.Now,
                    EndAt = input.EndAt,
                    IsPublic = (PublicVisibility)input.IsPublic,
                    IsFeatured = (FeaturedStatus)input.IsFeatured
                }, user).Ok();
            }

            public static void UpdateKegiatan(string encKey, NewKegiatan input, string user)
            {
                var obj = LibExtensions.Load<Activity>(encKey, "Kegiatan");
                Isi(obj, input);
                obj.Update(user).Ok();
            }

            public static void StatusKegiatan(GwaVM.StatusInput input, string user)
                => LibExtensions.Load<Activity>(input.Key, "Kegiatan").SetStatus((ProgramStatus)input.Status, user).Ok();

            public static void HapusKegiatan(string? key, string user)
                => LibExtensions.Load<Activity>(key, "Kegiatan").Delete(user).Ok();

            public static void TambahPeserta(PesertaInput input, string user)
            {
                var obj = LibExtensions.Load<Activity>(input.Key, "Kegiatan");
                ActivityParticipant.Save(new ActivityParticipant.NewRow
                {
                    ActivityId = obj.Id,
                    MemberId = input.MemberId ?? 0,
                    ParticipantRole = string.IsNullOrWhiteSpace(input.ParticipantRole) ? ActivityParticipant.DEFAULT_ROLE : input.ParticipantRole.Trim().ToUpperInvariant()
                }, user).Ok();
            }

            public static void KonfirmasiPeserta(string? key, string user)
                => LibExtensions.Load<ActivityParticipant>(key, "Peserta").Confirm(user).Ok();

            public static void BatalPeserta(string? key, string user)
                => LibExtensions.Load<ActivityParticipant>(key, "Peserta").Cancel(user).Ok();

            public static void TambahKolaborator(KolaboratorInput input, string user)
            {
                var obj = LibExtensions.Load<Activity>(input.Key, "Kegiatan");
                ActivityCollaborator.Save(new ActivityCollaborator.NewRow
                {
                    ActivityId = obj.Id,
                    ExternalPartyId = input.ExternalPartyId ?? 0,
                    CollabRole = (CollaboratorRole)input.CollabRole,
                    Notes = input.Notes
                }, user).Ok();
            }

            public static void ResponKolaborator(GwaVM.StatusInput input, string user)
                => LibExtensions.Load<ActivityCollaborator>(input.Key, "Kolaborator").Respond((CollaboratorStatus)input.Status, input.Keterangan, user).Ok();

            public static void HapusKolaborator(string? key, string user)
                => LibExtensions.Load<ActivityCollaborator>(key, "Kolaborator").Delete(user).Ok();

            public static IEnumerable<LaporanRow> GetLaporanList()
            {
                var kegiatan = Activity.GetDataList().ToDictionary(x => x.Id, x => x.Title);
                var program = WorkProgram.GetDataList().ToDictionary(x => x.Id, x => x.Title);
                return ActivityReport.GetDataList().Select(x => new LaporanRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Title = x.Title,
                    Kegiatan = x.ActivityId.HasValue && kegiatan.TryGetValue(x.ActivityId.Value, out var k) ? k : null,
                    Program = x.ProgramId.HasValue && program.TryGetValue(x.ProgramId.Value, out var p) ? p : null,
                    FileName = x.FileName,
                    SubmittedAt = x.SubmittedAt,
                    ApprovedAt = x.ApprovedAt,
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status)
                }).OrderByDescending(x => x.SubmittedAt ?? DateTime.MaxValue);
            }

            public static ActivityReport LoadLaporan(string? key) => LibExtensions.Load<ActivityReport>(key, "Laporan");

            public static void SimpanLaporan(NewLaporan input, byte[]? file, IFormFile? meta, string user)
            {
                var obj = ActivityReport.Save(new ActivityReport.NewRow
                {
                    ActivityId = input.ActivityId,
                    ProgramId = input.ProgramId,
                    Title = input.Title ?? "",
                    Summary = input.Summary
                }, user).Ok();
                if (file != null)
                {
                    obj.UpdateFile(file, meta!.FileName, meta.ContentType, user).Ok();
                }
            }

            public static void UpdateLaporan(string encKey, NewLaporan input, byte[]? file, IFormFile? meta, bool hapusFile, string user)
            {
                var obj = LoadLaporan(encKey);
                obj.Title = input.Title ?? "";
                obj.Summary = input.Summary;
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
