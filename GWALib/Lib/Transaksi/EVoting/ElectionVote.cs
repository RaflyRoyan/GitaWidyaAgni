using GWALib.General;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.EVoting
{
    /// <summary>
    /// Suara (t_election_vote), anonim dan terenkripsi. Tidak ada kolom yang menyambungkan suara ke pemilih:
    /// - id acak (bukan m_sequence), ins_by selalu NULL, ins_date hanya tanggal (jam 00:00).
    /// - encrypted_choice AES-256-GCM (ElectionCrypto); didekripsi hanya saat Election.Finalize. Kunci dari ConfigureKey saat startup.
    /// - ballot_reference = SHA-256 dari token tanda terima acak yang hanya diberikan sekali ke pemilih (Cast); VerifyReceipt memastikan
    ///   suaranya tercatat tanpa membuka identitas atau pilihan.
    /// Cast dijalankan dalam satu transaksi: voted_at pemilih diisi dengan UPDATE bersyarat (voted_at IS NULL) sehingga tidak bisa
    /// memilih dua kali, lalu suara disimpan. Tidak ada ubah / hapus.
    /// </summary>
    public partial class ElectionVote : BaseLib
    {
        public class NewRow
        {
            public int ElectionId { get; set; }
            public int CandidateId { get; set; }
        }
        // Receipt hanya dikembalikan sekali; simpan / tampilkan ke pemilih.
        public record VoteReceipt(int ElectionId, string Receipt, DateTime VotedDate);

        public int Id { get; set; }
        public int ElectionId { get; private set; }
        public string BallotReference { get; private set; } = null!;
        public DateTime VotedDate { get; private set; }

        // Pilihan terenkripsi tidak ikut di-select.
        private static IQueryable<EF.TElectionVote> _Querys
        {
            get
            {
                return DBClass.GetContext().TElectionVotes.Select(x => new EF.TElectionVote
                {
                    Id = x.Id,
                    ElectionId = x.ElectionId,
                    BallotReference = x.BallotReference,
                    InsDate = x.InsDate
                });
            }
        }
        public ElectionVote()
        {
        }
        internal ElectionVote(EF.TElectionVote row)
        {
            SetData(row);
        }
        private void SetData(EF.TElectionVote row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ElectionVote>();
            ElectionId = row.ElectionId;
            BallotReference = row.BallotReference;
            VotedDate = row.InsDate.Date;
        }
    }
}
