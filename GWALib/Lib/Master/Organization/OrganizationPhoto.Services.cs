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

namespace GWALib.Lib.Master.Organization
{
    public partial class OrganizationPhoto
    {
        // Data ringan foto (tanpa blob) untuk daftar: ukuran dan cap waktu untuk cache-buster (?v=).
        public record Info(int SeqPhoto, int FileSize, DateTime Stamp);

        #region SYNC
        public static List<Info> GetInfoList()
        {
            return InfoQuery().ToList();
        }
        public static List<OrganizationPhoto> GetDataList()
        {
            return FilterQuery().ToList().MappingData<OrganizationPhoto, EF.MOrganizationPhoto>();
        }
        public static Response<OrganizationPhoto> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MOrganizationPhotos.Add(row);
                context.SaveChanges();
                return new OrganizationPhoto(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<OrganizationPhoto>(msg));
        }
        public Response<OrganizationPhoto> Update(byte[] photo, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, SeqPhoto);
                SetRow(row, photo, userId);
                context.SaveChanges();
                return new OrganizationPhoto(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<OrganizationPhoto>(msg));
        }
        public Response<OrganizationPhoto> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, SeqPhoto);
                context.MOrganizationPhotos.Remove(row);
                context.SaveChanges();
                return new OrganizationPhoto(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<OrganizationPhoto>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<Info>> GetInfoListAsync()
        {
            return await InfoQuery().ToListAsync();
        }
        public static async Task<List<OrganizationPhoto>> GetDataListAsync()
        {
            return await (await FilterQuery().ToListAsync()).MappingDataAsync<OrganizationPhoto, EF.MOrganizationPhoto>();
        }
        public static async Task<Response<OrganizationPhoto>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await CreateRowAsync(context, newrow, userId);
                context.MOrganizationPhotos.Add(row);
                await context.SaveChangesAsync();
                return new OrganizationPhoto(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<OrganizationPhoto>(msg));
        }
        public async Task<Response<OrganizationPhoto>> UpdateAsync(byte[] photo, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, SeqPhoto);
                SetRow(row, photo, userId);
                await context.SaveChangesAsync();
                return new OrganizationPhoto(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<OrganizationPhoto>(msg));
        }
        public async Task<Response<OrganizationPhoto>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, SeqPhoto);
                context.MOrganizationPhotos.Remove(row);
                await context.SaveChangesAsync();
                return new OrganizationPhoto(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<OrganizationPhoto>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<Info> InfoQuery()
        {
            return _Querys.OrderBy(x => x.SeqPhoto)
                .Select(x => new Info(x.SeqPhoto, x.Photo.Length, x.UpdDate ?? x.InsDate));
        }
        private static IQueryable<EF.MOrganizationPhoto> FilterQuery()
        {
            return _Querys.OrderBy(x => x.SeqPhoto);
        }
        private static EF.MOrganizationPhoto GetRow(EF.ModelContext context, int seqPhoto)
        {
            return context.MOrganizationPhotos.SingleOrDefault(x => x.SeqPhoto == seqPhoto)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Foto Organisasi"));
        }
        private static async Task<EF.MOrganizationPhoto> GetRowAsync(EF.ModelContext context, int seqPhoto)
        {
            return await context.MOrganizationPhotos.SingleOrDefaultAsync(x => x.SeqPhoto == seqPhoto)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Foto Organisasi"));
        }
        // Gambar dinormalisasi dulu, baru id diambil, supaya upload yang ditolak tidak memakan nomor m_sequence.
        private static EF.MOrganizationPhoto CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var image = ImageUtility.Normalize(newrow.Data);
            return BuildRow(SequenceUtility.GetNextId<EF.MOrganizationPhoto>(context), image, userId);
        }
        private static async Task<EF.MOrganizationPhoto> CreateRowAsync(EF.ModelContext context, NewRow newrow, int userId)
        {
            var image = ImageUtility.Normalize(newrow.Data);
            return BuildRow(await SequenceUtility.GetNextIdAsync<EF.MOrganizationPhoto>(context), image, userId);
        }
        private static EF.MOrganizationPhoto BuildRow(int seqPhoto, ImageData image, int userId)
        {
            return new EF.MOrganizationPhoto
            {
                SeqPhoto = seqPhoto,
                Photo = image.Data,
                InsDate = DateTime.Now,
                InsBy = userId
            };
        }
        private static void SetRow(EF.MOrganizationPhoto row, byte[]? data, int userId)
        {
            var image = ImageUtility.Normalize(data);

            row.Photo = image.Data;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
