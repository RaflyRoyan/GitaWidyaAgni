using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Menu
{
    /// <summary>
    /// Master Controller (m_controller). Tidak dinamai "Controller" karena nama class berakhiran "Controller"
    /// akan dianggap MVC controller bila GWALib didaftarkan sebagai application part di aplikasi web.
    /// </summary>
    public partial class ControllerMenu : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public string IndexAction { get; set; } = "Index";
            public string? Description { get; set; }
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string IndexAction { get; set; } = null!;
        public string? Description { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private List<ControllerAction>? ControllerActionList;

        EF.ModelContext _context = null!;
        private IQueryable<EF.MController> _Query
        {
            get
            {
                return _context.MControllers.AsQueryable();
            }
        }
        private static IQueryable<EF.MController> _Querys
        {
            get
            {
                return DBClass.GetContext().MControllers.AsQueryable();
            }
        }
        public ControllerMenu()
        {
            _context = DBClass.GetContext();
        }
        public ControllerMenu(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<ControllerMenu>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ControllerMenu(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ControllerMenu(EF.MController row)
        {
            SetData(row);
        }
        private void SetData(EF.MController row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ControllerMenu>();
            Name = row.Name;
            IndexAction = row.IndexAction;
            Description = row.Description;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
