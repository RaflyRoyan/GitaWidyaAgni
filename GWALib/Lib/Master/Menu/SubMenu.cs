using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Menu
{
    /// <summary>
    /// Master Sub Menu (m_sub_menu).
    /// </summary>
    public partial class SubMenu : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public int? ControllerId { get; set; }
            public string? Icon { get; set; }
            public string? Description { get; set; }
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ControllerMenu? ControllerMenu { get; set; }
        public string? Icon { get; set; }
        public string? Description { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MSubMenu> _Query
        {
            get
            {
                return _context.MSubMenus.Include(x => x.Controller).AsQueryable();
            }
        }
        private static IQueryable<EF.MSubMenu> _Querys
        {
            get
            {
                return DBClass.GetContext().MSubMenus.Include(x => x.Controller).AsQueryable();
            }
        }
        public SubMenu()
        {
            _context = DBClass.GetContext();
        }
        public SubMenu(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<SubMenu>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal SubMenu(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal SubMenu(EF.MSubMenu row)
        {
            SetData(row);
        }
        private void SetData(EF.MSubMenu row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<SubMenu>();
            Name = row.Name;
            if (row.ControllerId != null)
            {
                ControllerMenu = row.Controller != null ? new ControllerMenu(row.Controller) : new ControllerMenu(row.ControllerId.Value);
            }
            Icon = row.Icon;
            Description = row.Description;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
