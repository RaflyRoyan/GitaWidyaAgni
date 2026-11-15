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
    public partial class RoleAction
    {
        #region SYNC
        public static List<RoleAction> GetDataList(int? roleId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(roleId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<RoleAction, EF.MRoleAction>();
        }
        public static Response<RoleAction> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MRoleActions.Add(row);
                context.SaveChanges();
                return new RoleAction(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<RoleAction>(msg));
        }
        public Response<RoleAction> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new RoleAction(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<RoleAction>(msg));
        }
        public Response<RoleAction> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                context.MRoleActions.Remove(row);
                context.SaveChanges();
                return new RoleAction(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<RoleAction>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<RoleAction>> GetDataListAsync(int? roleId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(roleId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<RoleAction, EF.MRoleAction>();
        }
        public static async Task<Response<RoleAction>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MRoleActions.Add(row);
                await context.SaveChangesAsync();
                return new RoleAction(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<RoleAction>(msg));
        }
        public async Task<Response<RoleAction>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new RoleAction(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<RoleAction>(msg));
        }
        public async Task<Response<RoleAction>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                context.MRoleActions.Remove(row);
                await context.SaveChangesAsync();
                return new RoleAction(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<RoleAction>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.MRoleAction> FilterQuery(int? roleId)
        {
            return _Querys.Where(x => roleId == null || x.RoleId == roleId)
                .OrderBy(x => x.RoleId).ThenBy(x => x.ControllerAction.Controller.Name).ThenBy(x => x.ControllerAction.Action);
        }
        private static EF.MRoleAction GetRow(EF.ModelContext context, int id)
        {
            return context.MRoleActions.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Role Action"));
        }
        private static async Task<EF.MRoleAction> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MRoleActions.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Role Action"));
        }
        private static void ValidateData(EF.ModelContext context, int id, int roleId, int controllerActionId)
        {
            Role.GetActiveRole(context, roleId);
            var action = context.MControllerActions.SingleOrDefault(x => x.Id == controllerActionId)
                ?? throw new ArgumentException(ExceptionMessage.CONTROLLER_ACTION_NOT_FOUND);
            if (action.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Controller Action"));
            }
            ValidateUtility.CheckDuplicateData<EF.MRoleAction>(context, x => x.RoleId == roleId && x.ControllerActionId == controllerActionId && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Role dan Controller Action"));
        }
        internal static EF.MRoleAction CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            ValidateData(context, 0, newrow.RoleId, newrow.ControllerActionId);

            return new EF.MRoleAction
            {
                Id = SequenceUtility.GetNextId<EF.MRoleAction>(context),
                RoleId = newrow.RoleId,
                ControllerActionId = newrow.ControllerActionId,
                InsDate = DateTime.Now,
                InsBy = userId
            };
        }
        private void SetRow(EF.ModelContext context, EF.MRoleAction row, int userId)
        {
            var roleId = Role?.Id ?? 0;
            var controllerActionId = ControllerAction?.Id ?? 0;
            ValidateData(context, row.Id, roleId, controllerActionId);

            row.RoleId = roleId;
            row.ControllerActionId = controllerActionId;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
