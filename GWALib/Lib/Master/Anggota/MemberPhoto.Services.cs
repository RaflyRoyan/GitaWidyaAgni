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

namespace GWALib.Lib.Master.Anggota
{
    public partial class MemberPhoto
    {
        // Data ringan foto (tanpa blob) untuk daftar: ada/tidaknya foto, ukuran dan cap waktu untuk cache-buster (?v=).
        public record Info(int Id, int MemberId, int FileSize, DateTime Stamp);

        #region SYNC
        public static List<Info> GetInfoList(int? memberId = null)
        {
            return InfoQuery(memberId).ToList();
        }
        public static List<MemberPhoto> GetDataList(int memberId)
        {
            return FilterQuery(memberId).ToList().MappingData<MemberPhoto, EF.MMemberPhoto>();
        }
        public static Response<MemberPhoto> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MMemberPhotos.Add(row);
                context.SaveChanges();
                return new MemberPhoto(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<MemberPhoto>(msg));
        }
        public Response<MemberPhoto> Update(byte[] photo, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(row, photo, userId);
                context.SaveChanges();
                return new MemberPhoto(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MemberPhoto>(msg));
        }
        public Response<MemberPhoto> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                context.MMemberPhotos.Remove(row);
                context.SaveChanges();
                return new MemberPhoto(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<MemberPhoto>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<Info>> GetInfoListAsync(int? memberId = null)
        {
            return await InfoQuery(memberId).ToListAsync();
        }
        public static async Task<List<MemberPhoto>> GetDataListAsync(int memberId)
        {
            return await (await FilterQuery(memberId).ToListAsync()).MappingDataAsync<MemberPhoto, EF.MMemberPhoto>();
        }
        public static async Task<Response<MemberPhoto>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MMemberPhotos.Add(row);
                await context.SaveChangesAsync();
                return new MemberPhoto(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<MemberPhoto>(msg));
        }
        public async Task<Response<MemberPhoto>> UpdateAsync(byte[] photo, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(row, photo, userId);
                await context.SaveChangesAsync();
                return new MemberPhoto(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MemberPhoto>(msg));
        }
        public async Task<Response<MemberPhoto>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                context.MMemberPhotos.Remove(row);
                await context.SaveChangesAsync();
                return new MemberPhoto(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<MemberPhoto>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<Info> InfoQuery(int? memberId)
        {
            return _Querys.Where(x => memberId == null || x.MemberId == memberId)
                .OrderBy(x => x.MemberId).ThenBy(x => x.Id)
                .Select(x => new Info(x.Id, x.MemberId, x.Photo.Length, x.UpdDate ?? x.InsDate));
        }
        private static IQueryable<EF.MMemberPhoto> FilterQuery(int memberId)
        {
            return _Querys.Where(x => x.MemberId == memberId).OrderBy(x => x.Id);
        }
        private static EF.MMemberPhoto GetRow(EF.ModelContext context, int id)
        {
            return context.MMemberPhotos.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Foto Anggota"));
        }
        private static async Task<EF.MMemberPhoto> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MMemberPhotos.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Foto Anggota"));
        }
        internal static EF.MMemberPhoto CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            Member.GetMember(context, newrow.MemberId);
            var image = ImageUtility.Normalize(newrow.Data);

            return new EF.MMemberPhoto
            {
                Id = SequenceUtility.GetNextId<EF.MMemberPhoto>(context),
                MemberId = newrow.MemberId,
                Photo = image.Data,
                InsDate = DateTime.Now,
                InsBy = userId
            };
        }
        private static void SetRow(EF.MMemberPhoto row, byte[]? data, int userId)
        {
            var image = ImageUtility.Normalize(data);

            row.Photo = image.Data;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
