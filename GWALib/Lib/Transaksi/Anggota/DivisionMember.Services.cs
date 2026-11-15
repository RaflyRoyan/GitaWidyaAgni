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

namespace GWALib.Lib.Transaksi.Anggota
{
    public partial class DivisionMember
    {
        #region SYNC
        public static List<DivisionMember> GetDataList(FilterAktif aktif, int? divisionId = null, int? memberId = null, int? periodId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, divisionId, memberId, periodId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<DivisionMember, EF.TDivisionMember>();
        }
        public static Response<DivisionMember> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TDivisionMembers.Add(row);
                context.SaveChanges();
                return new DivisionMember(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<DivisionMember>(msg));
        }
        public Response<DivisionMember> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(row, userId);
                context.SaveChanges();
                return new DivisionMember(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DivisionMember>(msg));
        }
        public Response<DivisionMember> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new DivisionMember(Reload(row.Id));
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<DivisionMember>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<DivisionMember>> GetDataListAsync(FilterAktif aktif, int? divisionId = null, int? memberId = null, int? periodId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, divisionId, memberId, periodId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<DivisionMember, EF.TDivisionMember>();
        }
        public static async Task<Response<DivisionMember>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TDivisionMembers.Add(row);
                await context.SaveChangesAsync();
                return new DivisionMember(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<DivisionMember>(msg));
        }
        public async Task<Response<DivisionMember>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(row, userId);
                await context.SaveChangesAsync();
                return new DivisionMember(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DivisionMember>(msg));
        }
        public async Task<Response<DivisionMember>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new DivisionMember(Reload(row.Id));
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<DivisionMember>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TDivisionMember Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TDivisionMember> FilterQuery(FilterAktif aktif, int? divisionId, int? memberId, int? periodId)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (divisionId == null || x.DivisionId == divisionId)
                    && (memberId == null || x.MemberId == memberId)
                    && (periodId == null || x.PeriodId == periodId))
                .OrderBy(x => x.Division.SortOrder).ThenBy(x => x.Division.Name).ThenBy(x => x.Member.Fullname);
        }
        private static EF.TDivisionMember GetRow(EF.ModelContext context, int id)
        {
            return context.TDivisionMembers.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Anggota Divisi"));
        }
        private static async Task<EF.TDivisionMember> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TDivisionMembers.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Anggota Divisi"));
        }
        private static string ToRoleCode(string? value)
        {
            var code = Regex.Replace(value?.Trim().ToUpperInvariant() ?? string.Empty, "[^A-Z0-9]+", "_").Trim('_');
            return code.Length == 0 ? DEFAULT_ROLE : code;
        }
        private static void ValidateDates(DateOnly? joinedAt, DateOnly? leftAt)
        {
            if (joinedAt != null && leftAt != null && leftAt < joinedAt)
            {
                throw new ArgumentException(ExceptionMessage.MEMBER_LEFT_DATE_INVALID);
            }
        }
        private static EF.TDivisionMember CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            Division.ValidateActiveDivision(context, newrow.DivisionId);
            Member.GetActiveMember(context, newrow.MemberId);
            var periodId = newrow.PeriodId > 0 ? newrow.PeriodId : null;
            if (periodId != null)
            {
                ManagementPeriod.ValidateOpenPeriod(context, periodId.Value);
            }
            // Unique index MySQL tidak menolak duplikat bila period_id NULL, jadi dicek di sini.
            ValidateUtility.CheckDuplicateData<EF.TDivisionMember>(context,
                x => x.DivisionId == newrow.DivisionId && x.MemberId == newrow.MemberId && x.PeriodId == periodId,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Anggota, Divisi dan Periode"));

            var row = new EF.TDivisionMember
            {
                Id = SequenceUtility.GetNextId<EF.TDivisionMember>(context),
                DivisionId = newrow.DivisionId,
                MemberId = newrow.MemberId,
                PeriodId = periodId,
                MemberRole = ToRoleCode(newrow.MemberRole),
                JoinedAt = newrow.JoinedAt ?? DateOnly.FromDateTime(DateTime.Today),
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Divisi, anggota dan periode tetap. Menonaktifkan mengisi tanggal keluar bila kosong.
        private void SetRow(EF.TDivisionMember row, int userId)
        {
            if (!Enum.IsDefined(IsActive))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status"));
            }
            var leftAt = IsActive == ActiveStatus.TidakAktif ? LeftAt ?? DateOnly.FromDateTime(DateTime.Today) : LeftAt;
            ValidateDates(JoinedAt, leftAt);

            row.MemberRole = ToRoleCode(MemberRole);
            row.JoinedAt = JoinedAt;
            row.LeftAt = leftAt;
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.TDivisionMember row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.LeftAt ??= DateOnly.FromDateTime(DateTime.Today);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
