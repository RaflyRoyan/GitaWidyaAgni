using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Akses;
using GWALib.Lib.Master.Approval;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Approval
{
    public partial class MessageOutbox
    {
        public const int MAX_ATTEMPTS = 5;
        private const int RETRY_DELAY_MINUTES = 5;
        private const int LAST_ERROR_LENGTH = 500;
        private const string MASKED_BODY = "[disamarkan setelah terkirim]";
        private const string SKIPPED_MESSAGE = "Pesan tidak diantrikan karena saluran ini dimatikan oleh pengguna";
        // Isi pesan berisi kode rahasia; disamarkan setelah terkirim (lihat komentar kolom body).
        private static readonly string[] SENSITIVE_TEMPLATES = { "RESET_PASSWORD" };

        #region SYNC
        public UserAccount? GetUser(bool refresh = false)
        {
            if (UserId != null && (refresh || UserData == null))
            {
                UserData = new UserAccount(UserId.Value);
            }
            return UserData;
        }
        public static List<MessageOutbox> GetDataList(MessageOutboxStatus? status = null, NotificationChannel? channel = null, int? userId = null, string? refType = null, int? refId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, channel, userId, refType, refId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<MessageOutbox, EF.TMessageOutbox>();
        }
        // Antrian yang sudah waktunya dikirim, untuk job SEND_OUTBOX.
        public static List<MessageOutbox> GetDueList(int jmldata = 50)
        {
            return DueQuery().Take(jmldata).ToList().MappingData<MessageOutbox, EF.TMessageOutbox>();
        }
        public static Response<MessageOutbox> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new MessageOutbox(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<MessageOutbox>(msg));
        }
        // Data null (status sukses) bila user mematikan saluran ini di m_user_notification_pref.
        public static Response<MessageOutbox?> SaveFromTemplate(TemplateRow input, string user)
        {
            return ServiceHandler.Handle<MessageOutbox?>(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateFromTemplate(context, input, userId);
                if (row == null)
                {
                    return null;
                }
                context.SaveChanges();
                return new MessageOutbox(row);
            }, data => data != null ? ResponseFactory.DataSaved<MessageOutbox?>(data) : ResponseFactory.Success<MessageOutbox?>(SKIPPED_MESSAGE),
                msg => ResponseFactory.DataSaveFailed<MessageOutbox?>(msg));
        }
        public Response<MessageOutbox> MarkSent(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetSent(row, userId);
                context.SaveChanges();
                return new MessageOutbox(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MessageOutbox>(msg));
        }
        public Response<MessageOutbox> MarkFailed(string error, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetFailed(row, error, userId);
                context.SaveChanges();
                return new MessageOutbox(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MessageOutbox>(msg));
        }
        public Response<MessageOutbox> Retry(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRetry(row, userId);
                context.SaveChanges();
                return new MessageOutbox(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MessageOutbox>(msg));
        }
        public Response<MessageOutbox> Cancel(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetCancel(row, userId);
                context.SaveChanges();
                return new MessageOutbox(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MessageOutbox>(msg));
        }
        // Membatalkan semua pesan yang masih Menunggu untuk satu dokumen sumber (mis. pengajuan dibatalkan).
        public static Response<int> CancelByRef(string refType, int refId, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = CancelByRef(context, refType, refId, userId);
                context.SaveChanges();
                return count;
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<int>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<MessageOutbox>> GetDataListAsync(MessageOutboxStatus? status = null, NotificationChannel? channel = null, int? userId = null, string? refType = null, int? refId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, channel, userId, refType, refId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<MessageOutbox, EF.TMessageOutbox>();
        }
        public static async Task<List<MessageOutbox>> GetDueListAsync(int jmldata = 50)
        {
            return await (await DueQuery().Take(jmldata).ToListAsync()).MappingDataAsync<MessageOutbox, EF.TMessageOutbox>();
        }
        public static async Task<Response<MessageOutbox>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new MessageOutbox(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<MessageOutbox>(msg));
        }
        public static async Task<Response<MessageOutbox?>> SaveFromTemplateAsync(TemplateRow input, string user)
        {
            return await ServiceHandler.HandleAsync<MessageOutbox?>(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await CreateFromTemplateAsync(context, input, userId);
                if (row == null)
                {
                    return null;
                }
                await context.SaveChangesAsync();
                return new MessageOutbox(row);
            }, data => data != null ? ResponseFactory.DataSaved<MessageOutbox?>(data) : ResponseFactory.Success<MessageOutbox?>(SKIPPED_MESSAGE),
                msg => ResponseFactory.DataSaveFailed<MessageOutbox?>(msg));
        }
        public async Task<Response<MessageOutbox>> MarkSentAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetSent(row, userId);
                await context.SaveChangesAsync();
                return new MessageOutbox(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MessageOutbox>(msg));
        }
        public async Task<Response<MessageOutbox>> MarkFailedAsync(string error, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetFailed(row, error, userId);
                await context.SaveChangesAsync();
                return new MessageOutbox(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MessageOutbox>(msg));
        }
        public async Task<Response<MessageOutbox>> RetryAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRetry(row, userId);
                await context.SaveChangesAsync();
                return new MessageOutbox(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MessageOutbox>(msg));
        }
        public async Task<Response<MessageOutbox>> CancelAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetCancel(row, userId);
                await context.SaveChangesAsync();
                return new MessageOutbox(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MessageOutbox>(msg));
        }
        public static async Task<Response<int>> CancelByRefAsync(string refType, int refId, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = await CancelByRefAsync(context, refType, refId, userId);
                await context.SaveChangesAsync();
                return count;
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<int>(msg));
        }
        #endregion

        #region INTERNAL
        // Menambah pesan ke context tanpa SaveChanges, supaya ikut tersimpan bersama transaksi pemanggil.
        internal static EF.TMessageOutbox CreateRow(EF.ModelContext context, NewRow input, int userId)
        {
            if (input.UserId != null)
            {
                UserAccount.GetUser(context, input.UserId.Value);
            }
            if (input.Channel != NotificationChannel.Email && input.Channel != NotificationChannel.WhatsApp)
            {
                throw new ArgumentException(ExceptionMessage.OUTBOX_CHANNEL_INVALID);
            }
            var recipient = NormalizeRecipient(input.Channel, input.Recipient);
            var subject = input.Channel == NotificationChannel.WhatsApp || string.IsNullOrWhiteSpace(input.Subject) ? null : input.Subject.Trim();
            if (input.Channel == NotificationChannel.Email && subject == null)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Subjek Email"));
            }
            if (string.IsNullOrWhiteSpace(input.Body))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Isi Pesan"));
            }
            if (input.RefId != null && string.IsNullOrWhiteSpace(input.RefType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Jenis Sumber Pesan"));
            }

            var now = DateTime.Now;
            var row = new EF.TMessageOutbox
            {
                Id = SequenceUtility.GetNextId<EF.TMessageOutbox>(context),
                UserId = input.UserId,
                Recipient = recipient,
                Channel = (int)input.Channel,
                TemplateCode = string.IsNullOrWhiteSpace(input.TemplateCode) ? null : DocumentNumberFormat.ToTypeCode(input.TemplateCode),
                Subject = subject,
                Body = input.Body.Trim(),
                RefType = string.IsNullOrWhiteSpace(input.RefType) ? null : input.RefType.Trim(),
                RefId = input.RefId,
                Status = (int)MessageOutboxStatus.Menunggu,
                AttemptCount = 0,
                ScheduledAt = input.ScheduledAt ?? now,
                InsDate = now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TMessageOutboxes.Add(row);
            return row;
        }
        // Null = tidak diantrikan karena user mematikan saluran ini.
        internal static EF.TMessageOutbox? CreateFromTemplate(EF.ModelContext context, TemplateRow input, int userId)
        {
            if (input.UserId != null && !UserNotificationPref.IsChannelEnabled(input.UserId.Value, input.NotifType ?? input.TemplateCode, input.Channel))
            {
                return null;
            }
            var template = MessageTemplate.GetTemplate(input.TemplateCode, input.Channel)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.MESSAGE_TEMPLATE_NOT_FOUND, DocumentNumberFormat.ToTypeCode(input.TemplateCode), input.Channel.GetDescription()));
            return CreateRow(context, ToNewRow(input, template), userId);
        }
        internal static async Task<EF.TMessageOutbox?> CreateFromTemplateAsync(EF.ModelContext context, TemplateRow input, int userId)
        {
            if (input.UserId != null && !await UserNotificationPref.IsChannelEnabledAsync(input.UserId.Value, input.NotifType ?? input.TemplateCode, input.Channel))
            {
                return null;
            }
            var template = await MessageTemplate.GetTemplateAsync(input.TemplateCode, input.Channel)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.MESSAGE_TEMPLATE_NOT_FOUND, DocumentNumberFormat.ToTypeCode(input.TemplateCode), input.Channel.GetDescription()));
            return CreateRow(context, ToNewRow(input, template), userId);
        }
        internal static int CancelByRef(EF.ModelContext context, string refType, int refId, int userId)
        {
            var type = refType?.Trim();
            var rows = context.TMessageOutboxes.Where(x => x.RefType == type && x.RefId == refId && x.Status == (int)MessageOutboxStatus.Menunggu).ToList();
            rows.ForEach(x => SetCancel(x, userId));
            return rows.Count;
        }
        internal static async Task<int> CancelByRefAsync(EF.ModelContext context, string refType, int refId, int userId)
        {
            var type = refType?.Trim();
            var rows = await context.TMessageOutboxes.Where(x => x.RefType == type && x.RefId == refId && x.Status == (int)MessageOutboxStatus.Menunggu).ToListAsync();
            rows.ForEach(x => SetCancel(x, userId));
            return rows.Count;
        }
        #endregion

        #region HELPER
        private static NewRow ToNewRow(TemplateRow input, MessageTemplate template)
        {
            return new NewRow
            {
                UserId = input.UserId,
                Recipient = input.Recipient,
                Channel = input.Channel,
                TemplateCode = template.TemplateCode,
                Subject = template.RenderSubject(input.Values),
                Body = template.RenderBody(input.Values),
                RefType = input.RefType,
                RefId = input.RefId,
                ScheduledAt = input.ScheduledAt
            };
        }
        // Email dicek formatnya; nomor WhatsApp disimpan sebagai digit berawalan kode negara (08xx → 628xx).
        private static string NormalizeRecipient(NotificationChannel channel, string? recipient)
        {
            if (string.IsNullOrWhiteSpace(recipient))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Penerima"));
            }
            var value = recipient.Trim();
            if (channel == NotificationChannel.Email)
            {
                if (!MailAddress.TryCreate(value, out var address) || address.Address != value)
                {
                    throw new ArgumentException(ExceptionMessage.OUTBOX_EMAIL_INVALID);
                }
                return value.ToLowerInvariant();
            }
            var digits = Regex.Replace(value, "[^0-9]", "");
            if (digits.StartsWith("0"))
            {
                digits = "62" + digits[1..];
            }
            if (digits.Length < 10 || digits.Length > 15)
            {
                throw new ArgumentException(ExceptionMessage.OUTBOX_WHATSAPP_INVALID);
            }
            return digits;
        }
        private static void SetSent(EF.TMessageOutbox row, int userId)
        {
            TransaksiHelper.ValidateStatus((MessageOutboxStatus)row.Status, "menandai pesan terkirim", MessageOutboxStatus.Menunggu);
            var now = DateTime.Now;
            row.Status = (int)MessageOutboxStatus.Terkirim;
            row.AttemptCount++;
            row.LastError = null;
            row.SentAt = now;
            if (row.TemplateCode != null && SENSITIVE_TEMPLATES.Contains(row.TemplateCode))
            {
                row.Body = MASKED_BODY;
            }
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        // Gagal kirim dijadwalkan ulang (jeda bertambah tiap percobaan) sampai MAX_ATTEMPTS, lalu berstatus Gagal.
        private static void SetFailed(EF.TMessageOutbox row, string error, int userId)
        {
            TransaksiHelper.ValidateStatus((MessageOutboxStatus)row.Status, "mencatat gagal kirim", MessageOutboxStatus.Menunggu);
            if (string.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Pesan Error"));
            }
            var now = DateTime.Now;
            row.AttemptCount++;
            row.LastError = TransaksiHelper.Truncate(error, LAST_ERROR_LENGTH);
            if (row.AttemptCount >= MAX_ATTEMPTS)
            {
                row.Status = (int)MessageOutboxStatus.Gagal;
            }
            else
            {
                row.ScheduledAt = now.AddMinutes(RETRY_DELAY_MINUTES * row.AttemptCount);
            }
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        // Kirim ulang manual pesan Gagal: jatah percobaan dihitung dari awal lagi.
        private static void SetRetry(EF.TMessageOutbox row, int userId)
        {
            TransaksiHelper.ValidateStatus((MessageOutboxStatus)row.Status, "mengirim ulang pesan", MessageOutboxStatus.Gagal);
            var now = DateTime.Now;
            row.Status = (int)MessageOutboxStatus.Menunggu;
            row.AttemptCount = 0;
            row.ScheduledAt = now;
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        private static void SetCancel(EF.TMessageOutbox row, int userId)
        {
            TransaksiHelper.ValidateStatus((MessageOutboxStatus)row.Status, "membatalkan pesan", MessageOutboxStatus.Menunggu);
            row.Status = (int)MessageOutboxStatus.Dibatalkan;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static IQueryable<EF.TMessageOutbox> DueQuery()
        {
            var now = DateTime.Now;
            return _Querys.Where(x => x.Status == (int)MessageOutboxStatus.Menunggu && x.ScheduledAt <= now)
                .OrderBy(x => x.ScheduledAt).ThenBy(x => x.Id);
        }
        private static IQueryable<EF.TMessageOutbox> FilterQuery(MessageOutboxStatus? status, NotificationChannel? channel, int? userId, string? refType, int? refId)
        {
            var type = string.IsNullOrWhiteSpace(refType) ? null : refType.Trim();
            return _Querys.Where(x => (status == null || x.Status == (int)status)
                    && (channel == null || x.Channel == (int)channel)
                    && (userId == null || x.UserId == userId)
                    && (type == null || x.RefType == type)
                    && (refId == null || x.RefId == refId))
                .OrderByDescending(x => x.InsDate).ThenByDescending(x => x.Id);
        }
        private static EF.TMessageOutbox GetRow(EF.ModelContext context, int id)
        {
            return context.TMessageOutboxes.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Antrian Pesan"));
        }
        private static async Task<EF.TMessageOutbox> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TMessageOutboxes.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Antrian Pesan"));
        }
        #endregion
    }
}
