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

namespace GWALib.Lib.Master.Anggota
{
    public partial class AlumniProfile
    {
        private const int MIN_YEAR = 1900;

        #region SYNC
        public static AlumniProfile? GetByMemberId(int memberId)
        {
            var row = _Querys.SingleOrDefault(x => x.MemberId == memberId);
            return row != null ? new AlumniProfile(row) : null;
        }
        public static List<AlumniProfile> GetDataList(ContactPermission? isContactable = null, short? graduationYear = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(isContactable, graduationYear);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<AlumniProfile, EF.MAlumniProfile>();
        }
        public static Response<AlumniProfile> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MAlumniProfiles.Add(row);
                context.SaveChanges();
                return new AlumniProfile(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AlumniProfile>(msg));
        }
        public Response<AlumniProfile> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(row, userId);
                context.SaveChanges();
                return new AlumniProfile(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AlumniProfile>(msg));
        }
        public Response<AlumniProfile> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                context.MAlumniProfiles.Remove(row);
                context.SaveChanges();
                return new AlumniProfile(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<AlumniProfile>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<AlumniProfile?> GetByMemberIdAsync(int memberId)
        {
            var row = await _Querys.SingleOrDefaultAsync(x => x.MemberId == memberId);
            return row != null ? new AlumniProfile(row) : null;
        }
        public static async Task<List<AlumniProfile>> GetDataListAsync(ContactPermission? isContactable = null, short? graduationYear = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(isContactable, graduationYear);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<AlumniProfile, EF.MAlumniProfile>();
        }
        public static async Task<Response<AlumniProfile>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MAlumniProfiles.Add(row);
                await context.SaveChangesAsync();
                return new AlumniProfile(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<AlumniProfile>(msg));
        }
        public async Task<Response<AlumniProfile>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(row, userId);
                await context.SaveChangesAsync();
                return new AlumniProfile(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<AlumniProfile>(msg));
        }
        public async Task<Response<AlumniProfile>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                context.MAlumniProfiles.Remove(row);
                await context.SaveChangesAsync();
                return new AlumniProfile(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<AlumniProfile>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.MAlumniProfile> FilterQuery(ContactPermission? isContactable, short? graduationYear)
        {
            return _Querys.Where(x => (isContactable == null || x.IsContactable == (int)isContactable)
                    && (graduationYear == null || x.GraduationYear == graduationYear))
                .OrderBy(x => x.Member.Fullname);
        }
        private static EF.MAlumniProfile GetRow(EF.ModelContext context, int id)
        {
            return context.MAlumniProfiles.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Profil Alumni"));
        }
        private static async Task<EF.MAlumniProfile> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MAlumniProfiles.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Profil Alumni"));
        }
        private static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        private static void ValidateData(short? graduationYear, ContactPermission isContactable)
        {
            if (graduationYear != null && (graduationYear < MIN_YEAR || graduationYear > DateTime.Today.Year))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Tahun Lulus"));
            }
            if (!Enum.IsDefined(isContactable))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kesediaan Dihubungi"));
            }
        }
        private static EF.MAlumniProfile CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var member = Member.GetMember(context, newrow.MemberId);
            if (member.Status != (int)MemberStatus.Alumni)
            {
                throw new ArgumentException(ExceptionMessage.MEMBER_NOT_ALUMNI);
            }
            ValidateUtility.CheckDuplicateData<EF.MAlumniProfile>(context, x => x.MemberId == newrow.MemberId,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Anggota"));
            ValidateData(newrow.GraduationYear, newrow.IsContactable);

            var row = new EF.MAlumniProfile
            {
                Id = SequenceUtility.GetNextId<EF.MAlumniProfile>(context),
                MemberId = newrow.MemberId,
                GraduationYear = newrow.GraduationYear,
                CurrentJob = ToNullable(newrow.CurrentJob),
                Company = ToNullable(newrow.Company),
                Industry = ToNullable(newrow.Industry),
                City = ToNullable(newrow.City),
                LinkedinUrl = ToNullable(newrow.LinkedinUrl),
                IsContactable = (int)newrow.IsContactable,
                Notes = ToNullable(newrow.Notes),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Anggota tidak bisa diganti.
        private void SetRow(EF.MAlumniProfile row, int userId)
        {
            ValidateData(GraduationYear, IsContactable);

            row.GraduationYear = GraduationYear;
            row.CurrentJob = ToNullable(CurrentJob);
            row.Company = ToNullable(Company);
            row.Industry = ToNullable(Industry);
            row.City = ToNullable(City);
            row.LinkedinUrl = ToNullable(LinkedinUrl);
            row.IsContactable = (int)IsContactable;
            row.Notes = ToNullable(Notes);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        #endregion
    }
}
