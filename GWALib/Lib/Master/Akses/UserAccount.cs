using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Akses
{
    /// <summary>
    /// Akun login (m_user). Tidak dinamai "User" karena bentrok dengan Controller.User di aplikasi web.
    /// Profilnya ada di m_member (opsional, user_id unik).
    /// </summary>
    public partial class UserAccount : BaseLib
    {
        public class NewRow
        {
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
        }

        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public ActiveStatus IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private List<UserRole>? UserRoleList;

        EF.ModelContext _context = null!;
        private IQueryable<EF.MUser> _Query
        {
            get
            {
                return _context.MUsers.AsQueryable();
            }
        }
        private static IQueryable<EF.MUser> _Querys
        {
            get
            {
                return DBClass.GetContext().MUsers.AsQueryable();
            }
        }
        public UserAccount()
        {
            _context = DBClass.GetContext();
        }
        public UserAccount(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<UserAccount>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal UserAccount(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal UserAccount(EF.MUser row)
        {
            SetData(row);
        }
        private void SetData(EF.MUser row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<UserAccount>();
            Username = row.Username;
            IsActive = (ActiveStatus)row.IsActive;
            LastLoginAt = row.LastLoginAt;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
