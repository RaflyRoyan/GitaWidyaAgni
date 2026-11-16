using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Prestasi
{
    public partial class Achievement
    {
        public const int MAX_FILE_BYTES = 5 * 1024 * 1024;

        #region SYNC
        public FileContent? GetCertificate()
        {
            using var context = DBClass.GetContext();
            var file = context.TAchievements.Where(x => x.Id == Id).Select(x => new { x.Certificate, x.CertificateFileName }).SingleOrDefault();
            return file?.Certificate == null ? null : new FileContent(file.Certificate, file.CertificateFileName ?? "sertifikat");
        }
        public byte[]? GetPhoto()
        {
            using var context = DBClass.GetContext();
            return context.TAchievements.Where(x => x.Id == Id).Select(x => x.Photo).SingleOrDefault();
        }
        public static List<Achievement> GetDataList(int? memberId = null, int? competitionId = null, EventLevel? level = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(memberId, competitionId, level);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Achievement, EF.TAchievement>();
        }
        // Landing page: publik, yang disorot lebih dulu.
        public static List<Achievement> GetPublicList(int jmldata = 10)
        {
            return PublicQuery().Take(jmldata).ToList().MappingData<Achievement, EF.TAchievement>();
        }
        public static Response<Achievement> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new Achievement(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Achievement>(msg));
        }
        public Response<Achievement> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new Achievement(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Achievement>(msg));
        }
        // file null / kosong = file sertifikat dihapus.
        public Response<Achievement> UpdateCertificate(byte[]? file, string? fileName, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetCertificate(row, file, fileName, userId);
                context.SaveChanges();
                return new Achievement(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Achievement>(msg));
        }
        // file null / kosong = foto dihapus.
        public Response<Achievement> UpdatePhoto(byte[]? file, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetPhoto(row, file, userId);
                context.SaveChanges();
                return new Achievement(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Achievement>(msg));
        }
        public Response<Achievement> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var data = new Achievement(Reload(row.Id));
                SetDelete(context, row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Achievement>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetCertificateAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TAchievements.Where(x => x.Id == Id).Select(x => new { x.Certificate, x.CertificateFileName }).SingleOrDefaultAsync();
            return file?.Certificate == null ? null : new FileContent(file.Certificate, file.CertificateFileName ?? "sertifikat");
        }
        public async Task<byte[]?> GetPhotoAsync()
        {
            using var context = DBClass.GetContext();
            return await context.TAchievements.Where(x => x.Id == Id).Select(x => x.Photo).SingleOrDefaultAsync();
        }
        public static async Task<List<Achievement>> GetDataListAsync(int? memberId = null, int? competitionId = null, EventLevel? level = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(memberId, competitionId, level);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Achievement, EF.TAchievement>();
        }
        public static async Task<List<Achievement>> GetPublicListAsync(int jmldata = 10)
        {
            return await (await PublicQuery().Take(jmldata).ToListAsync()).MappingDataAsync<Achievement, EF.TAchievement>();
        }
        public static async Task<Response<Achievement>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new Achievement(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Achievement>(msg));
        }
        public async Task<Response<Achievement>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new Achievement(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Achievement>(msg));
        }
        public async Task<Response<Achievement>> UpdateCertificateAsync(byte[]? file, string? fileName, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetCertificate(row, file, fileName, userId);
                await context.SaveChangesAsync();
                return new Achievement(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Achievement>(msg));
        }
        public async Task<Response<Achievement>> UpdatePhotoAsync(byte[]? file, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetPhoto(row, file, userId);
                await context.SaveChangesAsync();
                return new Achievement(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Achievement>(msg));
        }
        public async Task<Response<Achievement>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var data = new Achievement(Reload(row.Id));
                SetDelete(context, row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Achievement>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TAchievement Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TAchievement> FilterQuery(int? memberId, int? competitionId, EventLevel? level)
        {
            return _Querys.Where(x => (memberId == null || x.MemberId == memberId)
                    && (competitionId == null || x.CompetitionId == competitionId)
                    && (level == null || x.Level == (int)level))
                .OrderByDescending(x => x.AchievedDate).ThenByDescending(x => x.Id);
        }
        private static IQueryable<EF.TAchievement> PublicQuery()
        {
            return _Querys.Where(x => x.IsPublic == (int)PublicVisibility.Public)
                .OrderByDescending(x => x.IsFeatured).ThenByDescending(x => x.AchievedDate).ThenByDescending(x => x.Id);
        }
        private static EF.TAchievement GetRow(EF.ModelContext context, int id)
        {
            return context.TAchievements.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Prestasi"));
        }
        private static async Task<EF.TAchievement> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TAchievements.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Prestasi"));
        }
        private record Data(int? CompetitionId, int? MemberId, string? TeamName, EventLevel Level);

        private static Data ValidateData(EF.ModelContext context, int? competitionId, int? memberId, string? teamName, string? title, EventLevel? level,
            DateOnly achievedDate, PublicVisibility isPublic, FeaturedStatus isFeatured)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Prestasi"));
            }
            if (achievedDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Prestasi"));
            }
            if (!Enum.IsDefined(isPublic) || !Enum.IsDefined(isFeatured))
            {
                throw new ArgumentException(ExceptionMessage.DATA_INVALID);
            }
            if (isFeatured == FeaturedStatus.Disorot && isPublic != PublicVisibility.Public)
            {
                throw new ArgumentException(ExceptionMessage.FEATURED_NOT_PUBLIC);
            }
            memberId = memberId > 0 ? memberId : null;
            var team = TransaksiHelper.Truncate(teamName, 150);
            if (memberId == null && team == null)
            {
                throw new ArgumentException(ExceptionMessage.ACHIEVEMENT_OWNER_REQUIRED);
            }
            if (memberId != null)
            {
                Member.GetMember(context, memberId.Value);
            }
            competitionId = competitionId > 0 ? competitionId : null;
            if (competitionId != null)
            {
                level ??= (EventLevel)Competition.GetCompetition(context, competitionId.Value).Level;
            }
            if (level == null || !Enum.IsDefined(level.Value))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tingkat Prestasi"));
            }
            return new Data(competitionId, memberId, team, level.Value);
        }
        private static void AwardPoint(EF.ModelContext context, EF.TAchievement row, int userId)
        {
            if (row.MemberId != null)
            {
                MemberPoint.Award(context, row.MemberId.Value, PointSourceType.Prestasi, row.Id, null, (EventLevel)row.Level, row.AchievedDate, null,
                    "Prestasi: " + row.Title, userId);
            }
        }
        private static EF.TAchievement CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var data = ValidateData(context, newrow.CompetitionId, newrow.MemberId, newrow.TeamName, newrow.Title, newrow.Level, newrow.AchievedDate,
                newrow.IsPublic, newrow.IsFeatured);
            var row = new EF.TAchievement
            {
                Id = SequenceUtility.GetNextId<EF.TAchievement>(context),
                CompetitionId = data.CompetitionId,
                MemberId = data.MemberId,
                TeamName = data.TeamName,
                Title = TransaksiHelper.Truncate(newrow.Title, 200)!,
                RankTitle = TransaksiHelper.Truncate(newrow.RankTitle, 100),
                Level = (int)data.Level,
                AchievedDate = newrow.AchievedDate,
                Description = TransaksiHelper.ToNullable(newrow.Description),
                IsPublic = (int)newrow.IsPublic,
                IsFeatured = (int)newrow.IsFeatured,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TAchievements.Add(row);
            AwardPoint(context, row, userId);
            return row;
        }
        // Poin lama dicabut lalu dihitung ulang (anggota / tingkat bisa berubah).
        private void SetRow(EF.ModelContext context, EF.TAchievement row, int userId)
        {
            var data = ValidateData(context, CompetitionId, MemberId, TeamName, Title, Level, AchievedDate, IsPublic, IsFeatured);
            if (data.MemberId != row.MemberId && context.TCertificates.Any(x => x.AchievementId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.ACHIEVEMENT_HAS_CERTIFICATE);
            }
            if (data.MemberId != row.MemberId)
            {
                MemberPoint.Revoke(context, PointSourceType.Prestasi, row.Id);
            }
            row.CompetitionId = data.CompetitionId;
            row.MemberId = data.MemberId;
            row.TeamName = data.TeamName;
            row.Title = TransaksiHelper.Truncate(Title, 200)!;
            row.RankTitle = TransaksiHelper.Truncate(RankTitle, 100);
            row.Level = (int)data.Level;
            row.AchievedDate = AchievedDate;
            row.Description = TransaksiHelper.ToNullable(Description);
            row.IsPublic = (int)IsPublic;
            row.IsFeatured = (int)IsFeatured;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            AwardPoint(context, row, userId);
        }
        private static void SetCertificate(EF.TAchievement row, byte[]? file, string? fileName, int userId)
        {
            if (file == null || file.Length == 0)
            {
                row.Certificate = null;
                row.CertificateFileName = null;
            }
            else
            {
                row.CertificateFileName = TransaksiHelper.ValidateFile(file, fileName, "File Sertifikat", MAX_FILE_BYTES);
                row.Certificate = file;
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetPhoto(EF.TAchievement row, byte[]? file, int userId)
        {
            if (file != null && file.Length > MAX_FILE_BYTES)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FILE_TOO_LARGE, "Foto Prestasi", MAX_FILE_BYTES / 1024 / 1024));
            }
            row.Photo = file == null || file.Length == 0 ? null : file;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetDelete(EF.ModelContext context, EF.TAchievement row)
        {
            if (context.TCertificates.Any(x => x.AchievementId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.ACHIEVEMENT_HAS_CERTIFICATE);
            }
            MemberPoint.Revoke(context, PointSourceType.Prestasi, row.Id);
            context.TAchievements.Remove(row);
        }
        #endregion
    }
}
