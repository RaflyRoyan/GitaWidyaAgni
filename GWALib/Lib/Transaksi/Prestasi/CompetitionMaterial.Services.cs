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

namespace GWALib.Lib.Transaksi.Prestasi
{
    public partial class CompetitionMaterial
    {
        public const int MAX_FILE_BYTES = 20 * 1024 * 1024;

        #region SYNC
        public FileContent? GetFile()
        {
            using var context = DBClass.GetContext();
            var file = context.TCompetitionMaterials.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.ContentType }).SingleOrDefault();
            return file?.FileData == null ? null : new FileContent(file.FileData, file.FileName ?? "materi", file.ContentType);
        }
        // competitionId 0 = hanya materi umum; null = semua. visibilities null / kosong = semua.
        public static List<CompetitionMaterial> GetDataList(int? competitionId = null, List<MaterialVisibility>? visibilities = null)
        {
            return FilterQuery(competitionId, visibilities).ToList().MappingData<CompetitionMaterial, EF.TCompetitionMaterial>();
        }
        public static Response<CompetitionMaterial> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new CompetitionMaterial(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<CompetitionMaterial>(msg));
        }
        public Response<CompetitionMaterial> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new CompetitionMaterial(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CompetitionMaterial>(msg));
        }
        // file null / kosong = file dihapus (ditolak untuk materi berjenis File).
        public Response<CompetitionMaterial> UpdateFile(byte[]? file, string? fileName, string? contentType, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetFile(row, file, fileName, contentType);
                ValidateSource(row);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                context.SaveChanges();
                return new CompetitionMaterial(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CompetitionMaterial>(msg));
        }
        public Response<CompetitionMaterial> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var data = new CompetitionMaterial(Reload(row.Id));
                context.TCompetitionMaterials.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<CompetitionMaterial>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetFileAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TCompetitionMaterials.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.ContentType }).SingleOrDefaultAsync();
            return file?.FileData == null ? null : new FileContent(file.FileData, file.FileName ?? "materi", file.ContentType);
        }
        public static async Task<List<CompetitionMaterial>> GetDataListAsync(int? competitionId = null, List<MaterialVisibility>? visibilities = null)
        {
            return await (await FilterQuery(competitionId, visibilities).ToListAsync()).MappingDataAsync<CompetitionMaterial, EF.TCompetitionMaterial>();
        }
        public static async Task<Response<CompetitionMaterial>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new CompetitionMaterial(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<CompetitionMaterial>(msg));
        }
        public async Task<Response<CompetitionMaterial>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new CompetitionMaterial(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CompetitionMaterial>(msg));
        }
        public async Task<Response<CompetitionMaterial>> UpdateFileAsync(byte[]? file, string? fileName, string? contentType, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetFile(row, file, fileName, contentType);
                ValidateSource(row);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                await context.SaveChangesAsync();
                return new CompetitionMaterial(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CompetitionMaterial>(msg));
        }
        public async Task<Response<CompetitionMaterial>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var data = new CompetitionMaterial(Reload(row.Id));
                context.TCompetitionMaterials.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<CompetitionMaterial>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TCompetitionMaterial Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TCompetitionMaterial> FilterQuery(int? competitionId, List<MaterialVisibility>? visibilities)
        {
            var query = competitionId == 0 ? _Querys.Where(x => x.CompetitionId == null)
                : _Querys.Where(x => competitionId == null || x.CompetitionId == competitionId);
            if (visibilities != null && visibilities.Count > 0)
            {
                var values = visibilities.Select(x => (int)x).ToList();
                query = query.Where(x => values.Contains(x.Visibility));
            }
            return query.OrderByDescending(x => x.InsDate).ThenByDescending(x => x.Id);
        }
        private static EF.TCompetitionMaterial GetRow(EF.ModelContext context, int id)
        {
            return context.TCompetitionMaterials.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Materi Lomba"));
        }
        private static async Task<EF.TCompetitionMaterial> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TCompetitionMaterials.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Materi Lomba"));
        }
        private static void SetFile(EF.TCompetitionMaterial row, byte[]? file, string? fileName, string? contentType)
        {
            if (file == null || file.Length == 0)
            {
                row.FileData = null;
                row.FileName = null;
                row.ContentType = null;
                return;
            }
            row.FileName = TransaksiHelper.ValidateFile(file, fileName, "File Materi", MAX_FILE_BYTES);
            row.FileData = file;
            row.ContentType = TransaksiHelper.ToNullable(contentType);
        }
        // File wajib file; Link wajib URL; Video URL atau file.
        private static void ValidateSource(EF.TCompetitionMaterial row)
        {
            var hasFile = row.FileData != null || row.FileName != null;
            switch ((MaterialType)row.MaterialType)
            {
                case MaterialType.File when !hasFile:
                    throw new ArgumentException(ExceptionMessage.MATERIAL_FILE_REQUIRED);
                case MaterialType.Link when row.Url == null:
                case MaterialType.Video when row.Url == null && !hasFile:
                    throw new ArgumentException(ExceptionMessage.MATERIAL_SOURCE_REQUIRED);
            }
        }
        private static void ValidateData(EF.ModelContext context, int? competitionId, string? title, MaterialType materialType, MaterialVisibility visibility)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Materi"));
            }
            if (!Enum.IsDefined(materialType) || !Enum.IsDefined(visibility))
            {
                throw new ArgumentException(ExceptionMessage.DATA_INVALID);
            }
            if (competitionId != null)
            {
                Competition.GetCompetition(context, competitionId.Value);
            }
        }
        private static EF.TCompetitionMaterial CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var competitionId = newrow.CompetitionId > 0 ? newrow.CompetitionId : null;
            ValidateData(context, competitionId, newrow.Title, newrow.MaterialType, newrow.Visibility);
            var row = new EF.TCompetitionMaterial
            {
                Id = SequenceUtility.GetNextId<EF.TCompetitionMaterial>(context),
                CompetitionId = competitionId,
                Title = TransaksiHelper.Truncate(newrow.Title, 200)!,
                Description = TransaksiHelper.ToNullable(newrow.Description),
                MaterialType = (int)newrow.MaterialType,
                Url = TransaksiHelper.Truncate(newrow.Url, 255),
                Visibility = (int)newrow.Visibility,
                UploadedBy = userId,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            SetFile(row, newrow.FileData, newrow.FileName, newrow.ContentType);
            ValidateSource(row);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TCompetitionMaterials.Add(row);
            return row;
        }
        // File tidak diubah di sini (UpdateFile).
        private void SetRow(EF.ModelContext context, EF.TCompetitionMaterial row, int userId)
        {
            var competitionId = CompetitionId > 0 ? CompetitionId : null;
            ValidateData(context, competitionId, Title, MaterialType, Visibility);
            row.CompetitionId = competitionId;
            row.Title = TransaksiHelper.Truncate(Title, 200)!;
            row.Description = TransaksiHelper.ToNullable(Description);
            row.MaterialType = (int)MaterialType;
            row.Url = TransaksiHelper.Truncate(Url, 255);
            row.Visibility = (int)Visibility;
            ValidateSource(row);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        #endregion
    }
}
