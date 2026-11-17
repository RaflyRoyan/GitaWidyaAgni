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

namespace GWALib.Lib.Transaksi.TimPelaksana
{
    public partial class CommitteeApplication
    {
        private const int NOTE_LENGTH = 500;

        #region SYNC
        public static List<CommitteeApplication> GetDataList(int? recruitmentId = null, int? memberId = null, CommitteeApplicationStatus? status = null)
        {
            return FilterQuery(recruitmentId, memberId, status).ToList().MappingData<CommitteeApplication, EF.TCommitteeApplication>();
        }
        public static Response<CommitteeApplication> Apply(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = AddRow(context, newrow, userId);
                context.SaveChanges();
                return new CommitteeApplication(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<CommitteeApplication>(msg));
        }
        // Diterima: pelamar masuk tim pelaksana di sectionId (null = pilihan 1) dengan peran Staf.
        public Response<CommitteeApplication> Accept(int? sectionId, string? note, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetAccept(context, row, sectionId, note, userId);
                context.SaveChanges();
                return new CommitteeApplication(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeApplication>(msg));
        }
        public Response<CommitteeApplication> Reject(string? note, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetFinal(row, CommitteeApplicationStatus.Ditolak, note, userId, true);
                NotifyResult(context, row, false, note, userId);
                context.SaveChanges();
                return new CommitteeApplication(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeApplication>(msg));
        }
        // Pelamar menarik lamaran sebelum dinilai.
        public Response<CommitteeApplication> Withdraw(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetFinal(row, CommitteeApplicationStatus.Mundur, null, userId, false);
                context.SaveChanges();
                return new CommitteeApplication(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeApplication>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<CommitteeApplication>> GetDataListAsync(int? recruitmentId = null, int? memberId = null, CommitteeApplicationStatus? status = null)
        {
            return await (await FilterQuery(recruitmentId, memberId, status).ToListAsync()).MappingDataAsync<CommitteeApplication, EF.TCommitteeApplication>();
        }
        public static async Task<Response<CommitteeApplication>> ApplyAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = AddRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new CommitteeApplication(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<CommitteeApplication>(msg));
        }
        public async Task<Response<CommitteeApplication>> AcceptAsync(int? sectionId, string? note, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetAccept(context, row, sectionId, note, userId);
                await context.SaveChangesAsync();
                return new CommitteeApplication(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeApplication>(msg));
        }
        public async Task<Response<CommitteeApplication>> RejectAsync(string? note, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetFinal(row, CommitteeApplicationStatus.Ditolak, note, userId, true);
                NotifyResult(context, row, false, note, userId);
                await context.SaveChangesAsync();
                return new CommitteeApplication(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeApplication>(msg));
        }
        public async Task<Response<CommitteeApplication>> WithdrawAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetFinal(row, CommitteeApplicationStatus.Mundur, null, userId, false);
                await context.SaveChangesAsync();
                return new CommitteeApplication(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeApplication>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TCommitteeApplication Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TCommitteeApplication> FilterQuery(int? recruitmentId, int? memberId, CommitteeApplicationStatus? status)
        {
            return _Querys.Where(x => (recruitmentId == null || x.RecruitmentId == recruitmentId)
                    && (memberId == null || x.MemberId == memberId)
                    && (status == null || x.Status == (int)status))
                .OrderBy(x => x.InsDate).ThenBy(x => x.Id);
        }
        private static EF.TCommitteeApplication GetRow(EF.ModelContext context, int id)
        {
            return context.TCommitteeApplications.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Lamaran Tim Pelaksana"));
        }
        private static async Task<EF.TCommitteeApplication> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TCommitteeApplications.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Lamaran Tim Pelaksana"));
        }
        // Tanpa SaveChanges. Lamaran yang pernah Mundur diaktifkan lagi; anggota yang sudah tim pelaksana aktif ditolak.
        private static EF.TCommitteeApplication AddRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var recruitment = CommitteeRecruitment.GetOpenRecruitment(context, newrow.RecruitmentId);
            Member.GetActiveMember(context, newrow.MemberId);
            CommitteeSection.GetSection(context, newrow.SectionChoice1Id, recruitment.ActivityId);
            var choice2 = newrow.SectionChoice2Id > 0 && newrow.SectionChoice2Id != newrow.SectionChoice1Id ? newrow.SectionChoice2Id : null;
            if (choice2 != null)
            {
                CommitteeSection.GetSection(context, choice2.Value, recruitment.ActivityId);
            }
            if (context.TCommitteeMembers.Any(x => x.ActivityId == recruitment.ActivityId && x.MemberId == newrow.MemberId && x.Status == (int)CommitteeMemberStatus.Aktif))
            {
                throw new ArgumentException(ExceptionMessage.COMMITTEE_ALREADY_MEMBER);
            }
            var formResponseId = newrow.FormResponseId > 0 ? newrow.FormResponseId : null;
            if (formResponseId != null)
            {
                var response = context.TFormResponses.Where(x => x.Id == formResponseId).Select(x => new { x.FormId, x.MemberId }).SingleOrDefault()
                    ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Jawaban Formulir"));
                // Jawaban harus dari formulir rekrutmen ini dan milik anggota yang melamar.
                if (response.FormId != recruitment.FormId || response.MemberId != newrow.MemberId)
                {
                    throw new ArgumentException(ExceptionMessage.FORM_RESPONSE_MISMATCH);
                }
            }
            var existing = context.TCommitteeApplications.SingleOrDefault(x => x.RecruitmentId == newrow.RecruitmentId && x.MemberId == newrow.MemberId);
            if (existing != null && existing.Status != (int)CommitteeApplicationStatus.Mundur)
            {
                throw new ArgumentException(ExceptionMessage.COMMITTEE_ALREADY_APPLIED);
            }

            var row = existing ?? new EF.TCommitteeApplication
            {
                Id = SequenceUtility.GetNextId<EF.TCommitteeApplication>(context),
                RecruitmentId = newrow.RecruitmentId,
                MemberId = newrow.MemberId,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            row.SectionChoice1Id = newrow.SectionChoice1Id;
            row.SectionChoice2Id = choice2;
            row.Reason = string.IsNullOrWhiteSpace(newrow.Reason) ? null : newrow.Reason.Trim();
            row.FormResponseId = formResponseId;
            row.Status = (int)CommitteeApplicationStatus.Daftar;
            row.ReviewedBy = null;
            row.ReviewedAt = null;
            row.ReviewNote = null;
            if (existing != null)
            {
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
            }
            else
            {
                context.TCommitteeApplications.Add(row);
            }
            return row;
        }
        private static void SetAccept(EF.ModelContext context, EF.TCommitteeApplication row, int? sectionId, string? note, int userId)
        {
            TransaksiHelper.ValidateStatus((CommitteeApplicationStatus)row.Status, "menerima lamaran", CommitteeApplicationStatus.Daftar);
            var recruitment = context.TCommitteeRecruitments.Single(x => x.Id == row.RecruitmentId);
            if (recruitment.Quota != null && context.TCommitteeApplications.Count(x => x.RecruitmentId == row.RecruitmentId
                && x.Status == (int)CommitteeApplicationStatus.Diterima) >= recruitment.Quota)
            {
                throw new ArgumentException(ExceptionMessage.RECRUITMENT_QUOTA_FULL);
            }
            CommitteeMember.AddRow(context, new CommitteeMember.NewRow
            {
                ActivityId = recruitment.ActivityId,
                MemberId = row.MemberId,
                SectionId = sectionId > 0 ? sectionId : row.SectionChoice1Id,
                CommitteeRole = CommitteeRole.Staf
            }, row.Id, userId);
            SetFinal(row, CommitteeApplicationStatus.Diterima, note, userId, true);
            NotifyResult(context, row, true, note, userId);
        }
        private static void SetFinal(EF.TCommitteeApplication row, CommitteeApplicationStatus target, string? note, int userId, bool isReview)
        {
            TransaksiHelper.ValidateStatus((CommitteeApplicationStatus)row.Status, "mengubah lamaran", CommitteeApplicationStatus.Daftar);
            row.Status = (int)target;
            if (isReview)
            {
                row.ReviewedBy = userId;
                row.ReviewedAt = DateTime.Now;
                row.ReviewNote = TransaksiHelper.Truncate(note, NOTE_LENGTH);
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void NotifyResult(EF.ModelContext context, EF.TCommitteeApplication row, bool accepted, string? note, int userId)
        {
            var activity = context.TCommitteeRecruitments.Where(x => x.Id == row.RecruitmentId).Select(x => x.Activity.Title).FirstOrDefault();
            NotifyHelper.ToMember(context, row.MemberId, NotifyHelper.TYPE_COMMITTEE,
                (accepted ? "Lamaran panitia diterima: " : "Lamaran panitia belum diterima: ") + activity, note,
                NotifyHelper.LINK_MY_ACTIVITY, "t_committee_application", row.Id, userId);
        }
        #endregion
    }
}
