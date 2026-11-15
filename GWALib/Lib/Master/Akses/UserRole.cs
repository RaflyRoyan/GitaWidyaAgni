using GWALib.General;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Akses
{
    /// <summary>
    /// Role yang dipegang user (m_user_role). PeriodId diisi = role pengurus untuk periode kepengurusan itu;
    /// NULL = tidak terikat periode (mis. SUPERADMIN).
    /// </summary>
    public partial class UserRole : BaseLib
    {
        public class NewRow
        {
            public int UserId { get; set; }
            public int RoleId { get; set; }
            public int? PeriodId { get; set; }
        }

        public int Id { get; set; }
        public UserAccount User { get; set; } = null!;
        public Role Role { get; set; } = null!;
        public int? PeriodId { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MUserRole> _Query
        {
            get
            {
                return _context.MUserRoles.Include(x => x.User).Include(x => x.Role).AsQueryable();
            }
        }
        private static IQueryable<EF.MUserRole> _Querys
        {
            get
            {
                return DBClass.GetContext().MUserRoles.Include(x => x.User).Include(x => x.Role).AsQueryable();
            }
        }
        public UserRole()
        {
            _context = DBClass.GetContext();
        }
        public UserRole(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<UserRole>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal UserRole(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal UserRole(EF.MUserRole row)
        {
            SetData(row);
        }
        private void SetData(EF.MUserRole row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<UserRole>();
            User = row.User != null ? new UserAccount(row.User) : new UserAccount(row.UserId);
            Role = row.Role != null ? new Role(row.Role) : new Role(row.RoleId);
            PeriodId = row.PeriodId;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
