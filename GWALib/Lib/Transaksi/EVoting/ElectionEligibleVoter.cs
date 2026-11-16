using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.EVoting
{
    /// <summary>
    /// Daftar pemilih (t_election_eligible_voter), unik per (pemilihan, anggota). Add / Generate (anggota aktif, opsional per jenis anggota)
    /// dan SetEligibility boleh saat pemilihan Draft / Registration / Open; hapus hanya Draft / Registration. Pemilih yang sudah memilih
    /// tidak bisa diubah / dihapus. VotedAt diisi saat memilih tanpa menyimpan pilihannya (lihat ElectionVote).
    /// </summary>
    public partial class ElectionEligibleVoter : BaseLib
    {
        public int Id { get; set; }
        public int ElectionId { get; private set; }
        public int MemberId { get; private set; }
        public string MemberName { get; private set; } = null!;
        public string? Nim { get; private set; }
        public VoterEligibility EligibilityStatus { get; private set; }
        public bool HasVoted { get; private set; }
        public DateTime? VotedAt { get; private set; }

        private static IQueryable<EF.TElectionEligibleVoter> _Querys
        {
            get
            {
                return DBClass.GetContext().TElectionEligibleVoters.Select(x => new EF.TElectionEligibleVoter
                {
                    Id = x.Id,
                    ElectionId = x.ElectionId,
                    MemberId = x.MemberId,
                    Member = new EF.MMember { Id = x.MemberId, Fullname = x.Member.Fullname, Nim = x.Member.Nim, UserId = x.Member.UserId },
                    EligibilityStatus = x.EligibilityStatus,
                    VotedAt = x.VotedAt
                });
            }
        }
        public ElectionEligibleVoter()
        {
        }
        public ElectionEligibleVoter(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ElectionEligibleVoter>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ElectionEligibleVoter(EF.TElectionEligibleVoter row)
        {
            SetData(row);
        }
        private void SetData(EF.TElectionEligibleVoter row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ElectionEligibleVoter>();
            ElectionId = row.ElectionId;
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname ?? string.Empty;
            Nim = row.Member?.Nim;
            EligibilityStatus = (VoterEligibility)row.EligibilityStatus;
            HasVoted = row.VotedAt != null;
            VotedAt = row.VotedAt;
        }
    }
}
