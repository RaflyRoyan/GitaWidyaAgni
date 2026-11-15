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
    /// Jabatan pengurus (m_position), mis. Ketua, Sekretaris, Bendahara, Kadiv. Nama unik; IsCore menandai pengurus inti.
    /// </summary>
    public partial class Position : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public PositionLevel IsCore { get; set; } = PositionLevel.Biasa;
            public string? Description { get; set; }
            public int SortOrder { get; set; }
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public PositionLevel IsCore { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MPosition> _Query
        {
            get
            {
                return _context.MPositions.AsQueryable();
            }
        }
        private static IQueryable<EF.MPosition> _Querys
        {
            get
            {
                return DBClass.GetContext().MPositions.AsQueryable();
            }
        }
        public Position()
        {
            _context = DBClass.GetContext();
        }
        public Position(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<Position>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Position(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Position(EF.MPosition row)
        {
            SetData(row);
        }
        private void SetData(EF.MPosition row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Position>();
            Name = row.Name;
            IsCore = (PositionLevel)row.IsCore;
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
