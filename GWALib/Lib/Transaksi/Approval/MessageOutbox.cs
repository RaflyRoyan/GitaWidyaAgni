using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Approval
{
    /// <summary>
    /// Antrian pesan Email / WhatsApp (t_message_outbox), dikirim oleh job latar belakang (SEND_OUTBOX).
    /// Alur status: Menunggu → Terkirim, atau Menunggu → (dicoba ulang s.d. MAX_ATTEMPTS) → Gagal; Menunggu → Dibatalkan.
    /// Relasi berupa UserId + GetUser() supaya daftar antrian cukup satu query.
    /// </summary>
    public partial class MessageOutbox : BaseLib
    {
        public class NewRow
        {
            public int? UserId { get; set; }
            public string Recipient { get; set; } = null!;
            public NotificationChannel Channel { get; set; } = NotificationChannel.Email;
            public string? TemplateCode { get; set; }
            public string? Subject { get; set; }
            public string Body { get; set; } = null!;
            public string? RefType { get; set; }
            public int? RefId { get; set; }
            public DateTime? ScheduledAt { get; set; }
        }
        // Isi pesan dibentuk dari m_message_template; NotifType kosong = preferensi user dicek dengan TemplateCode.
        public class TemplateRow
        {
            public int? UserId { get; set; }
            public string Recipient { get; set; } = null!;
            public NotificationChannel Channel { get; set; } = NotificationChannel.Email;
            public string TemplateCode { get; set; } = null!;
            public string? NotifType { get; set; }
            public Dictionary<string, string?> Values { get; set; } = new();
            public string? RefType { get; set; }
            public int? RefId { get; set; }
            public DateTime? ScheduledAt { get; set; }
        }

        public int Id { get; private set; }
        public int? UserId { get; private set; }
        public string Recipient { get; private set; } = null!;
        public NotificationChannel Channel { get; private set; }
        public string? TemplateCode { get; private set; }
        public string? Subject { get; private set; }
        public string Body { get; private set; } = null!;
        public string? RefType { get; private set; }
        public int? RefId { get; private set; }
        public MessageOutboxStatus Status { get; private set; }
        public int AttemptCount { get; private set; }
        public string? LastError { get; private set; }
        public DateTime ScheduledAt { get; private set; }
        public DateTime? SentAt { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        public string ChannelNama => Channel.GetDescription();
        public string StatusNama => Status.GetDescription();

        private Master.Akses.UserAccount? UserData;

        private static IQueryable<EF.TMessageOutbox> _Querys
        {
            get
            {
                return DBClass.GetContext().TMessageOutboxes.AsQueryable();
            }
        }
        public MessageOutbox()
        {
        }
        public MessageOutbox(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<MessageOutbox>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal MessageOutbox(int id)
        {
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal MessageOutbox(EF.TMessageOutbox row)
        {
            SetData(row);
        }
        private void SetData(EF.TMessageOutbox row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<MessageOutbox>();
            UserId = row.UserId;
            Recipient = row.Recipient;
            Channel = (NotificationChannel)row.Channel;
            TemplateCode = row.TemplateCode;
            Subject = row.Subject;
            Body = row.Body;
            RefType = row.RefType;
            RefId = row.RefId;
            Status = (MessageOutboxStatus)row.Status;
            AttemptCount = row.AttemptCount;
            LastError = row.LastError;
            ScheduledAt = row.ScheduledAt;
            SentAt = row.SentAt;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
