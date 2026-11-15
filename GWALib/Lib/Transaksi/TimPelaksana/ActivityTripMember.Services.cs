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
    public partial class ActivityTripMember
    {
        public const int MAX_FILE_BYTES = 5 * 1024 * 1024;
        private const int NOTES_LENGTH = 500;

        #region SYNC
        public FileContent? GetPermissionFile()
        {
            using var context = DBClass.GetContext();
            var file = context.TActivityTripMembers.Where(x => x.Id == Id)
                .Select(x => new { x.PermissionFile, x.PermissionFileName, x.PermissionContentType }).SingleOrDefault();
            return file?.PermissionFile == null ? null
                : new FileContent(file.PermissionFile, file.PermissionFileName ?? "izin", file.PermissionContentType ?? "application/octet-stream");
        }
        public static List<ActivityTripMember> GetDataList(int activityId, TripRole? tripRole = null, TripPermissionStatus? permissionStatus = null)
        {
            return FilterQuery(activityId, tripRole, permissionStatus).ToList().MappingData<ActivityTripMember, EF.TActivityTripMember>();
        }
        public static Response<ActivityTripMember> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TActivityTripMembers.Add(row);
                context.SaveChanges();
                return new ActivityTripMember(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivityTripMember>(msg));
        }
        public Response<ActivityTripMember> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ActivityTripMember(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityTripMember>(msg));
        }
        // file null / kosong = file izin dihapus (status izin kembali Belum).
        public Response<ActivityTripMember> UpdatePermissionFile(byte[]? file, string? fileName, string? contentType, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetFile(row, file, fileName, contentType, userId);
                context.SaveChanges();
                return new ActivityTripMember(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityTripMember>(msg));
        }
        public Response<ActivityTripMember> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                Activity.GetOpenActivity(context, row.ActivityId);
                var data = new ActivityTripMember(Reload(row.Id));
                context.TActivityTripMembers.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ActivityTripMember>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetPermissionFileAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TActivityTripMembers.Where(x => x.Id == Id)
                .Select(x => new { x.PermissionFile, x.PermissionFileName, x.PermissionContentType }).SingleOrDefaultAsync();
            return file?.PermissionFile == null ? null
                : new FileContent(file.PermissionFile, file.PermissionFileName ?? "izin", file.PermissionContentType ?? "application/octet-stream");
        }
        public static async Task<List<ActivityTripMember>> GetDataListAsync(int activityId, TripRole? tripRole = null, TripPermissionStatus? permissionStatus = null)
        {
            return await (await FilterQuery(activityId, tripRole, permissionStatus).ToListAsync()).MappingDataAsync<ActivityTripMember, EF.TActivityTripMember>();
        }
        public static async Task<Response<ActivityTripMember>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TActivityTripMembers.Add(row);
                await context.SaveChangesAsync();
                return new ActivityTripMember(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ActivityTripMember>(msg));
        }
        public async Task<Response<ActivityTripMember>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ActivityTripMember(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityTripMember>(msg));
        }
        public async Task<Response<ActivityTripMember>> UpdatePermissionFileAsync(byte[]? file, string? fileName, string? contentType, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetFile(row, file, fileName, contentType, userId);
                await context.SaveChangesAsync();
                return new ActivityTripMember(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ActivityTripMember>(msg));
        }
        public async Task<Response<ActivityTripMember>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                Activity.GetOpenActivity(context, row.ActivityId);
                var data = new ActivityTripMember(Reload(row.Id));
                context.TActivityTripMembers.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ActivityTripMember>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TActivityTripMember Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TActivityTripMember> FilterQuery(int activityId, TripRole? tripRole, TripPermissionStatus? permissionStatus)
        {
            return _Querys.Where(x => x.ActivityId == activityId
                    && (tripRole == null || x.TripRole == (int)tripRole)
                    && (permissionStatus == null || x.PermissionStatus == (int)permissionStatus))
                .OrderBy(x => x.TripRole).ThenBy(x => x.Vehicle).ThenBy(x => x.Id);
        }
        private static EF.TActivityTripMember GetRow(EF.ModelContext context, int id)
        {
            return context.TActivityTripMembers.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Anggota Rombongan"));
        }
        private static async Task<EF.TActivityTripMember> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TActivityTripMembers.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Anggota Rombongan"));
        }
        private static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        private static void ValidateData(TripRole tripRole, TripPermissionStatus permissionStatus)
        {
            if (!Enum.IsDefined(tripRole) || !Enum.IsDefined(permissionStatus))
            {
                throw new ArgumentException(ExceptionMessage.DATA_INVALID);
            }
        }
        private static void Apply(EF.TActivityTripMember row, string? vehicle, string? room, string? contactName, string? contactPhone, string? notes)
        {
            row.Vehicle = ToNullable(vehicle);
            row.Room = ToNullable(room);
            row.EmergencyContactName = ToNullable(contactName);
            row.EmergencyContactPhone = ToNullable(contactPhone);
            row.Notes = TransaksiHelper.Truncate(notes, NOTES_LENGTH);
        }
        // Anggota (unik per kegiatan) atau tamu; nama tamu diabaikan bila anggota diisi.
        private static EF.TActivityTripMember CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            Activity.GetOpenActivity(context, newrow.ActivityId);
            var memberId = newrow.MemberId > 0 ? newrow.MemberId : null;
            var guestName = memberId == null ? ToNullable(newrow.GuestName) : null;
            if (memberId == null && guestName == null)
            {
                throw new ArgumentException(ExceptionMessage.TRIP_MEMBER_REQUIRED);
            }
            if (memberId != null)
            {
                Member.GetActiveMember(context, memberId.Value);
                ValidateUtility.CheckDuplicateData<EF.TActivityTripMember>(context, x => x.ActivityId == newrow.ActivityId && x.MemberId == memberId,
                    string.Format(ExceptionMessage.DATA_DUPLICATE, "Anggota pada rombongan"));
            }
            ValidateData(newrow.TripRole, newrow.PermissionStatus);

            var row = new EF.TActivityTripMember
            {
                Id = SequenceUtility.GetNextId<EF.TActivityTripMember>(context),
                ActivityId = newrow.ActivityId,
                MemberId = memberId,
                GuestName = guestName,
                TripRole = (int)newrow.TripRole,
                PermissionStatus = (int)newrow.PermissionStatus,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            Apply(row, newrow.Vehicle, newrow.Room, newrow.EmergencyContactName, newrow.EmergencyContactPhone, newrow.Notes);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Kegiatan dan anggota tetap; tamu boleh ganti nama.
        private void SetRow(EF.ModelContext context, EF.TActivityTripMember row, int userId)
        {
            Activity.GetOpenActivity(context, row.ActivityId);
            ValidateData(TripRole, PermissionStatus);
            if (row.MemberId == null)
            {
                row.GuestName = ToNullable(GuestName) ?? throw new ArgumentException(ExceptionMessage.TRIP_MEMBER_REQUIRED);
            }
            row.TripRole = (int)TripRole;
            row.PermissionStatus = (int)PermissionStatus;
            Apply(row, Vehicle, Room, EmergencyContactName, EmergencyContactPhone, Notes);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetFile(EF.TActivityTripMember row, byte[]? file, string? fileName, string? contentType, int userId)
        {
            if (file == null || file.Length == 0)
            {
                row.PermissionFile = null;
                row.PermissionFileName = null;
                row.PermissionContentType = null;
                if (row.PermissionStatus == (int)TripPermissionStatus.Ada)
                {
                    row.PermissionStatus = (int)TripPermissionStatus.Belum;
                }
            }
            else
            {
                if (file.Length > MAX_FILE_BYTES)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FILE_TOO_LARGE, "File Izin", MAX_FILE_BYTES / 1024 / 1024));
                }
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama File Izin"));
                }
                row.PermissionFile = file;
                row.PermissionFileName = Path.GetFileName(fileName.Trim());
                row.PermissionContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType.Trim();
                row.PermissionStatus = (int)TripPermissionStatus.Ada;
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        #endregion
    }
}
