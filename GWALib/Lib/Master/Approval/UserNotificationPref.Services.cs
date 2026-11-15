using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Akses;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Approval
{
    public partial class UserNotificationPref
    {
        #region SYNC
        // Tanpa baris preferensi, saluran dianggap dinyalakan.
        public static bool IsChannelEnabled(int userId, string notifType, NotificationChannel channel)
        {
            var state = StateQuery(userId, DocumentNumberFormat.ToTypeCode(notifType), channel).FirstOrDefault();
            return state == null || state == (int)NotificationState.Aktif;
        }
        public static List<UserNotificationPref> GetDataList(int? userId = null, string? notifType = null, NotificationChannel? channel = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(userId, notifType, channel);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<UserNotificationPref, EF.MUserNotificationPref>();
        }
        public static Response<UserNotificationPref> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MUserNotificationPrefs.Add(row);
                context.SaveChanges();
                return new UserNotificationPref(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<UserNotificationPref>(msg));
        }
        public Response<UserNotificationPref> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(row, userId);
                context.SaveChanges();
                return new UserNotificationPref(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<UserNotificationPref>(msg));
        }
        // Hard delete: preferensi kembali ke bawaan.
        public Response<UserNotificationPref> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                context.MUserNotificationPrefs.Remove(row);
                context.SaveChanges();
                return new UserNotificationPref(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<UserNotificationPref>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<bool> IsChannelEnabledAsync(int userId, string notifType, NotificationChannel channel)
        {
            var state = await StateQuery(userId, DocumentNumberFormat.ToTypeCode(notifType), channel).FirstOrDefaultAsync();
            return state == null || state == (int)NotificationState.Aktif;
        }
        public static async Task<List<UserNotificationPref>> GetDataListAsync(int? userId = null, string? notifType = null, NotificationChannel? channel = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(userId, notifType, channel);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<UserNotificationPref, EF.MUserNotificationPref>();
        }
        public static async Task<Response<UserNotificationPref>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MUserNotificationPrefs.Add(row);
                await context.SaveChangesAsync();
                return new UserNotificationPref(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<UserNotificationPref>(msg));
        }
        public async Task<Response<UserNotificationPref>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(row, userId);
                await context.SaveChangesAsync();
                return new UserNotificationPref(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<UserNotificationPref>(msg));
        }
        public async Task<Response<UserNotificationPref>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                context.MUserNotificationPrefs.Remove(row);
                await context.SaveChangesAsync();
                return new UserNotificationPref(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<UserNotificationPref>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<int?> StateQuery(int userId, string notifType, NotificationChannel channel)
        {
            return DBClass.GetContext().MUserNotificationPrefs
                .Where(x => x.UserId == userId && x.NotifType == notifType && x.Channel == (int)channel)
                .Select(x => (int?)x.IsEnabled);
        }
        private static IQueryable<EF.MUserNotificationPref> FilterQuery(int? userId, string? notifType, NotificationChannel? channel)
        {
            var type = string.IsNullOrWhiteSpace(notifType) ? null : DocumentNumberFormat.ToTypeCode(notifType);
            return _Querys.Where(x => (userId == null || x.UserId == userId)
                    && (type == null || x.NotifType == type)
                    && (channel == null || x.Channel == (int)channel))
                .OrderBy(x => x.UserId).ThenBy(x => x.NotifType).ThenBy(x => x.Channel);
        }
        private static EF.MUserNotificationPref GetRow(EF.ModelContext context, int id)
        {
            return context.MUserNotificationPrefs.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Preferensi Notifikasi"));
        }
        private static async Task<EF.MUserNotificationPref> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MUserNotificationPrefs.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Preferensi Notifikasi"));
        }
        private static void ValidateData(NotificationState isEnabled)
        {
            if (!Enum.IsDefined(isEnabled))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Notifikasi"));
            }
        }
        private static EF.MUserNotificationPref CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var notifType = DocumentNumberFormat.ToTypeCode(newrow.NotifType);
            var targetUser = UserAccount.GetUser(context, newrow.UserId);
            if (targetUser.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "User"));
            }
            if (string.IsNullOrWhiteSpace(notifType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Jenis Notifikasi"));
            }
            if (!Enum.IsDefined(newrow.Channel))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Saluran"));
            }
            ValidateData(newrow.IsEnabled);
            ValidateUtility.CheckDuplicateData<EF.MUserNotificationPref>(context,
                x => x.UserId == newrow.UserId && x.NotifType == notifType && x.Channel == (int)newrow.Channel,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Jenis Notifikasi dan Saluran"));

            var row = new EF.MUserNotificationPref
            {
                Id = SequenceUtility.GetNextId<EF.MUserNotificationPref>(context),
                UserId = newrow.UserId,
                NotifType = notifType,
                Channel = (int)newrow.Channel,
                IsEnabled = (int)newrow.IsEnabled,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // User, jenis notifikasi dan saluran tetap; yang bisa diubah hanya status nyala / mati.
        private void SetRow(EF.MUserNotificationPref row, int userId)
        {
            ValidateData(IsEnabled);

            row.IsEnabled = (int)IsEnabled;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
