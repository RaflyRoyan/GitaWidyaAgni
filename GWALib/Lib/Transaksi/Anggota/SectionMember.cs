using GWALib.General;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GWALib.Lib.Transaksi.Anggota
{
    /// <summary>
    /// Riwayat section anggota (t_section_member). Setiap anggota punya tepat satu baris aktif (EndDate NULL) = section saat ini.
    /// Pindah section (<see cref="Move"/>) menutup baris aktif sehari sebelum tanggal mulai baru lalu membuka baris baru,
    /// jadi anggota bisa berganti section kapan saja (mis. Brass di satu event, Colour Guard di event berikutnya).
    /// Section awal dibuat bersama m_member (Member.Save / penerimaan pendaftar rekrutmen).
    /// </summary>
    public partial class SectionMember : BaseLib
    {
        public class NewRow
        {
            public int MemberId { get; set; }
            public int SectionId { get; set; }
            /// <summary>Kosong = hari ini.</summary>
            public DateOnly? StartDate { get; set; }
            public string? Note { get; set; }
        }

        public const int NOTE_LENGTH = 255;

        public int Id { get; set; }
        public int SectionId { get; private set; }
        public string SectionName { get; private set; } = null!;
        public int MemberId { get; private set; }
        public string MemberName { get; private set; } = null!;
        public string? MemberNumber { get; private set; }
        public DateOnly StartDate { get; private set; }
        public DateOnly? EndDate { get; private set; }
        public string? Note { get; set; }
        public bool IsCurrent => EndDate == null;
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TSectionMember> _Querys
        {
            get
            {
                return DBClass.GetContext().TSectionMembers.Include(x => x.Section).Include(x => x.Member).AsQueryable();
            }
        }
        public SectionMember()
        {
        }
        public SectionMember(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<SectionMember>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal SectionMember(EF.TSectionMember row)
        {
            SetData(row);
        }
        private void SetData(EF.TSectionMember row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<SectionMember>();
            SectionId = row.SectionId;
            SectionName = row.Section?.Name ?? string.Empty;
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname ?? string.Empty;
            MemberNumber = row.Member?.MemberNumber;
            StartDate = row.StartDate;
            EndDate = row.EndDate;
            Note = row.Note;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
