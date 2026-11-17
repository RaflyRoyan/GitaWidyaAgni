using GWALib.General;
using GWALib.Lib.Transaksi.Information;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
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
    public partial class Election
    {
        #region SYNC
        public static List<Election> GetDataList(ElectionStatus? status = null, int? periodId = null)
        {
            return FilterQuery(status, periodId).ToList().MappingData<Election, EF.TElection>();
        }
        // Pemilihan yang sedang menerima suara.
        public static List<Election> GetOpenList()
        {
            return OpenQuery().ToList().MappingData<Election, EF.TElection>();
        }
        public static Response<Election> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new Election(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Election>(msg));
        }
        public Response<Election> Update(string user)
        {
            return Run(SetRow, user);
        }
        public Response<Election> StartRegistration(string user)
        {
            return Run(SetRegistration, user);
        }
        public Response<Election> OpenVoting(string user)
        {
            return Run(SetOpen, user);
        }
        public Response<Election> Close(string user)
        {
            return Run((context, row, userId) => SetClose(context, row, userId, false), user);
        }
        public Response<Election> Finalize(string user)
        {
            return Run(SetFinalize, user);
        }
        public Response<Election> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                RemoveRow(context, row);
                context.SaveChanges();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Election>(msg));
        }
        // Untuk job: menutup pemilihan Open yang sudah lewat EndAt. Mengembalikan jumlah yang ditutup.
        public static int CloseExpired()
        {
            using var context = DBClass.GetContext();
            var now = DateTime.Now;
            var rows = context.TElections.Where(x => x.Status == (int)ElectionStatus.Open && x.EndAt < now).ToList();
            foreach (var row in rows)
            {
                SetClose(context, row, null, true);
            }
            context.SaveChanges();
            return rows.Count;
        }
        #endregion

        #region ASYNC
        public static async Task<List<Election>> GetDataListAsync(ElectionStatus? status = null, int? periodId = null)
        {
            return await (await FilterQuery(status, periodId).ToListAsync()).MappingDataAsync<Election, EF.TElection>();
        }
        public static async Task<List<Election>> GetOpenListAsync()
        {
            return await (await OpenQuery().ToListAsync()).MappingDataAsync<Election, EF.TElection>();
        }
        public static async Task<Response<Election>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new Election(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Election>(msg));
        }
        public Task<Response<Election>> UpdateAsync(string user)
        {
            return RunAsync(SetRow, user);
        }
        public Task<Response<Election>> StartRegistrationAsync(string user)
        {
            return RunAsync(SetRegistration, user);
        }
        public Task<Response<Election>> OpenVotingAsync(string user)
        {
            return RunAsync(SetOpen, user);
        }
        public Task<Response<Election>> CloseAsync(string user)
        {
            return RunAsync((context, row, userId) => SetClose(context, row, userId, false), user);
        }
        public Task<Response<Election>> FinalizeAsync(string user)
        {
            return RunAsync(SetFinalize, user);
        }
        public async Task<Response<Election>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                RemoveRow(context, row);
                await context.SaveChangesAsync();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Election>(msg));
        }
        public static async Task<int> CloseExpiredAsync()
        {
            using var context = DBClass.GetContext();
            var now = DateTime.Now;
            var rows = await context.TElections.Where(x => x.Status == (int)ElectionStatus.Open && x.EndAt < now).ToListAsync();
            foreach (var row in rows)
            {
                SetClose(context, row, null, true);
            }
            await context.SaveChangesAsync();
            return rows.Count;
        }
        #endregion

        #region INTERNAL
        internal static EF.TElection GetElection(EF.ModelContext context, int electionId)
        {
            return context.TElections.SingleOrDefault(x => x.Id == electionId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pemilihan"));
        }
        // Pemilihan yang datanya (kandidat / pemilih) masih boleh diubah.
        internal static EF.TElection GetEditableElection(EF.ModelContext context, int electionId, params ElectionStatus[] allowed)
        {
            var row = GetElection(context, electionId);
            var statuses = allowed.Length == 0 ? new[] { ElectionStatus.Draft, ElectionStatus.Registration } : allowed;
            if (!statuses.Contains((ElectionStatus)row.Status))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.ELECTION_LOCKED, ((ElectionStatus)row.Status).GetDescription()));
            }
            return row;
        }
        // Pemilihan yang sedang menerima suara: Open dan sekarang di dalam StartAt..EndAt.
        internal static EF.TElection GetOpenElection(EF.ModelContext context, int electionId)
        {
            var row = GetElection(context, electionId);
            var now = DateTime.Now;
            if (row.Status != (int)ElectionStatus.Open || now < row.StartAt || now > row.EndAt)
            {
                throw new ArgumentException(ExceptionMessage.ELECTION_NOT_OPEN);
            }
            return row;
        }
        #endregion

        #region HELPER
        private delegate void RowAction(EF.ModelContext context, EF.TElection row, int userId);

        private Response<Election> Run(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                return new Election(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Election>(msg));
        }
        private async Task<Response<Election>> RunAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                return new Election(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Election>(msg));
        }
        private static EF.TElection Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TElection> FilterQuery(ElectionStatus? status, int? periodId)
        {
            return _Querys.Where(x => (status == null || x.Status == (int)status) && (periodId == null || x.PeriodId == periodId))
                .OrderByDescending(x => x.StartAt).ThenByDescending(x => x.Id);
        }
        private static IQueryable<EF.TElection> OpenQuery()
        {
            var now = DateTime.Now;
            return _Querys.Where(x => x.Status == (int)ElectionStatus.Open && x.StartAt <= now && x.EndAt >= now)
                .OrderBy(x => x.EndAt).ThenBy(x => x.Id);
        }
        private static EF.TElection GetRow(EF.ModelContext context, int id)
        {
            return GetElection(context, id);
        }
        private static async Task<EF.TElection> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TElections.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pemilihan"));
        }
        private static void SetSchedule(EF.TElection row, DateTime startAt, DateTime endAt)
        {
            if (startAt == default || endAt == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Waktu Mulai dan Selesai"));
            }
            if (endAt <= startAt)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            row.StartAt = startAt;
            row.EndAt = endAt;
        }
        private static int? ValidatePeriod(EF.ModelContext context, int? periodId)
        {
            if (periodId > 0)
            {
                ManagementPeriod.ValidateOpenPeriod(context, periodId.Value);
                return periodId;
            }
            return null;
        }
        private static void SetText(EF.TElection row, string? title, string? description)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Pemilihan"));
            }
            row.Title = title.Trim();
            row.Description = TransaksiHelper.ToNullable(description);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static EF.TElection CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var row = new EF.TElection
            {
                PeriodId = ValidatePeriod(context, newrow.PeriodId),
                Status = (int)ElectionStatus.Draft,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            SetText(row, newrow.Title, newrow.Description);
            SetSchedule(row, newrow.StartAt, newrow.EndAt);
            row.Id = SequenceUtility.GetNextId<EF.TElection>(context);
            context.TElections.Add(row);
            ElectionAuditEvent.Add(context, row.Id, ElectionAuditEvent.EVENT_CREATED, row.Title, userId);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.TElection row, int userId)
        {
            var status = (ElectionStatus)row.Status;
            if (status == ElectionStatus.Finalized)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.ELECTION_LOCKED, status.GetDescription()));
            }
            var changes = new List<string>();
            if (status == ElectionStatus.Draft || status == ElectionStatus.Registration)
            {
                var periodId = ValidatePeriod(context, PeriodId);
                if (periodId != row.PeriodId)
                {
                    changes.Add("periode");
                }
                if (StartAt != row.StartAt || EndAt != row.EndAt)
                {
                    changes.Add($"jadwal {StartAt:dd-MM-yyyy HH:mm} s.d. {EndAt:dd-MM-yyyy HH:mm}");
                }
                row.PeriodId = periodId;
                SetSchedule(row, StartAt, EndAt);
            }
            else if (status == ElectionStatus.Open && EndAt != row.EndAt)
            {
                // Saat Open hanya waktu selesai yang boleh diubah, dan tidak boleh ke masa lalu.
                if (EndAt <= DateTime.Now)
                {
                    throw new ArgumentException(ExceptionMessage.ELECTION_END_PASSED);
                }
                SetSchedule(row, row.StartAt, EndAt);
                changes.Add($"waktu selesai {EndAt:dd-MM-yyyy HH:mm}");
            }
            if (Title?.Trim() != row.Title)
            {
                changes.Add("judul");
            }
            SetText(row, Title, Description);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            if (changes.Count > 0)
            {
                ElectionAuditEvent.Add(context, row.Id, ElectionAuditEvent.EVENT_UPDATED, "Ubah " + string.Join(", ", changes), userId);
            }
        }
        private static void SetRegistration(EF.ModelContext context, EF.TElection row, int userId)
        {
            TransaksiHelper.ValidateStatus((ElectionStatus)row.Status, "membuka pendaftaran kandidat", ElectionStatus.Draft);
            row.Status = (int)ElectionStatus.Registration;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            ElectionAuditEvent.Add(context, row.Id, ElectionAuditEvent.EVENT_REGISTRATION_OPENED, null, userId);
        }
        private static void SetOpen(EF.ModelContext context, EF.TElection row, int userId)
        {
            TransaksiHelper.ValidateStatus((ElectionStatus)row.Status, "membuka pemungutan suara", ElectionStatus.Draft, ElectionStatus.Registration);
            ElectionCrypto.EnsureConfigured();
            if (row.EndAt <= DateTime.Now)
            {
                throw new ArgumentException(ExceptionMessage.ELECTION_END_PASSED);
            }
            var candidates = context.TElectionCandidates.Where(x => x.ElectionId == row.Id
                    && (x.Status == (int)CandidateStatus.Registered || x.Status == (int)CandidateStatus.Verified))
                .Select(x => new { x.Id, x.Status, x.CandidateNumber }).ToList();
            if (candidates.Any(x => x.Status == (int)CandidateStatus.Registered))
            {
                throw new ArgumentException(ExceptionMessage.ELECTION_PENDING_CANDIDATE);
            }
            if (candidates.Count == 0)
            {
                throw new ArgumentException(ExceptionMessage.ELECTION_CANDIDATE_REQUIRED);
            }
            if (!context.TElectionEligibleVoters.Any(x => x.ElectionId == row.Id && x.EligibilityStatus == (int)VoterEligibility.Eligible))
            {
                throw new ArgumentException(ExceptionMessage.ELECTION_VOTER_REQUIRED);
            }
            // Nomor urut untuk kandidat yang belum bernomor, melanjutkan nomor terbesar.
            var next = candidates.Max(x => x.CandidateNumber) ?? 0;
            foreach (var candidate in candidates.Where(x => x.CandidateNumber == null).OrderBy(x => x.Id))
            {
                var stub = new EF.TElectionCandidate { Id = candidate.Id };
                context.TElectionCandidates.Attach(stub);
                stub.CandidateNumber = ++next;
                stub.UpdDate = DateTime.Now;
                stub.UpdBy = userId;
            }
            row.Status = (int)ElectionStatus.Open;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            ElectionAuditEvent.Add(context, row.Id, ElectionAuditEvent.EVENT_OPENED, $"{candidates.Count} kandidat", userId);
            var voters = context.TElectionEligibleVoters
                .Where(x => x.ElectionId == row.Id && x.EligibilityStatus == (int)VoterEligibility.Eligible)
                .Select(x => (int?)x.MemberId).ToList();
            NotifyHelper.ToMembers(context, voters, NotifyHelper.TYPE_ELECTION, "Pemungutan suara dibuka: " + row.Title,
                $"Pilih sampai {row.EndAt:dd-MM-yyyy HH:mm}", NotifyHelper.LINK_MY_ELECTION, "t_election", row.Id, userId);
        }
        private static void SetClose(EF.ModelContext context, EF.TElection row, int? userId, bool automatic)
        {
            TransaksiHelper.ValidateStatus((ElectionStatus)row.Status, "menutup pemungutan suara", ElectionStatus.Open);
            row.Status = (int)ElectionStatus.Closed;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            ElectionAuditEvent.Add(context, row.Id, ElectionAuditEvent.EVENT_CLOSED, automatic ? "Ditutup otomatis setelah waktu selesai" : null, userId);
        }
        // Mendekripsi semua suara dan menulis hasil. Jumlah suara harus sama dengan pemilih yang sudah memilih.
        private static void SetFinalize(EF.ModelContext context, EF.TElection row, int userId)
        {
            TransaksiHelper.ValidateStatus((ElectionStatus)row.Status, "memfinalisasi hasil pemilihan", ElectionStatus.Closed);
            ElectionCrypto.EnsureConfigured();
            var votes = context.TElectionVotes.Where(x => x.ElectionId == row.Id)
                .Select(x => new { x.BallotReference, x.EncryptedChoice }).ToList();
            var voted = context.TElectionEligibleVoters.Count(x => x.ElectionId == row.Id && x.VotedAt != null);
            if (votes.Count != voted)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.ELECTION_TALLY_MISMATCH, votes.Count, voted));
            }
            var candidates = context.TElectionCandidates.Where(x => x.ElectionId == row.Id && x.Status == (int)CandidateStatus.Verified)
                .Select(x => new { x.Id, x.CandidateNumber }).ToList();
            var counts = candidates.ToDictionary(x => x.Id, x => 0);
            foreach (var vote in votes)
            {
                var candidateId = ElectionCrypto.Decrypt(row.Id, vote.BallotReference, vote.EncryptedChoice);
                if (candidateId == null || !counts.ContainsKey(candidateId.Value))
                {
                    throw new ArgumentException(ExceptionMessage.ELECTION_VOTE_CORRUPT);
                }
                counts[candidateId.Value]++;
            }

            var total = votes.Count;
            var ordered = candidates.OrderByDescending(x => counts[x.Id]).ThenBy(x => x.CandidateNumber).ToList();
            var top = total == 0 ? -1 : counts[ordered[0].Id];
            var singleWinner = total > 0 && ordered.Count(x => counts[x.Id] == top) == 1;
            var now = DateTime.Now;
            var firstId = SequenceUtility.ReserveIds<EF.TElectionResult>(context, ordered.Count);
            for (var i = 0; i < ordered.Count; i++)
            {
                var count = counts[ordered[i].Id];
                context.TElectionResults.Add(new EF.TElectionResult
                {
                    Id = firstId + i,
                    ElectionId = row.Id,
                    CandidateId = ordered[i].Id,
                    VoteCount = count,
                    VotePercentage = total == 0 ? 0 : Math.Round(count * 100m / total, 2),
                    // Peringkat kompetisi: suara sama = peringkat sama (1, 1, 3).
                    RankNo = 1 + ordered.Count(x => counts[x.Id] > count),
                    IsWinner = (int)(singleWinner && count == top ? ElectionOutcome.Winner : ElectionOutcome.Lose),
                    CalculatedAt = now,
                    InsDate = now,
                    InsBy = userId
                });
            }
            row.Status = (int)ElectionStatus.Finalized;
            row.FinalizedAt = now;
            row.FinalizedBy = userId;
            row.UpdDate = now;
            row.UpdBy = userId;
            ElectionAuditEvent.Add(context, row.Id, ElectionAuditEvent.EVENT_RESULT_FINALIZED,
                $"{total} suara dari {context.TElectionEligibleVoters.Count(x => x.ElectionId == row.Id && x.EligibilityStatus == (int)VoterEligibility.Eligible)} pemilih"
                + (total > 0 && !singleWinner ? "; hasil seri, tanpa pemenang" : string.Empty), userId);
        }
        // Hanya Draft / Registration (belum ada suara). Kandidat dihapus lewat stub supaya foto tidak dimuat.
        private static void RemoveRow(EF.ModelContext context, EF.TElection row)
        {
            TransaksiHelper.ValidateStatus((ElectionStatus)row.Status, "menghapus pemilihan", ElectionStatus.Draft, ElectionStatus.Registration);
            foreach (var candidateId in context.TElectionCandidates.Where(x => x.ElectionId == row.Id).Select(x => x.Id).ToList())
            {
                context.TElectionCandidates.Remove(new EF.TElectionCandidate { Id = candidateId, ElectionId = row.Id });
            }
            context.TElectionEligibleVoters.RemoveRange(context.TElectionEligibleVoters.Where(x => x.ElectionId == row.Id).ToList());
            context.TElectionAuditEvents.RemoveRange(context.TElectionAuditEvents.Where(x => x.ElectionId == row.Id).ToList());
            context.TElections.Remove(row);
        }
        #endregion
    }
}
