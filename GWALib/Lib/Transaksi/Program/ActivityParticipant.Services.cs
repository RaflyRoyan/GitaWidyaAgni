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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Program
{
    public partial class ActivityParticipant
    {
        #region SYNC
        public static List<ActivityParticipant> GetDataList(int? activityId = null, int? memberId = null, RegistrationStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(activityId, memberId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ActivityParticipant, EF.TActivityParticipant>();
        }
        // Mendaftarkan anggota; bila sebelumnya Cancelled, baris lama diaktifkan lagi.
        public static Response<ActivityParticipant> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = Register(context, newrow, userId);
                context.SaveChanges();
                return new ActivityParticipant(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivityParticipant>(msg));
        }
        public Response<ActivityParticipant> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ActivityParticipant(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityParticipant>(msg));
        }
        public Response<ActivityParticipant> Confirm(string user)
        {
            return SetStatus(RegistrationStatus.Confirmed, user);
        }
        public Response<ActivityParticipant> Cancel(string user)
        {
            return SetStatus(RegistrationStatus.Cancelled, user);
        }
        #endregion

        #region ASYNC
        public static async Task<List<ActivityParticipant>> GetDataListAsync(int? activityId = null, int? memberId = null, RegistrationStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(activityId, memberId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ActivityParticipant, EF.TActivityParticipant>();
        }
        public static async Task<Response<ActivityParticipant>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = Register(context, newrow, userId);
                await context.SaveChangesAsync();
                return new ActivityParticipant(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivityParticipant>(msg));
        }
        public async Task<Response<ActivityParticipant>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ActivityParticipant(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityParticipant>(msg));
        }
        public async Task<Response<ActivityParticipant>> ConfirmAsync(string user)
        {
            return await SetStatusAsync(RegistrationStatus.Confirmed, user);
        }
        public async Task<Response<ActivityParticipant>> CancelAsync(string user)
        {
            return await SetStatusAsync(RegistrationStatus.Cancelled, user);
        }
        #endregion

        #region HELPER
        private Response<ActivityParticipant> SetStatus(RegistrationStatus target, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ApplyStatus(context, row, target, userId);
                context.SaveChanges();
                return new ActivityParticipant(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityParticipant>(msg));
        }
        private async Task<Response<ActivityParticipant>> SetStatusAsync(RegistrationStatus target, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ApplyStatus(context, row, target, userId);
                await context.SaveChangesAsync();
                return new ActivityParticipant(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityParticipant>(msg));
        }
        private static EF.TActivityParticipant Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TActivityParticipant> FilterQuery(int? activityId, int? memberId, RegistrationStatus? status)
        {
            return _Querys.Where(x => (activityId == null || x.ActivityId == activityId)
                    && (memberId == null || x.MemberId == memberId)
                    && (status == null || x.RegistrationStatus == (int)status))
                .OrderBy(x => x.ActivityId).ThenBy(x => x.ParticipantRole).ThenBy(x => x.Member.Fullname);
        }
        private static EF.TActivityParticipant GetRow(EF.ModelContext context, int id)
        {
            return context.TActivityParticipants.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Peserta Kegiatan"));
        }
        private static async Task<EF.TActivityParticipant> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TActivityParticipants.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Peserta Kegiatan"));
        }
        private static string ToRoleCode(string? value)
        {
            var code = Regex.Replace(value?.Trim().ToUpperInvariant() ?? string.Empty, "[^A-Z0-9]+", "_").Trim('_');
            return code.Length == 0 ? DEFAULT_ROLE : code;
        }
        // Tanpa SaveChanges. Anggota yang sudah terdaftar aktif ditolak; yang Cancelled diaktifkan lagi.
        private static EF.TActivityParticipant Register(EF.ModelContext context, NewRow newrow, int userId)
        {
            Activity.GetOpenActivity(context, newrow.ActivityId);
            Member.GetActiveMember(context, newrow.MemberId);
            var existing = context.TActivityParticipants.SingleOrDefault(x => x.ActivityId == newrow.ActivityId && x.MemberId == newrow.MemberId);
            if (existing != null)
            {
                if (existing.RegistrationStatus != (int)RegistrationStatus.Cancelled)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.DATA_DUPLICATE, "Peserta pada kegiatan"));
                }
                existing.ParticipantRole = ToRoleCode(newrow.ParticipantRole);
                existing.RegistrationStatus = (int)RegistrationStatus.Registered;
                existing.UpdDate = DateTime.Now;
                existing.UpdBy = userId;
                return existing;
            }

            var row = new EF.TActivityParticipant
            {
                Id = SequenceUtility.GetNextId<EF.TActivityParticipant>(context),
                ActivityId = newrow.ActivityId,
                MemberId = newrow.MemberId,
                ParticipantRole = ToRoleCode(newrow.ParticipantRole),
                RegistrationStatus = (int)RegistrationStatus.Registered,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TActivityParticipants.Add(row);
            return row;
        }
        // Hanya peran yang bisa diubah, selama kegiatan masih berjalan.
        private void SetRow(EF.ModelContext context, EF.TActivityParticipant row, int userId)
        {
            Activity.GetOpenActivity(context, row.ActivityId);
            row.ParticipantRole = ToRoleCode(ParticipantRole);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void ApplyStatus(EF.ModelContext context, EF.TActivityParticipant row, RegistrationStatus target, int userId)
        {
            Activity.GetOpenActivity(context, row.ActivityId);
            var current = (RegistrationStatus)row.RegistrationStatus;
            if (target == RegistrationStatus.Confirmed)
            {
                TransaksiHelper.ValidateStatus(current, "mengonfirmasi peserta", RegistrationStatus.Registered);
            }
            else if (target == RegistrationStatus.Cancelled)
            {
                TransaksiHelper.ValidateStatus(current, "membatalkan peserta", RegistrationStatus.Registered, RegistrationStatus.Confirmed);
            }
            else
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Pendaftaran"));
            }
            row.RegistrationStatus = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
