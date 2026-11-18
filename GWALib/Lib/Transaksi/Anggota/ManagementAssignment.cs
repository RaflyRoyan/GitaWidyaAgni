using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Anggota
{
    /// <summary>
    /// Penugasan pengurus pada periode kepengurusan (t_management_assignment), mis. Ketua Umum atau Kadiv Humas.
    /// Jabatan inti (m_position.is_core) hanya boleh satu pemegang berjalan per (periode, jabatan, divisi).
    /// Anggota berjenis Anggota otomatis menjadi Pengurus saat ditugaskan. End = selesai menjabat; Delete hanya bila belum dipakai sertifikat.
    /// </summary>
    public partial class ManagementAssignment : BaseLib
    {
        public class NewRow
        {
            public int PeriodId { get; set; }
            public int MemberId { get; set; }
            public int PositionId { get; set; }
            // Divisi yang dipimpin, mis. untuk Kadiv.
            public int? DivisionId { get; set; }
            /// <summary>Section yang dipimpin (Section Leader). Tidak boleh bersamaan dengan DivisionId.</summary>
            public int? SectionId { get; set; }
            public string? SkNumber { get; set; }
            public DateOnly StartDate { get; set; }
            public DateOnly? EndDate { get; set; }
        }

        public int Id { get; set; }
        public int PeriodId { get; private set; }
        public string PeriodName { get; private set; } = null!;
        public int MemberId { get; private set; }
        public string MemberName { get; private set; } = null!;
        public int PositionId { get; private set; }
        public string PositionName { get; private set; } = null!;
        public PositionLevel PositionLevel { get; private set; }
        public int? DivisionId { get; set; }
        public string? DivisionName { get; private set; }
        public int? SectionId { get; set; }
        public string? SectionName { get; private set; }
        public string? SkNumber { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        // Hanya-baca: diubah lewat End.
        public AssignmentPeriod IsCurrent { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TManagementAssignment> _Querys
        {
            get
            {
                return DBClass.GetContext().TManagementAssignments
                    .Include(x => x.Period).Include(x => x.Member).Include(x => x.Position).Include(x => x.Division).Include(x => x.Section)
                    .AsQueryable();
            }
        }
        public ManagementAssignment()
        {
        }
        public ManagementAssignment(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ManagementAssignment>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ManagementAssignment(EF.TManagementAssignment row)
        {
            SetData(row);
        }
        private void SetData(EF.TManagementAssignment row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ManagementAssignment>();
            PeriodId = row.PeriodId;
            PeriodName = row.Period?.Name ?? string.Empty;
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname ?? string.Empty;
            PositionId = row.PositionId;
            PositionName = row.Position?.Name ?? string.Empty;
            PositionLevel = row.Position != null ? (PositionLevel)row.Position.IsCore : PositionLevel.Biasa;
            DivisionId = row.DivisionId;
            DivisionName = row.Division?.Name;
            SectionId = row.SectionId;
            SectionName = row.Section?.Name;
            SkNumber = row.SkNumber;
            StartDate = row.StartDate;
            EndDate = row.EndDate;
            IsCurrent = (AssignmentPeriod)row.IsCurrent;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
