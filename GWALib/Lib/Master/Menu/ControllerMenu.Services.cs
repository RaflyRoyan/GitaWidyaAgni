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
    public partial class ControllerMenu
    {
        #region SYNC
        public List<ControllerAction> GetControllerActions(bool refresh)
        {
            if (refresh || ControllerActionList == null)
            {
                ControllerActionList = ControllerAction.GetDataList(FilterAktif.Semua, Id);
            }
            return ControllerActionList;
        }
        public static List<ControllerMenu> GetDataList(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ControllerMenu, EF.MController>();
        }
        public static Response<ControllerMenu> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MControllers.Add(row);
                EnsureIndexAction(context, row, userId);
                context.SaveChanges();
                return new ControllerMenu(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ControllerMenu>(msg));
        }
        public Response<ControllerMenu> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ControllerMenu(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ControllerMenu>(msg));
        }
        public Response<ControllerMenu> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new ControllerMenu(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ControllerMenu>(msg));
        }
        public Response<ControllerMenu> SaveControllerActions(List<ControllerAction.NewRow> rows, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetControllerActions(context, row, rows, userId);
                context.SaveChanges();
                return new ControllerMenu(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ControllerMenu>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<ControllerAction>> GetControllerActionsAsync(bool refresh)
        {
            if (refresh || ControllerActionList == null)
            {
                ControllerActionList = await ControllerAction.GetDataListAsync(FilterAktif.Semua, Id);
            }
            return ControllerActionList;
        }
        public static async Task<List<ControllerMenu>> GetDataListAsync(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ControllerMenu, EF.MController>();
        }
        public static async Task<Response<ControllerMenu>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MControllers.Add(row);
                EnsureIndexAction(context, row, userId);
                await context.SaveChangesAsync();
                return new ControllerMenu(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ControllerMenu>(msg));
        }
        public async Task<Response<ControllerMenu>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ControllerMenu(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ControllerMenu>(msg));
        }
        public async Task<Response<ControllerMenu>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new ControllerMenu(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ControllerMenu>(msg));
        }
        public async Task<Response<ControllerMenu>> SaveControllerActionsAsync(List<ControllerAction.NewRow> rows, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetControllerActions(context, row, rows, userId);
                await context.SaveChangesAsync();
                return new ControllerMenu(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ControllerMenu>(msg));
        }
        #endregion

        #region HELPER
        internal static void ValidateActiveController(EF.ModelContext context, int controllerId)
        {
            // Local: controller yang baru di-Add dalam command yang sama (Save) belum ada di DB.
            var row = context.MControllers.Local.SingleOrDefault(x => x.Id == controllerId)
                ?? context.MControllers.SingleOrDefault(x => x.Id == controllerId)
                ?? throw new ArgumentException(ExceptionMessage.CONTROLLER_NOT_FOUND);
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(ExceptionMessage.CONTROLLER_NOT_ACTIVE);
            }
        }
        private static IQueryable<EF.MController> FilterQuery(FilterAktif aktif)
        {
            return _Querys.Where(x => x.IsActive == (int)aktif || aktif == FilterAktif.Semua).OrderBy(x => x.Name);
        }
        private static EF.MController GetRow(EF.ModelContext context, int id)
        {
            return context.MControllers.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(ExceptionMessage.CONTROLLER_NOT_FOUND);
        }
        private static async Task<EF.MController> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MControllers.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(ExceptionMessage.CONTROLLER_NOT_FOUND);
        }
        private static void ValidateData(EF.ModelContext context, int id, string name, string indexAction)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(ExceptionMessage.CONTROLLER_NAMA_REQUIRED);
            }
            if (string.IsNullOrWhiteSpace(indexAction))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Index Action"));
            }
            ValidateUtility.CheckDuplicateData<EF.MController>(context, x => x.Name == name && x.Id != id, ExceptionMessage.CONTROLLER_DUPLICATE);
        }
        private static EF.MController CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            var indexAction = newrow.IndexAction?.Trim() ?? string.Empty;
            ValidateData(context, 0, name, indexAction);

            var row = new EF.MController
            {
                Id = SequenceUtility.GetNextId<EF.MController>(context),
                Name = name,
                IndexAction = indexAction,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.MController row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            var indexAction = IndexAction?.Trim() ?? string.Empty;
            ValidateData(context, row.Id, name, indexAction);

            var indexChanged = !string.Equals(row.IndexAction, indexAction, StringComparison.OrdinalIgnoreCase);
            row.Name = name;
            row.IndexAction = indexAction;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            if (indexChanged)
            {
                // Dipanggil sebelum status diubah, karena action baru hanya boleh ditambahkan ke controller yang aktif.
                EnsureIndexAction(context, row, userId);
            }
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MController row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void EnsureIndexAction(EF.ModelContext context, EF.MController row, int userId)
        {
            var action = context.MControllerActions.SingleOrDefault(x => x.ControllerId == row.Id && x.Action == row.IndexAction);
            if (action == null)
            {
                var newrow = new ControllerAction.NewRow { ControllerId = row.Id, Action = row.IndexAction };
                context.MControllerActions.Add(ControllerAction.CreateRow(context, newrow, userId));
            }
            else if (action.IsActive != (int)ActiveStatus.Aktif)
            {
                action.IsActive = (int)ActiveStatus.Aktif;
                action.UpdDate = DateTime.Now;
                action.UpdBy = userId;
            }
        }
        private static void SetControllerActions(EF.ModelContext context, EF.MController row, List<ControllerAction.NewRow>? rows, int userId)
        {
            var desired = (rows ?? new()).Select(x => new ControllerAction.NewRow
            {
                ControllerId = row.Id,
                Action = x.Action?.Trim() ?? string.Empty,
                NeedAuth = x.NeedAuth,
                Description = string.IsNullOrWhiteSpace(x.Description) ? null : x.Description.Trim()
            }).ToList();
            if (desired.Count == 0)
            {
                throw new ArgumentException(ExceptionMessage.CONTROLLER_ACTION_REQUIRED);
            }
            if (desired.Any(x => string.IsNullOrWhiteSpace(x.Action)))
            {
                throw new ArgumentException(ExceptionMessage.CONTROLLER_ACTION_NAMA_REQUIRED);
            }
            if (desired.GroupBy(x => x.Action, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1))
            {
                throw new ArgumentException(ExceptionMessage.CONTROLLER_ACTION_DUPLICATE);
            }
            if (!desired.Any(x => string.Equals(x.Action, row.IndexAction, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.CONTROLLER_ACTION_INDEX_REQUIRED, row.IndexAction));
            }

            var existing = context.MControllerActions.Where(x => x.ControllerId == row.Id).ToList();
            foreach (var old in existing)
            {
                var item = desired.FirstOrDefault(x => string.Equals(x.Action, old.Action, StringComparison.OrdinalIgnoreCase));
                if (item == null)
                {
                    if (old.IsActive != (int)ActiveStatus.TidakAktif)
                    {
                        old.IsActive = (int)ActiveStatus.TidakAktif;
                        old.UpdDate = DateTime.Now;
                        old.UpdBy = userId;
                    }
                }
                else if (old.NeedAuth != (int)item.NeedAuth || old.Description != item.Description || old.IsActive != (int)ActiveStatus.Aktif)
                {
                    old.NeedAuth = (int)item.NeedAuth;
                    old.Description = item.Description;
                    old.IsActive = (int)ActiveStatus.Aktif;
                    old.UpdDate = DateTime.Now;
                    old.UpdBy = userId;
                    GWAShared.Utilities.ValidateUtility.ValidateDAModel(old);
                }
            }
            foreach (var item in desired.Where(d => !existing.Any(x => string.Equals(x.Action, d.Action, StringComparison.OrdinalIgnoreCase))))
            {
                context.MControllerActions.Add(ControllerAction.CreateRow(context, item, userId));
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
