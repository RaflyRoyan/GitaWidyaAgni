using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Prestasi;
using GWALib.Lib.Transaksi.Prestasi;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Prestasi
{
    /// <summary>
    /// Prestasi › Lomba &amp; Prestasi: tab Lomba (+ peserta), Prestasi, Materi, Poin (+ generate &amp; leaderboard), Aturan Poin.
    /// Prestasi anggota otomatis mendapat poin Prestasi sesuai aturan aktif.
    /// </summary>
    public partial class PrestasiVM
    {
        public class Index
        {
        }

        // ================= Lomba
        public class ListLomba
        {
            public List<LombaRow> Rows { get; set; } = Competition.GetDataList(jmldata: -1).Select(x => new LombaRow
            {
                EncDecrKey = x.EncDecrKey,
                Name = x.Name,
                Organizer = x.Organizer,
                Level = EnumHelper.Text(x.Level),
                Category = x.Category,
                RegistrationDeadline = x.RegistrationDeadline,
                StartDate = Opsi.KeDateTime(x.StartDate),
                EndDate = Opsi.KeDateTime(x.EndDate),
                Location = x.Location,
                Publik = x.IsPublic == PublicVisibility.Public,
                Disorot = x.IsFeatured == FeaturedStatus.Disorot,
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status)
            }).OrderByDescending(x => x.StartDate).ToList();
        }

        public class LombaRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Organizer { get; set; }
            public string Level { get; set; } = "";
            public string? Category { get; set; }
            public DateTime? RegistrationDeadline { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string? Location { get; set; }
            public bool Publik { get; set; }
            public bool Disorot { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
        }

        public class AddLomba
        {
            public NewLomba Row { get; set; } = new();
            public List<GwaVM.OpsiItem> TingkatOpsi { get; set; } = GwaVM.Opsi<EventLevel>();
            public bool HasPoster { get; set; }
        }

        public class EditLomba : AddLomba
        {
            public string StatusText { get; set; } = "";
            public string Key { get; set; } = "";
            public EditLomba(string encKey)
            {
                var obj = LibExtensions.Load<Competition>(encKey, "Lomba");
                Key = obj.EncDecrKey;
                StatusText = EnumHelper.Text(obj.Status);
                HasPoster = obj.GetPoster() != null;
                Row = new NewLomba
                {
                    Name = obj.Name,
                    Organizer = obj.Organizer,
                    Level = (int)obj.Level,
                    Category = obj.Category,
                    Description = obj.Description,
                    RegistrationDeadline = obj.RegistrationDeadline,
                    StartDate = Opsi.KeDateTime(obj.StartDate),
                    EndDate = Opsi.KeDateTime(obj.EndDate),
                    Location = obj.Location,
                    LinkUrl = obj.LinkUrl,
                    Publik = obj.IsPublic == PublicVisibility.Public,
                    Disorot = obj.IsFeatured == FeaturedStatus.Disorot
                };
            }
        }

        public class NewLomba
        {
            [Required(ErrorMessage = "Nama lomba harus diisi")]
            [StringLength(200, ErrorMessage = "Nama maksimal 200 karakter")]
            [Display(Name = "Nama Lomba")]
            public string? Name { get; set; }

            [StringLength(150, ErrorMessage = "Penyelenggara maksimal 150 karakter")]
            [Display(Name = "Penyelenggara")]
            public string? Organizer { get; set; }

            [Display(Name = "Tingkat")]
            public int Level { get; set; } = (int)EventLevel.Kampus;

            [StringLength(100, ErrorMessage = "Kategori maksimal 100 karakter")]
            [Display(Name = "Kategori")]
            public string? Category { get; set; }

            [Display(Name = "Batas Pendaftaran")]
            public DateTime? RegistrationDeadline { get; set; }

            [Display(Name = "Mulai")]
            public DateTime? StartDate { get; set; }

            [Display(Name = "Selesai")]
            public DateTime? EndDate { get; set; }

            [StringLength(255, ErrorMessage = "Lokasi maksimal 255 karakter")]
            [Display(Name = "Lokasi")]
            public string? Location { get; set; }

            [StringLength(255, ErrorMessage = "Tautan maksimal 255 karakter")]
            [Display(Name = "Tautan Info")]
            public string? LinkUrl { get; set; }

            [Display(Name = "Deskripsi")]
            public string? Description { get; set; }

            [Display(Name = "Tampil di landing page")]
            public bool Publik { get; set; } = true;

            [Display(Name = "Disorot")]
            public bool Disorot { get; set; }
        }

        // Peserta lomba (modal).
        public class Peserta
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public bool BisaDaftar { get; set; }
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public List<CompetitionParticipant> Rows { get; set; }

            public static readonly Dictionary<string, string> PeranList = new()
            {
                [CompetitionParticipant.ROLE_KETUA_TIM] = "Ketua Tim",
                [CompetitionParticipant.ROLE_ANGGOTA] = "Anggota",
                [CompetitionParticipant.ROLE_PEMBIMBING] = "Pembimbing"
            };

            public Peserta(string encKey)
            {
                var obj = LibExtensions.Load<Competition>(encKey, "Lomba");
                Key = obj.EncDecrKey;
                Judul = obj.Name + " · " + EnumHelper.Text(obj.Status) + (obj.RegistrationDeadline.HasValue ? " · daftar s.d. " + obj.RegistrationDeadline.Value.ToString("dd MMM yyyy HH:mm") : "");
                BisaDaftar = obj.Status == CompetitionStatus.Registration;
                Rows = obj.GetParticipants(true).OrderBy(x => x.TeamName).ThenBy(x => x.MemberName).ToList();
            }
        }

        public class PesertaInput
        {
            public string Key { get; set; } = "";
            public int MemberId { get; set; }
            public string? TeamName { get; set; }
            public string? ParticipantRole { get; set; }
        }

        // ================= Prestasi
        public class ListPrestasi
        {
            public List<PrestasiRow> Rows { get; set; }
            public ListPrestasi()
            {
                var lomba = Competition.GetDataList(jmldata: -1).ToDictionary(x => x.Id, x => x.Name);
                Rows = Achievement.GetDataList(jmldata: -1).Select(x => new PrestasiRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Title = x.Title,
                    RankTitle = x.RankTitle,
                    Peraih = x.MemberName ?? x.TeamName ?? "",
                    TeamName = x.TeamName,
                    Lomba = x.CompetitionId.HasValue && lomba.TryGetValue(x.CompetitionId.Value, out var n) ? n : null,
                    Level = EnumHelper.Text(x.Level),
                    AchievedDate = x.AchievedDate.ToDateTime(TimeOnly.MinValue),
                    CertificateFileName = x.CertificateFileName,
                    Publik = x.IsPublic == PublicVisibility.Public,
                    Disorot = x.IsFeatured == FeaturedStatus.Disorot
                }).OrderByDescending(x => x.AchievedDate).ToList();
            }
        }

        public class PrestasiRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string? RankTitle { get; set; }
            public string Peraih { get; set; } = "";
            public string? TeamName { get; set; }
            public string? Lomba { get; set; }
            public string Level { get; set; } = "";
            public DateTime AchievedDate { get; set; }
            public string? CertificateFileName { get; set; }
            public bool Publik { get; set; }
            public bool Disorot { get; set; }
        }

        public class AddPrestasi
        {
            public NewPrestasi Row { get; set; } = new() { AchievedDate = DateTime.Today };
            public List<GwaVM.OpsiItem> LombaOpsi { get; set; } = Opsi.Lomba();
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota(null);
            public List<GwaVM.OpsiItem> TingkatOpsi { get; set; } = GwaVM.Opsi<EventLevel>();
            public string? CertificateFileName { get; set; }
            public bool HasPhoto { get; set; }
            public string? Key { get; set; }
        }

        public class EditPrestasi : AddPrestasi
        {
            public EditPrestasi(string encKey)
            {
                var obj = LibExtensions.Load<Achievement>(encKey, "Prestasi");
                Key = obj.EncDecrKey;
                CertificateFileName = obj.CertificateFileName;
                HasPhoto = obj.GetPhoto() != null;
                Row = new NewPrestasi
                {
                    CompetitionId = obj.CompetitionId,
                    MemberId = obj.MemberId,
                    TeamName = obj.TeamName,
                    Title = obj.Title,
                    RankTitle = obj.RankTitle,
                    Level = (int)obj.Level,
                    AchievedDate = obj.AchievedDate.ToDateTime(TimeOnly.MinValue),
                    Description = obj.Description,
                    Publik = obj.IsPublic == PublicVisibility.Public,
                    Disorot = obj.IsFeatured == FeaturedStatus.Disorot
                };
            }
        }

        public class NewPrestasi
        {
            [Display(Name = "Lomba")]
            public int? CompetitionId { get; set; }

            [Display(Name = "Anggota")]
            public int? MemberId { get; set; }

            [StringLength(150, ErrorMessage = "Nama tim maksimal 150 karakter")]
            [Display(Name = "Nama Tim")]
            public string? TeamName { get; set; }

            [Required(ErrorMessage = "Judul prestasi harus diisi")]
            [StringLength(255, ErrorMessage = "Judul maksimal 255 karakter")]
            [Display(Name = "Judul Prestasi")]
            public string? Title { get; set; }

            [StringLength(100, ErrorMessage = "Peringkat maksimal 100 karakter")]
            [Display(Name = "Peringkat / Gelar")]
            public string? RankTitle { get; set; }

            [Display(Name = "Tingkat (kosong = tingkat lomba)")]
            public int? Level { get; set; }

            [Required(ErrorMessage = "Tanggal harus diisi")]
            [Display(Name = "Tanggal")]
            public DateTime? AchievedDate { get; set; }

            [Display(Name = "Deskripsi")]
            public string? Description { get; set; }

            [Display(Name = "Tampil di landing page")]
            public bool Publik { get; set; } = true;

            [Display(Name = "Disorot")]
            public bool Disorot { get; set; }
        }

        // ================= Materi
        public class ListMateri
        {
            public List<MateriRow> Rows { get; set; }
            public ListMateri()
            {
                var lomba = Competition.GetDataList(jmldata: -1).ToDictionary(x => x.Id, x => x.Name);
                Rows = CompetitionMaterial.GetDataList().Select(x => new MateriRow
                {
                    EncDecrKey = x.EncDecrKey,
                    Title = x.Title,
                    Lomba = x.CompetitionId.HasValue && lomba.TryGetValue(x.CompetitionId.Value, out var n) ? n : "(Umum)",
                    Jenis = EnumHelper.Text(x.MaterialType),
                    FileName = x.FileName,
                    Url = x.Url,
                    Visibility = EnumHelper.Text(x.Visibility),
                    InsDate = x.InsDate
                }).OrderByDescending(x => x.InsDate).ToList();
            }
        }

        public class MateriRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string Lomba { get; set; } = "";
            public string Jenis { get; set; } = "";
            public string? FileName { get; set; }
            public string? Url { get; set; }
            public string Visibility { get; set; } = "";
            public DateTime InsDate { get; set; }
        }

        public class AddMateri
        {
            public NewMateri Row { get; set; } = new();
            public List<GwaVM.OpsiItem> LombaOpsi { get; set; } = Opsi.Lomba();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = GwaVM.Opsi<MaterialType>();
            public List<GwaVM.OpsiItem> VisibilitasOpsi { get; set; } = GwaVM.Opsi<MaterialVisibility>();
            public string? FileName { get; set; }
        }

        public class EditMateri : AddMateri
        {
            public EditMateri(string encKey)
            {
                var obj = LibExtensions.Load<CompetitionMaterial>(encKey, "Materi");
                FileName = obj.FileName;
                Row = new NewMateri
                {
                    CompetitionId = obj.CompetitionId,
                    Title = obj.Title,
                    Description = obj.Description,
                    MaterialType = (int)obj.MaterialType,
                    Url = obj.Url,
                    Visibility = (int)obj.Visibility
                };
            }
        }

        public class NewMateri
        {
            [Display(Name = "Lomba (kosong = materi umum)")]
            public int? CompetitionId { get; set; }

            [Required(ErrorMessage = "Judul materi harus diisi")]
            [StringLength(255, ErrorMessage = "Judul maksimal 255 karakter")]
            [Display(Name = "Judul")]
            public string? Title { get; set; }

            [StringLength(1000, ErrorMessage = "Deskripsi maksimal 1000 karakter")]
            [Display(Name = "Deskripsi")]
            public string? Description { get; set; }

            [Display(Name = "Jenis")]
            public int MaterialType { get; set; } = (int)GWAShared.Enums.MaterialType.File;

            [StringLength(500, ErrorMessage = "URL maksimal 500 karakter")]
            [Display(Name = "URL (Link / Video)")]
            public string? Url { get; set; }

            [Display(Name = "Dapat dilihat oleh")]
            public int Visibility { get; set; } = (int)MaterialVisibility.Member;
        }

        // ================= Poin
        public class ListPoin
        {
            public List<PoinRow> Rows { get; set; } = MemberPoint.GetDataList(jmldata: -1).Select(x => new PoinRow
            {
                EncDecrKey = x.EncDecrKey,
                MemberName = x.MemberName,
                Sumber = EnumHelper.Text(x.SourceType),
                Manual = x.SourceType == PointSourceType.Pemateri || x.SourceType == PointSourceType.Lainnya,
                Points = x.Points,
                EarnedDate = x.EarnedDate.ToDateTime(TimeOnly.MinValue),
                Notes = x.Notes
            }).OrderByDescending(x => x.EarnedDate).ToList();
        }

        public class PoinRow
        {
            public string EncDecrKey { get; set; } = "";
            public string MemberName { get; set; } = "";
            public string Sumber { get; set; } = "";
            public bool Manual { get; set; }
            public int Points { get; set; }
            public DateTime EarnedDate { get; set; }
            public string? Notes { get; set; }
        }

        public class AddPoin
        {
            public NewPoin Row { get; set; } = new() { EarnedDate = DateTime.Today, PeriodId = Opsi.PeriodeAktifId() };
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode();
            public List<GwaVM.OpsiItem> SumberOpsi { get; set; } = new()
            {
                new((int)PointSourceType.Pemateri, EnumHelper.Text(PointSourceType.Pemateri)),
                new((int)PointSourceType.Lainnya, EnumHelper.Text(PointSourceType.Lainnya))
            };
        }

        public class NewPoin
        {
            [Required(ErrorMessage = "Pilih anggota")]
            [Display(Name = "Anggota")]
            public int? MemberId { get; set; }

            [Display(Name = "Sumber")]
            public int SourceType { get; set; } = (int)PointSourceType.Lainnya;

            [Display(Name = "Poin (boleh negatif)")]
            public int Points { get; set; }

            [Required(ErrorMessage = "Tanggal harus diisi")]
            [Display(Name = "Tanggal")]
            public DateTime? EarnedDate { get; set; }

            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }

            [StringLength(255, ErrorMessage = "Catatan maksimal 255 karakter")]
            [Display(Name = "Catatan")]
            public string? Notes { get; set; }
        }

        public class Peringkat
        {
            public int? PeriodId { get; set; }
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode();
            public List<MemberPoint.LeaderboardRow> Rows { get; set; }
            public Peringkat(int? periodId)
            {
                PeriodId = periodId;
                Rows = MemberPoint.GetLeaderboard(periodId, 20);
            }
        }

        // ================= Aturan poin
        public class ListAturan
        {
            public List<AturanRow> Rows { get; set; } = PointRule.GetDataList(FilterAktif.Semua, jmldata: -1).Select(x => new AturanRow
            {
                EncDecrKey = x.EncDecrKey,
                Sumber = EnumHelper.Text(x.SourceType),
                JenisKegiatan = x.ActivityType.HasValue ? EnumHelper.Text(x.ActivityType.Value) : null,
                Tingkat = x.AchievementLevel.HasValue ? EnumHelper.Text(x.AchievementLevel.Value) : null,
                Points = x.Points,
                Description = x.Description,
                Aktif = x.IsActive == ActiveStatus.Aktif
            }).OrderBy(x => x.Sumber).ThenBy(x => x.JenisKegiatan).ThenBy(x => x.Tingkat).ToList();
        }

        public class AturanRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Sumber { get; set; } = "";
            public string? JenisKegiatan { get; set; }
            public string? Tingkat { get; set; }
            public int Points { get; set; }
            public string? Description { get; set; }
            public bool Aktif { get; set; }
        }

        public class AddAturan
        {
            public NewAturan Row { get; set; } = new();
            public List<GwaVM.OpsiItem> SumberOpsi { get; set; } = GwaVM.Opsi<PointSourceType>();
            public List<GwaVM.OpsiItem> JenisOpsi { get; set; } = GwaVM.Opsi<ActivityType>();
            public List<GwaVM.OpsiItem> TingkatOpsi { get; set; } = GwaVM.Opsi<EventLevel>();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
        }

        public class EditAturan : AddAturan
        {
            public EditAturan(string encKey)
            {
                var obj = LibExtensions.Load<PointRule>(encKey, "Aturan poin");
                Row = new NewAturan
                {
                    SourceType = (int)obj.SourceType,
                    ActivityType = (int?)obj.ActivityType,
                    AchievementLevel = (int?)obj.AchievementLevel,
                    Points = obj.Points,
                    Description = obj.Description,
                    IsActive = (int)obj.IsActive
                };
            }
        }

        public class NewAturan
        {
            [Display(Name = "Sumber Poin")]
            public int SourceType { get; set; } = (int)PointSourceType.HadirKegiatan;

            [Display(Name = "Jenis Kegiatan (Hadir / Panitia / Pemateri)")]
            public int? ActivityType { get; set; }

            [Display(Name = "Tingkat (Prestasi)")]
            public int? AchievementLevel { get; set; }

            [Display(Name = "Poin")]
            public int Points { get; set; }

            [StringLength(255, ErrorMessage = "Keterangan maksimal 255 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }
    }
}
