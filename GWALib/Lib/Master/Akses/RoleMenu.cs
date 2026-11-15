using GWALib.General;
using GWALib.Lib.Master.Menu;
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
    /// Menu yang boleh dilihat role (m_role_menu). SubMenu NULL = akses ke header menu saja.
    /// </summary>
    public partial class RoleMenu : BaseLib
    {
        public class NewRow
        {
            public int RoleId { get; set; }
            public int HeaderMenuId { get; set; }
            public int? SubMenuId { get; set; }
        }

        public int Id { get; set; }
        public Role Role { get; set; } = null!;
        public HeaderMenu HeaderMenu { get; set; } = null!;
        public SubMenu? SubMenu { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MRoleMenu> _Query
        {
            get
            {
                return _context.MRoleMenus
                    .Include(x => x.Role)
                    .Include(x => x.HeaderMenu).ThenInclude(x => x.Controller)
                    .Include(x => x.SubMenu).ThenInclude(x => x!.Controller)
                    .AsQueryable();
            }
        }
        private static IQueryable<EF.MRoleMenu> _Querys
        {
            get
            {
                return DBClass.GetContext().MRoleMenus
                    .Include(x => x.Role)
                    .Include(x => x.HeaderMenu).ThenInclude(x => x.Controller)
                    .Include(x => x.SubMenu).ThenInclude(x => x!.Controller)
                    .AsQueryable();
            }
        }
        public RoleMenu()
        {
            _context = DBClass.GetContext();
        }
        public RoleMenu(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<RoleMenu>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal RoleMenu(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal RoleMenu(EF.MRoleMenu row)
        {
            SetData(row);
        }
        private void SetData(EF.MRoleMenu row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<RoleMenu>();
            Role = row.Role != null ? new Role(row.Role) : new Role(row.RoleId);
            HeaderMenu = row.HeaderMenu != null ? new HeaderMenu(row.HeaderMenu) : new HeaderMenu(row.HeaderMenuId);
            if (row.SubMenuId != null)
            {
                SubMenu = row.SubMenu != null ? new SubMenu(row.SubMenu) : new SubMenu(row.SubMenuId.Value);
            }
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
