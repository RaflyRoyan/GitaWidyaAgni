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
    /// Master Role (m_role), mis. SUPERADMIN, KETUA, BENDAHARA. Kode unik.
    /// </summary>
    public partial class Role : BaseLib
    {
        public class NewRow
        {
            public string Code { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
        }

        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private List<RoleAction>? RoleActionList;
        private List<RoleMenu>? RoleMenuList;
        private List<UserRole>? UserRoleList;

        EF.ModelContext _context = null!;
        private IQueryable<EF.MRole> _Query
        {
            get
            {
                return _context.MRoles.AsQueryable();
            }
        }
        private static IQueryable<EF.MRole> _Querys
        {
            get
            {
                return DBClass.GetContext().MRoles.AsQueryable();
            }
        }
        public Role()
        {
            _context = DBClass.GetContext();
        }
        public Role(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<Role>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Role(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Role(EF.MRole row)
        {
            SetData(row);
        }
        private void SetData(EF.MRole row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Role>();
            Code = row.Code;
            Name = row.Name;
            Description = row.Description;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
