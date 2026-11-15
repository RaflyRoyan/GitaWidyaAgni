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
    /// Hak akses role ke controller action (m_role_action).
    /// </summary>
    public partial class RoleAction : BaseLib
    {
        public class NewRow
        {
            public int RoleId { get; set; }
            public int ControllerActionId { get; set; }
        }

        public int Id { get; set; }
        public Role Role { get; set; } = null!;
        public ControllerAction ControllerAction { get; set; } = null!;
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MRoleAction> _Query
        {
            get
            {
                return _context.MRoleActions
                    .Include(x => x.Role)
                    .Include(x => x.ControllerAction).ThenInclude(x => x.Controller)
                    .AsQueryable();
            }
        }
        private static IQueryable<EF.MRoleAction> _Querys
        {
            get
            {
                return DBClass.GetContext().MRoleActions
                    .Include(x => x.Role)
                    .Include(x => x.ControllerAction).ThenInclude(x => x.Controller)
                    .AsQueryable();
            }
        }
        public RoleAction()
        {
            _context = DBClass.GetContext();
        }
        public RoleAction(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<RoleAction>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal RoleAction(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal RoleAction(EF.MRoleAction row)
        {
            SetData(row);
        }
        private void SetData(EF.MRoleAction row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<RoleAction>();
            Role = row.Role != null ? new Role(row.Role) : new Role(row.RoleId);
            ControllerAction = row.ControllerAction != null ? new ControllerAction(row.ControllerAction) : new ControllerAction(row.ControllerActionId);
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
