using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Approval
{
    /// <summary>
    /// Template pesan notifikasi (m_message_template), mis. RESET_PASSWORD, APPROVAL_WAITING, CHECKLIST_DUE.
    /// Satu template per (kode, saluran); isi memakai placeholder {nama} yang diisi lewat Render.
    /// </summary>
    public partial class MessageTemplate : BaseLib
    {
        public class NewRow
        {
            public string TemplateCode { get; set; } = null!;
            public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
            public string? Subject { get; set; }
            public string Body { get; set; } = null!;
            public string? Description { get; set; }
        }

        public int Id { get; set; }
        public string TemplateCode { get; set; } = null!;
        public NotificationChannel Channel { get; set; }
        public string? Subject { get; set; }
        public string Body { get; set; } = null!;
        public string? Description { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MMessageTemplate> _Query
        {
            get
            {
                return _context.MMessageTemplates.AsQueryable();
            }
        }
        private static IQueryable<EF.MMessageTemplate> _Querys
        {
            get
            {
                return DBClass.GetContext().MMessageTemplates.AsQueryable();
            }
        }
        public MessageTemplate()
        {
            _context = DBClass.GetContext();
        }
        public MessageTemplate(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<MessageTemplate>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal MessageTemplate(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal MessageTemplate(EF.MMessageTemplate row)
        {
            SetData(row);
        }
        private void SetData(EF.MMessageTemplate row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<MessageTemplate>();
            TemplateCode = row.TemplateCode;
            Channel = (NotificationChannel)row.Channel;
            Subject = row.Subject;
            Body = row.Body;
            Description = row.Description;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
