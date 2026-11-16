using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Prestasi
{
    /// <summary>
    /// Poin keaktifan anggota (t_member_point), unik per (anggota, sumber, RefId). Poin otomatis memakai PointRule.GetEffectiveRow
    /// (tanpa aturan aktif = tidak ada poin): Prestasi dari Achievement; GenerateForActivity memberi poin Hadir (RefId = t_attendance_record,
    /// status Hadir / Terlambat) dan Panitia (RefId = t_committee_member aktif); GenerateForPeriod memberi poin Pengurus (RefId = t_management_assignment).
    /// Menjalankan ulang memperbarui poin yang sudah ada. Pemateri / Lainnya dicatat manual (Save, poin tidak boleh 0). Periode kosong = periode aktif.
    /// </summary>
    public partial class MemberPoint : BaseLib
    {
        public class NewRow
        {
            public int MemberId { get; set; }
            public int? PeriodId { get; set; }
            public PointSourceType SourceType { get; set; } = PointSourceType.Lainnya;
            public int? RefId { get; set; }
            public int Points { get; set; }
            public DateOnly EarnedDate { get; set; }
            public string? Notes { get; set; }
        }
        public record LeaderboardRow(int MemberId, string MemberName, int TotalPoints);

        public int Id { get; set; }
        public int MemberId { get; private set; }
        public string MemberName { get; private set; } = null!;
        public int? PeriodId { get; set; }
        public PointSourceType SourceType { get; private set; }
        public int? RefId { get; private set; }
        public int Points { get; set; }
        public DateOnly EarnedDate { get; set; }
        public string? Notes { get; set; }
        public DateTime InsDate { get; private set; }

        private static IQueryable<EF.TMemberPoint> _Querys
        {
            get
            {
                return DBClass.GetContext().TMemberPoints.Include(x => x.Member).AsQueryable();
            }
        }
        public MemberPoint()
        {
        }
        public MemberPoint(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<MemberPoint>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal MemberPoint(EF.TMemberPoint row)
        {
            SetData(row);
        }
        private void SetData(EF.TMemberPoint row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<MemberPoint>();
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname ?? string.Empty;
            PeriodId = row.PeriodId;
            SourceType = (PointSourceType)row.SourceType;
            RefId = row.RefId;
            Points = row.Points;
            EarnedDate = row.EarnedDate;
            Notes = row.Notes;
            InsDate = row.InsDate;
        }
    }
}
