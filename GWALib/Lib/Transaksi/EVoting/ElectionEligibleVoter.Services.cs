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
    public partial class ElectionEligibleVoter
    {
        private static readonly ElectionStatus[] ChangeStatuses = { ElectionStatus.Draft, ElectionStatus.Registration, ElectionStatus.Open };

        #region SYNC
        public static List<ElectionEligibleVoter> GetDataList(int electionId, VoterEligibility? eligibility = null, bool? hasVoted = null, int startdata = -1, int jmldata = 50)
        {
            return Page(FilterQuery(electionId, eligibility, hasVoted), startdata, jmldata).ToList().MappingData<ElectionEligibleVoter, EF.TElectionEligibleVoter>();
        }
        // Status pemilih untuk user login; null bila tidak terdaftar.
        public static ElectionEligibleVoter? GetMyStatus(int electionId, string user)
        {
            var userId = ValidateUtility.ValidateUser(user);
            var row = _Querys.SingleOrDefault(x => x.ElectionId == electionId && x.Member.UserId == userId);
            return row == null ? null : new ElectionEligibleVoter(row);
        }
        // Menambah anggota aktif tertentu; yang sudah terdaftar dilewati. Mengembalikan jumlah yang ditambahkan.
        public static Response<int> Add(int electionId, List<int> memberIds, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = AddRows(context, electionId, memberIds, null, userId);
                context.SaveChanges();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        // Menambah semua anggota aktif (opsional hanya jenis tertentu) yang belum terdaftar.
        public static Response<int> Generate(int electionId, List<MemberType>? memberTypes, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = AddRows(context, electionId, null, memberTypes, userId);
                context.SaveChanges();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        public Response<ElectionEligibleVoter> SetEligibility(VoterEligibility eligibility, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetStatus(context, row, eligibility, userId);
                context.SaveChanges();
                return new ElectionEligibleVoter(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ElectionEligibleVoter>(msg));
        }
        public Response<ElectionEligibleVoter> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                RemoveRow(context, row, userId);
                context.SaveChanges();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ElectionEligibleVoter>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ElectionEligibleVoter>> GetDataListAsync(int electionId, VoterEligibility? eligibility = null, bool? hasVoted = null, int startdata = -1, int jmldata = 50)
        {
            return await (await Page(FilterQuery(electionId, eligibility, hasVoted), startdata, jmldata).ToListAsync())
                .MappingDataAsync<ElectionEligibleVoter, EF.TElectionEligibleVoter>();
        }
        public static async Task<ElectionEligibleVoter?> GetMyStatusAsync(int electionId, string user)
        {
            var userId = ValidateUtility.ValidateUser(user);
            var row = await _Querys.SingleOrDefaultAsync(x => x.ElectionId == electionId && x.Member.UserId == userId);
            return row == null ? null : new ElectionEligibleVoter(row);
        }
        public static async Task<Response<int>> AddAsync(int electionId, List<int> memberIds, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = AddRows(context, electionId, memberIds, null, userId);
                await context.SaveChangesAsync();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        public static async Task<Response<int>> GenerateAsync(int electionId, List<MemberType>? memberTypes, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = AddRows(context, electionId, null, memberTypes, userId);
                await context.SaveChangesAsync();
                return count;
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<int>(msg));
        }
        public async Task<Response<ElectionEligibleVoter>> SetEligibilityAsync(VoterEligibility eligibility, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetStatus(context, row, eligibility, userId);
                await context.SaveChangesAsync();
                return new ElectionEligibleVoter(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ElectionEligibleVoter>(msg));
        }
        public async Task<Response<ElectionEligibleVoter>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                RemoveRow(context, row, userId);
                await context.SaveChangesAsync();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ElectionEligibleVoter>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TElectionEligibleVoter Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TElectionEligibleVoter> FilterQuery(int electionId, VoterEligibility? eligibility, bool? hasVoted)
        {
            return _Querys.Where(x => x.ElectionId == electionId
                    && (eligibility == null || x.EligibilityStatus == (int)eligibility)
                    && (hasVoted == null || (x.VotedAt != null) == hasVoted))
                .OrderBy(x => x.Member.Fullname).ThenBy(x => x.Id);
        }
        private static IQueryable<EF.TElectionEligibleVoter> Page(IQueryable<EF.TElectionEligibleVoter> query, int startdata, int jmldata)
        {
            return startdata > -1 ? query.Skip(startdata).Take(jmldata) : query;
        }
        private static EF.TElectionEligibleVoter GetRow(EF.ModelContext context, int id)
        {
            return context.TElectionEligibleVoters.Include(x => x.Member).SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pemilih"));
        }
        private static async Task<EF.TElectionEligibleVoter> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TElectionEligibleVoters.Include(x => x.Member).SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pemilih"));
        }
        // memberIds diisi = anggota tertentu (harus aktif); null = semua anggota aktif, opsional per jenis anggota.
        private static int AddRows(EF.ModelContext context, int electionId, List<int>? memberIds, List<MemberType>? memberTypes, int userId)
        {
            var election = Election.GetEditableElection(context, electionId, ChangeStatuses);
            var active = context.MMembers.Where(x => x.Status == (int)MemberStatus.Active);
            List<int> candidates;
            if (memberIds != null)
            {
                var ids = memberIds.Where(x => x > 0).Distinct().ToList();
                candidates = active.Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToList();
                if (candidates.Count != ids.Count)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Sebagian anggota"));
                }
            }
            else
            {
                var types = memberTypes?.Select(x => (int)x).Distinct().ToList();
                candidates = active.Where(x => types == null || types.Count == 0 || types.Contains(x.MemberType)).Select(x => x.Id).ToList();
            }
            var existing = context.TElectionEligibleVoters.Where(x => x.ElectionId == election.Id).Select(x => x.MemberId).ToHashSet();
            var toAdd = candidates.Where(x => !existing.Contains(x)).ToList();
            if (toAdd.Count == 0)
            {
                return 0;
            }
            var firstId = SequenceUtility.ReserveIds<EF.TElectionEligibleVoter>(context, toAdd.Count);
            var now = DateTime.Now;
            for (var i = 0; i < toAdd.Count; i++)
            {
                context.TElectionEligibleVoters.Add(new EF.TElectionEligibleVoter
                {
                    Id = firstId + i,
                    ElectionId = election.Id,
                    MemberId = toAdd[i],
                    EligibilityStatus = (int)VoterEligibility.Eligible,
                    InsDate = now,
                    InsBy = userId
                });
            }
            ElectionAuditEvent.Add(context, election.Id, ElectionAuditEvent.EVENT_VOTERS_ADDED, $"{toAdd.Count} pemilih ditambahkan", userId);
            return toAdd.Count;
        }
        private static void SetStatus(EF.ModelContext context, EF.TElectionEligibleVoter row, VoterEligibility eligibility, int userId)
        {
            Election.GetEditableElection(context, row.ElectionId, ChangeStatuses);
            if (!Enum.IsDefined(eligibility))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Pemilih"));
            }
            if (row.VotedAt != null)
            {
                throw new ArgumentException(ExceptionMessage.ELECTION_VOTER_HAS_VOTED);
            }
            if (row.EligibilityStatus == (int)eligibility)
            {
                return;
            }
            row.EligibilityStatus = (int)eligibility;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            ElectionAuditEvent.Add(context, row.ElectionId, ElectionAuditEvent.EVENT_VOTER_CHANGED,
                $"{row.Member?.Fullname}: {eligibility.GetDescription()}", userId);
        }
        private static void RemoveRow(EF.ModelContext context, EF.TElectionEligibleVoter row, int userId)
        {
            Election.GetEditableElection(context, row.ElectionId);
            if (row.VotedAt != null)
            {
                throw new ArgumentException(ExceptionMessage.ELECTION_VOTER_HAS_VOTED);
            }
            ElectionAuditEvent.Add(context, row.ElectionId, ElectionAuditEvent.EVENT_VOTER_DELETED, row.Member?.Fullname, userId);
            context.TElectionEligibleVoters.Remove(row);
        }
        #endregion
    }
}
