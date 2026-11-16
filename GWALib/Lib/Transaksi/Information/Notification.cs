using GWALib.General;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Information
{
    /// <summary>
    /// Notifikasi in-app (t_notification). Dibuat lewat Send (atau internal Add dari modul lain); jenis notifikasi dinormalisasi ke kode
    /// (DocumentNumberFormat.ToTypeCode, sama dengan template_code / m_user_notification_pref.notif_type). Penerima yang mematikan
    /// saluran InApp untuk jenis itu dilewati. Link berupa URL http(s) atau path diawali "/".
    /// Penerima hanya melihat / menandai / menghapus notifikasinya sendiri (GetMyList, GetUnreadCount, MarkRead, MarkAllRead, Delete).
    /// Purge (untuk job) menghapus notifikasi yang sudah dibaca dan lebih lama dari N hari.
    /// </summary>
    public partial class Notification : BaseLib
    {
        public class NewRow
        {
            public List<int> UserIds { get; set; } = new();
            public string NotifType { get; set; } = null!;
            public string Title { get; set; } = null!;
            public string? Message { get; set; }
            public string? LinkUrl { get; set; }
            public string? RefType { get; set; }
            public int? RefId { get; set; }
        }

        public int Id { get; set; }
        public int UserId { get; private set; }
        public string NotifType { get; private set; } = null!;
        public string Title { get; private set; } = null!;
        public string? Message { get; private set; }
        public string? LinkUrl { get; private set; }
        public string? RefType { get; private set; }
        public int? RefId { get; private set; }
        public bool IsRead { get; private set; }
        public DateTime? ReadAt { get; private set; }
        public DateTime InsDate { get; private set; }

        private static IQueryable<EF.TNotification> _Querys
        {
            get
            {
                return DBClass.GetContext().TNotifications.AsQueryable();
            }
        }
        public Notification()
        {
        }
        public Notification(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Notification>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Notification(EF.TNotification row)
        {
            SetData(row);
        }
        private void SetData(EF.TNotification row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Notification>();
            UserId = row.UserId;
            NotifType = row.NotifType;
            Title = row.Title;
            Message = row.Message;
            LinkUrl = row.LinkUrl;
            RefType = row.RefType;
            RefId = row.RefId;
            IsRead = row.ReadAt != null;
            ReadAt = row.ReadAt;
            InsDate = row.InsDate;
        }
    }
}
