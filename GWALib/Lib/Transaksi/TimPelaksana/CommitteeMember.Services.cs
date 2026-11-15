using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Transaksi.Program;
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
    public partial class CommitteeMember
    {
        private const int JOB_DESC_LENGTH = 500;
        // Peran inti: tanpa sie dan hanya satu pemegang aktif per kegiatan.
        private static readonly CommitteeRole[] CORE_ROLES = { CommitteeRole.KetuaPelaksana, CommitteeRole.WakilKetua, CommitteeRole.Sekretaris, CommitteeRole.Bendahara };

        #region SYNC
        public static List<CommitteeMember> GetDataList(int? activityId = null, int? sectionId = null, int? memberId = null, CommitteeMemberStatus? status = null)
        {
            return FilterQuery(activityId, sectionId, memberId, status).ToList().MappingData<CommitteeMember, EF.TCommitteeMember>();
        }
        public static Response<CommitteeMember> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = AddRow(context, newrow, null, userId);
                context.SaveChanges();
                return new CommitteeMember(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<CommitteeMember>(msg));
        }
        public Response<CommitteeMember> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new CommitteeMember(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeMember>(msg));
        }
        public Response<CommitteeMember> Resign(string user)
        {
            return SetStatus(CommitteeMemberStatus.Mundur, user);
        }
        public Response<CommitteeMember> Remove(string user)
        {
            return SetStatus(CommitteeMemberStatus.Dikeluarkan, user);
        }
        #endregion

        #region ASYNC
        public static async Task<List<CommitteeMember>> GetDataListAsync(int? activityId = null, int? sectionId = null, int? memberId = null, CommitteeMemberStatus? status = null)
        {
            return await (await FilterQuery(activityId, sectionId, memberId, status).ToListAsync()).MappingDataAsync<CommitteeMember, EF.TCommitteeMember>();
        }
        public static async Task<Response<CommitteeMember>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = AddRow(context, newrow, null, userId);
                await context.SaveChangesAsync();
                return new CommitteeMember(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<CommitteeMember>(msg));
        }
        public async Task<Response<CommitteeMember>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new CommitteeMember(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeMember>(msg));
        }
        public async Task<Response<CommitteeMember>> ResignAsync(string user)
        {
            return await SetStatusAsync(CommitteeMemberStatus.Mundur, user);
        }
        public async Task<Response<CommitteeMember>> RemoveAsync(string user)
        {
            return await SetStatusAsync(CommitteeMemberStatus.Dikeluarkan, user);
        }
        #endregion

        #region INTERNAL
        // Tanpa SaveChanges. Anggota yang pernah mundur / dikeluarkan diaktifkan lagi; dipakai juga CommitteeApplication.Accept.
        internal static EF.TCommitteeMember AddRow(EF.ModelContext context, NewRow newrow, int? applicationId, int userId)
        {
            Activity.GetOpenActivity(context, newrow.ActivityId);
            Member.GetActiveMember(context, newrow.MemberId);
            var sectionId = ResolveSection(context, newrow.ActivityId, newrow.CommitteeRole, newrow.SectionId > 0 ? newrow.SectionId : null);
            var existing = context.TCommitteeMembers.SingleOrDefault(x => x.ActivityId == newrow.ActivityId && x.MemberId == newrow.MemberId);
            if (existing != null && existing.Status == (int)CommitteeMemberStatus.Aktif)
            {
                throw new ArgumentException(ExceptionMessage.COMMITTEE_ALREADY_MEMBER);
            }
            ValidateRoleHolder(context, existing?.Id ?? 0, newrow.ActivityId, newrow.CommitteeRole, sectionId);

            var row = existing ?? new EF.TCommitteeMember
            {
                Id = SequenceUtility.GetNextId<EF.TCommitteeMember>(context),
                ActivityId = newrow.ActivityId,
                MemberId = newrow.MemberId,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            row.SectionId = sectionId;
            row.CommitteeRole = (int)newrow.CommitteeRole;
            row.JobDesc = TransaksiHelper.Truncate(newrow.JobDesc, JOB_DESC_LENGTH);
            row.Status = (int)CommitteeMemberStatus.Aktif;
            row.ApplicationId = applicationId ?? row.ApplicationId;
            if (existing != null)
            {
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
            }
            else
            {
                context.TCommitteeMembers.Add(row);
            }
            SyncCoordinator(context, row, userId);
            return row;
        }
        #endregion

        #region HELPER
        private Response<CommitteeMember> SetStatus(CommitteeMemberStatus target, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ApplyStatus(context, row, target, userId);
                context.SaveChanges();
                return new CommitteeMember(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeMember>(msg));
        }
        private async Task<Response<CommitteeMember>> SetStatusAsync(CommitteeMemberStatus target, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ApplyStatus(context, row, target, userId);
                await context.SaveChangesAsync();
                return new CommitteeMember(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeMember>(msg));
        }
        private static EF.TCommitteeMember Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TCommitteeMember> FilterQuery(int? activityId, int? sectionId, int? memberId, CommitteeMemberStatus? status)
        {
            return _Querys.Where(x => (activityId == null || x.ActivityId == activityId)
                    && (sectionId == null || x.SectionId == sectionId)
                    && (memberId == null || x.MemberId == memberId)
                    && (status == null || x.Status == (int)status))
                .OrderBy(x => x.ActivityId).ThenBy(x => x.CommitteeRole).ThenBy(x => x.Section!.SeqNo).ThenBy(x => x.Member.Fullname);
        }
        private static EF.TCommitteeMember GetRow(EF.ModelContext context, int id)
        {
            return context.TCommitteeMembers.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Anggota Tim Pelaksana"));
        }
        private static async Task<EF.TCommitteeMember> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TCommitteeMembers.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Anggota Tim Pelaksana"));
        }
        // Peran inti tanpa sie; Koordinator Sie / Staf wajib sie; Official / Pelatih sie opsional.
        private static int? ResolveSection(EF.ModelContext context, int activityId, CommitteeRole role, int? sectionId)
        {
            if (!Enum.IsDefined(role))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Peran Tim Pelaksana"));
            }
            if (CORE_ROLES.Contains(role))
            {
                return null;
            }
            if (sectionId == null)
            {
                if (role == CommitteeRole.KoordinatorSie || role == CommitteeRole.Staf)
                {
                    throw new ArgumentException(ExceptionMessage.COMMITTEE_ROLE_SECTION_REQUIRED);
                }
                return null;
            }
            CommitteeSection.GetSection(context, sectionId.Value, activityId);
            return sectionId;
        }
        // Peran inti satu per kegiatan; Koordinator Sie satu per sie (hanya yang Aktif).
        private static void ValidateRoleHolder(EF.ModelContext context, int id, int activityId, CommitteeRole role, int? sectionId)
        {
            if (CORE_ROLES.Contains(role) || role == CommitteeRole.KoordinatorSie)
            {
                ValidateUtility.CheckDuplicateData<EF.TCommitteeMember>(context,
                    x => x.ActivityId == activityId && x.CommitteeRole == (int)role && x.SectionId == sectionId
                        && x.Status == (int)CommitteeMemberStatus.Aktif && x.Id != id,
                    string.Format(ExceptionMessage.COMMITTEE_ROLE_TAKEN, role.GetDescription()));
            }
        }
        // CommitteeSection.CoordinatorMemberId mengikuti Koordinator Sie yang aktif.
        private static void SyncCoordinator(EF.ModelContext context, EF.TCommitteeMember row, int userId)
        {
            var sections = context.TCommitteeSections.Where(x => x.ActivityId == row.ActivityId
                && (x.CoordinatorMemberId == row.MemberId || x.Id == row.SectionId)).ToList();
            var isCoordinator = row.Status == (int)CommitteeMemberStatus.Aktif && row.CommitteeRole == (int)CommitteeRole.KoordinatorSie;
            foreach (var section in sections)
            {
                int? coordinator = section.CoordinatorMemberId;
                if (section.Id == row.SectionId && isCoordinator)
                {
                    coordinator = row.MemberId;
                }
                else if (section.CoordinatorMemberId == row.MemberId)
                {
                    coordinator = null;
                }
                if (coordinator != section.CoordinatorMemberId)
                {
                    section.CoordinatorMemberId = coordinator;
                    section.UpdDate = DateTime.Now;
                    section.UpdBy = userId;
                }
            }
        }
        // Kegiatan dan anggota tetap; hanya anggota Aktif yang bisa diubah.
        private void SetRow(EF.ModelContext context, EF.TCommitteeMember row, int userId)
        {
            TransaksiHelper.ValidateStatus((CommitteeMemberStatus)row.Status, "mengubah anggota tim pelaksana", CommitteeMemberStatus.Aktif);
            Activity.GetOpenActivity(context, row.ActivityId);
            var sectionId = ResolveSection(context, row.ActivityId, CommitteeRole, SectionId > 0 ? SectionId : null);
            ValidateRoleHolder(context, row.Id, row.ActivityId, CommitteeRole, sectionId);

            row.SectionId = sectionId;
            row.CommitteeRole = (int)CommitteeRole;
            row.JobDesc = TransaksiHelper.Truncate(JobDesc, JOB_DESC_LENGTH);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            SyncCoordinator(context, row, userId);
        }
        private static void ApplyStatus(EF.ModelContext context, EF.TCommitteeMember row, CommitteeMemberStatus target, int userId)
        {
            TransaksiHelper.ValidateStatus((CommitteeMemberStatus)row.Status, "mengubah status anggota tim pelaksana", CommitteeMemberStatus.Aktif);
            row.Status = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            SyncCoordinator(context, row, userId);
        }
        #endregion
    }
}
