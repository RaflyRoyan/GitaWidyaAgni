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
    /// Master Header Menu (m_header_menu). Header dengan ControllerMenu langsung menuju halaman;
    /// tanpa ControllerMenu berarti grup yang berisi sub menu (m_header_menu_detail).
    /// </summary>
    public partial class HeaderMenu : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public string? Section { get; set; }
            public int? ControllerId { get; set; }
            public string? Icon { get; set; }
            public string? Description { get; set; }
            public int Seq { get; set; }
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Section { get; set; }
        public ControllerMenu? ControllerMenu { get; set; }
        public string? Icon { get; set; }
        public string? Description { get; set; }
        public int Seq { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private List<HeaderMenuDetail>? HeaderMenuDetailList;

        EF.ModelContext _context = null!;
        private IQueryable<EF.MHeaderMenu> _Query
        {
            get
            {
                return _context.MHeaderMenus.Include(x => x.Controller).AsQueryable();
            }
        }
        private static IQueryable<EF.MHeaderMenu> _Querys
        {
            get
            {
                return DBClass.GetContext().MHeaderMenus.Include(x => x.Controller).AsQueryable();
            }
        }
        public HeaderMenu()
        {
            _context = DBClass.GetContext();
        }
        public HeaderMenu(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<HeaderMenu>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal HeaderMenu(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal HeaderMenu(EF.MHeaderMenu row)
        {
            SetData(row);
        }
        private void SetData(EF.MHeaderMenu row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<HeaderMenu>();
            Name = row.Name;
            Section = row.Section;
            if (row.ControllerId != null)
            {
                ControllerMenu = row.Controller != null ? new ControllerMenu(row.Controller) : new ControllerMenu(row.ControllerId.Value);
            }
            Icon = row.Icon;
            Description = row.Description;
            Seq = row.Seq;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
