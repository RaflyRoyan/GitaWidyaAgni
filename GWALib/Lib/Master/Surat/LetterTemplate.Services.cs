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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Surat
{
    public partial class LetterTemplate
    {
        public const int MAX_FILE_BYTES = 5 * 1024 * 1024;
        private const string DOCX_EXTENSION = ".docx";
        private const string DOCX_CONTENT_TYPE = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        #region SYNC
        public FileContent? GetFile()
        {
            using var context = DBClass.GetContext();
            var file = context.MLetterTemplates.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.ContentType }).SingleOrDefault();
            return file?.FileData == null ? null
                : new FileContent(file.FileData, file.FileName ?? "template" + DOCX_EXTENSION, file.ContentType ?? DOCX_CONTENT_TYPE);
        }
        public static List<string> GetLetterTypes()
        {
            return LetterTypeQuery().ToList();
        }
        public static List<LetterTemplate> GetDataList(FilterAktif aktif, string? letterType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, letterType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<LetterTemplate, ListRow>();
        }
        public static Response<LetterTemplate> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MLetterTemplates.Add(row);
                context.SaveChanges();
                return new LetterTemplate(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<LetterTemplate>(msg));
        }
        public Response<LetterTemplate> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new LetterTemplate(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<LetterTemplate>(msg));
        }
        public Response<LetterTemplate> UpdateFile(byte[]? fileData, string? fileName, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowFile(row, fileData, fileName, userId);
                context.SaveChanges();
                return new LetterTemplate(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<LetterTemplate>(msg));
        }
        public Response<LetterTemplate> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new LetterTemplate(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<LetterTemplate>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetFileAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.MLetterTemplates.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.ContentType }).SingleOrDefaultAsync();
            return file?.FileData == null ? null
                : new FileContent(file.FileData, file.FileName ?? "template" + DOCX_EXTENSION, file.ContentType ?? DOCX_CONTENT_TYPE);
        }
        public static async Task<List<string>> GetLetterTypesAsync()
        {
            return await LetterTypeQuery().ToListAsync();
        }
        public static async Task<List<LetterTemplate>> GetDataListAsync(FilterAktif aktif, string? letterType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, letterType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<LetterTemplate, ListRow>();
        }
        public static async Task<Response<LetterTemplate>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MLetterTemplates.Add(row);
                await context.SaveChangesAsync();
                return new LetterTemplate(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<LetterTemplate>(msg));
        }
        public async Task<Response<LetterTemplate>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new LetterTemplate(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<LetterTemplate>(msg));
        }
        public async Task<Response<LetterTemplate>> UpdateFileAsync(byte[]? fileData, string? fileName, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowFile(row, fileData, fileName, userId);
                await context.SaveChangesAsync();
                return new LetterTemplate(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<LetterTemplate>(msg));
        }
        public async Task<Response<LetterTemplate>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new LetterTemplate(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<LetterTemplate>(msg));
        }
        #endregion

        #region HELPER
        internal static EF.MLetterTemplate GetActiveTemplate(EF.ModelContext context, int templateId)
        {
            var row = context.MLetterTemplates.SingleOrDefault(x => x.Id == templateId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Template Surat"));
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Template Surat"));
            }
            return row;
        }
        // Kode jenis surat: huruf besar, spasi / tanda baca jadi garis bawah (mis. "surat undangan" -> SURAT_UNDANGAN).
        internal static string ToTypeCode(string? value)
        {
            return Regex.Replace(value?.Trim().ToUpperInvariant() ?? string.Empty, "[^A-Z0-9]+", "_").Trim('_');
        }
        private static IQueryable<string> LetterTypeQuery()
        {
            return DBClass.GetContext().MLetterTemplates
                .Select(x => x.LetterType).Distinct().OrderBy(x => x);
        }
        private static IQueryable<ListRow> FilterQuery(FilterAktif aktif, string? letterType)
        {
            var type = string.IsNullOrWhiteSpace(letterType) ? null : ToTypeCode(letterType);
            return _Querys.Where(x => (x.Template.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (type == null || x.Template.LetterType == type))
                .OrderBy(x => x.Template.LetterType).ThenBy(x => x.Template.Name);
        }
        private static EF.MLetterTemplate GetRow(EF.ModelContext context, int id)
        {
            return context.MLetterTemplates.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Template Surat"));
        }
        private static async Task<EF.MLetterTemplate> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MLetterTemplates.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Template Surat"));
        }
        private static void ValidateData(EF.ModelContext context, int id, string name, string letterType)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Template"));
            }
            if (string.IsNullOrWhiteSpace(letterType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Jenis Surat"));
            }
            ValidateUtility.CheckDuplicateData<EF.MLetterTemplate>(context, x => x.Name == name && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Template"));
        }
        // File template harus .docx (zip, diawali "PK") dan maksimal 5 MB.
        private static void ApplyFile(EF.MLetterTemplate row, byte[] fileData, string? fileName)
        {
            if (fileData.Length > MAX_FILE_BYTES)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FILE_TOO_LARGE, "File Template", MAX_FILE_BYTES / 1024 / 1024));
            }
            var name = string.IsNullOrWhiteSpace(fileName) ? "template" + DOCX_EXTENSION : fileName.Trim();
            var isZip = fileData.Length > 4 && fileData[0] == 0x50 && fileData[1] == 0x4B && fileData[2] == 0x03 && fileData[3] == 0x04;
            if (!isZip || !name.EndsWith(DOCX_EXTENSION, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FILE_TYPE_INVALID, "File Template", "Word (.docx)"));
            }
            row.FileData = fileData;
            row.FileName = name;
            row.ContentType = DOCX_CONTENT_TYPE;
        }
        private static EF.MLetterTemplate CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            var letterType = ToTypeCode(newrow.LetterType);
            var body = string.IsNullOrWhiteSpace(newrow.Body) ? null : newrow.Body;
            ValidateData(context, 0, name, letterType);
            if (body == null && newrow.FileData == null)
            {
                throw new ArgumentException(ExceptionMessage.LETTER_TEMPLATE_EMPTY);
            }

            var row = new EF.MLetterTemplate
            {
                Id = SequenceUtility.GetNextId<EF.MLetterTemplate>(context),
                Name = name,
                LetterType = letterType,
                Body = body,
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            if (newrow.FileData != null)
            {
                ApplyFile(row, newrow.FileData, newrow.FileName);
            }
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // File diubah lewat UpdateFile. Body boleh dikosongkan selama file template ada.
        private void SetRow(EF.ModelContext context, EF.MLetterTemplate row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            var letterType = ToTypeCode(LetterType);
            var body = string.IsNullOrWhiteSpace(Body) ? null : Body;
            ValidateData(context, row.Id, name, letterType);
            if (body == null && row.FileName == null)
            {
                throw new ArgumentException(ExceptionMessage.LETTER_TEMPLATE_EMPTY);
            }

            row.Name = name;
            row.LetterType = letterType;
            row.Body = body;
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        // null = hapus file (ditolak bila body juga kosong).
        private static void SetRowFile(EF.MLetterTemplate row, byte[]? fileData, string? fileName, int userId)
        {
            if (fileData != null)
            {
                ApplyFile(row, fileData, fileName);
            }
            else
            {
                if (row.Body == null)
                {
                    throw new ArgumentException(ExceptionMessage.LETTER_TEMPLATE_EMPTY);
                }
                row.FileData = null;
                row.FileName = null;
                row.ContentType = null;
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MLetterTemplate row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
