using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Anggota;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Anggota
{
    /// <summary>
    /// Anggota › Rekrutmen: open recruitment (t_recruitment) dan pendaftarnya (t_recruitment_applicant).
    /// Draft → Dibuka → Ditutup. Pendaftar mendaftar tanpa login di /rekrutmen; status pendaftar hanya maju,
    /// Diterima / Ditolak final, dan Diterima membuat data anggota baru.
    /// </summary>
    public class RekrutmenVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = Method.GetDataList().ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public string? Periode { get; set; }
            public DateTime OpenDate { get; set; }
            public DateTime CloseDate { get; set; }
            public int? Quota { get; set; }
            public bool Umum { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public int JumlahPendaftar { get; set; }
            public int JumlahDiterima { get; set; }
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public List<GwaVM.OpsiItem> AudiensOpsi { get; set; } = GwaVM.Opsi<RecruitmentAudience>();

            public Add()
            {
                Row.PeriodId = Opsi.PeriodeAktifId();
                Row.OpenDate = DateTime.Today;
                Row.CloseDate = DateTime.Today.AddDays(14).AddHours(23).AddMinutes(59);
            }
        }

        public class Edit : Add
        {
            public string StatusText { get; set; } = "";

            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<Recruitment>(encKey, "Rekrutmen");
                StatusText = EnumHelper.Text(obj.Status);
                Row = new NewRow
                {
                    PeriodId = obj.PeriodId,
                    Title = obj.Title,
                    Description = obj.Description,
                    OpenDate = obj.OpenDate,
                    CloseDate = obj.CloseDate,
                    Quota = obj.Quota,
                    AcceptPublic = (int)obj.AcceptPublic
                };
            }
        }

        public class NewRow
        {
            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }

            [Required(ErrorMessage = "Judul harus diisi")]
            [StringLength(150, ErrorMessage = "Judul maksimal 150 karakter")]
            [Display(Name = "Judul")]
            public string? Title { get; set; }

            [Display(Name = "Deskripsi / Persyaratan")]
            public string? Description { get; set; }

            [Required(ErrorMessage = "Waktu buka harus diisi")]
            [Display(Name = "Dibuka")]
            public DateTime? OpenDate { get; set; }

            [Required(ErrorMessage = "Waktu tutup harus diisi")]
            [Display(Name = "Ditutup")]
            public DateTime? CloseDate { get; set; }

            [Range(1, 10000, ErrorMessage = "Kuota minimal 1")]
            [Display(Name = "Kuota (kosong = tanpa batas)")]
            public int? Quota { get; set; }

            [Display(Name = "Pendaftar")]
            public int AcceptPublic { get; set; } = (int)RecruitmentAudience.HanyaMahasiswa;
        }

        // ---------- Pendaftar (modal _Pendaftar + grid _PendaftarList)
        public class Pendaftar
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public string Ringkasan { get; set; } = "";
            public PendaftarList List { get; set; }

            public Pendaftar(string encKey)
            {
                var obj = LibExtensions.Load<Recruitment>(encKey, "Rekrutmen");
                Key = obj.EncDecrKey;
                Judul = obj.Title;
                List = new PendaftarList(obj.Id);
                var diterima = List.Rows.Count(x => x.Status == (int)ApplicantStatus.Accepted);
                Ringkasan = $"{List.Rows.Count} pendaftar, {diterima} diterima" + (obj.Quota.HasValue ? $" dari kuota {obj.Quota}" : "");
            }
        }

        public class PendaftarList
        {
            public List<PendaftarRow> Rows { get; set; }

            public PendaftarList(int recruitmentId)
            {
                Rows = RecruitmentApplicant.GetDataList(recruitmentId).Select(x => new PendaftarRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Fullname = x.Fullname,
                    Kategori = EnumHelper.Text(x.ApplicantCategory),
                    Nim = x.Nim,
                    Email = x.Email,
                    Phone = x.Phone,
                    Prodi = x.ProgramStudi ?? x.Institution,
                    Angkatan = x.Angkatan,
                    Motivation = x.Motivation,
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    Notes = x.Notes,
                    InsDate = x.InsDate,
                    SudahAnggota = x.MemberId.HasValue
                }).OrderBy(x => x.InsDate).ToList();
            }
            public PendaftarList(string encKey) : this(LibExtensions.Load<Recruitment>(encKey, "Rekrutmen").Id)
            {
            }
        }

        public class PendaftarRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Fullname { get; set; } = "";
            public string Kategori { get; set; } = "";
            public string? Nim { get; set; }
            public string Email { get; set; } = "";
            public string? Phone { get; set; }
            public string? Prodi { get; set; }
            public short? Angkatan { get; set; }
            public string? Motivation { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public string? Notes { get; set; }
            public DateTime InsDate { get; set; }
            public bool SudahAnggota { get; set; }
        }

        public class ReviewInput
        {
            public string Key { get; set; } = "";
            public int Status { get; set; }
            public string? Notes { get; set; }
            /// <summary>Section awal anggota baru (wajib saat Diterima bila pendaftar belum anggota).</summary>
            public int? SectionId { get; set; }
        }

        public class Method
        {
            public static IEnumerable<Row> GetDataList()
            {
                var periode = Opsi.Periode().ToDictionary(x => x.Value, x => x.Text);
                var pendaftar = RecruitmentApplicant.GetDataList().GroupBy(x => x.RecruitmentId)
                    .ToDictionary(g => g.Key, g => (Total: g.Count(), Diterima: g.Count(x => x.Status == ApplicantStatus.Accepted)));
                return Recruitment.GetDataList().Select(x => new Row
                {
                    EncDecrKey = x.EncDecrKey,
                    Id = x.Id,
                    Title = x.Title,
                    Periode = x.PeriodId.HasValue && periode.TryGetValue(x.PeriodId.Value, out var p) ? p : null,
                    OpenDate = x.OpenDate,
                    CloseDate = x.CloseDate,
                    Quota = x.Quota,
                    Umum = x.AcceptPublic == RecruitmentAudience.TermasukUmum,
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    JumlahPendaftar = pendaftar.TryGetValue(x.Id, out var n) ? n.Total : 0,
                    JumlahDiterima = pendaftar.TryGetValue(x.Id, out var n2) ? n2.Diterima : 0
                }).OrderByDescending(x => x.OpenDate);
            }

            public static void Simpan(NewRow input, string user)
            {
                Recruitment.Save(new Recruitment.NewRow
                {
                    PeriodId = input.PeriodId,
                    Title = input.Title ?? "",
                    Description = input.Description,
                    OpenDate = input.OpenDate ?? DateTime.Now,
                    CloseDate = input.CloseDate ?? DateTime.Now,
                    Quota = input.Quota,
                    AcceptPublic = (RecruitmentAudience)input.AcceptPublic
                }, user).Ok();
            }

            public static void Update(string encKey, NewRow input, string user)
            {
                var obj = LibExtensions.Load<Recruitment>(encKey, "Rekrutmen");
                obj.PeriodId = input.PeriodId;
                obj.Title = input.Title ?? "";
                obj.Description = input.Description;
                obj.OpenDate = input.OpenDate ?? obj.OpenDate;
                obj.CloseDate = input.CloseDate ?? obj.CloseDate;
                obj.Quota = input.Quota;
                obj.AcceptPublic = (RecruitmentAudience)input.AcceptPublic;
                obj.Update(user).Ok();
            }

            public static Recruitment Load(string? encKey) => LibExtensions.Load<Recruitment>(encKey, "Rekrutmen");

            public static void Review(ReviewInput input, string user)
            {
                LibExtensions.Load<RecruitmentApplicant>(input.Key, "Pendaftar").Review((ApplicantStatus)input.Status, input.Notes, user, input.SectionId).Ok();
            }
        }
    }
}
