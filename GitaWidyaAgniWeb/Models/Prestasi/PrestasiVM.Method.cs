using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Prestasi;
using GWALib.Lib.Transaksi.Prestasi;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Prestasi
{
    public partial class PrestasiVM
    {
        public class Method
        {
            private static PublicVisibility Publik(bool v) => v ? PublicVisibility.Public : PublicVisibility.Private;
            private static FeaturedStatus Sorot(bool v) => v ? FeaturedStatus.Disorot : FeaturedStatus.Biasa;

            // ---------- Lomba
            public static Competition LoadLomba(string? key) => LibExtensions.Load<Competition>(key, "Lomba");

            public static void SimpanLomba(NewLomba input, byte[]? poster, string user)
            {
                var obj = Competition.Save(new Competition.NewRow
                {
                    Name = input.Name ?? "",
                    Organizer = input.Organizer,
                    Level = (EventLevel)input.Level,
                    Category = input.Category,
                    Description = input.Description,
                    RegistrationDeadline = input.RegistrationDeadline,
                    StartDate = Opsi.Tanggal(input.StartDate),
                    EndDate = Opsi.Tanggal(input.EndDate),
                    Location = input.Location,
                    LinkUrl = input.LinkUrl,
                    IsPublic = Publik(input.Publik),
                    IsFeatured = Sorot(input.Disorot)
                }, user).Ok();
                if (poster != null)
                {
                    obj.UpdatePoster(poster, user).Ok();
                }
            }

            public static void UpdateLomba(string key, NewLomba input, byte[]? poster, bool hapusPoster, string user)
            {
                var obj = LoadLomba(key);
                obj.Name = input.Name ?? "";
                obj.Organizer = input.Organizer;
                obj.Level = (EventLevel)input.Level;
                obj.Category = input.Category;
                obj.Description = input.Description;
                obj.RegistrationDeadline = input.RegistrationDeadline;
                obj.StartDate = Opsi.Tanggal(input.StartDate);
                obj.EndDate = Opsi.Tanggal(input.EndDate);
                obj.Location = input.Location;
                obj.LinkUrl = input.LinkUrl;
                obj.IsPublic = Publik(input.Publik);
                obj.IsFeatured = Sorot(input.Disorot);
                obj.Update(user).Ok();
                if (poster != null || hapusPoster)
                {
                    obj.UpdatePoster(poster, user).Ok();
                }
            }

            public static void Daftar(PesertaInput input, string user)
            {
                var lomba = LoadLomba(input.Key);
                CompetitionParticipant.Register(new CompetitionParticipant.NewRow
                {
                    CompetitionId = lomba.Id,
                    MemberId = input.MemberId,
                    TeamName = input.TeamName,
                    ParticipantRole = input.ParticipantRole ?? CompetitionParticipant.ROLE_ANGGOTA
                }, user).Ok();
            }

            public static CompetitionParticipant LoadPeserta(string? key) => LibExtensions.Load<CompetitionParticipant>(key, "Peserta");

            // ---------- Prestasi
            public static Achievement LoadPrestasi(string? key) => LibExtensions.Load<Achievement>(key, "Prestasi");

            public static void SimpanPrestasi(NewPrestasi input, byte[]? sertifikat, string? sertifikatName, byte[]? foto, string user)
            {
                var obj = Achievement.Save(new Achievement.NewRow
                {
                    CompetitionId = input.CompetitionId,
                    MemberId = input.MemberId,
                    TeamName = input.TeamName,
                    Title = input.Title ?? "",
                    RankTitle = input.RankTitle,
                    Level = (EventLevel?)input.Level,
                    AchievedDate = Opsi.TanggalWajib(input.AchievedDate, "Tanggal"),
                    Description = input.Description,
                    IsPublic = Publik(input.Publik),
                    IsFeatured = Sorot(input.Disorot)
                }, user).Ok();
                if (sertifikat != null)
                {
                    obj.UpdateCertificate(sertifikat, sertifikatName, user).Ok();
                }
                if (foto != null)
                {
                    obj.UpdatePhoto(foto, user).Ok();
                }
            }

            public static void UpdatePrestasi(string key, NewPrestasi input, byte[]? sertifikat, string? sertifikatName, bool hapusSertifikat, byte[]? foto, bool hapusFoto, string user)
            {
                var obj = LoadPrestasi(key);
                obj.CompetitionId = input.CompetitionId;
                obj.MemberId = input.MemberId;
                obj.TeamName = input.TeamName;
                obj.Title = input.Title ?? "";
                obj.RankTitle = input.RankTitle;
                if (input.Level.HasValue)
                {
                    obj.Level = (EventLevel)input.Level.Value;
                }
                obj.AchievedDate = Opsi.TanggalWajib(input.AchievedDate, "Tanggal");
                obj.Description = input.Description;
                obj.IsPublic = Publik(input.Publik);
                obj.IsFeatured = Sorot(input.Disorot);
                obj.Update(user).Ok();
                if (sertifikat != null || hapusSertifikat)
                {
                    obj.UpdateCertificate(sertifikat, sertifikatName, user).Ok();
                }
                if (foto != null || hapusFoto)
                {
                    obj.UpdatePhoto(foto, user).Ok();
                }
            }

            // ---------- Materi
            public static CompetitionMaterial LoadMateri(string? key) => LibExtensions.Load<CompetitionMaterial>(key, "Materi");

            public static void SimpanMateri(NewMateri input, byte[]? file, IFormFile? meta, string user)
                => CompetitionMaterial.Save(new CompetitionMaterial.NewRow
                {
                    CompetitionId = input.CompetitionId,
                    Title = input.Title ?? "",
                    Description = input.Description,
                    MaterialType = (MaterialType)input.MaterialType,
                    Url = input.Url,
                    Visibility = (MaterialVisibility)input.Visibility,
                    FileData = file,
                    FileName = meta?.FileName,
                    ContentType = meta?.ContentType
                }, user).Ok();

            public static void UpdateMateri(string key, NewMateri input, byte[]? file, IFormFile? meta, string user)
            {
                var obj = LoadMateri(key);
                obj.CompetitionId = input.CompetitionId;
                obj.Title = input.Title ?? "";
                obj.Description = input.Description;
                obj.MaterialType = (MaterialType)input.MaterialType;
                obj.Url = input.Url;
                obj.Visibility = (MaterialVisibility)input.Visibility;
                if (file != null)
                {
                    obj.UpdateFile(file, meta!.FileName, meta.ContentType, user).Ok();
                }
                obj.Update(user).Ok();
            }

            // ---------- Poin
            public static void SimpanPoin(NewPoin input, string user)
                => MemberPoint.Save(new MemberPoint.NewRow
                {
                    MemberId = input.MemberId ?? 0,
                    PeriodId = input.PeriodId,
                    SourceType = (PointSourceType)input.SourceType,
                    Points = input.Points,
                    EarnedDate = Opsi.TanggalWajib(input.EarnedDate, "Tanggal"),
                    Notes = input.Notes
                }, user).Ok();

            public static int GenerateKegiatan(int activityId, string user) => MemberPoint.GenerateForActivity(activityId, user).Ok();
            public static int GeneratePeriode(int periodId, string user) => MemberPoint.GenerateForPeriod(periodId, user).Ok();

            // ---------- Aturan
            public static PointRule LoadAturan(string? key) => LibExtensions.Load<PointRule>(key, "Aturan poin");

            public static void SimpanAturan(NewAturan input, string user)
                => PointRule.Save(new PointRule.NewRow
                {
                    SourceType = (PointSourceType)input.SourceType,
                    ActivityType = (ActivityType?)input.ActivityType,
                    AchievementLevel = (EventLevel?)input.AchievementLevel,
                    Points = input.Points,
                    Description = input.Description
                }, user).Ok();

            public static void UpdateAturan(string key, NewAturan input, string user)
            {
                var obj = LoadAturan(key);
                obj.SourceType = (PointSourceType)input.SourceType;
                obj.ActivityType = (ActivityType?)input.ActivityType;
                obj.AchievementLevel = (EventLevel?)input.AchievementLevel;
                obj.Points = input.Points;
                obj.Description = input.Description;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }
        }
    }
}
