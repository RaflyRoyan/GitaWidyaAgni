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

namespace GWALib.Lib.Master.Approval
{
    public partial class MessageTemplate
    {
        #region SYNC
        // Placeholder yang tidak ada di values dibiarkan apa adanya, mis. {kode}.
        public string RenderSubject(IDictionary<string, string?> values)
        {
            return Fill(Subject, values);
        }
        public string RenderBody(IDictionary<string, string?> values)
        {
            return Fill(Body, values);
        }
        public static MessageTemplate? GetTemplate(string templateCode, NotificationChannel channel)
        {
            var row = TemplateQuery(DocumentNumberFormat.ToTypeCode(templateCode), channel).FirstOrDefault();
            return row != null ? new MessageTemplate(row) : null;
        }
        public static List<MessageTemplate> GetDataList(FilterAktif aktif, string? templateCode = null, NotificationChannel? channel = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, templateCode, channel);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<MessageTemplate, EF.MMessageTemplate>();
        }
        public static Response<MessageTemplate> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MMessageTemplates.Add(row);
                context.SaveChanges();
                return new MessageTemplate(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<MessageTemplate>(msg));
        }
        public Response<MessageTemplate> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(row, userId);
                context.SaveChanges();
                return new MessageTemplate(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MessageTemplate>(msg));
        }
        public Response<MessageTemplate> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new MessageTemplate(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<MessageTemplate>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<MessageTemplate?> GetTemplateAsync(string templateCode, NotificationChannel channel)
        {
            var row = await TemplateQuery(DocumentNumberFormat.ToTypeCode(templateCode), channel).FirstOrDefaultAsync();
            return row != null ? new MessageTemplate(row) : null;
        }
        public static async Task<List<MessageTemplate>> GetDataListAsync(FilterAktif aktif, string? templateCode = null, NotificationChannel? channel = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, templateCode, channel);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<MessageTemplate, EF.MMessageTemplate>();
        }
        public static async Task<Response<MessageTemplate>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MMessageTemplates.Add(row);
                await context.SaveChangesAsync();
                return new MessageTemplate(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<MessageTemplate>(msg));
        }
        public async Task<Response<MessageTemplate>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(row, userId);
                await context.SaveChangesAsync();
                return new MessageTemplate(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<MessageTemplate>(msg));
        }
        public async Task<Response<MessageTemplate>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new MessageTemplate(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<MessageTemplate>(msg));
        }
        #endregion

        #region HELPER
        private static string Fill(string? text, IDictionary<string, string?> values)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            return Regex.Replace(text, @"\{(\w+)\}", m => values.TryGetValue(m.Groups[1].Value, out var value) ? value ?? string.Empty : m.Value);
        }
        private static IQueryable<EF.MMessageTemplate> TemplateQuery(string templateCode, NotificationChannel channel)
        {
            return _Querys.Where(x => x.TemplateCode == templateCode && x.Channel == (int)channel
                && x.IsActive == (int)ActiveStatus.Aktif);
        }
        private static IQueryable<EF.MMessageTemplate> FilterQuery(FilterAktif aktif, string? templateCode, NotificationChannel? channel)
        {
            var code = string.IsNullOrWhiteSpace(templateCode) ? null : DocumentNumberFormat.ToTypeCode(templateCode);
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (code == null || x.TemplateCode == code)
                    && (channel == null || x.Channel == (int)channel))
                .OrderBy(x => x.TemplateCode).ThenBy(x => x.Channel);
        }
        private static EF.MMessageTemplate GetRow(EF.ModelContext context, int id)
        {
            return context.MMessageTemplates.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Template Pesan"));
        }
        private static async Task<EF.MMessageTemplate> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MMessageTemplates.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Template Pesan"));
        }
        // Email butuh subjek; untuk notifikasi in-app subjek dipakai sebagai judul. WhatsApp tidak memakai subjek.
        private static void ValidateData(NotificationChannel channel, string? subject, string body)
        {
            if (!Enum.IsDefined(channel))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Saluran"));
            }
            if (channel != NotificationChannel.WhatsApp && string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Subjek / Judul"));
            }
            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Isi Pesan"));
            }
        }
        private static EF.MMessageTemplate CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var templateCode = DocumentNumberFormat.ToTypeCode(newrow.TemplateCode);
            var subject = newrow.Channel == NotificationChannel.WhatsApp || string.IsNullOrWhiteSpace(newrow.Subject) ? null : newrow.Subject.Trim();
            if (string.IsNullOrWhiteSpace(templateCode))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Kode Template"));
            }
            ValidateData(newrow.Channel, subject, newrow.Body);
            ValidateUtility.CheckDuplicateData<EF.MMessageTemplate>(context,
                x => x.TemplateCode == templateCode && x.Channel == (int)newrow.Channel,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Kode Template dan Saluran"));

            var row = new EF.MMessageTemplate
            {
                Id = SequenceUtility.GetNextId<EF.MMessageTemplate>(context),
                TemplateCode = templateCode,
                Channel = (int)newrow.Channel,
                Subject = subject,
                Body = newrow.Body.Trim(),
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Kode dan saluran tetap (dipakai sebagai kunci pencarian template oleh pengirim notifikasi).
        private void SetRow(EF.MMessageTemplate row, int userId)
        {
            var channel = (NotificationChannel)row.Channel;
            var subject = channel == NotificationChannel.WhatsApp || string.IsNullOrWhiteSpace(Subject) ? null : Subject.Trim();
            ValidateData(channel, subject, Body);

            row.Subject = subject;
            row.Body = Body.Trim();
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MMessageTemplate row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
