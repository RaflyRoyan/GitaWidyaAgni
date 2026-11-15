using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Anggota
{
    /// <summary>
    /// Periode kepengurusan (m_management_period), mis. Periode 2026/2027.
    /// Maksimal satu periode Active; periode Closed tidak bisa diubah lagi.
    /// </summary>
    public partial class ManagementPeriod : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public ManagementPeriodStatus Status { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MManagementPeriod> _Query
        {
            get
            {
                return _context.MManagementPeriods.AsQueryable();
            }
        }
        private static IQueryable<EF.MManagementPeriod> _Querys
        {
            get
            {
                return DBClass.GetContext().MManagementPeriods.AsQueryable();
            }
        }
        public ManagementPeriod()
        {
            _context = DBClass.GetContext();
        }
        public ManagementPeriod(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<ManagementPeriod>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ManagementPeriod(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ManagementPeriod(EF.MManagementPeriod row)
        {
            SetData(row);
        }
        private void SetData(EF.MManagementPeriod row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ManagementPeriod>();
            Name = row.Name;
            StartDate = row.StartDate;
            EndDate = row.EndDate;
            Status = (ManagementPeriodStatus)row.Status;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
