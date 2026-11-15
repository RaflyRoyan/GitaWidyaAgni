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
    public partial class ControllerAction
    {
        #region SYNC
        public static List<ControllerAction> GetDataList(FilterAktif aktif, int? controllerId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, controllerId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ControllerAction, EF.MControllerAction>();
        }
        public static Response<ControllerAction> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MControllerActions.Add(row);
                context.SaveChanges();
                return new ControllerAction(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ControllerAction>(msg));
        }
        /// <summary>
        /// Controller tidak bisa dipindah. Yang diubah: Action, NeedAuth, Description dan IsActive.
        /// IndexAction controller tidak bisa diganti namanya atau dinonaktifkan.
        /// </summary>
        public Response<ControllerAction> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ControllerAction(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ControllerAction>(msg));
        }
        /// <summary>
        /// Soft delete (is_active = 0); baris tetap ada karena direferensikan m_role_action.
        /// </summary>
        public Response<ControllerAction> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(context, row, userId);
                context.SaveChanges();
                return new ControllerAction(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ControllerAction>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ControllerAction>> GetDataListAsync(FilterAktif aktif, int? controllerId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, controllerId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ControllerAction, EF.MControllerAction>();
        }
        public static async Task<Response<ControllerAction>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MControllerActions.Add(row);
                await context.SaveChangesAsync();
                return new ControllerAction(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ControllerAction>(msg));
        }
        public async Task<Response<ControllerAction>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ControllerAction(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ControllerAction>(msg));
        }
        public async Task<Response<ControllerAction>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(context, row, userId);
                await context.SaveChangesAsync();
                return new ControllerAction(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ControllerAction>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.MControllerAction> FilterQuery(FilterAktif aktif, int? controllerId)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (controllerId == null || x.ControllerId == controllerId))
                .OrderBy(x => x.Controller.Name).ThenBy(x => x.Action);
        }
        private static EF.MControllerAction GetRow(EF.ModelContext context, int id)
        {
            return context.MControllerActions.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(ExceptionMessage.CONTROLLER_ACTION_NOT_FOUND);
        }
        private static async Task<EF.MControllerAction> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MControllerActions.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(ExceptionMessage.CONTROLLER_ACTION_NOT_FOUND);
        }
        private static bool IsIndexAction(EF.ModelContext context, EF.MControllerAction row)
        {
            return context.MControllers.Any(x => x.Id == row.ControllerId && x.IndexAction == row.Action);
        }
        private static void ValidateAction(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException(ExceptionMessage.CONTROLLER_ACTION_NAMA_REQUIRED);
            }
        }
        /// <summary>
        /// Membuat baris action tanpa SaveChanges; dipakai juga oleh ControllerMenu (IndexAction dan SaveControllerActions).
        /// </summary>
        internal static EF.MControllerAction CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var action = newrow.Action?.Trim() ?? string.Empty;
            ValidateAction(action);
            ControllerMenu.ValidateActiveController(context, newrow.ControllerId);
            ValidateUtility.CheckDuplicateData<EF.MControllerAction>(context, x => x.ControllerId == newrow.ControllerId && x.Action == action,
                ExceptionMessage.CONTROLLER_ACTION_DUPLICATE);

            var row = new EF.MControllerAction
            {
                Id = SequenceUtility.GetNextId<EF.MControllerAction>(context),
                ControllerId = newrow.ControllerId,
                Action = action,
                NeedAuth = (int)newrow.NeedAuth,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.MControllerAction row, int userId)
        {
            var action = Action?.Trim() ?? string.Empty;
            ValidateAction(action);
            var renamed = !string.Equals(row.Action, action, StringComparison.OrdinalIgnoreCase);
            if ((renamed || IsActive != ActiveStatus.Aktif) && IsIndexAction(context, row))
            {
                throw new ArgumentException(ExceptionMessage.CONTROLLER_ACTION_INDEX_LOCKED);
            }
            if (renamed)
            {
                ValidateUtility.CheckDuplicateData<EF.MControllerAction>(context, x => x.ControllerId == row.ControllerId && x.Action == action && x.Id != row.Id,
                    ExceptionMessage.CONTROLLER_ACTION_DUPLICATE);
            }

            row.Action = action;
            row.NeedAuth = (int)NeedAuth;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.ModelContext context, EF.MControllerAction row, int userId)
        {
            if (IsIndexAction(context, row))
            {
                throw new ArgumentException(ExceptionMessage.CONTROLLER_ACTION_INDEX_LOCKED);
            }
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
