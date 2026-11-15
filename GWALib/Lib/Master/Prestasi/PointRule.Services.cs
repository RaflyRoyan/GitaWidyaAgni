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

namespace GWALib.Lib.Master.Prestasi
{
    public partial class PointRule
    {
        // Sumber poin yang terkait satu kegiatan (boleh punya aturan khusus per jenis kegiatan).
        private static readonly PointSourceType[] ACTIVITY_SOURCES = { PointSourceType.HadirKegiatan, PointSourceType.Panitia, PointSourceType.Pemateri };

        #region SYNC
        // Aturan aktif paling spesifik untuk sumber itu; null = tidak ada aturan (tidak memberi poin).
        public static PointRule? GetEffective(PointSourceType sourceType, ActivityType? activityType = null, EventLevel? achievementLevel = null)
        {
            var row = EffectiveQuery(_Querys, sourceType, activityType, achievementLevel).FirstOrDefault();
            return row != null ? new PointRule(row) : null;
        }
        public static List<PointRule> GetDataList(FilterAktif aktif, PointSourceType? sourceType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, sourceType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<PointRule, EF.MPointRule>();
        }
        public static Response<PointRule> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MPointRules.Add(row);
                context.SaveChanges();
                return new PointRule(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<PointRule>(msg));
        }
        public Response<PointRule> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new PointRule(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PointRule>(msg));
        }
        public Response<PointRule> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new PointRule(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<PointRule>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<PointRule?> GetEffectiveAsync(PointSourceType sourceType, ActivityType? activityType = null, EventLevel? achievementLevel = null)
        {
            var row = await EffectiveQuery(_Querys, sourceType, activityType, achievementLevel).FirstOrDefaultAsync();
            return row != null ? new PointRule(row) : null;
        }
        public static async Task<List<PointRule>> GetDataListAsync(FilterAktif aktif, PointSourceType? sourceType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, sourceType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<PointRule, EF.MPointRule>();
        }
        public static async Task<Response<PointRule>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MPointRules.Add(row);
                await context.SaveChangesAsync();
                return new PointRule(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<PointRule>(msg));
        }
        public async Task<Response<PointRule>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new PointRule(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PointRule>(msg));
        }
        public async Task<Response<PointRule>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new PointRule(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<PointRule>(msg));
        }
        #endregion

        #region HELPER
        // Dipakai pencatatan poin anggota di dalam context transaksinya.
        internal static EF.MPointRule? GetEffectiveRow(EF.ModelContext context, PointSourceType sourceType, ActivityType? activityType, EventLevel? achievementLevel)
        {
            return EffectiveQuery(context.MPointRules, sourceType, activityType, achievementLevel).FirstOrDefault();
        }
        // Aturan khusus (jenis kegiatan / tingkat prestasi sama) diutamakan, kalau tidak aturan umum (kolom itu NULL).
        private static IQueryable<EF.MPointRule> EffectiveQuery(IQueryable<EF.MPointRule> source, PointSourceType sourceType,
            ActivityType? activityType, EventLevel? achievementLevel)
        {
            int? activity = activityType != null ? (int)activityType : null;
            int? level = achievementLevel != null ? (int)achievementLevel : null;
            return source.Where(x => x.SourceType == (int)sourceType && x.IsActive == (int)ActiveStatus.Aktif
                    && (x.ActivityType == null || x.ActivityType == activity)
                    && (x.AchievementLevel == null || x.AchievementLevel == level))
                .OrderBy(x => x.ActivityType == null).ThenBy(x => x.AchievementLevel == null);
        }
        private static IQueryable<EF.MPointRule> FilterQuery(FilterAktif aktif, PointSourceType? sourceType)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (sourceType == null || x.SourceType == (int)sourceType))
                .OrderBy(x => x.SourceType).ThenBy(x => x.ActivityType).ThenBy(x => x.AchievementLevel);
        }
        private static EF.MPointRule GetRow(EF.ModelContext context, int id)
        {
            return context.MPointRules.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Aturan Poin"));
        }
        private static async Task<EF.MPointRule> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MPointRules.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Aturan Poin"));
        }
        // Kolom yang tidak berlaku untuk sumber poin itu dikosongkan, lalu yang dipakai divalidasi.
        private static (int? ActivityType, int? AchievementLevel) ResolveScope(PointSourceType sourceType, ActivityType? activityType, EventLevel? achievementLevel)
        {
            if (!Enum.IsDefined(sourceType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Sumber Poin"));
            }
            activityType = ACTIVITY_SOURCES.Contains(sourceType) ? activityType : null;
            achievementLevel = sourceType == PointSourceType.Prestasi ? achievementLevel : null;
            if (activityType != null && !Enum.IsDefined(activityType.Value))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Kegiatan"));
            }
            if (achievementLevel != null && !Enum.IsDefined(achievementLevel.Value))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Tingkat Prestasi"));
            }
            return (activityType != null ? (int)activityType : null, achievementLevel != null ? (int)achievementLevel : null);
        }
        private static void ValidateData(int points)
        {
            if (points == 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Poin"));
            }
        }
        private static void ValidateSingleActive(EF.ModelContext context, int id, int sourceType, int? activityType, int? achievementLevel)
        {
            if (context.MPointRules.Any(x => x.SourceType == sourceType && x.ActivityType == activityType && x.AchievementLevel == achievementLevel
                && x.IsActive == (int)ActiveStatus.Aktif && x.Id != id))
            {
                throw new ArgumentException(ExceptionMessage.POINT_RULE_ACTIVE_EXISTS);
            }
        }
        private static EF.MPointRule CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var scope = ResolveScope(newrow.SourceType, newrow.ActivityType, newrow.AchievementLevel);
            ValidateData(newrow.Points);
            ValidateSingleActive(context, 0, (int)newrow.SourceType, scope.ActivityType, scope.AchievementLevel);

            var row = new EF.MPointRule
            {
                Id = SequenceUtility.GetNextId<EF.MPointRule>(context),
                SourceType = (int)newrow.SourceType,
                ActivityType = scope.ActivityType,
                AchievementLevel = scope.AchievementLevel,
                Points = newrow.Points,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Mengubah poin tidak mengubah t_member_point yang sudah tercatat.
        private void SetRow(EF.ModelContext context, EF.MPointRule row, int userId)
        {
            var scope = ResolveScope(SourceType, ActivityType, AchievementLevel);
            ValidateData(Points);
            if (!Enum.IsDefined(IsActive))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status"));
            }
            if (IsActive == ActiveStatus.Aktif)
            {
                ValidateSingleActive(context, row.Id, (int)SourceType, scope.ActivityType, scope.AchievementLevel);
            }

            row.SourceType = (int)SourceType;
            row.ActivityType = scope.ActivityType;
            row.AchievementLevel = scope.AchievementLevel;
            row.Points = Points;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MPointRule row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
