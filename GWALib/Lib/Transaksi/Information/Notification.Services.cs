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

namespace GWALib.Lib.Transaksi.Information
{
    public partial class Notification
    {
        #region SYNC
        public static List<Notification> GetMyList(string user, bool unreadOnly = false, int startdata = -1, int jmldata = 20)
        {
            var userId = ValidateUtility.ValidateUser(user);
            return Page(MyQuery(userId, unreadOnly), startdata, jmldata).ToList().MappingData<Notification, EF.TNotification>();
        }
        public static int GetUnreadCount(string user)
        {
            var userId = ValidateUtility.ValidateUser(user);
            using var context = DBClass.GetContext();
            return context.TNotifications.Count(x => x.UserId == userId && x.ReadAt == null);
        }
        // Mengembalikan jumlah notifikasi yang dibuat (penerima yang mematikan InApp dilewati).
        public static Response<int> Send(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = AddRows(context, newrow, userId);
                context.SaveChanges();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        public Response<Notification> MarkRead(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetOwnRow(context, Id, userId);
                SetRead(row, userId);
                context.SaveChanges();
                return new Notification(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Notification>(msg));
        }
        // Mengembalikan jumlah notifikasi yang ditandai dibaca.
        public static Response<int> MarkAllRead(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var now = DateTime.Now;
                return context.TNotifications.Where(x => x.UserId == userId && x.ReadAt == null)
                    .ExecuteUpdate(s => s.SetProperty(x => x.ReadAt, now).SetProperty(x => x.UpdDate, now).SetProperty(x => x.UpdBy, userId));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<int>(msg));
        }
        public Response<Notification> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetOwnRow(context, Id, userId);
                context.TNotifications.Remove(row);
                context.SaveChanges();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Notification>(msg));
        }
        // Untuk job: hapus notifikasi yang sudah dibaca dan lebih lama dari olderThanDays hari. Mengembalikan jumlah yang dihapus.
        public static int Purge(int olderThanDays = 90)
        {
            using var context = DBClass.GetContext();
            var limit = DateTime.Now.AddDays(-Math.Max(1, olderThanDays));
            return context.TNotifications.Where(x => x.ReadAt != null && x.InsDate < limit).ExecuteDelete();
        }
        #endregion

        #region ASYNC
        public static async Task<List<Notification>> GetMyListAsync(string user, bool unreadOnly = false, int startdata = -1, int jmldata = 20)
        {
            var userId = ValidateUtility.ValidateUser(user);
            return await (await Page(MyQuery(userId, unreadOnly), startdata, jmldata).ToListAsync()).MappingDataAsync<Notification, EF.TNotification>();
        }
        public static async Task<int> GetUnreadCountAsync(string user)
        {
            var userId = ValidateUtility.ValidateUser(user);
            using var context = DBClass.GetContext();
            return await context.TNotifications.CountAsync(x => x.UserId == userId && x.ReadAt == null);
        }
        public static async Task<Response<int>> SendAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = AddRows(context, newrow, userId);
                await context.SaveChangesAsync();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        public async Task<Response<Notification>> MarkReadAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetOwnRowAsync(context, Id, userId);
                SetRead(row, userId);
                await context.SaveChangesAsync();
                return new Notification(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Notification>(msg));
        }
        public static async Task<Response<int>> MarkAllReadAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var now = DateTime.Now;
                return await context.TNotifications.Where(x => x.UserId == userId && x.ReadAt == null)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.ReadAt, now).SetProperty(x => x.UpdDate, now).SetProperty(x => x.UpdBy, userId));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<int>(msg));
        }
        public async Task<Response<Notification>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetOwnRowAsync(context, Id, userId);
                context.TNotifications.Remove(row);
                await context.SaveChangesAsync();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Notification>(msg));
        }
        public static async Task<int> PurgeAsync(int olderThanDays = 90)
        {
            using var context = DBClass.GetContext();
            var limit = DateTime.Now.AddDays(-Math.Max(1, olderThanDays));
            return await context.TNotifications.Where(x => x.ReadAt != null && x.InsDate < limit).ExecuteDeleteAsync();
        }
        #endregion

        #region INTERNAL
        // Tanpa SaveChanges; untuk modul lain di dalam context-nya. False bila penerima mematikan InApp untuk jenis ini.
        internal static bool Add(EF.ModelContext context, int userId, string notifType, string title, string? message = null,
            string? linkUrl = null, string? refType = null, int? refId = null, int? insBy = null)
        {
            return AddRows(context, new NewRow
            {
                UserIds = new List<int> { userId },
                NotifType = notifType,
                Title = title,
                Message = message,
                LinkUrl = linkUrl,
                RefType = refType,
                RefId = refId
            }, insBy) > 0;
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TNotification> MyQuery(int userId, bool unreadOnly)
        {
            return _Querys.Where(x => x.UserId == userId && (!unreadOnly || x.ReadAt == null))
                .OrderByDescending(x => x.InsDate).ThenByDescending(x => x.Id);
        }
        private static IQueryable<EF.TNotification> Page(IQueryable<EF.TNotification> query, int startdata, int jmldata)
        {
            return startdata > -1 ? query.Skip(startdata).Take(jmldata) : query;
        }
        private static EF.TNotification GetOwnRow(EF.ModelContext context, int id, int userId)
        {
            var row = context.TNotifications.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Notifikasi"));
            return row.UserId == userId ? row : throw new ArgumentException(ExceptionMessage.NOTIFICATION_NOT_OWNER);
        }
        private static async Task<EF.TNotification> GetOwnRowAsync(EF.ModelContext context, int id, int userId)
        {
            var row = await context.TNotifications.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Notifikasi"));
            return row.UserId == userId ? row : throw new ArgumentException(ExceptionMessage.NOTIFICATION_NOT_OWNER);
        }
        private static void SetRead(EF.TNotification row, int userId)
        {
            if (row.ReadAt != null)
            {
                return;
            }
            row.ReadAt = DateTime.Now;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static int AddRows(EF.ModelContext context, NewRow newrow, int? insBy)
        {
            var type = DocumentNumberFormat.ToTypeCode(newrow.NotifType);
            if (type.Length == 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Jenis Notifikasi"));
            }
            if (string.IsNullOrWhiteSpace(newrow.Title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Notifikasi"));
            }
            var ids = (newrow.UserIds ?? new List<int>()).Where(x => x > 0).Distinct().ToList();
            if (ids.Count == 0)
            {
                throw new ArgumentException(ExceptionMessage.NOTIFICATION_RECIPIENT_REQUIRED);
            }
            var link = InformationHelper.ValidateLink(newrow.LinkUrl, "Link Notifikasi");
            var existing = context.MUsers.Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToList();
            if (existing.Count != ids.Count)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Penerima Notifikasi"));
            }
            // Tanpa baris preferensi = InApp dinyalakan.
            var muted = context.MUserNotificationPrefs
                .Where(x => ids.Contains(x.UserId) && x.NotifType == type && x.Channel == (int)NotificationChannel.InApp && x.IsEnabled == (int)NotificationState.Nonaktif)
                .Select(x => x.UserId).ToHashSet();
            var recipients = ids.Where(x => !muted.Contains(x)).ToList();
            if (recipients.Count == 0)
            {
                return 0;
            }
            var template = new EF.TNotification
            {
                NotifType = type,
                Title = newrow.Title.Trim(),
                Message = TransaksiHelper.ToNullable(newrow.Message),
                LinkUrl = link,
                RefType = TransaksiHelper.ToNullable(newrow.RefType)?.ToLowerInvariant(),
                RefId = newrow.RefId > 0 ? newrow.RefId : null
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(template);
            var firstId = SequenceUtility.ReserveIds<EF.TNotification>(context, recipients.Count);
            var now = DateTime.Now;
            for (var i = 0; i < recipients.Count; i++)
            {
                context.TNotifications.Add(new EF.TNotification
                {
                    Id = firstId + i,
                    UserId = recipients[i],
                    NotifType = template.NotifType,
                    Title = template.Title,
                    Message = template.Message,
                    LinkUrl = template.LinkUrl,
                    RefType = template.RefType,
                    RefId = template.RefId,
                    InsDate = now,
                    InsBy = insBy
                });
            }
            return recipients.Count;
        }
        #endregion
    }
}
