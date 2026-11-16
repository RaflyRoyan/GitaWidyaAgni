using GWALib.General;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Information
{
    /// <summary>
    /// Log aktivitas (t_activity_log). Ditulis lewat Log (lapisan aplikasi, mis. LOGIN / LOGOUT / LOGIN_FAILED) atau internal Add dari modul
    /// lain; tidak ada ubah / hapus selain Purge (untuk job). Module dan Action dinormalisasi ke kode huruf besar, EntityName = nama tabel.
    /// OldValue / NewValue diserialisasi ke JSON; nilai dengan kunci yang memuat password / token / secret / hash / key / signature
    /// diganti "***". User bisa kosong (mis. login gagal). Old / new value hanya dimuat per data, tidak di daftar.
    /// </summary>
    public partial class ActivityLog : BaseLib
    {
        public class NewRow
        {
            public int? UserId { get; set; }
            public string Module { get; set; } = null!;
            public string Action { get; set; } = null!;
            public string? EntityName { get; set; }
            public int? EntityId { get; set; }
            // Objek apa pun (diserialisasi) atau string JSON.
            public object? OldValue { get; set; }
            public object? NewValue { get; set; }
            public string? IpAddress { get; set; }
            public string? UserAgent { get; set; }
        }

        public int Id { get; set; }
        public int? UserId { get; private set; }
        public string? Username { get; private set; }
        public string Module { get; private set; } = null!;
        public string Action { get; private set; } = null!;
        public string? EntityName { get; private set; }
        public int? EntityId { get; private set; }
        public string? OldValue { get; private set; }
        public string? NewValue { get; private set; }
        public string? IpAddress { get; private set; }
        public string? UserAgent { get; private set; }
        public DateTime InsDate { get; private set; }

        // Daftar: tanpa old / new value.
        private static IQueryable<EF.TActivityLog> _Querys
        {
            get
            {
                return DBClass.GetContext().TActivityLogs.Select(x => new EF.TActivityLog
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    User = x.UserId == null ? null : new EF.MUser { Id = x.User!.Id, Username = x.User.Username },
                    Module = x.Module,
                    Action = x.Action,
                    EntityName = x.EntityName,
                    EntityId = x.EntityId,
                    IpAddress = x.IpAddress,
                    UserAgent = x.UserAgent,
                    InsDate = x.InsDate
                });
            }
        }
        public ActivityLog()
        {
        }
        public ActivityLog(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ActivityLog>());
            var row = DBClass.GetContext().TActivityLogs.Include(x => x.User).SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ActivityLog(EF.TActivityLog row)
        {
            SetData(row);
        }
        private void SetData(EF.TActivityLog row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ActivityLog>();
            UserId = row.UserId;
            Username = row.User?.Username;
            Module = row.Module;
            Action = row.Action;
            EntityName = row.EntityName;
            EntityId = row.EntityId;
            OldValue = row.OldValue;
            NewValue = row.NewValue;
            IpAddress = row.IpAddress;
            UserAgent = row.UserAgent;
            InsDate = row.InsDate;
        }
    }
}
