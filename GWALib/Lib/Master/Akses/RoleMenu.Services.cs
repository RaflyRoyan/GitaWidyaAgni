using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Menu;
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
    public partial class RoleMenu
    {
        #region SYNC
        public static List<RoleMenu> GetDataList(int? roleId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(roleId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<RoleMenu, EF.MRoleMenu>();
        }
        public static Response<RoleMenu> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MRoleMenus.Add(row);
                context.SaveChanges();
                return new RoleMenu(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<RoleMenu>(msg));
        }
        public Response<RoleMenu> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new RoleMenu(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<RoleMenu>(msg));
        }
        public Response<RoleMenu> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                context.MRoleMenus.Remove(row);
                context.SaveChanges();
                return new RoleMenu(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<RoleMenu>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<RoleMenu>> GetDataListAsync(int? roleId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(roleId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<RoleMenu, EF.MRoleMenu>();
        }
        public static async Task<Response<RoleMenu>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MRoleMenus.Add(row);
                await context.SaveChangesAsync();
                return new RoleMenu(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<RoleMenu>(msg));
        }
        public async Task<Response<RoleMenu>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new RoleMenu(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<RoleMenu>(msg));
        }
        public async Task<Response<RoleMenu>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                context.MRoleMenus.Remove(row);
                await context.SaveChangesAsync();
                return new RoleMenu(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<RoleMenu>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.MRoleMenu> FilterQuery(int? roleId)
        {
            return _Querys.Where(x => roleId == null || x.RoleId == roleId)
                .OrderBy(x => x.RoleId).ThenBy(x => x.HeaderMenu.Seq).ThenBy(x => x.HeaderMenuId).ThenBy(x => x.SubMenuId);
        }
        private static EF.MRoleMenu GetRow(EF.ModelContext context, int id)
        {
            return context.MRoleMenus.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Role Menu"));
        }
        private static async Task<EF.MRoleMenu> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MRoleMenus.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Role Menu"));
        }
        // Sub menu harus terdaftar di header yang dipilih (m_header_menu_detail).
        private static void ValidateData(EF.ModelContext context, int id, int roleId, int headerMenuId, int? subMenuId)
        {
            Role.GetActiveRole(context, roleId);
            HeaderMenu.ValidateActiveHeaderMenu(context, headerMenuId);
            if (subMenuId != null)
            {
                SubMenu.ValidateActiveSubMenu(context, subMenuId.Value);
                if (!context.MHeaderMenuDetails.Any(x => x.HeaderMenuId == headerMenuId && x.SubMenuId == subMenuId))
                {
                    throw new ArgumentException(ExceptionMessage.ROLE_MENU_SUBMENU_NOT_IN_HEADER);
                }
            }
            // Unique index MySQL tidak menolak duplikat bila sub_menu_id NULL, jadi dicek di sini.
            ValidateUtility.CheckDuplicateData<EF.MRoleMenu>(context,
                x => x.RoleId == roleId && x.HeaderMenuId == headerMenuId && x.SubMenuId == subMenuId && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Role dan Menu"));
        }
        internal static EF.MRoleMenu CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var subMenuId = newrow.SubMenuId > 0 ? newrow.SubMenuId : null;
            ValidateData(context, 0, newrow.RoleId, newrow.HeaderMenuId, subMenuId);

            return new EF.MRoleMenu
            {
                Id = SequenceUtility.GetNextId<EF.MRoleMenu>(context),
                RoleId = newrow.RoleId,
                HeaderMenuId = newrow.HeaderMenuId,
                SubMenuId = subMenuId,
                InsDate = DateTime.Now,
                InsBy = userId
            };
        }
        private void SetRow(EF.ModelContext context, EF.MRoleMenu row, int userId)
        {
            var roleId = Role?.Id ?? 0;
            var headerMenuId = HeaderMenu?.Id ?? 0;
            int? subMenuId = SubMenu != null && SubMenu.Id > 0 ? SubMenu.Id : null;
            ValidateData(context, row.Id, roleId, headerMenuId, subMenuId);

            row.RoleId = roleId;
            row.HeaderMenuId = headerMenuId;
            row.SubMenuId = subMenuId;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
