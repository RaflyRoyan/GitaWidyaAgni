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
    /// Section musik (m_section), mis. Brass, Battery, Front Ensemble, Colour Guard. Nama unik. Semua anggota punya section;
    /// riwayatnya di SectionMember (t_section_member). Beda dengan Division: divisi hanya anggota pilihan Ketua Umum.
    /// </summary>
    public partial class Section : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public int SortOrder { get; set; }
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MSection> _Query
        {
            get
            {
                return _context.MSections.AsQueryable();
            }
        }
        private static IQueryable<EF.MSection> _Querys
        {
            get
            {
                return DBClass.GetContext().MSections.AsQueryable();
            }
        }
        public Section()
        {
            _context = DBClass.GetContext();
        }
        public Section(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<Section>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Section(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Section(EF.MSection row)
        {
            SetData(row);
        }
        private void SetData(EF.MSection row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Section>();
            Name = row.Name;
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
