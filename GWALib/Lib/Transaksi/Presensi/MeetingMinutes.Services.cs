using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Presensi
{
    public partial class MeetingMinutes
    {
        public const int MAX_FILE_BYTES = 10 * 1024 * 1024;
        private const int TEXT_LENGTH = 2000;

        #region SYNC
        public FileContent? GetFile()
        {
            using var context = DBClass.GetContext();
            var file = context.TMeetingMinutes.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.ContentType }).SingleOrDefault();
            return file?.FileData == null ? null : new FileContent(file.FileData, file.FileName ?? "lampiran", file.ContentType ?? "application/octet-stream");
        }
        public static MeetingMinutes? GetBySession(int sessionId)
        {
            var row = _Querys.SingleOrDefault(x => x.SessionId == sessionId);
            return row != null ? new MeetingMinutes(row) : null;
        }
        public static List<MeetingMinutes> GetDataList(int? activityId = null, MeetingMinuteStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(activityId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<MeetingMinutes, EF.TMeetingMinute>();
        }
        public static Response<MeetingMinutes> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TMeetingMinutes.Add(row);
                context.SaveChanges();
                return new MeetingMinutes(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<MeetingMinutes>(msg));
        }
        public Response<MeetingMinutes> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new MeetingMinutes(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MeetingMinutes>(msg));
        }
        // file null / kosong = lampiran dihapus.
        public Response<MeetingMinutes> UpdateFile(byte[]? file, string? fileName, string? contentType, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetFile(row, file, fileName, contentType, userId);
                context.SaveChanges();
                return new MeetingMinutes(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MeetingMinutes>(msg));
        }
        public Response<MeetingMinutes> Finalize(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetFinal(row, userId);
                context.SaveChanges();
                return new MeetingMinutes(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MeetingMinutes>(msg));
        }
        public Response<MeetingMinutes> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateDraft(row, "menghapus notulen");
                var data = new MeetingMinutes(Reload(row.Id));
                context.TMeetingMinutes.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<MeetingMinutes>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetFileAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TMeetingMinutes.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName, x.ContentType }).SingleOrDefaultAsync();
            return file?.FileData == null ? null : new FileContent(file.FileData, file.FileName ?? "lampiran", file.ContentType ?? "application/octet-stream");
        }
        public static async Task<MeetingMinutes?> GetBySessionAsync(int sessionId)
        {
            var row = await _Querys.SingleOrDefaultAsync(x => x.SessionId == sessionId);
            return row != null ? new MeetingMinutes(row) : null;
        }
        public static async Task<List<MeetingMinutes>> GetDataListAsync(int? activityId = null, MeetingMinuteStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(activityId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<MeetingMinutes, EF.TMeetingMinute>();
        }
        public static async Task<Response<MeetingMinutes>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TMeetingMinutes.Add(row);
                await context.SaveChangesAsync();
                return new MeetingMinutes(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<MeetingMinutes>(msg));
        }
        public async Task<Response<MeetingMinutes>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new MeetingMinutes(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MeetingMinutes>(msg));
        }
        public async Task<Response<MeetingMinutes>> UpdateFileAsync(byte[]? file, string? fileName, string? contentType, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetFile(row, file, fileName, contentType, userId);
                await context.SaveChangesAsync();
                return new MeetingMinutes(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MeetingMinutes>(msg));
        }
        public async Task<Response<MeetingMinutes>> FinalizeAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetFinal(row, userId);
                await context.SaveChangesAsync();
                return new MeetingMinutes(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MeetingMinutes>(msg));
        }
        public async Task<Response<MeetingMinutes>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateDraft(row, "menghapus notulen");
                var data = new MeetingMinutes(Reload(row.Id));
                context.TMeetingMinutes.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<MeetingMinutes>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TMeetingMinute Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TMeetingMinute> FilterQuery(int? activityId, MeetingMinuteStatus? status)
        {
            return Project(DBClass.GetContext().TMeetingMinutes
                    .Where(x => (activityId == null || x.Session.ActivityId == activityId) && (status == null || x.Status == (int)status))
                    .OrderByDescending(x => x.InsDate));
        }
        private static EF.TMeetingMinute GetRow(EF.ModelContext context, int id)
        {
            return context.TMeetingMinutes.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Notulen"));
        }
        private static async Task<EF.TMeetingMinute> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TMeetingMinutes.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Notulen"));
        }
        private static void ValidateDraft(EF.TMeetingMinute row, string action)
        {
            TransaksiHelper.ValidateStatus((MeetingMinuteStatus)row.Status, action, MeetingMinuteStatus.Draft);
        }
        private static void ValidateData(EF.ModelContext context, string content, int? chairedBy, int? notulis)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Isi Notulen"));
            }
            if (chairedBy != null)
            {
                Member.GetMember(context, chairedBy.Value);
            }
            if (notulis != null)
            {
                Member.GetMember(context, notulis.Value);
            }
        }
        private static void Apply(EF.TMeetingMinute row, int? chairedBy, int? notulis, string? agenda, string content, string? decisions, string? followUp)
        {
            row.ChairedByMemberId = chairedBy;
            row.NotulisMemberId = notulis;
            row.Agenda = TransaksiHelper.Truncate(agenda, TEXT_LENGTH);
            row.Content = content.Trim();
            row.Decisions = TransaksiHelper.Truncate(decisions, TEXT_LENGTH);
            row.FollowUp = TransaksiHelper.Truncate(followUp, TEXT_LENGTH);
        }
        // Sesi Scheduled / Cancelled tidak punya notulen; satu notulen per sesi.
        private static EF.TMeetingMinute CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var session = context.TAttendanceSessions.SingleOrDefault(x => x.Id == newrow.SessionId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sesi Presensi"));
            TransaksiHelper.ValidateStatus((AttendanceSessionStatus)session.Status, "membuat notulen", AttendanceSessionStatus.Open, AttendanceSessionStatus.Closed);
            ValidateUtility.CheckDuplicateData<EF.TMeetingMinute>(context, x => x.SessionId == newrow.SessionId,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Notulen untuk sesi"));
            var chairedBy = newrow.ChairedByMemberId > 0 ? newrow.ChairedByMemberId : null;
            var notulis = newrow.NotulisMemberId > 0 ? newrow.NotulisMemberId : null;
            ValidateData(context, newrow.Content, chairedBy, notulis);

            var row = new EF.TMeetingMinute
            {
                Id = SequenceUtility.GetNextId<EF.TMeetingMinute>(context),
                SessionId = newrow.SessionId,
                Status = (int)MeetingMinuteStatus.Draft,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            Apply(row, chairedBy, notulis, newrow.Agenda, newrow.Content, newrow.Decisions, newrow.FollowUp);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.TMeetingMinute row, int userId)
        {
            ValidateDraft(row, "mengubah notulen");
            var chairedBy = ChairedByMemberId > 0 ? ChairedByMemberId : null;
            var notulis = NotulisMemberId > 0 ? NotulisMemberId : null;
            ValidateData(context, Content, chairedBy, notulis);
            Apply(row, chairedBy, notulis, Agenda, Content, Decisions, FollowUp);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetFile(EF.TMeetingMinute row, byte[]? file, string? fileName, string? contentType, int userId)
        {
            ValidateDraft(row, "mengubah lampiran notulen");
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
                    throw new ArgumentException(string.Format(ExceptionMessage.FILE_TOO_LARGE, "Lampiran", MAX_FILE_BYTES / 1024 / 1024));
                }
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama File Lampiran"));
                }
                row.FileData = file;
                row.FileName = Path.GetFileName(fileName.Trim());
                row.ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType.Trim();
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetFinal(EF.TMeetingMinute row, int userId)
        {
            ValidateDraft(row, "memfinalkan notulen");
            row.Status = (int)MeetingMinuteStatus.Final;
            row.FinalizedBy = userId;
            row.FinalizedAt = DateTime.Now;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
