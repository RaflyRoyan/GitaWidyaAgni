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
    /// Pemilihan (t_election). Periode opsional dan tidak boleh Closed.
    /// Alur: Draft → Registration → Open → Closed → Finalized (Draft → Open langsung boleh). Open butuh minimal 1 kandidat Verified,
    /// tidak ada kandidat Registered yang belum diputuskan, minimal 1 pemilih Eligible, dan waktu selesai belum lewat; kandidat Verified
    /// tanpa nomor urut diberi nomor berikutnya. Suara hanya diterima saat Open dan StartAt..EndAt. CloseExpired (untuk job) menutup
    /// pemilihan yang lewat EndAt. Finalize mendekripsi semua suara, mencocokkan jumlah suara dengan pemilih yang sudah memilih, lalu
    /// menulis t_election_result (pemenang = peringkat 1 tunggal; seri = tanpa pemenang).
    /// Judul / deskripsi bisa diubah sebelum Finalized; periode dan jadwal hanya Draft / Registration, EndAt boleh diperpanjang saat Open.
    /// Hapus hanya Draft / Registration (kandidat, pemilih, dan audit ikut terhapus). Setiap perubahan status dicatat di audit.
    /// </summary>
    public partial class Election : BaseLib
    {
        public class NewRow
        {
            public int? PeriodId { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public DateTime StartAt { get; set; }
            public DateTime EndAt { get; set; }
        }

        public int Id { get; set; }
        public int? PeriodId { get; set; }
        public string? PeriodName { get; private set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        // Hanya-baca: diubah lewat StartRegistration / OpenVoting / Close / Finalize.
        public ElectionStatus Status { get; private set; }
        public DateTime? FinalizedAt { get; private set; }
        public int? FinalizedBy { get; private set; }
        public int CandidateCount { get; private set; }
        public int VoterCount { get; private set; }
        public int VotedCount { get; private set; }
        public DateTime InsDate { get; private set; }

        private static IQueryable<EF.TElection> _Querys
        {
            get
            {
                return DBClass.GetContext().TElections.Select(x => new EF.TElection
                {
                    Id = x.Id,
                    PeriodId = x.PeriodId,
                    Period = x.PeriodId == null ? null : new EF.MManagementPeriod { Id = x.Period!.Id, Name = x.Period.Name },
                    Title = x.Title,
                    Description = x.Description,
                    StartAt = x.StartAt,
                    EndAt = x.EndAt,
                    Status = x.Status,
                    FinalizedAt = x.FinalizedAt,
                    FinalizedBy = x.FinalizedBy,
                    InsDate = x.InsDate,
                    TElectionCandidates = x.TElectionCandidates.Where(c => c.Status == (int)CandidateStatus.Verified)
                        .Select(c => new EF.TElectionCandidate { Id = c.Id }).ToList(),
                    TElectionEligibleVoters = x.TElectionEligibleVoters.Where(v => v.EligibilityStatus == (int)VoterEligibility.Eligible)
                        .Select(v => new EF.TElectionEligibleVoter { Id = v.Id, VotedAt = v.VotedAt }).ToList()
                });
            }
        }
        public Election()
        {
        }
        public Election(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Election>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Election(EF.TElection row)
        {
            SetData(row);
        }
        private void SetData(EF.TElection row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Election>();
            PeriodId = row.PeriodId;
            PeriodName = row.Period?.Name;
            Title = row.Title;
            Description = row.Description;
            StartAt = row.StartAt;
            EndAt = row.EndAt;
            Status = (ElectionStatus)row.Status;
            FinalizedAt = row.FinalizedAt;
            FinalizedBy = row.FinalizedBy;
            CandidateCount = row.TElectionCandidates.Count;
            VoterCount = row.TElectionEligibleVoters.Count;
            VotedCount = row.TElectionEligibleVoters.Count(x => x.VotedAt != null);
            InsDate = row.InsDate;
        }
    }
}
