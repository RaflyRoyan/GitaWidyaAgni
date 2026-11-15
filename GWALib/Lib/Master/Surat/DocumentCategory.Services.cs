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

namespace GWALib.Lib.Master.Surat
{
    public partial class DocumentCategory
    {
        #region SYNC
        public static List<DocumentCategory> GetDataList(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<DocumentCategory, EF.MDocumentCategory>();
        }
        public static Response<DocumentCategory> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MDocumentCategories.Add(row);
                context.SaveChanges();
                return new DocumentCategory(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<DocumentCategory>(msg));
        }
        public Response<DocumentCategory> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new DocumentCategory(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DocumentCategory>(msg));
        }
        public Response<DocumentCategory> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new DocumentCategory(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<DocumentCategory>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<DocumentCategory>> GetDataListAsync(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<DocumentCategory, EF.MDocumentCategory>();
        }
        public static async Task<Response<DocumentCategory>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MDocumentCategories.Add(row);
                await context.SaveChangesAsync();
                return new DocumentCategory(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<DocumentCategory>(msg));
        }
        public async Task<Response<DocumentCategory>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new DocumentCategory(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DocumentCategory>(msg));
        }
        public async Task<Response<DocumentCategory>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new DocumentCategory(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<DocumentCategory>(msg));
        }
        #endregion

        #region HELPER
        internal static void ValidateActiveDocumentCategory(EF.ModelContext context, int categoryId)
        {
            var row = context.MDocumentCategories.SingleOrDefault(x => x.Id == categoryId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kategori Dokumen"));
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Kategori Dokumen"));
            }
        }
        private static IQueryable<EF.MDocumentCategory> FilterQuery(FilterAktif aktif)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua))
                .OrderBy(x => x.Name);
        }
        private static EF.MDocumentCategory GetRow(EF.ModelContext context, int id)
        {
            return context.MDocumentCategories.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kategori Dokumen"));
        }
        private static async Task<EF.MDocumentCategory> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MDocumentCategories.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kategori Dokumen"));
        }
        private static void ValidateData(EF.ModelContext context, int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Kategori Dokumen"));
            }
            ValidateUtility.CheckDuplicateData<EF.MDocumentCategory>(context, x => x.Name == name && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Kategori Dokumen"));
        }
        private static EF.MDocumentCategory CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            ValidateData(context, 0, name);

            var row = new EF.MDocumentCategory
            {
                Id = SequenceUtility.GetNextId<EF.MDocumentCategory>(context),
                Name = name,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.MDocumentCategory row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            ValidateData(context, row.Id, name);

            row.Name = name;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MDocumentCategory row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
