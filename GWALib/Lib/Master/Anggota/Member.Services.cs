using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Akses;
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
    public partial class Member
    {
        private const int MIN_YEAR = 1900;

        #region SYNC
        public List<MemberPhoto.Info> GetPhotos(bool refresh)
        {
            if (refresh || PhotoList == null)
            {
                PhotoList = MemberPhoto.GetInfoList(Id);
            }
            return PhotoList;
        }
        public AlumniProfile? GetAlumniProfile(bool refresh)
        {
            if (refresh || !_isAlumniProfileLoaded)
            {
                AlumniProfileData = AlumniProfile.GetByMemberId(Id);
                _isAlumniProfileLoaded = true;
            }
            return AlumniProfileData;
        }
        public static List<Member> GetDataList(MemberStatus? status = null, MemberType? memberType = null, MemberCategory? memberCategory = null, short? angkatan = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, memberType, memberCategory, angkatan);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Member, EF.MMember>();
        }
        public static Response<Member> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MMembers.Add(row);
                context.SaveChanges();
                return new Member(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Member>(msg));
        }
        public Response<Member> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new Member(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Member>(msg));
        }
        public Response<Member> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new Member(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Member>(msg));
        }
        // Anggota milik user yang login (Area Anggota); null bila user tidak terhubung ke anggota.
        public static Member? GetByUser(string user)
        {
            var userId = ValidateUtility.ValidateUser(user);
            using var context = DBClass.GetContext();
            var id = context.MMembers.Where(x => x.UserId == userId).Select(x => (int?)x.Id).FirstOrDefault();
            return id.HasValue ? new Member(id.Value) : null;
        }
        #endregion

        #region ASYNC
        public async Task<List<MemberPhoto.Info>> GetPhotosAsync(bool refresh)
        {
            if (refresh || PhotoList == null)
            {
                PhotoList = await MemberPhoto.GetInfoListAsync(Id);
            }
            return PhotoList;
        }
        public async Task<AlumniProfile?> GetAlumniProfileAsync(bool refresh)
        {
            if (refresh || !_isAlumniProfileLoaded)
            {
                AlumniProfileData = await AlumniProfile.GetByMemberIdAsync(Id);
                _isAlumniProfileLoaded = true;
            }
            return AlumniProfileData;
        }
        public static async Task<List<Member>> GetDataListAsync(MemberStatus? status = null, MemberType? memberType = null, MemberCategory? memberCategory = null, short? angkatan = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, memberType, memberCategory, angkatan);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Member, EF.MMember>();
        }
        public static async Task<Response<Member>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MMembers.Add(row);
                await context.SaveChangesAsync();
                return new Member(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Member>(msg));
        }
        public async Task<Response<Member>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new Member(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Member>(msg));
        }
        public async Task<Response<Member>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new Member(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Member>(msg));
        }
        #endregion

        #region HELPER
        // Status apa pun (alumni tetap boleh punya foto / profil alumni).
        internal static EF.MMember GetMember(EF.ModelContext context, int memberId)
        {
            var row = context.MMembers.SingleOrDefault(x => x.Id == memberId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Anggota"));
            return row;
        }
        internal static EF.MMember GetActiveMember(EF.ModelContext context, int memberId)
        {
            var row = GetMember(context, memberId);
            if (row.Status != (int)MemberStatus.Active)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Anggota"));
            }
            return row;
        }
        private static IQueryable<EF.MMember> FilterQuery(MemberStatus? status, MemberType? memberType, MemberCategory? memberCategory, short? angkatan)
        {
            return _Querys.Where(x => (status == null || x.Status == (int)status)
                    && (memberType == null || x.MemberType == (int)memberType)
                    && (memberCategory == null || x.MemberCategory == (int)memberCategory)
                    && (angkatan == null || x.Angkatan == angkatan))
                .OrderBy(x => x.Fullname);
        }
        private static EF.MMember GetRow(EF.ModelContext context, int id)
        {
            return context.MMembers.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Anggota"));
        }
        private static async Task<EF.MMember> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MMembers.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Anggota"));
        }
        private static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        private static void ValidateData(EF.ModelContext context, int id, int? memberUserId, string? memberNumber, MemberType memberType,
            DateOnly? joinedAt, DateOnly? leftAt, MemberCategory memberCategory, string? nim, string fullname, Gender? gender, DateOnly? birthDate, short? angkatan)
        {
            if (string.IsNullOrWhiteSpace(fullname))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Lengkap"));
            }
            if (!Enum.IsDefined(memberType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Anggota"));
            }
            if (joinedAt != null && leftAt != null && leftAt < joinedAt)
            {
                throw new ArgumentException(ExceptionMessage.MEMBER_LEFT_DATE_INVALID);
            }
            if (!Enum.IsDefined(memberCategory))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kategori Anggota"));
            }
            if (memberCategory == MemberCategory.Mahasiswa && nim == null)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "NIM"));
            }
            if (gender != null && !Enum.IsDefined(gender.Value))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Kelamin"));
            }
            if (birthDate != null && birthDate > DateOnly.FromDateTime(DateTime.Today))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Tanggal Lahir"));
            }
            if (angkatan != null && (angkatan < MIN_YEAR || angkatan > DateTime.Today.Year + 1))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Angkatan"));
            }
            if (nim != null)
            {
                ValidateUtility.CheckDuplicateData<EF.MMember>(context, x => x.Nim == nim && x.Id != id,
                    string.Format(ExceptionMessage.DATA_DUPLICATE, "NIM"));
            }
            if (memberNumber != null)
            {
                ValidateUtility.CheckDuplicateData<EF.MMember>(context, x => x.MemberNumber == memberNumber && x.Id != id,
                    string.Format(ExceptionMessage.DATA_DUPLICATE, "Nomor Anggota"));
            }
            if (memberUserId != null)
            {
                UserAccount.GetUser(context, memberUserId.Value);
                ValidateUtility.CheckDuplicateData<EF.MMember>(context, x => x.UserId == memberUserId && x.Id != id,
                    ExceptionMessage.USER_MEMBER_USED);
            }
        }
        // Dipakai juga RecruitmentApplicant saat pendaftar diterima (tanpa SaveChanges).
        internal static EF.MMember CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var memberUserId = newrow.UserId > 0 ? newrow.UserId : null;
            var memberNumber = ToNullable(newrow.MemberNumber)?.ToUpperInvariant();
            var nim = ToNullable(newrow.Nim);
            var fullname = newrow.Fullname?.Trim() ?? string.Empty;
            ValidateData(context, 0, memberUserId, memberNumber, newrow.MemberType, newrow.JoinedAt, null, newrow.MemberCategory, nim, fullname,
                newrow.Gender, newrow.BirthDate, newrow.Angkatan);

            var row = new EF.MMember
            {
                Id = SequenceUtility.GetNextId<EF.MMember>(context),
                UserId = memberUserId,
                MemberNumber = memberNumber,
                MemberType = (int)newrow.MemberType,
                JoinedAt = newrow.JoinedAt,
                MemberCategory = (int)newrow.MemberCategory,
                Nim = nim,
                Fullname = fullname,
                Gender = newrow.Gender != null ? (int)newrow.Gender : null,
                BirthDate = newrow.BirthDate,
                Fakultas = ToNullable(newrow.Fakultas),
                ProgramStudi = ToNullable(newrow.ProgramStudi),
                Angkatan = newrow.Angkatan,
                Institution = ToNullable(newrow.Institution),
                Phone = ToNullable(newrow.Phone),
                Email = ToNullable(newrow.Email),
                Address = ToNullable(newrow.Address),
                Status = (int)MemberStatus.Active,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            // Semua anggota wajib punya section: section awal dicatat mulai tanggal bergabung (atau hari ini).
            row.TSectionMembers.Add(Transaksi.Anggota.SectionMember.NewHistoryRow(context, row, newrow.SectionId,
                newrow.JoinedAt ?? DateOnly.FromDateTime(DateTime.Today), "Section awal", userId));
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.MMember row, int userId)
        {
            int? memberUserId = User != null && User.Id > 0 ? User.Id : null;
            var memberNumber = ToNullable(MemberNumber)?.ToUpperInvariant();
            var nim = ToNullable(Nim);
            var fullname = Fullname?.Trim() ?? string.Empty;
            ValidateData(context, row.Id, memberUserId, memberNumber, MemberType, JoinedAt, LeftAt, MemberCategory, nim, fullname,
                Gender, BirthDate, Angkatan);
            if (!Enum.IsDefined(Status))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Anggota"));
            }

            row.UserId = memberUserId;
            row.MemberNumber = memberNumber;
            row.MemberType = (int)MemberType;
            row.JoinedAt = JoinedAt;
            row.LeftAt = LeftAt;
            row.MemberCategory = (int)MemberCategory;
            row.Nim = nim;
            row.Fullname = fullname;
            row.Gender = Gender != null ? (int)Gender : null;
            row.BirthDate = BirthDate;
            row.Fakultas = ToNullable(Fakultas);
            row.ProgramStudi = ToNullable(ProgramStudi);
            row.Angkatan = Angkatan;
            row.Institution = ToNullable(Institution);
            row.Phone = ToNullable(Phone);
            row.Email = ToNullable(Email);
            row.Address = ToNullable(Address);
            row.Status = (int)Status;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MMember row, int userId)
        {
            row.Status = (int)MemberStatus.Inactive;
            row.LeftAt ??= DateOnly.FromDateTime(DateTime.Today);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
