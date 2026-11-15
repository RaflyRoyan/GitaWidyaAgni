using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Transaksi.Program;
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
    public partial class CommitteeSection
    {
        #region SYNC
        public static List<CommitteeSection> GetDataList(int activityId)
        {
            return FilterQuery(activityId).ToList().MappingData<CommitteeSection, EF.TCommitteeSection>();
        }
        public static Response<CommitteeSection> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TCommitteeSections.Add(row);
                context.SaveChanges();
                return new CommitteeSection(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<CommitteeSection>(msg));
        }
        public Response<CommitteeSection> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new CommitteeSection(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeSection>(msg));
        }
        public Response<CommitteeSection> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateDelete(context, row);
                var data = new CommitteeSection(Reload(row.Id));
                context.TCommitteeSections.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<CommitteeSection>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<CommitteeSection>> GetDataListAsync(int activityId)
        {
            return await (await FilterQuery(activityId).ToListAsync()).MappingDataAsync<CommitteeSection, EF.TCommitteeSection>();
        }
        public static async Task<Response<CommitteeSection>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TCommitteeSections.Add(row);
                await context.SaveChangesAsync();
                return new CommitteeSection(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<CommitteeSection>(msg));
        }
        public async Task<Response<CommitteeSection>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new CommitteeSection(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<CommitteeSection>(msg));
        }
        public async Task<Response<CommitteeSection>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateDelete(context, row);
                var data = new CommitteeSection(Reload(row.Id));
                context.TCommitteeSections.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<CommitteeSection>(msg));
        }
        #endregion

        #region INTERNAL
        // Sie harus milik kegiatan yang dimaksud; dipakai anggota tim pelaksana, lamaran, rundown, checklist.
        internal static EF.TCommitteeSection GetSection(EF.ModelContext context, int sectionId, int activityId)
        {
            var row = context.TCommitteeSections.SingleOrDefault(x => x.Id == sectionId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sie"));
            if (row.ActivityId != activityId)
            {
                throw new ArgumentException(ExceptionMessage.COMMITTEE_SECTION_MISMATCH);
            }
            return row;
        }
        #endregion

        #region HELPER
        private static EF.TCommitteeSection Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TCommitteeSection> FilterQuery(int activityId)
        {
            return _Querys.Where(x => x.ActivityId == activityId).OrderBy(x => x.SeqNo).ThenBy(x => x.Name);
        }
        private static EF.TCommitteeSection GetRow(EF.ModelContext context, int id)
        {
            return context.TCommitteeSections.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sie"));
        }
        private static async Task<EF.TCommitteeSection> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TCommitteeSections.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sie"));
        }
        private static void ValidateData(EF.ModelContext context, int id, int activityId, string name, int seqNo)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Sie"));
            }
            if (seqNo <= 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Urutan Sie"));
            }
            ValidateUtility.CheckDuplicateData<EF.TCommitteeSection>(context, x => x.ActivityId == activityId && x.Name == name && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Sie"));
        }
        private static void ValidateDelete(EF.ModelContext context, EF.TCommitteeSection row)
        {
            Activity.GetOpenActivity(context, row.ActivityId);
            if (context.TCommitteeMembers.Any(x => x.SectionId == row.Id) || context.TActivityRundowns.Any(x => x.SectionId == row.Id)
                || context.TActivityChecklists.Any(x => x.SectionId == row.Id)
                || context.TCommitteeApplications.Any(x => x.SectionChoice1Id == row.Id || x.SectionChoice2Id == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.COMMITTEE_SECTION_IN_USE);
            }
        }
        private static EF.TCommitteeSection CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            Activity.GetOpenActivity(context, newrow.ActivityId);
            var name = newrow.Name?.Trim() ?? string.Empty;
            var seqNo = newrow.SeqNo > 0 ? newrow.SeqNo : ValueUtility.GetNextId<EF.TCommitteeSection>(context, x => x.SeqNo, x => x.ActivityId == newrow.ActivityId);
            ValidateData(context, 0, newrow.ActivityId, name, seqNo);

            var row = new EF.TCommitteeSection
            {
                Id = SequenceUtility.GetNextId<EF.TCommitteeSection>(context),
                ActivityId = newrow.ActivityId,
                Name = name,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                SeqNo = seqNo,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Kegiatan tetap; koordinator diatur lewat CommitteeMember.
        private void SetRow(EF.ModelContext context, EF.TCommitteeSection row, int userId)
        {
            Activity.GetOpenActivity(context, row.ActivityId);
            var name = Name?.Trim() ?? string.Empty;
            var seqNo = SeqNo > 0 ? SeqNo : ValueUtility.GetNextId<EF.TCommitteeSection>(context, x => x.SeqNo, x => x.ActivityId == row.ActivityId);
            ValidateData(context, row.Id, row.ActivityId, name, seqNo);

            row.Name = name;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.SeqNo = seqNo;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        #endregion
    }
}
