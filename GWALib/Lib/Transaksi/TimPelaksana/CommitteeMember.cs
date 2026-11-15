using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.TimPelaksana
{
    /// <summary>
    /// Anggota tim pelaksana kegiatan (t_committee_member). Satu anggota sekali per kegiatan.
    /// Peran inti (Ketua Pelaksana, Wakil, Sekretaris, Bendahara) tanpa sie dan hanya satu per kegiatan; Koordinator Sie / Staf wajib sie,
    /// Koordinator hanya satu per sie dan tercatat di CommitteeSection.CoordinatorMemberId. Status: Aktif → Mundur / Dikeluarkan;
    /// menambahkan lagi anggota yang mundur / dikeluarkan mengaktifkan baris lama.
    /// </summary>
    public partial class CommitteeMember : BaseLib
    {
        public class NewRow
        {
            public int ActivityId { get; set; }
            public int? SectionId { get; set; }
            public int MemberId { get; set; }
            public CommitteeRole CommitteeRole { get; set; } = CommitteeRole.Staf;
            public string? JobDesc { get; set; }
        }

        public int Id { get; set; }
        public int ActivityId { get; private set; }
        public int? SectionId { get; set; }
        public string? SectionName { get; private set; }
        public int MemberId { get; private set; }
        public string MemberName { get; private set; } = null!;
        public CommitteeRole CommitteeRole { get; set; }
        public string? JobDesc { get; set; }
        // Hanya-baca: diubah lewat Resign / Remove.
        public CommitteeMemberStatus Status { get; private set; }
        public int? ApplicationId { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TCommitteeMember> _Querys
        {
            get
            {
                return DBClass.GetContext().TCommitteeMembers.Include(x => x.Section).Include(x => x.Member).AsQueryable();
            }
        }
        public CommitteeMember()
        {
        }
        public CommitteeMember(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<CommitteeMember>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal CommitteeMember(EF.TCommitteeMember row)
        {
            SetData(row);
        }
        private void SetData(EF.TCommitteeMember row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<CommitteeMember>();
            ActivityId = row.ActivityId;
            SectionId = row.SectionId;
            SectionName = row.Section?.Name;
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname ?? string.Empty;
            CommitteeRole = (CommitteeRole)row.CommitteeRole;
            JobDesc = row.JobDesc;
            Status = (CommitteeMemberStatus)row.Status;
            ApplicationId = row.ApplicationId;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
