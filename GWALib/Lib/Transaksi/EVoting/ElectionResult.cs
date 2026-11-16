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
    /// Hasil pemilihan (t_election_result), unik per (pemilihan, kandidat). Hanya-baca; ditulis sekali oleh Election.Finalize untuk semua
    /// kandidat Verified. Persentase dari total suara (2 desimal), peringkat kompetisi (suara sama = peringkat sama), pemenang = peringkat 1
    /// tunggal (seri = tanpa pemenang).
    /// </summary>
    public partial class ElectionResult : BaseLib
    {
        public int Id { get; set; }
        public int ElectionId { get; private set; }
        public int CandidateId { get; private set; }
        public int? CandidateNumber { get; private set; }
        public int MemberId { get; private set; }
        public string MemberName { get; private set; } = null!;
        public int VoteCount { get; private set; }
        public decimal VotePercentage { get; private set; }
        public int? RankNo { get; private set; }
        public ElectionOutcome IsWinner { get; private set; }
        public DateTime CalculatedAt { get; private set; }

        private static IQueryable<EF.TElectionResult> _Querys
        {
            get
            {
                return DBClass.GetContext().TElectionResults.Select(x => new EF.TElectionResult
                {
                    Id = x.Id,
                    ElectionId = x.ElectionId,
                    CandidateId = x.CandidateId,
                    Candidate = new EF.TElectionCandidate
                    {
                        Id = x.CandidateId,
                        CandidateNumber = x.Candidate.CandidateNumber,
                        MemberId = x.Candidate.MemberId,
                        Member = new EF.MMember { Id = x.Candidate.MemberId, Fullname = x.Candidate.Member.Fullname }
                    },
                    VoteCount = x.VoteCount,
                    VotePercentage = x.VotePercentage,
                    RankNo = x.RankNo,
                    IsWinner = x.IsWinner,
                    CalculatedAt = x.CalculatedAt
                });
            }
        }
        public ElectionResult()
        {
        }
        internal ElectionResult(EF.TElectionResult row)
        {
            SetData(row);
        }
        private void SetData(EF.TElectionResult row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ElectionResult>();
            ElectionId = row.ElectionId;
            CandidateId = row.CandidateId;
            CandidateNumber = row.Candidate?.CandidateNumber;
            MemberId = row.Candidate?.MemberId ?? 0;
            MemberName = row.Candidate?.Member?.Fullname ?? string.Empty;
            VoteCount = row.VoteCount;
            VotePercentage = row.VotePercentage;
            RankNo = row.RankNo;
            IsWinner = (ElectionOutcome)row.IsWinner;
            CalculatedAt = row.CalculatedAt;
        }
    }
}
