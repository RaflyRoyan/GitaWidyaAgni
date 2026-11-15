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

namespace GWALib.Lib.Transaksi.Program
{
    public partial class ActivityReport
    {
        public const int MAX_FILE_BYTES = 10 * 1024 * 1024;

        #region SYNC
        public FileContent? GetFile()
        {
            using var context = DBClass.GetContext();
            var file = context.TActivityReports.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.ContentType }).SingleOrDefault();
            return file?.FileData == null ? null : new FileContent(file.FileData, file.FileName ?? "laporan", file.ContentType ?? "application/octet-stream");
        }
        public static List<ActivityReport> GetDataList(int? activityId = null, int? programId = null, ActivityReportStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(activityId, programId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ActivityReport, EF.TActivityReport>();
        }
        public static Response<ActivityReport> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TActivityReports.Add(row);
                context.SaveChanges();
                return new ActivityReport(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivityReport>(msg));
        }
        public Response<ActivityReport> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(row, userId);
                context.SaveChanges();
                return new ActivityReport(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityReport>(msg));
        }
        // file null / kosong = file laporan dihapus.
        public Response<ActivityReport> UpdateFile(byte[]? file, string? fileName, string? contentType, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetFile(row, file, fileName, contentType, userId);
                context.SaveChanges();
                return new ActivityReport(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityReport>(msg));
        }
        public Response<ActivityReport> Submit(string user)
        {
            return SetStatus(ActivityReportStatus.Submitted, user);
        }
        public Response<ActivityReport> RequestRevision(string user)
        {
            return SetStatus(ActivityReportStatus.Revision, user);
        }
        public Response<ActivityReport> Approve(string user)
        {
            return SetStatus(ActivityReportStatus.Approved, user);
        }
        public Response<ActivityReport> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((ActivityReportStatus)row.Status, "menghapus laporan", ActivityReportStatus.Draft);
                var data = new ActivityReport(Reload(row.Id));
                context.TActivityReports.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ActivityReport>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetFileAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TActivityReports.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.ContentType }).SingleOrDefaultAsync();
            return file?.FileData == null ? null : new FileContent(file.FileData, file.FileName ?? "laporan", file.ContentType ?? "application/octet-stream");
        }
        public static async Task<List<ActivityReport>> GetDataListAsync(int? activityId = null, int? programId = null, ActivityReportStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(activityId, programId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ActivityReport, EF.TActivityReport>();
        }
        public static async Task<Response<ActivityReport>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TActivityReports.Add(row);
                await context.SaveChangesAsync();
                return new ActivityReport(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivityReport>(msg));
        }
        public async Task<Response<ActivityReport>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(row, userId);
                await context.SaveChangesAsync();
                return new ActivityReport(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityReport>(msg));
        }
        public async Task<Response<ActivityReport>> UpdateFileAsync(byte[]? file, string? fileName, string? contentType, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetFile(row, file, fileName, contentType, userId);
                await context.SaveChangesAsync();
                return new ActivityReport(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityReport>(msg));
        }
        public async Task<Response<ActivityReport>> SubmitAsync(string user)
        {
            return await SetStatusAsync(ActivityReportStatus.Submitted, user);
        }
        public async Task<Response<ActivityReport>> RequestRevisionAsync(string user)
        {
            return await SetStatusAsync(ActivityReportStatus.Revision, user);
        }
        public async Task<Response<ActivityReport>> ApproveAsync(string user)
        {
            return await SetStatusAsync(ActivityReportStatus.Approved, user);
        }
        public async Task<Response<ActivityReport>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                TransaksiHelper.ValidateStatus((ActivityReportStatus)row.Status, "menghapus laporan", ActivityReportStatus.Draft);
                var data = new ActivityReport(Reload(row.Id));
                context.TActivityReports.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ActivityReport>(msg));
        }
        #endregion

        #region HELPER
        private Response<ActivityReport> SetStatus(ActivityReportStatus target, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ApplyStatus(row, target, userId);
                context.SaveChanges();
                return new ActivityReport(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityReport>(msg));
        }
        private async Task<Response<ActivityReport>> SetStatusAsync(ActivityReportStatus target, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ApplyStatus(row, target, userId);
                await context.SaveChangesAsync();
                return new ActivityReport(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityReport>(msg));
        }
        private static EF.TActivityReport Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TActivityReport> FilterQuery(int? activityId, int? programId, ActivityReportStatus? status)
        {
            return _Querys.Where(x => (activityId == null || x.ActivityId == activityId)
                    && (programId == null || x.ProgramId == programId)
                    && (status == null || x.Status == (int)status))
                .OrderByDescending(x => x.InsDate).ThenByDescending(x => x.Id);
        }
        private static EF.TActivityReport GetRow(EF.ModelContext context, int id)
        {
            return context.TActivityReports.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Laporan Kegiatan"));
        }
        private static async Task<EF.TActivityReport> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TActivityReports.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Laporan Kegiatan"));
        }
        private static void ValidateEditable(EF.TActivityReport row, string action)
        {
            TransaksiHelper.ValidateStatus((ActivityReportStatus)row.Status, action, ActivityReportStatus.Draft, ActivityReportStatus.Revision);
        }
        private static EF.TActivityReport CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var title = newrow.Title?.Trim() ?? string.Empty;
            var activityId = newrow.ActivityId > 0 ? newrow.ActivityId : null;
            var programId = newrow.ProgramId > 0 ? newrow.ProgramId : null;
            if (activityId == null && programId == null)
            {
                throw new ArgumentException(ExceptionMessage.REPORT_SOURCE_REQUIRED);
            }
            if (activityId != null)
            {
                programId ??= Activity.GetActivity(context, activityId.Value).ProgramId;
            }
            if (programId != null && !context.TPrograms.Any(x => x.Id == programId))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Program Kerja"));
            }
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Laporan"));
            }

            var row = new EF.TActivityReport
            {
                Id = SequenceUtility.GetNextId<EF.TActivityReport>(context),
                ActivityId = activityId,
                ProgramId = programId,
                SubmittedBy = userId,
                Title = title,
                Summary = string.IsNullOrWhiteSpace(newrow.Summary) ? null : newrow.Summary.Trim(),
                Status = (int)ActivityReportStatus.Draft,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Kegiatan / program tetap.
        private void SetRow(EF.TActivityReport row, int userId)
        {
            ValidateEditable(row, "mengubah laporan");
            var title = Title?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Laporan"));
            }
            row.Title = title;
            row.Summary = string.IsNullOrWhiteSpace(Summary) ? null : Summary.Trim();
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetFile(EF.TActivityReport row, byte[]? file, string? fileName, string? contentType, int userId)
        {
            ValidateEditable(row, "mengubah file laporan");
            if (file == null || file.Length == 0)
            {
                row.FileData = null;
                row.FileName = null;
                row.ContentType = null;
            }
            else
            {
                if (file.Length > MAX_FILE_BYTES)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FILE_TOO_LARGE, "File Laporan", MAX_FILE_BYTES / 1024 / 1024));
                }
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama File Laporan"));
                }
                row.FileData = file;
                row.FileName = Path.GetFileName(fileName.Trim());
                row.ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType.Trim();
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        // Draft / Revision → Submitted → Approved; Submitted → Revision.
        private static void ApplyStatus(EF.TActivityReport row, ActivityReportStatus target, int userId)
        {
            var current = (ActivityReportStatus)row.Status;
            var now = DateTime.Now;
            switch (target)
            {
                case ActivityReportStatus.Submitted:
                    ValidateEditable(row, "mengajukan laporan");
                    row.SubmittedAt = now;
                    break;
                case ActivityReportStatus.Revision:
                    TransaksiHelper.ValidateStatus(current, "meminta revisi laporan", ActivityReportStatus.Submitted);
                    break;
                case ActivityReportStatus.Approved:
                    TransaksiHelper.ValidateStatus(current, "menyetujui laporan", ActivityReportStatus.Submitted);
                    row.ApprovedBy = userId;
                    row.ApprovedAt = now;
                    break;
                default:
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Laporan"));
            }
            row.Status = (int)target;
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
