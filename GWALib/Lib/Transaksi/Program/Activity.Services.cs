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

namespace GWALib.Lib.Transaksi.Program
{
    public partial class Activity
    {
        #region SYNC
        public static List<Activity> GetDataList(int? programId = null, int? parentActivityId = null, ActivityType? activityType = null, ProgramStatus? status = null,
            DateTime? from = null, DateTime? to = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(programId, parentActivityId, activityType, status, from, to);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Activity, EF.TActivity>();
        }
        // Untuk landing page: kegiatan publik yang tidak dibatalkan, yang disorot lebih dulu.
        public static List<Activity> GetPublicList(int jmldata = 20)
        {
            return PublicQuery().Take(jmldata).ToList().MappingData<Activity, EF.TActivity>();
        }
        public static Response<Activity> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TActivities.Add(row);
                context.SaveChanges();
                return new Activity(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Activity>(msg));
        }
        public Response<Activity> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new Activity(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Activity>(msg));
        }
        public Response<Activity> SetStatus(ProgramStatus status, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ApplyStatus(row, status, userId);
                context.SaveChanges();
                return new Activity(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Activity>(msg));
        }
        public Response<Activity> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateDelete(context, row);
                var data = new Activity(Reload(row.Id));
                context.TActivities.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Activity>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<Activity>> GetDataListAsync(int? programId = null, int? parentActivityId = null, ActivityType? activityType = null, ProgramStatus? status = null,
            DateTime? from = null, DateTime? to = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(programId, parentActivityId, activityType, status, from, to);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Activity, EF.TActivity>();
        }
        public static async Task<List<Activity>> GetPublicListAsync(int jmldata = 20)
        {
            return await (await PublicQuery().Take(jmldata).ToListAsync()).MappingDataAsync<Activity, EF.TActivity>();
        }
        public static async Task<Response<Activity>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TActivities.Add(row);
                await context.SaveChangesAsync();
                return new Activity(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Activity>(msg));
        }
        public async Task<Response<Activity>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new Activity(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Activity>(msg));
        }
        public async Task<Response<Activity>> SetStatusAsync(ProgramStatus status, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ApplyStatus(row, status, userId);
                await context.SaveChangesAsync();
                return new Activity(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Activity>(msg));
        }
        public async Task<Response<Activity>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateDelete(context, row);
                var data = new Activity(Reload(row.Id));
                context.TActivities.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Activity>(msg));
        }
        #endregion

        #region INTERNAL
        internal static EF.TActivity GetActivity(EF.ModelContext context, int activityId)
        {
            return context.TActivities.SingleOrDefault(x => x.Id == activityId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kegiatan"));
        }
        // Kegiatan yang masih berjalan (Planned / Ongoing); dipakai peserta, kolaborator, panitia, dll.
        internal static EF.TActivity GetOpenActivity(EF.ModelContext context, int activityId)
        {
            var row = GetActivity(context, activityId);
            TransaksiHelper.ValidateStatus((ProgramStatus)row.Status, "mengubah data kegiatan", ProgramStatus.Planned, ProgramStatus.Ongoing);
            return row;
        }
        #endregion

        #region HELPER
        private static EF.TActivity Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TActivity> FilterQuery(int? programId, int? parentActivityId, ActivityType? activityType, ProgramStatus? status, DateTime? from, DateTime? to)
        {
            return _Querys.Where(x => (programId == null || x.ProgramId == programId)
                    && (parentActivityId == null || x.ParentActivityId == parentActivityId)
                    && (activityType == null || x.ActivityType == (int)activityType)
                    && (status == null || x.Status == (int)status)
                    && (from == null || x.StartAt >= from)
                    && (to == null || x.StartAt < to))
                .OrderBy(x => x.StartAt).ThenBy(x => x.Title);
        }
        private static IQueryable<EF.TActivity> PublicQuery()
        {
            return _Querys.Where(x => x.IsPublic == (int)PublicVisibility.Public && x.Status != (int)ProgramStatus.Cancelled)
                .OrderByDescending(x => x.IsFeatured).ThenByDescending(x => x.StartAt);
        }
        private static EF.TActivity GetRow(EF.ModelContext context, int id)
        {
            return context.TActivities.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kegiatan"));
        }
        private static async Task<EF.TActivity> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TActivities.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kegiatan"));
        }
        private static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        // Kolom yang tidak berlaku dikosongkan: CompetitionId hanya untuk Lomba, DivisionId hanya untuk audiens Divisi.
        private static (int? CompetitionId, int? DivisionId) ValidateData(EF.ModelContext context, int id, int? programId, int? parentId, int? competitionId,
            ActivityType activityType, AudienceType audienceType, int? divisionId, string title, decimal? latitude, decimal? longitude,
            DateTime startAt, DateTime? endAt, PublicVisibility isPublic, FeaturedStatus isFeatured)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Kegiatan"));
            }
            if (!Enum.IsDefined(activityType) || !Enum.IsDefined(audienceType) || !Enum.IsDefined(isPublic) || !Enum.IsDefined(isFeatured))
            {
                throw new ArgumentException(ExceptionMessage.DATA_INVALID);
            }
            if (startAt == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Waktu Mulai"));
            }
            if (endAt != null && endAt < startAt)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            if ((latitude == null) != (longitude == null) || latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Koordinat Lokasi"));
            }
            if (isFeatured == FeaturedStatus.Disorot && isPublic != PublicVisibility.Public)
            {
                throw new ArgumentException(ExceptionMessage.FEATURED_NOT_PUBLIC);
            }
            if (programId != null)
            {
                WorkProgram.GetOpenProgram(context, programId.Value);
            }
            if (parentId != null)
            {
                var parent = context.TActivities.Where(x => x.Id == parentId).Select(x => new { x.Id, x.ParentActivityId }).SingleOrDefault()
                    ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kegiatan Induk"));
                if (parent.Id == id || parent.ParentActivityId != null || (id > 0 && context.TActivities.Any(x => x.ParentActivityId == id)))
                {
                    throw new ArgumentException(ExceptionMessage.ACTIVITY_PARENT_INVALID);
                }
            }
            if (audienceType == AudienceType.TimPelaksana && parentId == null)
            {
                throw new ArgumentException(ExceptionMessage.ACTIVITY_COMMITTEE_NEEDS_PARENT);
            }
            competitionId = activityType == ActivityType.Lomba ? competitionId : null;
            if (activityType == ActivityType.Lomba)
            {
                if (competitionId == null)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Lomba"));
                }
                if (!context.TCompetitions.Any(x => x.Id == competitionId))
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Lomba"));
                }
            }
            divisionId = audienceType == AudienceType.Divisi ? divisionId : null;
            if (audienceType == AudienceType.Divisi)
            {
                if (divisionId == null)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Divisi"));
                }
                Division.ValidateActiveDivision(context, divisionId.Value);
            }
            return (competitionId, divisionId);
        }
        // Hanya kegiatan Planned yang belum punya data turunan yang bisa dihapus; selebihnya dibatalkan.
        private static void ValidateDelete(EF.ModelContext context, EF.TActivity row)
        {
            TransaksiHelper.ValidateStatus((ProgramStatus)row.Status, "menghapus kegiatan", ProgramStatus.Planned);
            var used = context.TActivities.Where(x => x.Id == row.Id)
                .Select(x => x.InverseParentActivity.Any() || x.MProducts.Any() || x.TActivityChecklists.Any() || x.TActivityCollaborators.Any()
                    || x.TActivityParticipants.Any() || x.TActivityReports.Any() || x.TActivityRundowns.Any() || x.TActivitySchedules.Any()
                    || x.TActivityTripMembers.Any() || x.TAttendanceSessions.Any() || x.TCertificates.Any() || x.TCommitteeMembers.Any()
                    || x.TCommitteeSections.Any() || x.TFinancialTransactions.Any() || x.TForms.Any() || x.TFundingRequests.Any()
                    || x.TGalleries.Any() || x.TProcurementRequests.Any() || x.TSalesOrders.Any() || x.TSponsorships.Any() || x.TTimelinePlans.Any())
                .Single();
            if (used || context.TCommitteeRecruitments.Any(x => x.ActivityId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.ACTIVITY_IN_USE);
            }
        }
        private static EF.TActivity CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var title = newrow.Title?.Trim() ?? string.Empty;
            var programId = newrow.ProgramId > 0 ? newrow.ProgramId : null;
            var parentId = newrow.ParentActivityId > 0 ? newrow.ParentActivityId : null;
            var scope = ValidateData(context, 0, programId, parentId, newrow.CompetitionId > 0 ? newrow.CompetitionId : null, newrow.ActivityType,
                newrow.AudienceType, newrow.DivisionId > 0 ? newrow.DivisionId : null, title, newrow.Latitude, newrow.Longitude,
                newrow.StartAt, newrow.EndAt, newrow.IsPublic, newrow.IsFeatured);

            var row = new EF.TActivity
            {
                Id = SequenceUtility.GetNextId<EF.TActivity>(context),
                ProgramId = programId,
                ParentActivityId = parentId,
                CompetitionId = scope.CompetitionId,
                ActivityType = (int)newrow.ActivityType,
                AudienceType = (int)newrow.AudienceType,
                DivisionId = scope.DivisionId,
                Title = title,
                Description = ToNullable(newrow.Description),
                Location = ToNullable(newrow.Location),
                Latitude = newrow.Latitude,
                Longitude = newrow.Longitude,
                StartAt = newrow.StartAt,
                EndAt = newrow.EndAt,
                IsPublic = (int)newrow.IsPublic,
                IsFeatured = (int)newrow.IsFeatured,
                Status = (int)ProgramStatus.Planned,
                CreatedBy = userId,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Kegiatan Done / Cancelled dikunci. Program hanya divalidasi bila diganti.
        private void SetRow(EF.ModelContext context, EF.TActivity row, int userId)
        {
            TransaksiHelper.ValidateStatus((ProgramStatus)row.Status, "mengubah kegiatan", ProgramStatus.Planned, ProgramStatus.Ongoing);
            var title = Title?.Trim() ?? string.Empty;
            var programId = ProgramId > 0 ? ProgramId : null;
            var parentId = ParentActivityId > 0 ? ParentActivityId : null;
            var scope = ValidateData(context, row.Id, programId != row.ProgramId ? programId : null, parentId, CompetitionId > 0 ? CompetitionId : null,
                ActivityType, AudienceType, DivisionId > 0 ? DivisionId : null, title, Latitude, Longitude, StartAt, EndAt, IsPublic, IsFeatured);

            row.ProgramId = programId;
            row.ParentActivityId = parentId;
            row.CompetitionId = scope.CompetitionId;
            row.ActivityType = (int)ActivityType;
            row.AudienceType = (int)AudienceType;
            row.DivisionId = scope.DivisionId;
            row.Title = title;
            row.Description = ToNullable(Description);
            row.Location = ToNullable(Location);
            row.Latitude = Latitude;
            row.Longitude = Longitude;
            row.StartAt = StartAt;
            row.EndAt = EndAt;
            row.IsPublic = (int)IsPublic;
            row.IsFeatured = (int)IsFeatured;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void ApplyStatus(EF.TActivity row, ProgramStatus target, int userId)
        {
            var current = (ProgramStatus)row.Status;
            switch (target)
            {
                case ProgramStatus.Ongoing:
                    TransaksiHelper.ValidateStatus(current, "memulai kegiatan", ProgramStatus.Planned);
                    break;
                case ProgramStatus.Done:
                    TransaksiHelper.ValidateStatus(current, "menyelesaikan kegiatan", ProgramStatus.Planned, ProgramStatus.Ongoing);
                    break;
                case ProgramStatus.Cancelled:
                    TransaksiHelper.ValidateStatus(current, "membatalkan kegiatan", ProgramStatus.Planned, ProgramStatus.Ongoing);
                    break;
                default:
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Kegiatan"));
            }
            row.Status = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
