using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Prestasi
{
    /// <summary>
    /// Aturan poin keaktifan anggota (m_point_rule), dipakai saat mencatat t_member_point.
    /// ActivityType hanya untuk sumber berbasis kegiatan (Hadir / Panitia / Pemateri), AchievementLevel hanya untuk Prestasi;
    /// NULL = aturan umum sumber itu. Satu aturan aktif per (SourceType, ActivityType, AchievementLevel); lihat GetEffective.
    /// </summary>
    public partial class PointRule : BaseLib
    {
        public class NewRow
        {
            public PointSourceType SourceType { get; set; }
            public ActivityType? ActivityType { get; set; }
            public EventLevel? AchievementLevel { get; set; }
            public int Points { get; set; }
            public string? Description { get; set; }
        }

        public int Id { get; set; }
        public PointSourceType SourceType { get; set; }
        public ActivityType? ActivityType { get; set; }
        public EventLevel? AchievementLevel { get; set; }
        public int Points { get; set; }
        public string? Description { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MPointRule> _Query
        {
            get
            {
                return _context.MPointRules.AsQueryable();
            }
        }
        private static IQueryable<EF.MPointRule> _Querys
        {
            get
            {
                return DBClass.GetContext().MPointRules.AsQueryable();
            }
        }
        public PointRule()
        {
            _context = DBClass.GetContext();
        }
        public PointRule(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<PointRule>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal PointRule(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal PointRule(EF.MPointRule row)
        {
            SetData(row);
        }
        private void SetData(EF.MPointRule row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<PointRule>();
            SourceType = (PointSourceType)row.SourceType;
            ActivityType = row.ActivityType != null ? (ActivityType)row.ActivityType : null;
            AchievementLevel = row.AchievementLevel != null ? (EventLevel)row.AchievementLevel : null;
            Points = row.Points;
            Description = row.Description;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
