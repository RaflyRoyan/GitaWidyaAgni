using GWALib.General;
using GWALib.Lib.Master.Akses;
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
    /// Preferensi saluran notifikasi per user (m_user_notification_pref). Tidak ada baris = pakai bawaan (dinyalakan).
    /// Unik per (user, jenis notifikasi, saluran); jenis notifikasi sama dengan template_code di m_message_template.
    /// </summary>
    public partial class UserNotificationPref : BaseLib
    {
        public class NewRow
        {
            public int UserId { get; set; }
            public string NotifType { get; set; } = null!;
            public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
            public NotificationState IsEnabled { get; set; } = NotificationState.Aktif;
        }

        public int Id { get; set; }
        public UserAccount User { get; set; } = null!;
        public string NotifType { get; set; } = null!;
        public NotificationChannel Channel { get; set; }
        public NotificationState IsEnabled { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MUserNotificationPref> _Query
        {
            get
            {
                return _context.MUserNotificationPrefs.Include(x => x.User).AsQueryable();
            }
        }
        private static IQueryable<EF.MUserNotificationPref> _Querys
        {
            get
            {
                return DBClass.GetContext().MUserNotificationPrefs.Include(x => x.User).AsQueryable();
            }
        }
        public UserNotificationPref()
        {
            _context = DBClass.GetContext();
        }
        public UserNotificationPref(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<UserNotificationPref>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal UserNotificationPref(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal UserNotificationPref(EF.MUserNotificationPref row)
        {
            SetData(row);
        }
        private void SetData(EF.MUserNotificationPref row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<UserNotificationPref>();
            User = row.User != null ? new UserAccount(row.User) : new UserAccount(row.UserId);
            NotifType = row.NotifType;
            Channel = (NotificationChannel)row.Channel;
            IsEnabled = (NotificationState)row.IsEnabled;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
