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

namespace GWALib.Lib.Master.Akses
{
    public partial class UserRole
    {
        #region SYNC
        public static List<UserRole> GetDataList(int? userId = null, int? roleId = null, int? periodId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(userId, roleId, periodId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<UserRole, EF.MUserRole>();
        }
        public static Response<UserRole> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MUserRoles.Add(row);
                context.SaveChanges();
                return new UserRole(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<UserRole>(msg));
        }
        public Response<UserRole> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new UserRole(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<UserRole>(msg));
        }
        public Response<UserRole> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                context.MUserRoles.Remove(row);
                context.SaveChanges();
                return new UserRole(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<UserRole>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<UserRole>> GetDataListAsync(int? userId = null, int? roleId = null, int? periodId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(userId, roleId, periodId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<UserRole, EF.MUserRole>();
        }
        public static async Task<Response<UserRole>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MUserRoles.Add(row);
                await context.SaveChangesAsync();
                return new UserRole(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<UserRole>(msg));
        }
        public async Task<Response<UserRole>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new UserRole(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<UserRole>(msg));
        }
        public async Task<Response<UserRole>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                context.MUserRoles.Remove(row);
                await context.SaveChangesAsync();
                return new UserRole(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<UserRole>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.MUserRole> FilterQuery(int? userId, int? roleId, int? periodId)
        {
            return _Querys.Where(x => (userId == null || x.UserId == userId)
                    && (roleId == null || x.RoleId == roleId)
                    && (periodId == null || x.PeriodId == periodId))
                .OrderBy(x => x.UserId).ThenBy(x => x.RoleId).ThenBy(x => x.PeriodId);
        }
        private static EF.MUserRole GetRow(EF.ModelContext context, int id)
        {
            return context.MUserRoles.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "User Role"));
        }
        private static async Task<EF.MUserRole> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MUserRoles.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "User Role"));
        }
        private static void ValidateData(EF.ModelContext context, int id, int userId, int roleId, int? periodId)
        {
            UserAccount.GetUser(context, userId);
            Role.GetActiveRole(context, roleId);
            if (periodId != null)
            {
                Anggota.ManagementPeriod.ValidateOpenPeriod(context, periodId.Value);
            }
            // Unique index MySQL tidak menolak duplikat bila period_id NULL, jadi dicek di sini.
            ValidateUtility.CheckDuplicateData<EF.MUserRole>(context,
                x => x.UserId == userId && x.RoleId == roleId && x.PeriodId == periodId && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "User, Role dan Periode"));
        }
        internal static EF.MUserRole CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var periodId = newrow.PeriodId > 0 ? newrow.PeriodId : null;
            ValidateData(context, 0, newrow.UserId, newrow.RoleId, periodId);

            return new EF.MUserRole
            {
                Id = SequenceUtility.GetNextId<EF.MUserRole>(context),
                UserId = newrow.UserId,
                RoleId = newrow.RoleId,
                PeriodId = periodId,
                InsDate = DateTime.Now,
                InsBy = userId
            };
        }
        private void SetRow(EF.ModelContext context, EF.MUserRole row, int userId)
        {
            var roleUserId = User?.Id ?? 0;
            var roleId = Role?.Id ?? 0;
            var periodId = PeriodId > 0 ? PeriodId : null;
            ValidateData(context, row.Id, roleUserId, roleId, periodId);

            row.UserId = roleUserId;
            row.RoleId = roleId;
            row.PeriodId = periodId;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
