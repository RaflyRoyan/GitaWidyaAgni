using GWALib.General;
using GWALib.General.Utilities;
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
    public partial class Competition
    {
        #region SYNC
        public List<CompetitionParticipant> GetParticipants(bool refresh)
        {
            if (refresh || ParticipantList == null)
            {
                ParticipantList = CompetitionParticipant.GetDataList(competitionId: Id);
            }
            return ParticipantList;
        }
        public List<CompetitionMaterial> GetMaterials(bool refresh)
        {
            if (refresh || MaterialList == null)
            {
                MaterialList = CompetitionMaterial.GetDataList(Id);
            }
            return MaterialList;
        }
        public byte[]? GetPoster()
        {
            using var context = DBClass.GetContext();
            return context.TCompetitions.Where(x => x.Id == Id).Select(x => x.Poster).SingleOrDefault();
        }
        public static List<Competition> GetDataList(CompetitionStatus? status = null, EventLevel? level = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, level);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Competition, EF.TCompetition>();
        }
        // Landing page: publik dan tidak dibatalkan, yang disorot lebih dulu.
        public static List<Competition> GetPublicList(int jmldata = 10)
        {
            return PublicQuery().Take(jmldata).ToList().MappingData<Competition, EF.TCompetition>();
        }
        public static Response<Competition> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new Competition(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Competition>(msg));
        }
        public Response<Competition> Update(string user)
        {
            return Run(SetRow, user);
        }
        // file null / kosong = poster dihapus.
        public Response<Competition> UpdatePoster(byte[]? file, string user)
        {
            return Run((context, row, userId) => SetPoster(row, file, userId), user);
        }
        public Response<Competition> SetStatus(CompetitionStatus status, string user)
        {
            return Run((context, row, userId) => SetStatusRow(context, row, status, userId), user);
        }
        public Response<Competition> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var data = new Competition(Reload(row.Id));
                SetDelete(context, row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Competition>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<CompetitionParticipant>> GetParticipantsAsync(bool refresh)
        {
            if (refresh || ParticipantList == null)
            {
                ParticipantList = await CompetitionParticipant.GetDataListAsync(competitionId: Id);
            }
            return ParticipantList;
        }
        public async Task<List<CompetitionMaterial>> GetMaterialsAsync(bool refresh)
        {
            if (refresh || MaterialList == null)
            {
                MaterialList = await CompetitionMaterial.GetDataListAsync(Id);
            }
            return MaterialList;
        }
        public async Task<byte[]?> GetPosterAsync()
        {
            using var context = DBClass.GetContext();
            return await context.TCompetitions.Where(x => x.Id == Id).Select(x => x.Poster).SingleOrDefaultAsync();
        }
        public static async Task<List<Competition>> GetDataListAsync(CompetitionStatus? status = null, EventLevel? level = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, level);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Competition, EF.TCompetition>();
        }
        public static async Task<List<Competition>> GetPublicListAsync(int jmldata = 10)
        {
            return await (await PublicQuery().Take(jmldata).ToListAsync()).MappingDataAsync<Competition, EF.TCompetition>();
        }
        public static async Task<Response<Competition>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new Competition(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Competition>(msg));
        }
        public Task<Response<Competition>> UpdateAsync(string user)
        {
            return RunAsync(SetRow, user);
        }
        public Task<Response<Competition>> UpdatePosterAsync(byte[]? file, string user)
        {
            return RunAsync((context, row, userId) => SetPoster(row, file, userId), user);
        }
        public Task<Response<Competition>> SetStatusAsync(CompetitionStatus status, string user)
        {
            return RunAsync((context, row, userId) => SetStatusRow(context, row, status, userId), user);
        }
        public async Task<Response<Competition>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var data = new Competition(Reload(row.Id));
                SetDelete(context, row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Competition>(msg));
        }
        #endregion

        #region INTERNAL
        internal static EF.TCompetition GetCompetition(EF.ModelContext context, int competitionId)
        {
            return context.TCompetitions.SingleOrDefault(x => x.Id == competitionId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Lomba"));
        }
        #endregion

        #region HELPER
        private delegate void RowAction(EF.ModelContext context, EF.TCompetition row, int userId);

        private Response<Competition> Run(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                return new Competition(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Competition>(msg));
        }
        private async Task<Response<Competition>> RunAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                return new Competition(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Competition>(msg));
        }
        private static EF.TCompetition Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TCompetition> FilterQuery(CompetitionStatus? status, EventLevel? level)
        {
            return _Querys.Where(x => (status == null || x.Status == (int)status) && (level == null || x.Level == (int)level))
                .OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id);
        }
        private static IQueryable<EF.TCompetition> PublicQuery()
        {
            return _Querys.Where(x => x.IsPublic == (int)PublicVisibility.Public && x.Status != (int)CompetitionStatus.Cancelled)
                .OrderByDescending(x => x.IsFeatured).ThenByDescending(x => x.StartDate).ThenByDescending(x => x.Id);
        }
        private static EF.TCompetition GetRow(EF.ModelContext context, int id)
        {
            return GetCompetition(context, id);
        }
        private static async Task<EF.TCompetition> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TCompetitions.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Lomba"));
        }
        private static void ValidateData(string? name, EventLevel level, DateOnly? startDate, DateOnly? endDate, PublicVisibility isPublic, FeaturedStatus isFeatured)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Lomba"));
            }
            if (!Enum.IsDefined(level) || !Enum.IsDefined(isPublic) || !Enum.IsDefined(isFeatured))
            {
                throw new ArgumentException(ExceptionMessage.DATA_INVALID);
            }
            if (startDate != null && endDate != null && endDate < startDate)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            if (isFeatured == FeaturedStatus.Disorot && isPublic != PublicVisibility.Public)
            {
                throw new ArgumentException(ExceptionMessage.FEATURED_NOT_PUBLIC);
            }
        }
        private static EF.TCompetition CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            ValidateData(newrow.Name, newrow.Level, newrow.StartDate, newrow.EndDate, newrow.IsPublic, newrow.IsFeatured);
            var row = new EF.TCompetition
            {
                Id = SequenceUtility.GetNextId<EF.TCompetition>(context),
                Name = TransaksiHelper.Truncate(newrow.Name, 200)!,
                Organizer = TransaksiHelper.Truncate(newrow.Organizer, 200),
                Level = (int)newrow.Level,
                Category = TransaksiHelper.Truncate(newrow.Category, 100),
                Description = TransaksiHelper.ToNullable(newrow.Description),
                RegistrationDeadline = newrow.RegistrationDeadline,
                StartDate = newrow.StartDate,
                EndDate = newrow.EndDate,
                Location = TransaksiHelper.Truncate(newrow.Location, 255),
                LinkUrl = TransaksiHelper.Truncate(newrow.LinkUrl, 255),
                IsPublic = (int)newrow.IsPublic,
                IsFeatured = (int)newrow.IsFeatured,
                Status = (int)CompetitionStatus.Upcoming,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TCompetitions.Add(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.TCompetition row, int userId)
        {
            TransaksiHelper.ValidateStatus((CompetitionStatus)row.Status, "mengubah lomba", CompetitionStatus.Upcoming, CompetitionStatus.Registration,
                CompetitionStatus.Ongoing, CompetitionStatus.Finished);
            ValidateData(Name, Level, StartDate, EndDate, IsPublic, IsFeatured);
            row.Name = TransaksiHelper.Truncate(Name, 200)!;
            row.Organizer = TransaksiHelper.Truncate(Organizer, 200);
            row.Level = (int)Level;
            row.Category = TransaksiHelper.Truncate(Category, 100);
            row.Description = TransaksiHelper.ToNullable(Description);
            row.RegistrationDeadline = RegistrationDeadline;
            row.StartDate = StartDate;
            row.EndDate = EndDate;
            row.Location = TransaksiHelper.Truncate(Location, 255);
            row.LinkUrl = TransaksiHelper.Truncate(LinkUrl, 255);
            row.IsPublic = (int)IsPublic;
            row.IsFeatured = (int)IsFeatured;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetPoster(EF.TCompetition row, byte[]? file, int userId)
        {
            if (file != null && file.Length > MAX_POSTER_BYTES)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FILE_TOO_LARGE, "Poster", MAX_POSTER_BYTES / 1024 / 1024));
            }
            row.Poster = file == null || file.Length == 0 ? null : file;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        // Maju saja (Upcoming → Registration → Ongoing → Finished); Cancelled dari status apa pun selain Finished.
        private static void SetStatusRow(EF.ModelContext context, EF.TCompetition row, CompetitionStatus target, int userId)
        {
            if (!Enum.IsDefined(target))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Lomba"));
            }
            var current = (CompetitionStatus)row.Status;
            var allowed = target == CompetitionStatus.Cancelled
                ? current != CompetitionStatus.Finished && current != CompetitionStatus.Cancelled
                : current != CompetitionStatus.Cancelled && (int)target > (int)current;
            if (!allowed)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.STATUS_ACTION_INVALID, "mengubah status lomba menjadi " + target.GetDescription(), current.GetDescription()));
            }
            var now = DateTime.Now;
            if (target == CompetitionStatus.Finished)
            {
                foreach (var participant in context.TCompetitionParticipants.Where(x => x.CompetitionId == row.Id
                    && (x.Status == (int)CompetitionParticipantStatus.Registered || x.Status == (int)CompetitionParticipantStatus.Participating)).ToList())
                {
                    participant.Status = (int)CompetitionParticipantStatus.Finished;
                    participant.UpdDate = now;
                    participant.UpdBy = userId;
                }
            }
            row.Status = (int)target;
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        private static void SetDelete(EF.ModelContext context, EF.TCompetition row)
        {
            TransaksiHelper.ValidateStatus((CompetitionStatus)row.Status, "menghapus lomba", CompetitionStatus.Upcoming);
            if (context.TCompetitionParticipants.Any(x => x.CompetitionId == row.Id) || context.TAchievements.Any(x => x.CompetitionId == row.Id)
                || context.TCompetitionMaterials.Any(x => x.CompetitionId == row.Id) || context.TActivities.Any(x => x.CompetitionId == row.Id)
                || context.TFinancialTransactions.Any(x => x.CompetitionId == row.Id) || context.TFundingRequests.Any(x => x.CompetitionId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.COMPETITION_IN_USE);
            }
            context.TCompetitions.Remove(row);
        }
        #endregion
    }
}
