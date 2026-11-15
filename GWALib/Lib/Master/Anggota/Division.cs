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
    /// Divisi / bidang / tim dalam organisasi (m_division), mis. Humas, Acara, Tim Futsal Putra. Nama unik.
    /// </summary>
    public partial class Division : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public DivisionType DivisionType { get; set; } = DivisionType.Divisi;
            public string? Description { get; set; }
            public int SortOrder { get; set; }
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DivisionType DivisionType { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MDivision> _Query
        {
            get
            {
                return _context.MDivisions.AsQueryable();
            }
        }
        private static IQueryable<EF.MDivision> _Querys
        {
            get
            {
                return DBClass.GetContext().MDivisions.AsQueryable();
            }
        }
        public Division()
        {
            _context = DBClass.GetContext();
        }
        public Division(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<Division>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Division(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Division(EF.MDivision row)
        {
            SetData(row);
        }
        private void SetData(EF.MDivision row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Division>();
            Name = row.Name;
            DivisionType = (DivisionType)row.DivisionType;
            Description = row.Description;
            SortOrder = row.SortOrder;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
