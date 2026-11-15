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
    public partial class HeaderMenu
    {
        #region SYNC
        public List<HeaderMenuDetail> GetHeaderMenuDetails(bool refresh)
        {
            if (refresh || HeaderMenuDetailList == null)
            {
                HeaderMenuDetailList = HeaderMenuDetail.GetDataList(Id);
            }
            return HeaderMenuDetailList;
        }
        public static List<HeaderMenu> GetDataList(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<HeaderMenu, EF.MHeaderMenu>();
        }
        public static Response<HeaderMenu> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MHeaderMenus.Add(row);
                context.SaveChanges();
                return new HeaderMenu(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<HeaderMenu>(msg));
        }
        public Response<HeaderMenu> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new HeaderMenu(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<HeaderMenu>(msg));
        }
        public Response<HeaderMenu> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new HeaderMenu(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<HeaderMenu>(msg));
        }
        /// <summary>
        /// Mengganti seluruh sub menu (m_header_menu_detail) header ini dengan daftar yang diberikan, dalam satu SaveChanges.
        /// Sub menu yang tidak ada di daftar dilepas (ditolak bila masih dipakai role menu), yang baru ditambahkan,
        /// yang sudah ada diperbarui Seq-nya. Daftar kosong = header tanpa sub menu.
        /// </summary>
        public Response<HeaderMenu> SaveHeaderMenuDetails(List<HeaderMenuDetail.NewRow> rows, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetHeaderMenuDetails(context, row, rows, userId);
                context.SaveChanges();
                return new HeaderMenu(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<HeaderMenu>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<HeaderMenuDetail>> GetHeaderMenuDetailsAsync(bool refresh)
        {
            if (refresh || HeaderMenuDetailList == null)
            {
                HeaderMenuDetailList = await HeaderMenuDetail.GetDataListAsync(Id);
            }
            return HeaderMenuDetailList;
        }
        public static async Task<List<HeaderMenu>> GetDataListAsync(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<HeaderMenu, EF.MHeaderMenu>();
        }
        public static async Task<Response<HeaderMenu>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MHeaderMenus.Add(row);
                await context.SaveChangesAsync();
                return new HeaderMenu(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<HeaderMenu>(msg));
        }
        public async Task<Response<HeaderMenu>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new HeaderMenu(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<HeaderMenu>(msg));
        }
        public async Task<Response<HeaderMenu>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new HeaderMenu(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<HeaderMenu>(msg));
        }
        public async Task<Response<HeaderMenu>> SaveHeaderMenuDetailsAsync(List<HeaderMenuDetail.NewRow> rows, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetHeaderMenuDetails(context, row, rows, userId);
                await context.SaveChangesAsync();
                return new HeaderMenu(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<HeaderMenu>(msg));
        }
        #endregion

        #region HELPER
        /// <summary>
        /// Dipakai HeaderMenuDetail untuk memastikan header menu ada dan aktif.
        /// </summary>
        internal static void ValidateActiveHeaderMenu(EF.ModelContext context, int headerMenuId)
        {
            var row = context.MHeaderMenus.SingleOrDefault(x => x.Id == headerMenuId)
                ?? throw new ArgumentException(ExceptionMessage.HEADERMENU_NOT_FOUND);
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(ExceptionMessage.HEADERMENU_NOT_ACTIVE);
            }
        }
        private static IQueryable<EF.MHeaderMenu> FilterQuery(FilterAktif aktif)
        {
            return _Querys.Where(x => x.IsActive == (int)aktif || aktif == FilterAktif.Semua).OrderBy(x => x.Seq).ThenBy(x => x.Id);
        }
        private static EF.MHeaderMenu GetRow(EF.ModelContext context, int id)
        {
            return context.MHeaderMenus.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(ExceptionMessage.HEADERMENU_NOT_FOUND);
        }
        private static async Task<EF.MHeaderMenu> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MHeaderMenus.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(ExceptionMessage.HEADERMENU_NOT_FOUND);
        }
        private static void ValidateData(EF.ModelContext context, string name, int? controllerId, int seq)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(ExceptionMessage.HEADERMENU_NAMA_REQUIRED);
            }
            if (seq <= 0)
            {
                throw new ArgumentException(ExceptionMessage.HEADERMENU_SEQ_INVALID);
            }
            if (controllerId != null)
            {
                ControllerMenu.ValidateActiveController(context, controllerId.Value);
            }
        }
        private static EF.MHeaderMenu CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            var controllerId = newrow.ControllerId > 0 ? newrow.ControllerId : null;
            ValidateData(context, name, controllerId, newrow.Seq);

            var row = new EF.MHeaderMenu
            {
                Id = SequenceUtility.GetNextId<EF.MHeaderMenu>(context),
                Name = name,
                Section = string.IsNullOrWhiteSpace(newrow.Section) ? null : newrow.Section.Trim(),
                ControllerId = controllerId,
                Icon = string.IsNullOrWhiteSpace(newrow.Icon) ? null : newrow.Icon.Trim(),
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                Seq = newrow.Seq,
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.MHeaderMenu row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            int? controllerId = ControllerMenu != null && ControllerMenu.Id > 0 ? ControllerMenu.Id : null;
            ValidateData(context, name, controllerId, Seq);

            row.Name = name;
            row.Section = string.IsNullOrWhiteSpace(Section) ? null : Section.Trim();
            row.ControllerId = controllerId;
            row.Icon = string.IsNullOrWhiteSpace(Icon) ? null : Icon.Trim();
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.Seq = Seq;
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MHeaderMenu row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetHeaderMenuDetails(EF.ModelContext context, EF.MHeaderMenu row, List<HeaderMenuDetail.NewRow>? rows, int userId)
        {
            var desired = (rows ?? new()).Select(x => new HeaderMenuDetail.NewRow
            {
                HeaderMenuId = row.Id,
                SubMenuId = x.SubMenuId,
                Seq = x.Seq
            }).ToList();
            if (desired.GroupBy(x => x.SubMenuId).Any(g => g.Count() > 1))
            {
                throw new ArgumentException(ExceptionMessage.HEADERMENU_DETAIL_DUPLICATE);
            }

            var existing = context.MHeaderMenuDetails.Where(x => x.HeaderMenuId == row.Id).ToList();
            foreach (var old in existing)
            {
                var item = desired.FirstOrDefault(x => x.SubMenuId == old.SubMenuId);
                if (item == null)
                {
                    HeaderMenuDetail.ValidateDelete(context, old);
                    context.MHeaderMenuDetails.Remove(old);
                }
                else if (old.Seq != item.Seq)
                {
                    HeaderMenuDetail.ValidateData(item.Seq);
                    old.Seq = item.Seq;
                    old.UpdDate = DateTime.Now;
                    old.UpdBy = userId;
                }
            }
            foreach (var item in desired.Where(d => !existing.Any(x => x.SubMenuId == d.SubMenuId)))
            {
                context.MHeaderMenuDetails.Add(HeaderMenuDetail.CreateRow(context, item, userId));
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
