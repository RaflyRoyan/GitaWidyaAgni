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

namespace GWALib.Lib.Master.Menu
{
    public partial class SubMenu
    {
        #region SYNC
        public static List<SubMenu> GetDataList(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<SubMenu, EF.MSubMenu>();
        }
        public static Response<SubMenu> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MSubMenus.Add(row);
                context.SaveChanges();
                return new SubMenu(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<SubMenu>(msg));
        }
        public Response<SubMenu> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new SubMenu(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SubMenu>(msg));
        }
        public Response<SubMenu> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new SubMenu(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<SubMenu>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<SubMenu>> GetDataListAsync(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<SubMenu, EF.MSubMenu>();
        }
        public static async Task<Response<SubMenu>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MSubMenus.Add(row);
                await context.SaveChangesAsync();
                return new SubMenu(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<SubMenu>(msg));
        }
        public async Task<Response<SubMenu>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new SubMenu(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SubMenu>(msg));
        }
        public async Task<Response<SubMenu>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new SubMenu(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<SubMenu>(msg));
        }
        #endregion

        #region HELPER
        /// <summary>
        /// Dipakai HeaderMenuDetail untuk memastikan sub menu ada dan aktif.
        /// </summary>
        internal static void ValidateActiveSubMenu(EF.ModelContext context, int subMenuId)
        {
            var row = context.MSubMenus.SingleOrDefault(x => x.Id == subMenuId)
                ?? throw new ArgumentException(ExceptionMessage.SUBMENU_NOT_FOUND);
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(ExceptionMessage.SUBMENU_NOT_ACTIVE);
            }
        }
        private static IQueryable<EF.MSubMenu> FilterQuery(FilterAktif aktif)
        {
            return _Querys.Where(x => x.IsActive == (int)aktif || aktif == FilterAktif.Semua).OrderBy(x => x.Id);
        }
        private static EF.MSubMenu GetRow(EF.ModelContext context, int id)
        {
            return context.MSubMenus.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(ExceptionMessage.SUBMENU_NOT_FOUND);
        }
        private static async Task<EF.MSubMenu> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MSubMenus.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(ExceptionMessage.SUBMENU_NOT_FOUND);
        }
        private static void ValidateData(EF.ModelContext context, string name, int? controllerId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(ExceptionMessage.SUBMENU_NAMA_REQUIRED);
            }
            if (controllerId != null)
            {
                ControllerMenu.ValidateActiveController(context, controllerId.Value);
            }
        }
        private static EF.MSubMenu CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            var controllerId = newrow.ControllerId > 0 ? newrow.ControllerId : null;
            ValidateData(context, name, controllerId);

            var row = new EF.MSubMenu
            {
                Id = SequenceUtility.GetNextId<EF.MSubMenu>(context),
                Name = name,
                ControllerId = controllerId,
                Icon = string.IsNullOrWhiteSpace(newrow.Icon) ? null : newrow.Icon.Trim(),
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.MSubMenu row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            int? controllerId = ControllerMenu != null && ControllerMenu.Id > 0 ? ControllerMenu.Id : null;
            ValidateData(context, name, controllerId);

            row.Name = name;
            row.ControllerId = controllerId;
            row.Icon = string.IsNullOrWhiteSpace(Icon) ? null : Icon.Trim();
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MSubMenu row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
