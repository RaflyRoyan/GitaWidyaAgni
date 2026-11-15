using GWALib.General;
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
    /// Sie / seksi tim pelaksana kegiatan (t_committee_section), mis. Acara, Konsumsi, Perlengkapan. Nama unik per kegiatan.
    /// CoordinatorMemberId diisi otomatis saat anggota tim pelaksana berperan Koordinator Sie. Delete hanya bila sie belum dipakai.
    /// </summary>
    public partial class CommitteeSection : BaseLib
    {
        public class NewRow
        {
            public int ActivityId { get; set; }
            public string Name { get; set; } = null!;
            // Job desk sie.
            public string? Description { get; set; }
            // 0 = otomatis ditaruh paling akhir.
            public int SeqNo { get; set; }
        }

        public int Id { get; set; }
        public int ActivityId { get; private set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        // Hanya-baca: diatur lewat CommitteeMember (peran Koordinator Sie).
        public int? CoordinatorMemberId { get; private set; }
        public string? CoordinatorName { get; private set; }
        public int SeqNo { get; set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TCommitteeSection> _Querys
        {
            get
            {
                return DBClass.GetContext().TCommitteeSections.Include(x => x.CoordinatorMember).AsQueryable();
            }
        }
        public CommitteeSection()
        {
        }
        public CommitteeSection(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<CommitteeSection>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal CommitteeSection(EF.TCommitteeSection row)
        {
            SetData(row);
        }
        private void SetData(EF.TCommitteeSection row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<CommitteeSection>();
            ActivityId = row.ActivityId;
            Name = row.Name;
            Description = row.Description;
            CoordinatorMemberId = row.CoordinatorMemberId;
            CoordinatorName = row.CoordinatorMember?.Fullname;
            SeqNo = row.SeqNo;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
