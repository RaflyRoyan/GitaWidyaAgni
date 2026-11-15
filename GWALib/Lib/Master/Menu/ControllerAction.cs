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
    /// Master Controller Action (m_controller_action).
    /// </summary>
    public partial class ControllerAction : BaseLib
    {
        public class NewRow
        {
            public int ControllerId { get; set; }
            public string Action { get; set; } = null!;
            public AuthRequirement NeedAuth { get; set; } = AuthRequirement.Auth;
            public string? Description { get; set; }
        }

        public int Id { get; set; }
        public ControllerMenu ControllerMenu { get; set; } = null!;
        public string Action { get; set; } = null!;
        public AuthRequirement NeedAuth { get; set; }
        public string? Description { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MControllerAction> _Query
        {
            get
            {
                return _context.MControllerActions.Include(x => x.Controller).AsQueryable();
            }
        }
        private static IQueryable<EF.MControllerAction> _Querys
        {
            get
            {
                return DBClass.GetContext().MControllerActions.Include(x => x.Controller).AsQueryable();
            }
        }
        public ControllerAction()
        {
            _context = DBClass.GetContext();
        }
        public ControllerAction(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<ControllerAction>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ControllerAction(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ControllerAction(EF.MControllerAction row)
        {
            SetData(row);
        }
        private void SetData(EF.MControllerAction row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ControllerAction>();
            ControllerMenu = row.Controller != null ? new ControllerMenu(row.Controller) : new ControllerMenu(row.ControllerId);
            Action = row.Action;
            NeedAuth = (AuthRequirement)row.NeedAuth;
            Description = row.Description;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
