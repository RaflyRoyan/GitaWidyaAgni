using GWALib.General;
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
    /// Sub menu di dalam header menu (m_header_menu_detail).
    /// </summary>
    public partial class HeaderMenuDetail : BaseLib
    {
        public class NewRow
        {
            public int HeaderMenuId { get; set; }
            public int SubMenuId { get; set; }
            public int Seq { get; set; }
        }

        public int Id { get; set; }
        public HeaderMenu HeaderMenu { get; set; } = null!;
        public SubMenu SubMenu { get; set; } = null!;
        public int Seq { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MHeaderMenuDetail> _Query
        {
            get
            {
                return _context.MHeaderMenuDetails
                    .Include(x => x.HeaderMenu).ThenInclude(x => x.Controller)
                    .Include(x => x.SubMenu).ThenInclude(x => x.Controller)
                    .AsQueryable();
            }
        }
        private static IQueryable<EF.MHeaderMenuDetail> _Querys
        {
            get
            {
                return DBClass.GetContext().MHeaderMenuDetails
                    .Include(x => x.HeaderMenu).ThenInclude(x => x.Controller)
                    .Include(x => x.SubMenu).ThenInclude(x => x.Controller)
                    .AsQueryable();
            }
        }
        public HeaderMenuDetail()
        {
            _context = DBClass.GetContext();
        }
        public HeaderMenuDetail(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<HeaderMenuDetail>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal HeaderMenuDetail(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal HeaderMenuDetail(EF.MHeaderMenuDetail row)
        {
            SetData(row);
        }
        private void SetData(EF.MHeaderMenuDetail row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<HeaderMenuDetail>();
            HeaderMenu = row.HeaderMenu != null ? new HeaderMenu(row.HeaderMenu) : new HeaderMenu(row.HeaderMenuId);
            SubMenu = row.SubMenu != null ? new SubMenu(row.SubMenu) : new SubMenu(row.SubMenuId);
            Seq = row.Seq;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
