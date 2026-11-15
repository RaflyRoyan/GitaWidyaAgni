using GWALib.General;
using GWALib.General.Utilities;
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
    public partial class HeaderMenuDetail
    {
        #region SYNC
        public static List<HeaderMenuDetail> GetDataList(int? headerMenuId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(headerMenuId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<HeaderMenuDetail, EF.MHeaderMenuDetail>();
        }
        public static Response<HeaderMenuDetail> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MHeaderMenuDetails.Add(row);
                context.SaveChanges();
                return new HeaderMenuDetail(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<HeaderMenuDetail>(msg));
        }
        /// <summary>
        /// HeaderMenu dan SubMenu tidak bisa diubah (unik dan direferensikan m_role_menu). Yang diubah: Seq.
        /// </summary>
        public Response<HeaderMenuDetail> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new HeaderMenuDetail(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<HeaderMenuDetail>(msg));
        }
        /// <summary>
        /// Hard delete (tabel tidak punya is_active), ditolak bila sub menu ini masih dipakai role menu.
        /// </summary>
        public Response<HeaderMenuDetail> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateDelete(context, row);
                context.MHeaderMenuDetails.Remove(row);
                context.SaveChanges();
                return new HeaderMenuDetail(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<HeaderMenuDetail>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<HeaderMenuDetail>> GetDataListAsync(int? headerMenuId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(headerMenuId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<HeaderMenuDetail, EF.MHeaderMenuDetail>();
        }
        public static async Task<Response<HeaderMenuDetail>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MHeaderMenuDetails.Add(row);
                await context.SaveChangesAsync();
                return new HeaderMenuDetail(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<HeaderMenuDetail>(msg));
        }
        public async Task<Response<HeaderMenuDetail>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new HeaderMenuDetail(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<HeaderMenuDetail>(msg));
        }
        public async Task<Response<HeaderMenuDetail>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateDelete(context, row);
                context.MHeaderMenuDetails.Remove(row);
                await context.SaveChangesAsync();
                return new HeaderMenuDetail(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<HeaderMenuDetail>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.MHeaderMenuDetail> FilterQuery(int? headerMenuId)
        {
            return _Querys.Where(x => (headerMenuId == null || x.HeaderMenuId == headerMenuId))
                .OrderBy(x => x.HeaderMenuId).ThenBy(x => x.Seq).ThenBy(x => x.SubMenuId);
        }
        private static EF.MHeaderMenuDetail GetRow(EF.ModelContext context, int id)
        {
            return context.MHeaderMenuDetails.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(ExceptionMessage.HEADERMENU_DETAIL_NOT_FOUND);
        }
        private static async Task<EF.MHeaderMenuDetail> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MHeaderMenuDetails.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(ExceptionMessage.HEADERMENU_DETAIL_NOT_FOUND);
        }
        /// <summary>
        /// Validasi Seq; dipakai juga HeaderMenu.SaveHeaderMenuDetails saat memperbarui baris yang sudah ada.
        /// </summary>
        internal static void ValidateData(int seq)
        {
            if (seq <= 0)
            {
                throw new ArgumentException(ExceptionMessage.HEADERMENU_SEQ_INVALID);
            }
        }
        internal static void ValidateDelete(EF.ModelContext context, EF.MHeaderMenuDetail row)
        {
            if (context.MRoleMenus.Any(x => x.HeaderMenuId == row.HeaderMenuId && x.SubMenuId == row.SubMenuId))
            {
                throw new ArgumentException(ExceptionMessage.HEADERMENU_DETAIL_NO_DEL_ROLE_MENU);
            }
        }
        /// <summary>
        /// Membuat baris tanpa SaveChanges; dipakai juga oleh HeaderMenu.SaveHeaderMenuDetails.
        /// </summary>
        internal static EF.MHeaderMenuDetail CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            ValidateData(newrow.Seq);
            HeaderMenu.ValidateActiveHeaderMenu(context, newrow.HeaderMenuId);
            SubMenu.ValidateActiveSubMenu(context, newrow.SubMenuId);
            ValidateUtility.CheckDuplicateData<EF.MHeaderMenuDetail>(context,
                x => x.HeaderMenuId == newrow.HeaderMenuId && x.SubMenuId == newrow.SubMenuId,
                ExceptionMessage.HEADERMENU_DETAIL_DUPLICATE);

            return new EF.MHeaderMenuDetail
            {
                Id = SequenceUtility.GetNextId<EF.MHeaderMenuDetail>(context),
                HeaderMenuId = newrow.HeaderMenuId,
                SubMenuId = newrow.SubMenuId,
                Seq = newrow.Seq,
                InsDate = DateTime.Now,
                InsBy = userId
            };
        }
        private void SetRow(EF.ModelContext context, EF.MHeaderMenuDetail row, int userId)
        {
            ValidateData(Seq);

            row.Seq = Seq;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
