using GWALib.General;
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
    public partial class ElectionCandidate
    {
        #region SYNC
        public static List<ElectionCandidate> GetDataList(int electionId, CandidateStatus? status = null)
        {
            return FilterQuery(electionId, status).ToList().MappingData<ElectionCandidate, EF.TElectionCandidate>();
        }
        // Surat suara: kandidat Verified urut nomor.
        public static List<ElectionCandidate> GetBallot(int electionId)
        {
            return FilterQuery(electionId, CandidateStatus.Verified).ToList().MappingData<ElectionCandidate, EF.TElectionCandidate>();
        }
        public static Response<ElectionCandidate> Register(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = SetRegister(context, newrow, userId);
                context.SaveChanges();
                return new ElectionCandidate(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ElectionCandidate>(msg));
        }
        // Nomor urut, visi dan misi.
        public Response<ElectionCandidate> Update(string user)
        {
            return Run(SetRow, user);
        }
        // null = hapus foto.
        public Response<ElectionCandidate> UpdatePhoto(byte[]? photo, string user)
        {
            return Run((context, row, userId) => SetPhoto(context, row, photo, userId), user);
        }
        public Response<ElectionCandidate> Verify(string user)
        {
            return Run((context, row, userId) => SetStatus(context, row, CandidateStatus.Verified, userId), user);
        }
        public Response<ElectionCandidate> Reject(string user)
        {
            return Run((context, row, userId) => SetStatus(context, row, CandidateStatus.Rejected, userId), user);
        }
        public Response<ElectionCandidate> Withdraw(string user)
        {
            return Run((context, row, userId) => SetStatus(context, row, CandidateStatus.Withdrawn, userId), user);
        }
        public Response<ElectionCandidate> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                RemoveRow(context, row, userId);
                context.SaveChanges();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ElectionCandidate>(msg));
        }
        public PhotoContent? GetPhoto()
        {
            using var context = DBClass.GetContext();
            var photo = context.TElectionCandidates.Where(x => x.Id == Id).Select(x => x.Photo).SingleOrDefault();
            return photo == null ? null : new PhotoContent(photo, ImageUtility.GetContentType(photo));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ElectionCandidate>> GetDataListAsync(int electionId, CandidateStatus? status = null)
        {
            return await (await FilterQuery(electionId, status).ToListAsync()).MappingDataAsync<ElectionCandidate, EF.TElectionCandidate>();
        }
        public static async Task<List<ElectionCandidate>> GetBallotAsync(int electionId)
        {
            return await (await FilterQuery(electionId, CandidateStatus.Verified).ToListAsync()).MappingDataAsync<ElectionCandidate, EF.TElectionCandidate>();
        }
        public static async Task<Response<ElectionCandidate>> RegisterAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = SetRegister(context, newrow, userId);
                await context.SaveChangesAsync();
                return new ElectionCandidate(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ElectionCandidate>(msg));
        }
        public Task<Response<ElectionCandidate>> UpdateAsync(string user)
        {
            return RunAsync(SetRow, user);
        }
        public Task<Response<ElectionCandidate>> UpdatePhotoAsync(byte[]? photo, string user)
        {
            return RunAsync((context, row, userId) => SetPhoto(context, row, photo, userId), user);
        }
        public Task<Response<ElectionCandidate>> VerifyAsync(string user)
        {
            return RunAsync((context, row, userId) => SetStatus(context, row, CandidateStatus.Verified, userId), user);
        }
        public Task<Response<ElectionCandidate>> RejectAsync(string user)
        {
            return RunAsync((context, row, userId) => SetStatus(context, row, CandidateStatus.Rejected, userId), user);
        }
        public Task<Response<ElectionCandidate>> WithdrawAsync(string user)
        {
            return RunAsync((context, row, userId) => SetStatus(context, row, CandidateStatus.Withdrawn, userId), user);
        }
        public async Task<Response<ElectionCandidate>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                RemoveRow(context, row, userId);
                await context.SaveChangesAsync();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ElectionCandidate>(msg));
        }
        public async Task<PhotoContent?> GetPhotoAsync()
        {
            using var context = DBClass.GetContext();
            var photo = await context.TElectionCandidates.Where(x => x.Id == Id).Select(x => x.Photo).SingleOrDefaultAsync();
            return photo == null ? null : new PhotoContent(photo, ImageUtility.GetContentType(photo));
        }
        #endregion

        #region HELPER
        private delegate void RowAction(EF.ModelContext context, EF.TElectionCandidate row, int userId);

        private Response<ElectionCandidate> Run(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                return new ElectionCandidate(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ElectionCandidate>(msg));
        }
        private async Task<Response<ElectionCandidate>> RunAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                return new ElectionCandidate(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ElectionCandidate>(msg));
        }
        private static EF.TElectionCandidate Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TElectionCandidate> FilterQuery(int electionId, CandidateStatus? status)
        {
            return _Querys.Where(x => x.ElectionId == electionId && (status == null || x.Status == (int)status))
                .OrderBy(x => x.CandidateNumber == null).ThenBy(x => x.CandidateNumber).ThenBy(x => x.Member.Fullname);
        }
        private static EF.TElectionCandidate GetRow(EF.ModelContext context, int id)
        {
            return context.TElectionCandidates.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kandidat"));
        }
        private static async Task<EF.TElectionCandidate> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TElectionCandidates.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kandidat"));
        }
        private static int? ValidateNumber(EF.ModelContext context, int electionId, int id, int? number)
        {
            if (number == null || number <= 0)
            {
                return null;
            }
            ValidateUtility.CheckDuplicateData<EF.TElectionCandidate>(context, x => x.ElectionId == electionId && x.CandidateNumber == number && x.Id != id,
                ExceptionMessage.ELECTION_CANDIDATE_NUMBER_EXISTS);
            return number;
        }
        private static string GetMemberName(EF.ModelContext context, int memberId)
        {
            return context.MMembers.Where(x => x.Id == memberId).Select(x => x.Fullname).SingleOrDefault() ?? memberId.ToString();
        }
        private static EF.TElectionCandidate SetRegister(EF.ModelContext context, NewRow newrow, int userId)
        {
            var election = Election.GetEditableElection(context, newrow.ElectionId);
            var member = Member.GetActiveMember(context, newrow.MemberId);
            var row = context.TElectionCandidates.SingleOrDefault(x => x.ElectionId == election.Id && x.MemberId == member.Id);
            if (row != null)
            {
                if (row.Status != (int)CandidateStatus.Rejected && row.Status != (int)CandidateStatus.Withdrawn)
                {
                    throw new ArgumentException(ExceptionMessage.ELECTION_ALREADY_CANDIDATE);
                }
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
            }
            else
            {
                row = new EF.TElectionCandidate
                {
                    Id = SequenceUtility.GetNextId<EF.TElectionCandidate>(context),
                    ElectionId = election.Id,
                    MemberId = member.Id,
                    InsDate = DateTime.Now,
                    InsBy = userId
                };
                context.TElectionCandidates.Add(row);
            }
            row.CandidateNumber = ValidateNumber(context, election.Id, row.Id, newrow.CandidateNumber);
            row.Vision = TransaksiHelper.ToNullable(newrow.Vision);
            row.Mission = TransaksiHelper.ToNullable(newrow.Mission);
            row.Status = (int)CandidateStatus.Registered;
            if (newrow.Photo != null && newrow.Photo.Length > 0)
            {
                row.Photo = ImageUtility.Normalize(newrow.Photo).Data;
            }
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            ElectionAuditEvent.Add(context, election.Id, ElectionAuditEvent.EVENT_CANDIDATE_REGISTERED, member.Fullname, userId);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.TElectionCandidate row, int userId)
        {
            Election.GetEditableElection(context, row.ElectionId);
            if (row.Status == (int)CandidateStatus.Withdrawn || row.Status == (int)CandidateStatus.Rejected)
            {
                TransaksiHelper.ValidateStatus((CandidateStatus)row.Status, "mengubah kandidat", CandidateStatus.Registered, CandidateStatus.Verified);
            }
            row.CandidateNumber = ValidateNumber(context, row.ElectionId, row.Id, CandidateNumber);
            row.Vision = TransaksiHelper.ToNullable(Vision);
            row.Mission = TransaksiHelper.ToNullable(Mission);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetPhoto(EF.ModelContext context, EF.TElectionCandidate row, byte[]? photo, int userId)
        {
            Election.GetEditableElection(context, row.ElectionId);
            row.Photo = photo == null || photo.Length == 0 ? null : ImageUtility.Normalize(photo).Data;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetStatus(EF.ModelContext context, EF.TElectionCandidate row, CandidateStatus target, int userId)
        {
            Election.GetEditableElection(context, row.ElectionId);
            var current = (CandidateStatus)row.Status;
            string eventType;
            switch (target)
            {
                case CandidateStatus.Verified:
                    TransaksiHelper.ValidateStatus(current, "memverifikasi kandidat", CandidateStatus.Registered, CandidateStatus.Rejected);
                    if (row.CandidateNumber != null)
                    {
                        ValidateNumber(context, row.ElectionId, row.Id, row.CandidateNumber);
                    }
                    eventType = ElectionAuditEvent.EVENT_CANDIDATE_VERIFIED;
                    break;
                case CandidateStatus.Rejected:
                    TransaksiHelper.ValidateStatus(current, "menolak kandidat", CandidateStatus.Registered, CandidateStatus.Verified);
                    row.CandidateNumber = null;
                    eventType = ElectionAuditEvent.EVENT_CANDIDATE_REJECTED;
                    break;
                default:
                    TransaksiHelper.ValidateStatus(current, "mengundurkan kandidat", CandidateStatus.Registered, CandidateStatus.Verified);
                    row.CandidateNumber = null;
                    eventType = ElectionAuditEvent.EVENT_CANDIDATE_WITHDRAWN;
                    break;
            }
            row.Status = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            ElectionAuditEvent.Add(context, row.ElectionId, eventType, GetMemberName(context, row.MemberId), userId);
        }
        private static void RemoveRow(EF.ModelContext context, EF.TElectionCandidate row, int userId)
        {
            Election.GetEditableElection(context, row.ElectionId);
            ElectionAuditEvent.Add(context, row.ElectionId, ElectionAuditEvent.EVENT_CANDIDATE_DELETED, GetMemberName(context, row.MemberId), userId);
            context.TElectionCandidates.Remove(row);
        }
        #endregion
    }
}
