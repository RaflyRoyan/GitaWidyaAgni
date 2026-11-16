using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Surat;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Surat
{
    public partial class Document
    {
        #region SYNC
        public FileContent? GetFile()
        {
            using var context = DBClass.GetContext();
            var file = context.TDocuments.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.ContentType }).SingleOrDefault();
            return file == null ? null : new FileContent(file.FileData, file.FileName, file.ContentType);
        }
        // visibilities null / kosong = semua.
        public static List<Document> GetDataList(int? categoryId = null, List<DocumentVisibility>? visibilities = null, string? search = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(categoryId, visibilities, search);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Document, EF.TDocument>();
        }
        public static Response<Document> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new Document(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Document>(msg));
        }
        public Response<Document> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new Document(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Document>(msg));
        }
        public Response<Document> UpdateFile(byte[] file, string fileName, string? contentType, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetFile(row, file, fileName, contentType);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                context.SaveChanges();
                return new Document(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Document>(msg));
        }
        public Response<Document> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var data = new Document(Reload(row.Id));
                context.TDocuments.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Document>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetFileAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TDocuments.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.ContentType }).SingleOrDefaultAsync();
            return file == null ? null : new FileContent(file.FileData, file.FileName, file.ContentType);
        }
        public static async Task<List<Document>> GetDataListAsync(int? categoryId = null, List<DocumentVisibility>? visibilities = null, string? search = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(categoryId, visibilities, search);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Document, EF.TDocument>();
        }
        public static async Task<Response<Document>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new Document(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Document>(msg));
        }
        public async Task<Response<Document>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new Document(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Document>(msg));
        }
        public async Task<Response<Document>> UpdateFileAsync(byte[] file, string fileName, string? contentType, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetFile(row, file, fileName, contentType);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                await context.SaveChangesAsync();
                return new Document(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Document>(msg));
        }
        public async Task<Response<Document>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var data = new Document(Reload(row.Id));
                context.TDocuments.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Document>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TDocument Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TDocument> FilterQuery(int? categoryId, List<DocumentVisibility>? visibilities, string? search)
        {
            var query = _Querys.Where(x => categoryId == null || x.CategoryId == categoryId);
            if (visibilities != null && visibilities.Count > 0)
            {
                var values = visibilities.Select(x => (int)x).ToList();
                query = query.Where(x => values.Contains(x.Visibility));
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                var text = search.Trim();
                query = query.Where(x => x.Title.Contains(text));
            }
            return query.OrderByDescending(x => x.InsDate).ThenByDescending(x => x.Id);
        }
        private static EF.TDocument GetRow(EF.ModelContext context, int id)
        {
            return context.TDocuments.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Dokumen"));
        }
        private static async Task<EF.TDocument> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TDocuments.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Dokumen"));
        }
        private static string ValidateData(EF.ModelContext context, int categoryId, string? title, DocumentVisibility visibility)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Dokumen"));
            }
            if (!Enum.IsDefined(visibility))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Visibilitas"));
            }
            DocumentCategory.ValidateActiveDocumentCategory(context, categoryId);
            return TransaksiHelper.Truncate(title, 200)!;
        }
        private static void SetFile(EF.TDocument row, byte[]? file, string? fileName, string? contentType)
        {
            row.FileName = TransaksiHelper.ValidateFile(file, fileName, "File Dokumen", MAX_FILE_BYTES);
            row.FileData = file!;
            row.FileSize = file!.Length;
            row.ContentType = TransaksiHelper.ToNullable(contentType);
        }
        private static EF.TDocument CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var row = new EF.TDocument
            {
                Id = SequenceUtility.GetNextId<EF.TDocument>(context),
                CategoryId = newrow.CategoryId,
                UploadedBy = userId,
                Title = ValidateData(context, newrow.CategoryId, newrow.Title, newrow.Visibility),
                Description = TransaksiHelper.ToNullable(newrow.Description),
                Visibility = (int)newrow.Visibility,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            SetFile(row, newrow.FileData, newrow.FileName, newrow.ContentType);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TDocuments.Add(row);
            return row;
        }
        // Kategori hanya divalidasi aktif bila diganti.
        private void SetRow(EF.ModelContext context, EF.TDocument row, int userId)
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Dokumen"));
            }
            if (!Enum.IsDefined(Visibility))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Visibilitas"));
            }
            if (CategoryId != row.CategoryId)
            {
                DocumentCategory.ValidateActiveDocumentCategory(context, CategoryId);
            }
            row.CategoryId = CategoryId;
            row.Title = TransaksiHelper.Truncate(Title, 200)!;
            row.Description = TransaksiHelper.ToNullable(Description);
            row.Visibility = (int)Visibility;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        #endregion
    }
}
