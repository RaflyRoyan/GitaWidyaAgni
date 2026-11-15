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
    public partial class Role
    {
        #region SYNC
        public List<RoleAction> GetRoleActions(bool refresh)
        {
            if (refresh || RoleActionList == null)
            {
                RoleActionList = RoleAction.GetDataList(Id);
            }
            return RoleActionList;
        }
        public List<RoleMenu> GetRoleMenus(bool refresh)
        {
            if (refresh || RoleMenuList == null)
            {
                RoleMenuList = RoleMenu.GetDataList(Id);
            }
            return RoleMenuList;
        }
        public List<UserRole> GetUserRoles(bool refresh)
        {
            if (refresh || UserRoleList == null)
            {
                UserRoleList = UserRole.GetDataList(roleId: Id);
            }
            return UserRoleList;
        }
        public static List<Role> GetDataList(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Role, EF.MRole>();
        }
        public static Response<Role> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MRoles.Add(row);
                context.SaveChanges();
                return new Role(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Role>(msg));
        }
        public Response<Role> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new Role(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Role>(msg));
        }
        public Response<Role> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new Role(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Role>(msg));
        }
        public Response<Role> SaveRoleActions(List<int> controllerActionIds, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRoleActions(context, row, controllerActionIds, userId);
                context.SaveChanges();
                return new Role(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Role>(msg));
        }
        public Response<Role> SaveRoleMenus(List<RoleMenu.NewRow> rows, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRoleMenus(context, row, rows, userId);
                context.SaveChanges();
                return new Role(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Role>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<RoleAction>> GetRoleActionsAsync(bool refresh)
        {
            if (refresh || RoleActionList == null)
            {
                RoleActionList = await RoleAction.GetDataListAsync(Id);
            }
            return RoleActionList;
        }
        public async Task<List<RoleMenu>> GetRoleMenusAsync(bool refresh)
        {
            if (refresh || RoleMenuList == null)
            {
                RoleMenuList = await RoleMenu.GetDataListAsync(Id);
            }
            return RoleMenuList;
        }
        public async Task<List<UserRole>> GetUserRolesAsync(bool refresh)
        {
            if (refresh || UserRoleList == null)
            {
                UserRoleList = await UserRole.GetDataListAsync(roleId: Id);
            }
            return UserRoleList;
        }
        public static async Task<List<Role>> GetDataListAsync(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Role, EF.MRole>();
        }
        public static async Task<Response<Role>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MRoles.Add(row);
                await context.SaveChangesAsync();
                return new Role(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Role>(msg));
        }
        public async Task<Response<Role>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new Role(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Role>(msg));
        }
        public async Task<Response<Role>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new Role(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Role>(msg));
        }
        public async Task<Response<Role>> SaveRoleActionsAsync(List<int> controllerActionIds, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRoleActions(context, row, controllerActionIds, userId);
                await context.SaveChangesAsync();
                return new Role(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Role>(msg));
        }
        public async Task<Response<Role>> SaveRoleMenusAsync(List<RoleMenu.NewRow> rows, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRoleMenus(context, row, rows, userId);
                await context.SaveChangesAsync();
                return new Role(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Role>(msg));
        }
        #endregion

        #region HELPER
        internal static EF.MRole GetActiveRole(EF.ModelContext context, int roleId)
        {
            var row = context.MRoles.SingleOrDefault(x => x.Id == roleId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Role"));
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Role"));
            }
            return row;
        }
        private static IQueryable<EF.MRole> FilterQuery(FilterAktif aktif)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua))
                .OrderBy(x => x.Code);
        }
        private static EF.MRole GetRow(EF.ModelContext context, int id)
        {
            return context.MRoles.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Role"));
        }
        private static async Task<EF.MRole> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MRoles.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Role"));
        }
        private static void ValidateData(EF.ModelContext context, int id, string code, string name)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Kode Role"));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Role"));
            }
            ValidateUtility.CheckDuplicateData<EF.MRole>(context, x => x.Code == code && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Kode Role"));
        }
        private static EF.MRole CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var code = newrow.Code?.Trim().ToUpperInvariant() ?? string.Empty;
            var name = newrow.Name?.Trim() ?? string.Empty;
            ValidateData(context, 0, code, name);

            var row = new EF.MRole
            {
                Id = SequenceUtility.GetNextId<EF.MRole>(context),
                Code = code,
                Name = name,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.MRole row, int userId)
        {
            var code = Code?.Trim().ToUpperInvariant() ?? string.Empty;
            var name = Name?.Trim() ?? string.Empty;
            ValidateData(context, row.Id, code, name);

            row.Code = code;
            row.Name = name;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MRole row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        // Row yang tidak ada di daftar dihapus, yang belum ada ditambah lewat CreateRow anak (termasuk validasinya),
        // yang sudah ada dibiarkan. Daftar kosong = cabut semua akses.
        private static void SetRoleActions(EF.ModelContext context, EF.MRole row, List<int>? controllerActionIds, int userId)
        {
            var desired = (controllerActionIds ?? new()).Distinct().ToList();
            var existing = context.MRoleActions.Where(x => x.RoleId == row.Id).ToList();
            context.MRoleActions.RemoveRange(existing.Where(x => !desired.Contains(x.ControllerActionId)));
            foreach (var actionId in desired.Where(id => !existing.Any(x => x.ControllerActionId == id)))
            {
                var newrow = new RoleAction.NewRow { RoleId = row.Id, ControllerActionId = actionId };
                context.MRoleActions.Add(RoleAction.CreateRow(context, newrow, userId));
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetRoleMenus(EF.ModelContext context, EF.MRole row, List<RoleMenu.NewRow>? rows, int userId)
        {
            var desired = (rows ?? new()).Select(x => (x.HeaderMenuId, SubMenuId: x.SubMenuId > 0 ? x.SubMenuId : null)).Distinct().ToList();
            var existing = context.MRoleMenus.Where(x => x.RoleId == row.Id).ToList();
            context.MRoleMenus.RemoveRange(existing.Where(x => !desired.Contains((x.HeaderMenuId, x.SubMenuId))));
            foreach (var item in desired.Where(d => !existing.Any(x => x.HeaderMenuId == d.HeaderMenuId && x.SubMenuId == d.SubMenuId)))
            {
                var newrow = new RoleMenu.NewRow { RoleId = row.Id, HeaderMenuId = item.HeaderMenuId, SubMenuId = item.SubMenuId };
                context.MRoleMenus.Add(RoleMenu.CreateRow(context, newrow, userId));
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
