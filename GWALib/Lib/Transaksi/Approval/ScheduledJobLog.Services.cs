using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Approval;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Approval
{
    public partial class ScheduledJobLog
    {
        public const int DEFAULT_STALE_MINUTES = 60;
        private const int MESSAGE_LENGTH = 1000;
        private const string STALE_MESSAGE = "Tidak selesai (dianggap gagal saat job berikutnya dimulai)";

        #region SYNC
        public static ScheduledJobLog? GetLastRun(string jobName, ScheduledJobStatus? status = null)
        {
            var row = LastRunQuery(DocumentNumberFormat.ToTypeCode(jobName), status).FirstOrDefault();
            return row != null ? new ScheduledJobLog(row) : null;
        }
        public static List<ScheduledJobLog> GetDataList(string? jobName = null, ScheduledJobStatus? status = null, DateTime? startFrom = null, DateTime? startTo = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(jobName, status, startFrom, startTo);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ScheduledJobLog, EF.TScheduledJobLog>();
        }
        public static Response<ScheduledJobLog> Start(string jobName, string user, int staleMinutes = DEFAULT_STALE_MINUTES)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var code = ValidateJobName(jobName);
                var running = context.TScheduledJobLogs.Where(x => x.JobName == code && x.Status == (int)ScheduledJobStatus.Berjalan).ToList();
                var row = CreateRow(context, code, running, staleMinutes, userId);
                context.SaveChanges();
                return new ScheduledJobLog(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ScheduledJobLog>(msg));
        }
        public Response<ScheduledJobLog> Finish(int processedCount, string? message, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetFinished(row, ScheduledJobStatus.Sukses, processedCount, message, userId);
                context.SaveChanges();
                return new ScheduledJobLog(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ScheduledJobLog>(msg));
        }
        public Response<ScheduledJobLog> Fail(string message, int processedCount, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetFinished(row, ScheduledJobStatus.Gagal, processedCount, message, userId);
                context.SaveChanges();
                return new ScheduledJobLog(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ScheduledJobLog>(msg));
        }
        // Hapus log lama yang sudah selesai; log yang masih Berjalan tidak ikut terhapus.
        public static Response<int> Purge(DateTime startedBefore, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var rows = context.TScheduledJobLogs.Where(x => x.StartedAt < startedBefore && x.Status != (int)ScheduledJobStatus.Berjalan).ToList();
                context.TScheduledJobLogs.RemoveRange(rows);
                context.SaveChanges();
                return rows.Count;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<int>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<ScheduledJobLog?> GetLastRunAsync(string jobName, ScheduledJobStatus? status = null)
        {
            var row = await LastRunQuery(DocumentNumberFormat.ToTypeCode(jobName), status).FirstOrDefaultAsync();
            return row != null ? new ScheduledJobLog(row) : null;
        }
        public static async Task<List<ScheduledJobLog>> GetDataListAsync(string? jobName = null, ScheduledJobStatus? status = null, DateTime? startFrom = null, DateTime? startTo = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(jobName, status, startFrom, startTo);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ScheduledJobLog, EF.TScheduledJobLog>();
        }
        public static async Task<Response<ScheduledJobLog>> StartAsync(string jobName, string user, int staleMinutes = DEFAULT_STALE_MINUTES)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var code = ValidateJobName(jobName);
                var running = await context.TScheduledJobLogs.Where(x => x.JobName == code && x.Status == (int)ScheduledJobStatus.Berjalan).ToListAsync();
                var row = CreateRow(context, code, running, staleMinutes, userId);
                await context.SaveChangesAsync();
                return new ScheduledJobLog(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ScheduledJobLog>(msg));
        }
        public async Task<Response<ScheduledJobLog>> FinishAsync(int processedCount, string? message, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetFinished(row, ScheduledJobStatus.Sukses, processedCount, message, userId);
                await context.SaveChangesAsync();
                return new ScheduledJobLog(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ScheduledJobLog>(msg));
        }
        public async Task<Response<ScheduledJobLog>> FailAsync(string message, int processedCount, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetFinished(row, ScheduledJobStatus.Gagal, processedCount, message, userId);
                await context.SaveChangesAsync();
                return new ScheduledJobLog(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ScheduledJobLog>(msg));
        }
        public static async Task<Response<int>> PurgeAsync(DateTime startedBefore, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var rows = await context.TScheduledJobLogs.Where(x => x.StartedAt < startedBefore && x.Status != (int)ScheduledJobStatus.Berjalan).ToListAsync();
                context.TScheduledJobLogs.RemoveRange(rows);
                await context.SaveChangesAsync();
                return rows.Count;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<int>(msg));
        }
        #endregion

        #region HELPER
        private static string ValidateJobName(string jobName)
        {
            var code = DocumentNumberFormat.ToTypeCode(jobName);
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Job"));
            }
            return code;
        }
        // Job yang sama tidak boleh jalan dobel. Baris Berjalan yang lebih lama dari staleMinutes dianggap
        // tertinggal (proses mati sebelum Finish/Fail) dan ditutup sebagai Gagal dalam SaveChanges yang sama.
        private static EF.TScheduledJobLog CreateRow(EF.ModelContext context, string jobName, List<EF.TScheduledJobLog> running, int staleMinutes, int userId)
        {
            var now = DateTime.Now;
            var active = running.Where(x => x.StartedAt > now.AddMinutes(-staleMinutes)).OrderByDescending(x => x.StartedAt).FirstOrDefault();
            if (active != null)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.JOB_ALREADY_RUNNING, jobName, active.StartedAt));
            }
            foreach (var stale in running)
            {
                stale.Status = (int)ScheduledJobStatus.Gagal;
                stale.FinishedAt = now;
                stale.Message = TransaksiHelper.Truncate(string.IsNullOrWhiteSpace(stale.Message) ? STALE_MESSAGE : stale.Message + " | " + STALE_MESSAGE, MESSAGE_LENGTH);
                stale.UpdDate = now;
                stale.UpdBy = userId;
            }

            var row = new EF.TScheduledJobLog
            {
                Id = SequenceUtility.GetNextId<EF.TScheduledJobLog>(context),
                JobName = jobName,
                StartedAt = now,
                Status = (int)ScheduledJobStatus.Berjalan,
                ProcessedCount = 0,
                InsDate = now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TScheduledJobLogs.Add(row);
            return row;
        }
        private static void SetFinished(EF.TScheduledJobLog row, ScheduledJobStatus status, int processedCount, string? message, int userId)
        {
            TransaksiHelper.ValidateStatus((ScheduledJobStatus)row.Status, "menutup log job", ScheduledJobStatus.Berjalan);
            if (processedCount < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jumlah Diproses"));
            }
            if (status == ScheduledJobStatus.Gagal && string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Pesan Error"));
            }

            row.Status = (int)status;
            row.FinishedAt = DateTime.Now;
            row.ProcessedCount = processedCount;
            row.Message = TransaksiHelper.Truncate(message, MESSAGE_LENGTH);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static IQueryable<EF.TScheduledJobLog> LastRunQuery(string jobName, ScheduledJobStatus? status)
        {
            return _Querys.Where(x => x.JobName == jobName && (status == null || x.Status == (int)status))
                .OrderByDescending(x => x.StartedAt).ThenByDescending(x => x.Id);
        }
        private static IQueryable<EF.TScheduledJobLog> FilterQuery(string? jobName, ScheduledJobStatus? status, DateTime? startFrom, DateTime? startTo)
        {
            var code = string.IsNullOrWhiteSpace(jobName) ? null : DocumentNumberFormat.ToTypeCode(jobName);
            return _Querys.Where(x => (code == null || x.JobName == code)
                    && (status == null || x.Status == (int)status)
                    && (startFrom == null || x.StartedAt >= startFrom)
                    && (startTo == null || x.StartedAt < startTo))
                .OrderByDescending(x => x.StartedAt).ThenByDescending(x => x.Id);
        }
        private static EF.TScheduledJobLog GetRow(EF.ModelContext context, int id)
        {
            return context.TScheduledJobLogs.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Log Job"));
        }
        private static async Task<EF.TScheduledJobLog> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TScheduledJobLogs.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Log Job"));
        }
        #endregion
    }
}
