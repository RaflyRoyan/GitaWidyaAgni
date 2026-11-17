using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Program;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.ProgramKerja
{
    /// <summary>
    /// Program Kerja: tab Program (t_program), Tugas (t_program_task), dan Timeline (t_timeline_plan).
    /// Program: Direncanakan → Berjalan → Selesai, atau Dibatalkan. Tugas: Belum → Proses → Selesai (bisa dibuka lagi) / Batal.
    /// Timeline: Rencana → Terjadwal (tautkan kegiatan) → Selesai / Batal.
    /// </summary>
    public class ProgramKerjaVM
    {
        public class Index
        {
        }

        // ================= Program
        public class ListProgram
        {
            public List<ProgramRow> Rows { get; set; } = Method.GetProgramList().ToList();
        }

        public class ProgramRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string Periode { get; set; } = "";
            public string? Pic { get; set; }
            public string? Bidang { get; set; }
            public string? Jangka { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public decimal BudgetAmount { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public int JumlahTugas { get; set; }
            public int TugasSelesai { get; set; }
            public int Progres => JumlahTugas == 0 ? 0 : (int)Math.Round(TugasSelesai * 100.0 / JumlahTugas);
        }

        public class AddProgram
        {
            public NewProgram Row { get; set; } = new();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public List<GwaVM.OpsiItem> ProgramOpsi { get; set; } = Opsi.Program();
            public List<GwaVM.OpsiItem> DivisiOpsi { get; set; } = Opsi.Divisi();
            public List<GwaVM.OpsiItem> JangkaOpsi { get; set; } = GwaVM.Opsi<ProgramTerm>();

            public AddProgram()
            {
                Row.PeriodId = Opsi.PeriodeAktifId();
            }
        }

        public class EditProgram : AddProgram
        {
            public string StatusText { get; set; } = "";

            public EditProgram(string encKey)
            {
                var obj = LibExtensions.Load<WorkProgram>(encKey, "Program");
                StatusText = EnumHelper.Text(obj.Status);
                ProgramOpsi = ProgramOpsi.Where(x => x.Value != obj.Id).ToList();
                Row = new NewProgram
                {
                    PeriodId = obj.PeriodId,
                    PreviousProgramId = obj.PreviousProgramId,
                    DivisionId = obj.DivisionId,
                    Term = (int?)obj.Term,
                    PicMemberId = obj.PicMemberId,
                    Title = obj.Title,
                    Description = obj.Description,
                    StartDate = Opsi.KeDateTime(obj.StartDate),
                    EndDate = Opsi.KeDateTime(obj.EndDate),
                    BudgetAmount = obj.BudgetAmount
                };
            }
        }

        public class NewProgram
        {
            [Required(ErrorMessage = "Pilih periode")]
            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }

            [Required(ErrorMessage = "Nama program harus diisi")]
            [StringLength(200, ErrorMessage = "Nama program maksimal 200 karakter")]
            [Display(Name = "Nama Program")]
            public string? Title { get; set; }

            [Display(Name = "Deskripsi")]
            public string? Description { get; set; }

            [Display(Name = "Penanggung Jawab")]
            public int? PicMemberId { get; set; }

            [Display(Name = "Lanjutan dari Program")]
            public int? PreviousProgramId { get; set; }

            [Display(Name = "Bidang")]
            public int? DivisionId { get; set; }

            [Display(Name = "Jangka")]
            public int? Term { get; set; }

            [Display(Name = "Mulai")]
            public DateTime? StartDate { get; set; }

            [Display(Name = "Selesai")]
            public DateTime? EndDate { get; set; }

            [Range(0, 999999999999, ErrorMessage = "Anggaran tidak boleh negatif")]
            [Display(Name = "Rencana Anggaran (Rp)")]
            public decimal BudgetAmount { get; set; }
        }

        // ================= Tugas
        public class ListTugas
        {
            public List<TugasRow> Rows { get; set; } = Method.GetTugasList().ToList();
        }

        public class TugasRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string? Program { get; set; }
            public string? Divisi { get; set; }
            public string? Pj { get; set; }
            public string Prioritas { get; set; } = "";
            public bool Milestone { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? DueDate { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public DateTime? CompletedAt { get; set; }
            public bool Terlambat => DueDate.HasValue && DueDate.Value.Date < DateTime.Today && Status < (int)TaskProgressStatus.Selesai;
        }

        public class AddTugas
        {
            public NewTugas Row { get; set; } = new();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public List<GwaVM.OpsiItem> ProgramOpsi { get; set; } = Opsi.Program(hanyaTerbuka: true);
            public List<GwaVM.OpsiItem> DivisiOpsi { get; set; } = Opsi.Divisi();
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public List<GwaVM.OpsiItem> PrioritasOpsi { get; set; } = GwaVM.Opsi<Priority>();
            public List<GwaVM.OpsiItem> MilestoneOpsi { get; set; } = GwaVM.Opsi<MilestoneFlag>();

            public AddTugas()
            {
                Row.PeriodId = Opsi.PeriodeAktifId();
            }
        }

        public class EditTugas : AddTugas
        {
            public string StatusText { get; set; } = "";

            public EditTugas(string encKey)
            {
                var obj = LibExtensions.Load<ProgramTask>(encKey, "Tugas");
                StatusText = EnumHelper.Text(obj.Status);
                Row = new NewTugas
                {
                    PeriodId = obj.PeriodId,
                    ProgramId = obj.ProgramId,
                    DivisionId = obj.DivisionId,
                    ResponsibleMemberId = obj.ResponsibleMemberId,
                    Title = obj.Title,
                    Description = obj.Description,
                    Priority = (int)obj.Priority,
                    StartDate = Opsi.KeDateTime(obj.StartDate),
                    DueDate = Opsi.KeDateTime(obj.DueDate),
                    IsMilestone = (int)obj.IsMilestone
                };
            }
        }

        public class NewTugas
        {
            [Display(Name = "Periode (tugas umum)")]
            public int? PeriodId { get; set; }

            [Display(Name = "Program")]
            public int? ProgramId { get; set; }

            [Required(ErrorMessage = "Judul tugas harus diisi")]
            [StringLength(200, ErrorMessage = "Judul maksimal 200 karakter")]
            [Display(Name = "Judul Tugas")]
            public string? Title { get; set; }

            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Display(Name = "Divisi")]
            public int? DivisionId { get; set; }

            [Display(Name = "Penanggung Jawab")]
            public int? ResponsibleMemberId { get; set; }

            [Display(Name = "Prioritas")]
            public int Priority { get; set; } = (int)GWAShared.Enums.Priority.Sedang;

            [Display(Name = "Jenis")]
            public int IsMilestone { get; set; } = (int)MilestoneFlag.Biasa;

            [Display(Name = "Mulai")]
            public DateTime? StartDate { get; set; }

            [Display(Name = "Tenggat")]
            public DateTime? DueDate { get; set; }
        }

        // ================= Timeline
        public class ListTimeline
        {
            public List<TimelineRow> Rows { get; set; } = Method.GetTimelineList().ToList();
        }

        public class TimelineRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string Jenis { get; set; } = "";
            public string Bulan { get; set; } = "";
            public int Urut { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string? Program { get; set; }
            public string? Pic { get; set; }
            public string? Kegiatan { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
        }

        public class AddTimeline
        {
            public NewTimeline Row { get; set; } = new();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public List<GwaVM.OpsiItem> ProgramOpsi { get; set; } = Opsi.Program(hanyaTerbuka: true);
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = GwaVM.Opsi<TimelinePlanType>();
            public List<GwaVM.OpsiItem> BulanOpsi { get; set; } = Enumerable.Range(1, 12)
                .Select(m => new GwaVM.OpsiItem(m, new DateTime(2000, m, 1).ToString("MMMM", new System.Globalization.CultureInfo("id-ID")))).ToList();

            public AddTimeline()
            {
                Row.PeriodId = Opsi.PeriodeAktifId();
                Row.PlanYear = (short)DateTime.Today.Year;
                Row.PlanMonth = DateTime.Today.Month;
            }
        }

        public class EditTimeline : AddTimeline
        {
            public EditTimeline(string encKey)
            {
                var obj = LibExtensions.Load<TimelinePlan>(encKey, "Timeline");
                Row = new NewTimeline
                {
                    PeriodId = obj.PeriodId,
                    Title = obj.Title,
                    Description = obj.Description,
                    PlanType = (int)obj.PlanType,
                    PlanYear = obj.PlanYear,
                    PlanMonth = obj.PlanMonth,
                    StartDate = Opsi.KeDateTime(obj.StartDate),
                    EndDate = Opsi.KeDateTime(obj.EndDate),
                    ProgramId = obj.ProgramId,
                    PicMemberId = obj.PicMemberId
                };
            }
        }

        public class NewTimeline
        {
            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }

            [Required(ErrorMessage = "Judul harus diisi")]
            [StringLength(200, ErrorMessage = "Judul maksimal 200 karakter")]
            [Display(Name = "Judul")]
            public string? Title { get; set; }

            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Display(Name = "Jenis")]
            public int PlanType { get; set; } = (int)TimelinePlanType.Event;

            [Range(2000, 2100, ErrorMessage = "Tahun tidak valid")]
            [Display(Name = "Tahun")]
            public short PlanYear { get; set; }

            [Range(1, 12, ErrorMessage = "Bulan tidak valid")]
            [Display(Name = "Bulan")]
            public int PlanMonth { get; set; }

            [Display(Name = "Tanggal Mulai (mengisi tahun & bulan)")]
            public DateTime? StartDate { get; set; }

            [Display(Name = "Tanggal Selesai")]
            public DateTime? EndDate { get; set; }

            [Display(Name = "Program")]
            public int? ProgramId { get; set; }

            [Display(Name = "PIC")]
            public int? PicMemberId { get; set; }
        }

        public class TautInput
        {
            public string Key { get; set; } = "";
            public int ActivityId { get; set; }
        }

        public class Method
        {
            private static readonly System.Globalization.CultureInfo ID = new("id-ID");

            public static IEnumerable<ProgramRow> GetProgramList()
            {
                var tugas = ProgramTask.GetDataList().Where(x => x.ProgramId.HasValue && x.Status != TaskProgressStatus.Batal)
                    .GroupBy(x => x.ProgramId!.Value)
                    .ToDictionary(g => g.Key, g => (Total: g.Count(), Selesai: g.Count(x => x.Status == TaskProgressStatus.Selesai)));
                return WorkProgram.GetDataList().Select(x => new ProgramRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Title = x.Title,
                    Periode = x.PeriodName,
                    Pic = x.PicMemberName,
                    Bidang = x.DivisionName,
                    Jangka = x.Term == null ? null : EnumHelper.Text(x.Term.Value),
                    StartDate = Opsi.KeDateTime(x.StartDate),
                    EndDate = Opsi.KeDateTime(x.EndDate),
                    BudgetAmount = x.BudgetAmount,
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    JumlahTugas = tugas.TryGetValue(x.Id, out var t) ? t.Total : 0,
                    TugasSelesai = tugas.TryGetValue(x.Id, out var t2) ? t2.Selesai : 0
                }).OrderBy(x => x.Status == (int)ProgramStatus.Cancelled).ThenByDescending(x => x.StartDate).ThenBy(x => x.Title);
            }

            public static void SimpanProgram(NewProgram input, string user)
            {
                WorkProgram.Save(new WorkProgram.NewRow
                {
                    PeriodId = input.PeriodId ?? 0,
                    PreviousProgramId = input.PreviousProgramId,
                    DivisionId = input.DivisionId,
                    Term = (ProgramTerm?)input.Term,
                    PicMemberId = input.PicMemberId,
                    Title = input.Title ?? "",
                    Description = input.Description,
                    StartDate = Opsi.Tanggal(input.StartDate),
                    EndDate = Opsi.Tanggal(input.EndDate),
                    BudgetAmount = input.BudgetAmount
                }, user).Ok();
            }

            public static void UpdateProgram(string encKey, NewProgram input, string user)
            {
                var obj = LibExtensions.Load<WorkProgram>(encKey, "Program");
                obj.PreviousProgramId = input.PreviousProgramId;
                obj.DivisionId = input.DivisionId;
                obj.Term = (ProgramTerm?)input.Term;
                obj.PicMemberId = input.PicMemberId;
                obj.Title = input.Title ?? "";
                obj.Description = input.Description;
                obj.StartDate = Opsi.Tanggal(input.StartDate);
                obj.EndDate = Opsi.Tanggal(input.EndDate);
                obj.BudgetAmount = input.BudgetAmount;
                obj.Update(user).Ok();
            }

            public static void StatusProgram(GwaVM.StatusInput input, string user)
                => LibExtensions.Load<WorkProgram>(input.Key, "Program").SetStatus((ProgramStatus)input.Status, user).Ok();

            public static void HapusProgram(string? key, string user)
                => LibExtensions.Load<WorkProgram>(key, "Program").Delete(user).Ok();

            public static IEnumerable<TugasRow> GetTugasList()
            {
                var divisi = Opsi.Divisi().ToDictionary(x => x.Value, x => x.Text);
                return ProgramTask.GetDataList().Select(x => new TugasRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Title = x.Title,
                    Program = x.ProgramTitle,
                    Divisi = x.DivisionId.HasValue && divisi.TryGetValue(x.DivisionId.Value, out var d) ? d : null,
                    Pj = x.ResponsibleMemberName,
                    Prioritas = EnumHelper.Text(x.Priority),
                    Milestone = x.IsMilestone == MilestoneFlag.Milestone,
                    StartDate = Opsi.KeDateTime(x.StartDate),
                    DueDate = Opsi.KeDateTime(x.DueDate),
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    CompletedAt = x.CompletedAt
                }).OrderBy(x => x.Status).ThenBy(x => x.DueDate ?? DateTime.MaxValue);
            }

            public static void SimpanTugas(NewTugas input, string user)
            {
                ProgramTask.Save(new ProgramTask.NewRow
                {
                    PeriodId = input.PeriodId,
                    ProgramId = input.ProgramId,
                    DivisionId = input.DivisionId,
                    ResponsibleMemberId = input.ResponsibleMemberId,
                    Title = input.Title ?? "",
                    Description = input.Description,
                    Priority = (Priority)input.Priority,
                    StartDate = Opsi.Tanggal(input.StartDate),
                    DueDate = Opsi.Tanggal(input.DueDate),
                    IsMilestone = (MilestoneFlag)input.IsMilestone
                }, user).Ok();
            }

            public static void UpdateTugas(string encKey, NewTugas input, string user)
            {
                var obj = LibExtensions.Load<ProgramTask>(encKey, "Tugas");
                obj.DivisionId = input.DivisionId;
                obj.ResponsibleMemberId = input.ResponsibleMemberId;
                obj.Title = input.Title ?? "";
                obj.Description = input.Description;
                obj.Priority = (Priority)input.Priority;
                obj.StartDate = Opsi.Tanggal(input.StartDate);
                obj.DueDate = Opsi.Tanggal(input.DueDate);
                obj.IsMilestone = (MilestoneFlag)input.IsMilestone;
                obj.Update(user).Ok();
            }

            public static void StatusTugas(GwaVM.StatusInput input, string user)
                => LibExtensions.Load<ProgramTask>(input.Key, "Tugas").SetStatus((TaskProgressStatus)input.Status, user).Ok();

            public static void HapusTugas(string? key, string user)
                => LibExtensions.Load<ProgramTask>(key, "Tugas").Delete(user).Ok();

            public static IEnumerable<TimelineRow> GetTimelineList()
            {
                var program = WorkProgram.GetDataList().ToDictionary(x => x.Id, x => x.Title);
                var kegiatan = Activity.GetDataList().ToDictionary(x => x.Id, x => x.Title);
                return TimelinePlan.GetDataList().Select(x => new TimelineRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Title = x.Title,
                    Jenis = EnumHelper.Text(x.PlanType),
                    Bulan = new DateTime(x.PlanYear, Math.Clamp(x.PlanMonth, 1, 12), 1).ToString("MMMM yyyy", ID),
                    Urut = x.PlanYear * 100 + x.PlanMonth,
                    StartDate = Opsi.KeDateTime(x.StartDate),
                    EndDate = Opsi.KeDateTime(x.EndDate),
                    Program = x.ProgramId.HasValue && program.TryGetValue(x.ProgramId.Value, out var p) ? p : null,
                    Pic = x.PicMemberName,
                    Kegiatan = x.ActivityId.HasValue && kegiatan.TryGetValue(x.ActivityId.Value, out var k) ? k : null,
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status)
                }).OrderBy(x => x.Urut).ThenBy(x => x.StartDate);
            }

            public static void SimpanTimeline(NewTimeline input, string user)
            {
                TimelinePlan.Save(new TimelinePlan.NewRow
                {
                    PeriodId = input.PeriodId,
                    Title = input.Title ?? "",
                    Description = input.Description,
                    PlanType = (TimelinePlanType)input.PlanType,
                    PlanYear = input.PlanYear,
                    PlanMonth = input.PlanMonth,
                    StartDate = Opsi.Tanggal(input.StartDate),
                    EndDate = Opsi.Tanggal(input.EndDate),
                    ProgramId = input.ProgramId,
                    PicMemberId = input.PicMemberId
                }, user).Ok();
            }

            public static void UpdateTimeline(string encKey, NewTimeline input, string user)
            {
                var obj = LibExtensions.Load<TimelinePlan>(encKey, "Timeline");
                obj.PeriodId = input.PeriodId;
                obj.Title = input.Title ?? "";
                obj.Description = input.Description;
                obj.PlanType = (TimelinePlanType)input.PlanType;
                obj.PlanYear = input.PlanYear;
                obj.PlanMonth = input.PlanMonth;
                obj.StartDate = Opsi.Tanggal(input.StartDate);
                obj.EndDate = Opsi.Tanggal(input.EndDate);
                obj.ProgramId = input.ProgramId;
                obj.PicMemberId = input.PicMemberId;
                obj.Update(user).Ok();
            }

            public static void StatusTimeline(GwaVM.StatusInput input, string user)
                => LibExtensions.Load<TimelinePlan>(input.Key, "Timeline").SetStatus((TimelinePlanStatus)input.Status, user).Ok();

            public static void TautKegiatan(TautInput input, string user)
                => LibExtensions.Load<TimelinePlan>(input.Key, "Timeline").LinkActivity(input.ActivityId, user).Ok();

            public static void HapusTimeline(string? key, string user)
                => LibExtensions.Load<TimelinePlan>(key, "Timeline").Delete(user).Ok();
        }
    }
}
