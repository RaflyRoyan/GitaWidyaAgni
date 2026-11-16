using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.EVoting
{
    public partial class ElectionVote
    {
        #region SYNC
        // Dipanggil sekali saat startup dengan base64 dari 32 byte acak (secret aplikasi, bukan database). Kunci yang sama
        // wajib dipakai sampai semua pemilihan yang memakainya difinalisasi.
        public static void ConfigureKey(string base64Key)
        {
            ElectionCrypto.Configure(base64Key);
        }
        // Papan buletin: daftar ballot reference (hash) tanpa pilihan, untuk verifikasi publik.
        public static List<ElectionVote> GetDataList(int electionId, int startdata = -1, int jmldata = 100)
        {
            return Page(FilterQuery(electionId), startdata, jmldata).ToList().MappingData<ElectionVote, EF.TElectionVote>();
        }
        public static int GetCount(int electionId)
        {
            using var context = DBClass.GetContext();
            return context.TElectionVotes.Count(x => x.ElectionId == electionId);
        }
        // Tanpa login: memastikan token tanda terima tercatat di pemilihan ini.
        public static bool VerifyReceipt(int electionId, string receipt)
        {
            if (string.IsNullOrWhiteSpace(receipt))
            {
                return false;
            }
            using var context = DBClass.GetContext();
            var reference = ElectionCrypto.HashReceipt(receipt);
            return context.TElectionVotes.Any(x => x.ElectionId == electionId && x.BallotReference == reference);
        }
        public static Response<VoteReceipt> Cast(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                ElectionCrypto.EnsureConfigured();
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var voterId = ValidateVoter(context, newrow, userId);
                var now = DateTime.Now;
                var updated = context.TElectionEligibleVoters
                    .Where(x => x.Id == voterId && x.VotedAt == null)
                    .ExecuteUpdate(s => s.SetProperty(x => x.VotedAt, now).SetProperty(x => x.UpdDate, now).SetProperty(x => x.UpdBy, userId));
                if (updated == 0)
                {
                    throw new ArgumentException(ExceptionMessage.ELECTION_ALREADY_VOTED);
                }
                var receipt = AddVote(context, newrow);
                context.SaveChanges();
                transaction.Commit();
                return receipt;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<VoteReceipt>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ElectionVote>> GetDataListAsync(int electionId, int startdata = -1, int jmldata = 100)
        {
            return await (await Page(FilterQuery(electionId), startdata, jmldata).ToListAsync()).MappingDataAsync<ElectionVote, EF.TElectionVote>();
        }
        public static async Task<int> GetCountAsync(int electionId)
        {
            using var context = DBClass.GetContext();
            return await context.TElectionVotes.CountAsync(x => x.ElectionId == electionId);
        }
        public static async Task<bool> VerifyReceiptAsync(int electionId, string receipt)
        {
            if (string.IsNullOrWhiteSpace(receipt))
            {
                return false;
            }
            using var context = DBClass.GetContext();
            var reference = ElectionCrypto.HashReceipt(receipt);
            return await context.TElectionVotes.AnyAsync(x => x.ElectionId == electionId && x.BallotReference == reference);
        }
        public static async Task<Response<VoteReceipt>> CastAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                ElectionCrypto.EnsureConfigured();
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var voterId = ValidateVoter(context, newrow, userId);
                var now = DateTime.Now;
                var updated = await context.TElectionEligibleVoters
                    .Where(x => x.Id == voterId && x.VotedAt == null)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.VotedAt, now).SetProperty(x => x.UpdDate, now).SetProperty(x => x.UpdBy, userId));
                if (updated == 0)
                {
                    throw new ArgumentException(ExceptionMessage.ELECTION_ALREADY_VOTED);
                }
                var receipt = AddVote(context, newrow);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return receipt;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<VoteReceipt>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TElectionVote> FilterQuery(int electionId)
        {
            return _Querys.Where(x => x.ElectionId == electionId).OrderBy(x => x.BallotReference);
        }
        private static IQueryable<EF.TElectionVote> Page(IQueryable<EF.TElectionVote> query, int startdata, int jmldata)
        {
            return startdata > -1 ? query.Skip(startdata).Take(jmldata) : query;
        }
        // Pemilihan Open dalam jadwal, user adalah anggota aktif yang terdaftar Eligible dan belum memilih, kandidat Verified.
        private static int ValidateVoter(EF.ModelContext context, NewRow newrow, int userId)
        {
            Election.GetOpenElection(context, newrow.ElectionId);
            var member = context.MMembers.AsNoTracking().Where(x => x.UserId == userId).Select(x => new { x.Id, x.Status }).SingleOrDefault()
                ?? throw new ArgumentException(ExceptionMessage.USER_NOT_MEMBER);
            if (member.Status != (int)MemberStatus.Active)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Anggota"));
            }
            var voter = context.TElectionEligibleVoters.AsNoTracking()
                .Where(x => x.ElectionId == newrow.ElectionId && x.MemberId == member.Id && x.EligibilityStatus == (int)VoterEligibility.Eligible)
                .Select(x => new { x.Id, x.VotedAt }).SingleOrDefault()
                ?? throw new ArgumentException(ExceptionMessage.ELECTION_NOT_ELIGIBLE);
            if (voter.VotedAt != null)
            {
                throw new ArgumentException(ExceptionMessage.ELECTION_ALREADY_VOTED);
            }
            if (!context.TElectionCandidates.Any(x => x.Id == newrow.CandidateId && x.ElectionId == newrow.ElectionId && x.Status == (int)CandidateStatus.Verified))
            {
                throw new ArgumentException(ExceptionMessage.ELECTION_CANDIDATE_INVALID);
            }
            return voter.Id;
        }
        // Tanpa SaveChanges. Tidak ada data pemilih yang ikut disimpan.
        private static VoteReceipt AddVote(EF.ModelContext context, NewRow newrow)
        {
            var (receipt, reference) = ElectionCrypto.NewBallot();
            int id;
            do
            {
                id = ElectionCrypto.NewVoteId();
            }
            while (context.TElectionVotes.Any(x => x.Id == id));
            var today = DateTime.Today;
            context.TElectionVotes.Add(new EF.TElectionVote
            {
                Id = id,
                ElectionId = newrow.ElectionId,
                BallotReference = reference,
                EncryptedChoice = ElectionCrypto.Encrypt(newrow.ElectionId, reference, newrow.CandidateId),
                InsDate = today,
                InsBy = null
            });
            return new VoteReceipt(newrow.ElectionId, receipt, today);
        }
        #endregion
    }
}
