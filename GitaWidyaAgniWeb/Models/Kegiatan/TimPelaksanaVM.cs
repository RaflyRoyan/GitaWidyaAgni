using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.TimPelaksana;
using GWALib.Lib.Transaksi.Program;
using GWALib.Lib.Transaksi.TimPelaksana;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Kegiatan
{
    /// <summary>
    /// Kegiatan › Tim Pelaksana: per kegiatan (dipilih di atas halaman) — seksi, panitia, rekrutmen panitia + lamaran,
    /// rundown, checklist (bisa dibuat dari template), dan rombongan perjalanan. Tab Template mengelola master template checklist.
    /// Semua list / form tab kegiatan menerima activityId (id kegiatan terpilih).
    /// </summary>
    public partial class TimPelaksanaVM
    {
        public class Index
        {
            public List<GwaVM.OpsiItem> KegiatanOpsi { get; set; } = Opsi.Kegiatan();
            public int? ActivityId { get; set; }

            public Index(int? activityId)
            {
                ActivityId = activityId.HasValue && KegiatanOpsi.Any(x => x.Value == activityId) ? activityId
                    : Activity.GetDataList().Where(x => x.Status == ProgramStatus.Planned || x.Status == ProgramStatus.Ongoing)
                        .OrderBy(x => x.StartAt).Select(x => (int?)x.Id).FirstOrDefault() ?? KegiatanOpsi.FirstOrDefault()?.Value;
            }
        }

        // Opsi bersama form tab kegiatan.
        public abstract class FormKegiatan
        {
            public int ActivityId { get; set; }
            public List<GwaVM.OpsiItem> SeksiOpsi { get; set; } = new();
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = new();

            protected void Muat(int activityId, bool anggota = true)
            {
                ActivityId = activityId;
                SeksiOpsi = CommitteeSection.GetDataList(activityId).OrderBy(x => x.SeqNo).Select(x => new GwaVM.OpsiItem(x.Id, x.Name)).ToList();
                if (anggota)
                {
                    AnggotaOpsi = Opsi.Anggota();
                }
            }
        }

        public static string Judul(int activityId) => Activity.GetDataList().FirstOrDefault(x => x.Id == activityId)?.Title ?? "";

        // ================= Seksi
        public class ListSeksi
        {
            public List<SeksiRow> Rows { get; set; }
            public ListSeksi(int activityId)
            {
                var anggota = CommitteeMember.GetDataList(activityId, status: CommitteeMemberStatus.Aktif).Where(x => x.SectionId.HasValue)
                    .GroupBy(x => x.SectionId!.Value).ToDictionary(g => g.Key, g => g.Count());
                Rows = CommitteeSection.GetDataList(activityId).Select(x => new SeksiRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Name = x.Name,
                    Description = x.Description,
                    Koordinator = x.CoordinatorName,
                    SeqNo = x.SeqNo,
                    JumlahAnggota = anggota.TryGetValue(x.Id, out var n) ? n : 0
                }).OrderBy(x => x.SeqNo).ToList();
            }
        }

        public class SeksiRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Description { get; set; }
            public string? Koordinator { get; set; }
            public int SeqNo { get; set; }
            public int JumlahAnggota { get; set; }
        }

        public class AddSeksi
        {
            public NewSeksi Row { get; set; } = new();
            public AddSeksi() { }
            public AddSeksi(int activityId) { Row.ActivityId = activityId; }
        }

        public class EditSeksi : AddSeksi
        {
            public EditSeksi(string encKey)
            {
                var obj = LibExtensions.Load<CommitteeSection>(encKey, "Seksi");
                Row = new NewSeksi { ActivityId = obj.ActivityId, Name = obj.Name, Description = obj.Description, SeqNo = obj.SeqNo };
            }
        }

        public class NewSeksi
        {
            public int ActivityId { get; set; }

            [Required(ErrorMessage = "Nama seksi harus diisi")]
            [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
            [Display(Name = "Nama Seksi")]
            public string? Name { get; set; }

            [StringLength(255, ErrorMessage = "Keterangan maksimal 255 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Range(0, 999, ErrorMessage = "Urutan 0 - 999")]
            [Display(Name = "Urutan (0 = paling akhir)")]
            public int SeqNo { get; set; }
        }

        // ================= Panitia
        public class ListPanitia
        {
            public List<PanitiaRow> Rows { get; set; }
            public ListPanitia(int activityId)
            {
                Rows = CommitteeMember.GetDataList(activityId).Select(x => new PanitiaRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Nama = x.MemberName,
                    Peran = EnumHelper.Text(x.CommitteeRole),
                    PeranKode = (int)x.CommitteeRole,
                    Seksi = x.SectionName,
                    JobDesc = x.JobDesc,
                    StatusText = EnumHelper.Text(x.Status),
                    Aktif = x.Status == CommitteeMemberStatus.Aktif,
                    DariLamaran = x.ApplicationId.HasValue
                }).OrderByDescending(x => x.Aktif).ThenBy(x => x.PeranKode).ThenBy(x => x.Seksi).ThenBy(x => x.Nama).ToList();
            }
        }

        public class PanitiaRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Nama { get; set; } = "";
            public string Peran { get; set; } = "";
            public int PeranKode { get; set; }
            public string? Seksi { get; set; }
            public string? JobDesc { get; set; }
            public string StatusText { get; set; } = "";
            public bool Aktif { get; set; }
            public bool DariLamaran { get; set; }
        }

        public class AddPanitia : FormKegiatan
        {
            public NewPanitia Row { get; set; } = new();
            public List<GwaVM.OpsiItem> PeranOpsi { get; set; } = GwaVM.Opsi<CommitteeRole>();
            public AddPanitia() { }
            public AddPanitia(int activityId) { Muat(activityId); Row.ActivityId = activityId; }
        }

        public class EditPanitia : AddPanitia
        {
            public string Nama { get; set; } = "";
            public EditPanitia(string encKey)
            {
                var obj = LibExtensions.Load<CommitteeMember>(encKey, "Panitia");
                Muat(obj.ActivityId, false);
                Nama = obj.MemberName;
                Row = new NewPanitia { ActivityId = obj.ActivityId, MemberId = obj.MemberId, SectionId = obj.SectionId, CommitteeRole = (int)obj.CommitteeRole, JobDesc = obj.JobDesc };
            }
        }

        public class NewPanitia
        {
            public int ActivityId { get; set; }

            [Required(ErrorMessage = "Pilih anggota")]
            [Display(Name = "Anggota")]
            public int? MemberId { get; set; }

            [Display(Name = "Peran")]
            public int CommitteeRole { get; set; } = (int)GWAShared.Enums.CommitteeRole.Staf;

            [Display(Name = "Seksi (wajib untuk Koordinator Sie / Staf)")]
            public int? SectionId { get; set; }

            [StringLength(500, ErrorMessage = "Job desc maksimal 500 karakter")]
            [Display(Name = "Job Desc")]
            public string? JobDesc { get; set; }
        }

        // ================= Rekrutmen panitia
        public class ListRekrutmen
        {
            public int ActivityId { get; set; }
            public string? Key { get; set; }
            public int? Status { get; set; }
            public string? StatusText { get; set; }
            public DateTime? OpenAt { get; set; }
            public DateTime? CloseAt { get; set; }
            public int? Quota { get; set; }
            public bool AdaFormulir { get; set; }
            public List<LamaranRow> Rows { get; set; } = new();

            public ListRekrutmen(int activityId)
            {
                ActivityId = activityId;
                var obj = CommitteeRecruitment.GetByActivity(activityId);
                if (obj == null)
                {
                    return;
                }
                Key = obj.EncDecrKey;
                Status = (int)obj.Status;
                StatusText = EnumHelper.Text(obj.Status);
                OpenAt = obj.OpenAt;
                CloseAt = obj.CloseAt;
                Quota = obj.Quota;
                AdaFormulir = obj.FormId.HasValue;
                Rows = CommitteeApplication.GetDataList(obj.Id).Select(x => new LamaranRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Nama = x.MemberName,
                    Pilihan1 = x.SectionChoice1Name,
                    Pilihan2 = x.SectionChoice2Name,
                    Reason = x.Reason,
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    ReviewNote = x.ReviewNote,
                    InsDate = x.InsDate
                }).OrderBy(x => x.InsDate).ToList();
            }
        }

        public class LamaranRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Nama { get; set; } = "";
            public string Pilihan1 { get; set; } = "";
            public string? Pilihan2 { get; set; }
            public string? Reason { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public string? ReviewNote { get; set; }
            public DateTime InsDate { get; set; }
        }

        public class AddRekrutmen
        {
            public NewRekrutmen Row { get; set; } = new();
            public List<GwaVM.OpsiItem> FormulirOpsi { get; set; }

            public AddRekrutmen() : this(0) { }
            public AddRekrutmen(int activityId)
            {
                Row.ActivityId = activityId;
                Row.OpenAt = DateTime.Today;
                Row.CloseAt = DateTime.Today.AddDays(7).AddHours(23).AddMinutes(59);
                // Formulir PendaftaranRekrutmen tanpa rekrutmen anggota (dipakai rekrutmen panitia).
                FormulirOpsi = GWALib.Lib.Transaksi.Formulir.Form.GetDataList()
                    .Where(x => x.FormType == FormType.PendaftaranRekrutmen && x.RecruitmentId == null)
                    .Select(x => new GwaVM.OpsiItem(x.Id, x.Title)).ToList();
            }
        }

        public class EditRekrutmen : AddRekrutmen
        {
            public EditRekrutmen(string encKey) : this(LibExtensions.Load<CommitteeRecruitment>(encKey, "Rekrutmen panitia"))
            {
            }
            private EditRekrutmen(CommitteeRecruitment obj) : base(obj.ActivityId)
            {
                Row = new NewRekrutmen { ActivityId = obj.ActivityId, OpenAt = obj.OpenAt, CloseAt = obj.CloseAt, Quota = obj.Quota, FormId = obj.FormId };
            }
        }

        public class NewRekrutmen
        {
            public int ActivityId { get; set; }

            [Required(ErrorMessage = "Waktu buka harus diisi")]
            [Display(Name = "Dibuka")]
            public DateTime? OpenAt { get; set; }

            [Required(ErrorMessage = "Waktu tutup harus diisi")]
            [Display(Name = "Ditutup")]
            public DateTime? CloseAt { get; set; }

            [Range(1, 1000, ErrorMessage = "Kuota minimal 1")]
            [Display(Name = "Kuota (kosong = tanpa batas)")]
            public int? Quota { get; set; }

            [Display(Name = "Formulir Tambahan")]
            public int? FormId { get; set; }
        }

        public class TerimaInput
        {
            public string Key { get; set; } = "";
            public int? SectionId { get; set; }
            public string? Keterangan { get; set; }
        }

        // ================= Rundown
        public class ListRundown
        {
            public List<RundownRow> Rows { get; set; }
            public ListRundown(int activityId)
            {
                Rows = ActivityRundown.GetDataList(activityId).Select((x, i) => new RundownRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Urut = i,
                    Tanggal = x.RundownDate.ToDateTime(TimeOnly.MinValue),
                    Jam = x.StartTime.ToString("HH:mm") + (x.EndTime.HasValue ? " – " + x.EndTime.Value.ToString("HH:mm") : ""),
                    Title = x.Title,
                    Description = x.Description,
                    Lokasi = x.LocationName,
                    Seksi = x.SectionName,
                    Pic = x.PicMemberName
                }).ToList();
            }
        }

        public class RundownRow
        {
            public string EncDecrKey { get; set; } = "";
            public int Urut { get; set; }
            public DateTime Tanggal { get; set; }
            public string Jam { get; set; } = "";
            public string Title { get; set; } = "";
            public string? Description { get; set; }
            public string? Lokasi { get; set; }
            public string? Seksi { get; set; }
            public string? Pic { get; set; }
        }

        public class AddRundown : FormKegiatan
        {
            public NewRundown Row { get; set; } = new();
            public AddRundown() { }
            public AddRundown(int activityId)
            {
                Muat(activityId);
                Row.ActivityId = activityId;
                var start = Activity.GetDataList().FirstOrDefault(x => x.Id == activityId)?.StartAt ?? DateTime.Today;
                Row.RundownDate = start.Date;
                Row.StartTime = start;
            }
        }

        public class EditRundown : AddRundown
        {
            public EditRundown(string encKey)
            {
                var obj = LibExtensions.Load<ActivityRundown>(encKey, "Rundown");
                Muat(obj.ActivityId);
                var tgl = obj.RundownDate.ToDateTime(TimeOnly.MinValue);
                Row = new NewRundown
                {
                    ActivityId = obj.ActivityId,
                    RundownDate = tgl,
                    StartTime = tgl.Add(obj.StartTime.ToTimeSpan()),
                    EndTime = obj.EndTime.HasValue ? tgl.Add(obj.EndTime.Value.ToTimeSpan()) : null,
                    Title = obj.Title,
                    Description = obj.Description,
                    LocationName = obj.LocationName,
                    SectionId = obj.SectionId,
                    PicMemberId = obj.PicMemberId,
                    SeqNo = obj.SeqNo
                };
            }
        }

        public class NewRundown
        {
            public int ActivityId { get; set; }

            [Required(ErrorMessage = "Tanggal harus diisi")]
            [Display(Name = "Tanggal")]
            public DateTime? RundownDate { get; set; }

            [Required(ErrorMessage = "Jam mulai harus diisi")]
            [Display(Name = "Mulai")]
            public DateTime? StartTime { get; set; }

            [Display(Name = "Selesai")]
            public DateTime? EndTime { get; set; }

            [Required(ErrorMessage = "Acara harus diisi")]
            [StringLength(200, ErrorMessage = "Acara maksimal 200 karakter")]
            [Display(Name = "Acara")]
            public string? Title { get; set; }

            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [StringLength(150, ErrorMessage = "Lokasi maksimal 150 karakter")]
            [Display(Name = "Lokasi")]
            public string? LocationName { get; set; }

            [Display(Name = "Seksi")]
            public int? SectionId { get; set; }

            [Display(Name = "PIC")]
            public int? PicMemberId { get; set; }

            [Range(0, 999, ErrorMessage = "Urutan 0 - 999")]
            [Display(Name = "Urutan di jam yang sama")]
            public int SeqNo { get; set; }
        }

        // ================= Checklist
        public class ListChecklist
        {
            public int ActivityId { get; set; }
            public List<GwaVM.OpsiItem> TemplateOpsi { get; set; }
            public List<ChecklistRow> Rows { get; set; }

            public ListChecklist(int activityId)
            {
                ActivityId = activityId;
                var type = Activity.GetDataList().FirstOrDefault(x => x.Id == activityId)?.ActivityType;
                TemplateOpsi = ChecklistTemplate.GetAvailableList(type).Select(x => new GwaVM.OpsiItem(x.Id, x.Name)).ToList();
                Rows = ActivityChecklist.GetDataList(activityId).Select(x => new ChecklistRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Title = x.Title,
                    Seksi = x.SectionName,
                    Pic = x.PicMemberName,
                    DueDate = Opsi.KeDateTime(x.DueDate),
                    Prioritas = EnumHelper.Text(x.Priority),
                    Milestone = x.IsMilestone == MilestoneFlag.Milestone,
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    DoneAt = x.DoneAt,
                    DariTemplate = x.TemplateItemId.HasValue
                }).OrderBy(x => x.Status).ThenBy(x => x.DueDate ?? DateTime.MaxValue).ToList();
            }
        }

        public class ChecklistRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string? Seksi { get; set; }
            public string? Pic { get; set; }
            public DateTime? DueDate { get; set; }
            public string Prioritas { get; set; } = "";
            public bool Milestone { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public DateTime? DoneAt { get; set; }
            public bool DariTemplate { get; set; }
            public bool Terlambat => DueDate.HasValue && DueDate.Value.Date < DateTime.Today && Status < (int)TaskProgressStatus.Selesai;
        }

        public class AddChecklist : FormKegiatan
        {
            public NewChecklist Row { get; set; } = new();
            public List<GwaVM.OpsiItem> PrioritasOpsi { get; set; } = GwaVM.Opsi<Priority>();
            public List<GwaVM.OpsiItem> MilestoneOpsi { get; set; } = GwaVM.Opsi<MilestoneFlag>();
            public AddChecklist() { }
            public AddChecklist(int activityId) { Muat(activityId); Row.ActivityId = activityId; }
        }

        public class EditChecklist : AddChecklist
        {
            public EditChecklist(string encKey)
            {
                var obj = LibExtensions.Load<ActivityChecklist>(encKey, "Checklist");
                Muat(obj.ActivityId);
                Row = new NewChecklist
                {
                    ActivityId = obj.ActivityId,
                    SectionId = obj.SectionId,
                    Title = obj.Title,
                    Description = obj.Description,
                    DueDate = Opsi.KeDateTime(obj.DueDate),
                    PicMemberId = obj.PicMemberId,
                    Priority = (int)obj.Priority,
                    IsMilestone = (int)obj.IsMilestone
                };
            }
        }

        public class NewChecklist
        {
            public int ActivityId { get; set; }

            [Required(ErrorMessage = "Judul harus diisi")]
            [StringLength(200, ErrorMessage = "Judul maksimal 200 karakter")]
            [Display(Name = "Pekerjaan")]
            public string? Title { get; set; }

            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Display(Name = "Seksi")]
            public int? SectionId { get; set; }

            [Display(Name = "PIC")]
            public int? PicMemberId { get; set; }

            [Display(Name = "Tenggat")]
            public DateTime? DueDate { get; set; }

            [Display(Name = "Prioritas")]
            public int Priority { get; set; } = (int)GWAShared.Enums.Priority.Sedang;

            [Display(Name = "Jenis")]
            public int IsMilestone { get; set; } = (int)MilestoneFlag.Biasa;
        }

        public class GenerateInput
        {
            public int ActivityId { get; set; }
            public int TemplateId { get; set; }
        }

        // ================= Rombongan
        public class ListRombongan
        {
            public List<RombonganRow> Rows { get; set; }
            public ListRombongan(int activityId)
            {
                Rows = ActivityTripMember.GetDataList(activityId).Select(x => new RombonganRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Nama = x.DisplayName,
                    Tamu = !x.MemberId.HasValue,
                    Peran = EnumHelper.Text(x.TripRole),
                    Vehicle = x.Vehicle,
                    Room = x.Room,
                    KontakDarurat = string.Join(" · ", new[] { x.EmergencyContactName, x.EmergencyContactPhone }.Where(s => !string.IsNullOrWhiteSpace(s))),
                    Izin = EnumHelper.Text(x.PermissionStatus),
                    IzinFile = x.PermissionFileName,
                    Notes = x.Notes
                }).OrderBy(x => x.Peran).ThenBy(x => x.Nama).ToList();
            }
        }

        public class RombonganRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Nama { get; set; } = "";
            public bool Tamu { get; set; }
            public string Peran { get; set; } = "";
            public string? Vehicle { get; set; }
            public string? Room { get; set; }
            public string? KontakDarurat { get; set; }
            public string Izin { get; set; } = "";
            public string? IzinFile { get; set; }
            public string? Notes { get; set; }
        }

        public class AddRombongan : FormKegiatan
        {
            public NewRombongan Row { get; set; } = new();
            public List<GwaVM.OpsiItem> PeranOpsi { get; set; } = GwaVM.Opsi<TripRole>();
            public List<GwaVM.OpsiItem> IzinOpsi { get; set; } = GwaVM.Opsi<TripPermissionStatus>();
            public string? IzinFile { get; set; }
            public AddRombongan() { }
            public AddRombongan(int activityId) { Muat(activityId); Row.ActivityId = activityId; }
        }

        public class EditRombongan : AddRombongan
        {
            public string Nama { get; set; } = "";
            public EditRombongan(string encKey)
            {
                var obj = LibExtensions.Load<ActivityTripMember>(encKey, "Rombongan");
                Muat(obj.ActivityId, false);
                Nama = obj.DisplayName;
                IzinFile = obj.PermissionFileName;
                Row = new NewRombongan
                {
                    ActivityId = obj.ActivityId,
                    MemberId = obj.MemberId,
                    GuestName = obj.GuestName,
                    TripRole = (int)obj.TripRole,
                    Vehicle = obj.Vehicle,
                    Room = obj.Room,
                    EmergencyContactName = obj.EmergencyContactName,
                    EmergencyContactPhone = obj.EmergencyContactPhone,
                    PermissionStatus = (int)obj.PermissionStatus,
                    Notes = obj.Notes
                };
            }
        }

        public class NewRombongan
        {
            public int ActivityId { get; set; }

            [Display(Name = "Anggota")]
            public int? MemberId { get; set; }

            [StringLength(150, ErrorMessage = "Nama tamu maksimal 150 karakter")]
            [Display(Name = "Nama Tamu (bila bukan anggota)")]
            public string? GuestName { get; set; }

            [Display(Name = "Peran")]
            public int TripRole { get; set; } = (int)GWAShared.Enums.TripRole.Peserta;

            [StringLength(100, ErrorMessage = "Kendaraan maksimal 100 karakter")]
            [Display(Name = "Kendaraan")]
            public string? Vehicle { get; set; }

            [StringLength(100, ErrorMessage = "Kamar maksimal 100 karakter")]
            [Display(Name = "Kamar")]
            public string? Room { get; set; }

            [StringLength(150, ErrorMessage = "Nama kontak maksimal 150 karakter")]
            [Display(Name = "Kontak Darurat")]
            public string? EmergencyContactName { get; set; }

            [StringLength(30, ErrorMessage = "Telepon maksimal 30 karakter")]
            [Display(Name = "Telepon Darurat")]
            public string? EmergencyContactPhone { get; set; }

            [Display(Name = "Surat Izin")]
            public int PermissionStatus { get; set; } = (int)TripPermissionStatus.Belum;

            [StringLength(255, ErrorMessage = "Catatan maksimal 255 karakter")]
            [Display(Name = "Catatan")]
            public string? Notes { get; set; }
        }

        // ================= Template checklist (master)
        public class ListTemplate
        {
            public List<TemplateRow> Rows { get; set; } = ChecklistTemplate.GetDataList(FilterAktif.Semua).Select(x => new TemplateRow
            {
                EncDecrKey = x.EncDecrKey,
                Id = x.Id,
                Name = x.Name,
                Jenis = x.ActivityType.HasValue ? EnumHelper.Text(x.ActivityType.Value) : "Semua jenis",
                Description = x.Description,
                JumlahItem = x.GetItems(true).Count,
                Aktif = x.IsActive == ActiveStatus.Aktif
            }).OrderBy(x => x.Name).ToList();
        }

        public class TemplateRow
        {
            public string EncDecrKey { get; set; } = "";
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Jenis { get; set; } = "";
            public string? Description { get; set; }
            public int JumlahItem { get; set; }
            public bool Aktif { get; set; }
        }

        public class AddTemplate
        {
            public NewTemplate Row { get; set; } = new();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = GwaVM.Opsi<ActivityType>();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
        }

        public class EditTemplate : AddTemplate
        {
            public EditTemplate(string encKey)
            {
                var obj = LibExtensions.Load<ChecklistTemplate>(encKey, "Template");
                Row = new NewTemplate { Name = obj.Name, ActivityType = obj.ActivityType.HasValue ? (int)obj.ActivityType.Value : null, Description = obj.Description, IsActive = (int)obj.IsActive };
            }
        }

        public class NewTemplate
        {
            [Required(ErrorMessage = "Nama template harus diisi")]
            [StringLength(150, ErrorMessage = "Nama maksimal 150 karakter")]
            [Display(Name = "Nama Template")]
            public string? Name { get; set; }

            [Display(Name = "Untuk Jenis Kegiatan")]
            public int? ActivityType { get; set; }

            [StringLength(255, ErrorMessage = "Keterangan maksimal 255 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        // Item template: modal _Item (+ _ItemList), tambah inline.
        public class Item
        {
            public string Key { get; set; } = "";
            public string Nama { get; set; } = "";
            public NewItem Input { get; set; } = new();
            public List<GwaVM.OpsiItem> PrioritasOpsi { get; set; } = GwaVM.Opsi<Priority>();
            public List<GwaVM.OpsiItem> MilestoneOpsi { get; set; } = GwaVM.Opsi<MilestoneFlag>();
            public ItemList List { get; set; }

            public Item(string encKey)
            {
                var obj = LibExtensions.Load<ChecklistTemplate>(encKey, "Template");
                Key = obj.EncDecrKey;
                Nama = obj.Name;
                List = new ItemList(obj.Id);
            }
        }

        public class ItemList
        {
            public List<ItemRow> Rows { get; set; }
            public ItemList(int templateId)
            {
                Rows = ChecklistTemplateItem.GetDataList(templateId).Select(x => new ItemRow
                {
                    EncDecrKey = x.EncDecrKey,
                    SeqNo = x.SeqNo,
                    Title = x.Title,
                    SectionName = x.SectionName,
                    OffsetDay = x.OffsetDay,
                    Prioritas = EnumHelper.Text(x.Priority),
                    Milestone = x.IsMilestone == MilestoneFlag.Milestone
                }).OrderBy(x => x.SeqNo).ToList();
            }
            public ItemList(string encKey) : this(LibExtensions.Load<ChecklistTemplate>(encKey, "Template").Id)
            {
            }
        }

        public class ItemRow
        {
            public string EncDecrKey { get; set; } = "";
            public int SeqNo { get; set; }
            public string Title { get; set; } = "";
            public string? SectionName { get; set; }
            public int OffsetDay { get; set; }
            public string Prioritas { get; set; } = "";
            public bool Milestone { get; set; }
        }

        public class NewItem
        {
            public string Key { get; set; } = "";

            [Required(ErrorMessage = "Pekerjaan harus diisi")]
            [StringLength(200, ErrorMessage = "Pekerjaan maksimal 200 karakter")]
            [Display(Name = "Pekerjaan")]
            public string? Title { get; set; }

            [StringLength(100, ErrorMessage = "Seksi maksimal 100 karakter")]
            [Display(Name = "Seksi (nama)")]
            public string? SectionName { get; set; }

            [Range(-365, 365, ErrorMessage = "Hari -365 sampai 365")]
            [Display(Name = "H± (hari dari mulai)")]
            public int OffsetDay { get; set; }

            [Display(Name = "Prioritas")]
            public int Priority { get; set; } = (int)GWAShared.Enums.Priority.Sedang;

            [Display(Name = "Jenis")]
            public int IsMilestone { get; set; } = (int)MilestoneFlag.Biasa;
        }
    }
}
