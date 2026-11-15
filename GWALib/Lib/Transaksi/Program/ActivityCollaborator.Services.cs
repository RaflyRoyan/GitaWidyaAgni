using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Mitra;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Program
{
    public partial class ActivityCollaborator
    {
        private const int NOTES_LENGTH = 255;

        #region SYNC
        public static List<ActivityCollaborator> GetDataList(int? activityId = null, int? externalPartyId = null, CollaboratorStatus? status = null)
        {
            return FilterQuery(activityId, externalPartyId, status).ToList().MappingData<ActivityCollaborator, EF.TActivityCollaborator>();
        }
        public static Response<ActivityCollaborator> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TActivityCollaborators.Add(row);
                context.SaveChanges();
                return new ActivityCollaborator(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivityCollaborator>(msg));
        }
        public Response<ActivityCollaborator> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(row, userId);
                context.SaveChanges();
                return new ActivityCollaborator(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityCollaborator>(msg));
        }
        // Mencatat jawaban mitra: Diterima atau Ditolak.
        public Response<ActivityCollaborator> Respond(CollaboratorStatus status, string? notes, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRespond(row, status, notes, userId);
                context.SaveChanges();
                return new ActivityCollaborator(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityCollaborator>(msg));
        }
        public Response<ActivityCollaborator> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                Activity.GetOpenActivity(context, row.ActivityId);
                var data = new ActivityCollaborator(Reload(row.Id));
                context.TActivityCollaborators.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ActivityCollaborator>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<ActivityCollaborator>> GetDataListAsync(int? activityId = null, int? externalPartyId = null, CollaboratorStatus? status = null)
        {
            return await (await FilterQuery(activityId, externalPartyId, status).ToListAsync()).MappingDataAsync<ActivityCollaborator, EF.TActivityCollaborator>();
        }
        public static async Task<Response<ActivityCollaborator>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TActivityCollaborators.Add(row);
                await context.SaveChangesAsync();
                return new ActivityCollaborator(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivityCollaborator>(msg));
        }
        public async Task<Response<ActivityCollaborator>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(row, userId);
                await context.SaveChangesAsync();
                return new ActivityCollaborator(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityCollaborator>(msg));
        }
        public async Task<Response<ActivityCollaborator>> RespondAsync(CollaboratorStatus status, string? notes, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRespond(row, status, notes, userId);
                await context.SaveChangesAsync();
                return new ActivityCollaborator(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityCollaborator>(msg));
        }
        public async Task<Response<ActivityCollaborator>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                Activity.GetOpenActivity(context, row.ActivityId);
                var data = new ActivityCollaborator(Reload(row.Id));
                context.TActivityCollaborators.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ActivityCollaborator>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TActivityCollaborator Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TActivityCollaborator> FilterQuery(int? activityId, int? externalPartyId, CollaboratorStatus? status)
        {
            return _Querys.Where(x => (activityId == null || x.ActivityId == activityId)
                    && (externalPartyId == null || x.ExternalPartyId == externalPartyId)
                    && (status == null || x.Status == (int)status))
                .OrderBy(x => x.ActivityId).ThenBy(x => x.ExternalParty.Name);
        }
        private static EF.TActivityCollaborator GetRow(EF.ModelContext context, int id)
        {
            return context.TActivityCollaborators.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kolaborator Kegiatan"));
        }
        private static async Task<EF.TActivityCollaborator> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TActivityCollaborators.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kolaborator Kegiatan"));
        }
        private static void ValidateRole(CollaboratorRole collabRole)
        {
            if (!Enum.IsDefined(collabRole))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Peran Kolaborator"));
            }
        }
        private static EF.TActivityCollaborator CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            Activity.GetOpenActivity(context, newrow.ActivityId);
            ExternalParty.GetActiveExternalParty(context, newrow.ExternalPartyId);
            ValidateRole(newrow.CollabRole);
            ValidateUtility.CheckDuplicateData<EF.TActivityCollaborator>(context,
                x => x.ActivityId == newrow.ActivityId && x.ExternalPartyId == newrow.ExternalPartyId,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Mitra pada kegiatan"));

            var row = new EF.TActivityCollaborator
            {
                Id = SequenceUtility.GetNextId<EF.TActivityCollaborator>(context),
                ActivityId = newrow.ActivityId,
                ExternalPartyId = newrow.ExternalPartyId,
                CollabRole = (int)newrow.CollabRole,
                Status = (int)CollaboratorStatus.Diundang,
                Notes = TransaksiHelper.Truncate(newrow.Notes, NOTES_LENGTH),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Kegiatan dan mitra tetap.
        private void SetRow(EF.TActivityCollaborator row, int userId)
        {
            ValidateRole(CollabRole);
            row.CollabRole = (int)CollabRole;
            row.Notes = TransaksiHelper.Truncate(Notes, NOTES_LENGTH);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetRespond(EF.TActivityCollaborator row, CollaboratorStatus status, string? notes, int userId)
        {
            TransaksiHelper.ValidateStatus((CollaboratorStatus)row.Status, "mencatat jawaban mitra", CollaboratorStatus.Diundang);
            if (status != CollaboratorStatus.Diterima && status != CollaboratorStatus.Ditolak)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jawaban Mitra"));
            }
            row.Status = (int)status;
            row.Notes = TransaksiHelper.Truncate(notes, NOTES_LENGTH) ?? row.Notes;
            row.RespondedBy = userId;
            row.RespondedAt = DateTime.Now;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
